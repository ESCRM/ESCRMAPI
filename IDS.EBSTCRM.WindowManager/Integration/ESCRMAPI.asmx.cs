using System.Web.Services;
using IDS.EBSTCRM.Base;
using IDS.EBSTCRM.WindowManager.Integration.Models;
using IDS.EBSTCRM.WindowManager.Integration.Services;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Data;
using System.Text;
using Newtonsoft.Json;
using IDS.EBSTCRM.WindowManager.Service;

namespace IDS.EBSTCRM.WindowManager.Integration {

    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class ESCRMAPI : System.Web.Services.WebService {

        [WebMethod]
        public SysDynamicFields GetDynamicFields(Credential Credential) {

            var user = new User();
            var response = new SysDynamicFields();
            var validation = DynamicFieldService.IsValid(Credential, out user);
            if (validation.Status) {
                var context = new SQLDB();

                // Dynamic Fields
                var Fields = context.dynamicFields_getFieldsForOrganisationFromSQL(user.Organisation, "");
                foreach (DynamicField f in Fields) {

                    if (!f.FieldType.ContainsAny(DynamicFieldService.InvalidUIFieldType)) {

                        var restrictedList = SysField.GetRestrictedList(context, user.OrganisationId, f);
                        if (f.DatabaseTable.IndexOf("contacts", StringComparison.OrdinalIgnoreCase) > 0) {
                            response.Contact.Add(new SysField() {
                                Name = f.DatabaseColumn,
                                DataType = f.FieldType,
                                IsRequired = (f.RequiredState == 1 ? true : false),
                                Value = string.Empty,
                                FieldView = f.ViewState,
                                ValueRestrictedList = restrictedList
                            });
                        } else {
                            response.Company.Add(new SysField() {
                                Name = f.DatabaseColumn,
                                DataType = f.FieldType,
                                IsRequired = (f.RequiredState == 1 ? true : false),
                                Value = string.Empty,
                                FieldView = f.ViewState,
                                ValueRestrictedList = restrictedList
                            });
                        }
                    }
                }

                context.Dispose();
                context = null;
            }
            response.Status = validation.Status;
            response.Message = validation.Message;
            return (response);
        }

        [WebMethod]
        public Response CreateUpdateCompany(Credential Credential, SysCompany Company) {
            try {
                var user = new User();
                var validation = CredentialService.IsValid(Credential, out user);
                if (validation.Status) {

                    var context = new SQLDB();
                    var lFields = context.dynamicFields_getFieldsForOrganisationFromSQL(user.Organisation, string.Empty);
                    var companyDynamicField = lFields.Where(w => (w.DatabaseTable).Contains("companies")
                                                    && !DynamicFieldService.InvalidUIFieldType.Contains(w.FieldType)
                                                    && (DynamicFieldService.ValidUIFieldViewState.Contains(w.ViewState) || w.ViewState == (Company.CompanyDetail.IsPOT ? "POT" : "SMV"))).ToList();

                    var contactDynamicField = lFields.Where(w => (w.DatabaseTable).Contains("contacts")
                                                    && !DynamicFieldService.InvalidUIFieldType.Contains(w.FieldType)
                                                    && (DynamicFieldService.ValidUIFieldViewState.Contains(w.ViewState) || w.ViewState == (Company.CompanyDetail.IsPOT ? "POT" : "SMV"))).ToList();

                    validation = CompanyService.IsValid(Company, companyDynamicField, contactDynamicField);

                    if (validation.Status) {

                        // Save Company
                        var c = new Base.Company();
                        c.Id = Company.CompanyDetail.CompanyId;
                        c.CreatedById = user.Id;
                        c.Type = Company.CompanyDetail.IsPOT ? 1 : 0;
                        c.NNEId = Company.CompanyDetail.NNEID;

                        foreach (var field in Company.CompanyDetail.Fields) {
                            //** Check for null, hvis ja så spring over, ESCRM-21/35
                            if (field == null)
                                continue;

                            var baseField = companyDynamicField.FirstOrDefault(w => w.DatabaseColumn == field.Name);
                            if (baseField != null && (field.FieldView == "General" || field.FieldView == (Company.CompanyDetail.IsPOT ? "POT" : "SMV"))) {

                                string[] tmp = baseField.DatabaseTable.Split('_');
                                string key = (baseField.DatabaseTable + "_" + baseField.DatabaseColumn + "_" + tmp[tmp.Length - 1]);
                                object val = field.Value;
                                string dataType = CompanyService.GetDataType(companyDynamicField, key, ref val);

                                c.DynamicTblColumns.Add(new TableColumnWithValue(key, dataType, 0, val));
                            }
                        }

                        foreach (TableColumnWithValue t in c.DynamicTblColumns) {
                            t.MatchDynamicField(lFields);
                        }

                        //Get PNR from POST or CPR
                        string PNR = "";
                        var nneCompanyPNR = companyDynamicField.FirstOrDefault(w => w.DataLink == "NNECompanyPNR");
                        if (nneCompanyPNR != null) {
                            //var field = Company.CompanyDetail.Fields.FirstOrDefault(w => w.Name == nneCompanyPNR.DatabaseColumn);
                            var field = Company.CompanyDetail.Fields.Where(w => w.Name == nneCompanyPNR.DatabaseColumn).FirstOrDefault();
                            if (field != null)
                            {
                                PNR = field.Value;
                            }
                        }

                        // Add Classification details 
                        if (c.DynamicTblColumns != null) {
                            foreach (DynamicField f in lFields) {
                                string[] tmp = f.DatabaseTable.Split('_');
                                var name = f.DatabaseTable + "_" + f.DatabaseColumn + "_" + tmp[tmp.Length - 1];
                                var contactdf = c.DynamicTblColumns.FirstOrDefault(w => w.Name == name);
                                if (contactdf != null) {
                                    c.DynamicTblColumns.FirstOrDefault(w => w.Name == name).ClassificationId = f.DataClassificationId;
                                    c.DynamicTblColumns.FirstOrDefault(w => w.Name == name).AnonymizationId = f.AnonymizationId;
                                }
                            }
                        }

                        var existingPnumbers = context.companies_GetCompaniesWithSamePNR(user.OrganisationId, c.Id, PNR);

                        //No save, if pnumber exists
                        if (existingPnumbers.Count == 0) {
                            c.Id = context.Company_Update(user, c);

                            //If the ID is zero - something went really wrong and we do not convert anything.
                            if (c.Id > 0) {
                                //if (Request.Form["convertToSMV"] == "true")
                                context.companies_convert(user, c);
                            }

                            validation.Status = true;
                            validation.Message.Add("CompanyID - " + c.Id);
                        } else {
                            validation.Status = false;
                            validation.Message.Add("Company - PNumber already exist.");
                        }


                        // Save Contact when company saved successfully
                        if (validation.Status && Company.CompanyDetail.CompanyId == 0) {
                            var contact = new Base.Contact();
                            contact.Id = 0;
                            contact.CompanyId = c.Id;
                            contact.CreatedById = user.Id;
                            contact.OrganisationId = user.OrganisationId;
                            contact.Type = c.Type;

                            foreach (var field in Company.CompanyDetail.Contact.Fields) {
                                var baseField = contactDynamicField.FirstOrDefault(w => w.DatabaseColumn == field.Name);
                                if (baseField != null && (field.FieldView == "General" || field.FieldView == (Company.CompanyDetail.IsPOT ? "POT" : "SMV"))) {
                                    string[] tmp = baseField.DatabaseTable.Split('_');
                                    string key = (baseField.DatabaseTable + "_" + baseField.DatabaseColumn + "_" + tmp[tmp.Length - 1]);
                                    object val = field.Value;
                                    string dataType = CompanyService.GetDataType(contactDynamicField, key, ref val);
                                    contact.DynamicTblColumns.Add(new TableColumnWithValue(key, dataType, 0, val));
                                }
                            }

                            //20220630 Match contact fields to update db
                            foreach (TableColumnWithValue t in contact.DynamicTblColumns)
                            {
                                t.MatchDynamicField(lFields);
                            }

                            //Get CPR from POST 
                            string CPR = "";
                            var companyCPR = contactDynamicField.FirstOrDefault(w => w.DataLink == "CompanyCPR");
                            if (companyCPR != null) {
                                var field = Company.CompanyDetail.Contact.Fields.FirstOrDefault(w => w.Name == companyCPR.DatabaseColumn);
                                if (field != null) {
                                    CPR = field.Value;
                                }
                            }

                            var existingCPRNumbers = context.contacts_GetContactsWithSameCPR(user.OrganisationId, contact.Id, CPR);
                            //No save, if CPR exists
                            if (existingCPRNumbers.Count == 0) {
                                contact.Id = context.Contact_Update(user, contact);

                                // Change stage of the Contact
                                context.companies_verifyStructure(0, contact.Id);

                                // Change stage of the Company
                                context.companies_verifyStructure(c.Id, 0);

                                validation.Status = true;
                                validation.Message.Add("ContactID - " + contact.Id);
                            } else {
                                validation.Status = false;
                                validation.Message.Add("Contact - CPR already exist.");
                            }
                        } else {

                            // When it's a company update request
                            if (validation.Status) {
                                // Change stage of the Company
                                context.companies_verifyStructure(c.Id, 0);
                            }
                        }
                    }

                    context.Dispose();
                    context = null;
                }
                return validation;
            } catch (Exception ex) {
                return (new Response() {
                    Status = false,
                    Message = new List<string>() {
                        ex.Message
                    }
                });
            }
        }

        [WebMethod]
        public Response CreateUpdateContact(Credential Credential, SysContact Contact) {
            try {
                var user = new User();
                var validation = CredentialService.IsValid(Credential, out user);
                if (validation.Status) {

                    var context = new SQLDB();
                    var lFields = context.dynamicFields_getFieldsForOrganisationFromSQL(user.Organisation, string.Empty);
                    //var allContactFields = lFields.Where(w => w.DatabaseTable.Contains("contacts") && !DynamicFieldService.InvalidUIFieldType.Contains(w.FieldType) && DynamicFieldService.ValidUIFieldViewState.Contains(w.ViewState)).ToList();
                    //var contactDynamicField = new List<DynamicField>();
                    //foreach (var field in allContactFields) {
                    //    contactDynamicField.Add(field);
                    //}
                    var contactDynamicField = lFields.Where(w => w.DatabaseTable.Contains("contacts")
                                                    && !DynamicFieldService.InvalidUIFieldType.Contains(w.FieldType)
                                                    && (DynamicFieldService.ValidUIFieldViewState.Contains(w.ViewState) || w.ViewState == (Contact.ContactDetail.IsPOT ? "POT" : "SMV"))).ToList();

                    validation = ContactService.IsValid(Contact.ContactDetail.Fields, contactDynamicField);
                    if (Contact.ContactDetail.CompanyId == 0) {
                        validation.Status = false;
                        validation.Message.Add("Invalid Company Id");
                    } else {
                        var companyId = 0;
                        var company = context.APIGetCompanyIdForContact(Contact.ContactDetail.ContactId);
                        foreach (DataRow row in company.Rows) {
                            companyId = (row.Table.Columns.Contains("CompanyOwnerId") ? Convert.ToInt32(row["CompanyOwnerId"]) : 0);
                        }

                        // validate company id
                        if (Contact.ContactDetail.CompanyId != companyId) {
                            validation.Status = false;
                            validation.Message.Add("Det er ikke tilladt at flytte kontaktpersoner, den angivne virksomhedsnøgle [x] matcher ikke den registrerede [y]");
                        }
                    }

                    if (validation.Status) {

                        // Save Contact when company saved successfully
                        if (validation.Status) {
                            var contact = new Base.Contact();
                            contact.Id = Contact.ContactDetail.ContactId;
                            contact.CompanyId = Contact.ContactDetail.CompanyId;
                            contact.CreatedById = user.Id;
                            contact.OrganisationId = user.OrganisationId;
                            contact.Type = Contact.ContactDetail.IsPOT ? 1 : 0;

                            foreach (var field in Contact.ContactDetail.Fields) {
                                var baseField = contactDynamicField.FirstOrDefault(w => w.DatabaseColumn == field.Name);
                                if (baseField != null && (field.FieldView == "General" || field.FieldView == (Contact.ContactDetail.IsPOT ? "POT" : "SMV"))) {
                                    string[] tmp = baseField.DatabaseTable.Split('_');
                                    string key = (baseField.DatabaseTable + "_" + baseField.DatabaseColumn + "_" + tmp[tmp.Length - 1]);
                                    object val = field.Value;
                                    string dataType = CompanyService.GetDataType(contactDynamicField, key, ref val);
                                    contact.DynamicTblColumns.Add(new TableColumnWithValue(key, dataType, 0, val));
                                }
                            }
                            foreach (TableColumnWithValue t in contact.DynamicTblColumns) {
                                t.MatchDynamicField(lFields);
                            }

                            //Get CPR from POST 
                            string CPR = "";
                            var companyCPR = contactDynamicField.FirstOrDefault(w => w.DataLink == "CompanyCPR");
                            if (companyCPR != null) {
                                var field = Contact.ContactDetail.Fields.FirstOrDefault(w => w.Name == companyCPR.DatabaseColumn);
                                if (field != null) {
                                    CPR = field.Value;
                                }
                            }

                            // Add Classification details 
                            if (contact.DynamicTblColumns != null) {
                                foreach (DynamicField f in lFields) {
                                    string[] tmp = f.DatabaseTable.Split('_');
                                    var name = f.DatabaseTable + "_" + f.DatabaseColumn + "_" + tmp[tmp.Length - 1];
                                    var contactdf = contact.DynamicTblColumns.FirstOrDefault(w => w.Name == name);
                                    if (contactdf != null) {
                                        contact.DynamicTblColumns.FirstOrDefault(w => w.Name == name).ClassificationId = f.DataClassificationId;
                                        contact.DynamicTblColumns.FirstOrDefault(w => w.Name == name).AnonymizationId = f.AnonymizationId;
                                    }
                                }
                            }

                            var existingCPRNumbers = context.contacts_GetContactsWithSameCPR(user.OrganisationId, contact.Id, CPR);
                            //No save, if CPR exists
                            if (existingCPRNumbers.Count == 0) {
                                contact.Id = context.Contact_Update(user, contact);

                                // Change stage of the Contact
                                context.companies_verifyStructure(0, contact.Id);

                                validation.Status = true;
                                validation.Message.Add("ContactID - " + contact.Id);
                            } else {
                                validation.Status = false;
                                validation.Message.Add("Contact - CPR already exist.");
                            }
                        }
                    }

                    context.Dispose();
                    context = null;
                }
                return validation;
            } catch (Exception ex) {
                return (new Response() {
                    Status = false,
                    Message = new List<string>() {
                        ex.Message
                    }
                });
            }
        }

        [WebMethod]
        public Response UploadContactFile(Credential Credential, Models.UploadContactFile upload) {
            try {
                var user = new User();
                var response = new DownloadFileResponse();
                var validation = CredentialService.IsValid(Credential, out user);
                if (validation.Status) {
                    // File name
                    if (string.IsNullOrEmpty(upload.FileName)) {
                        response.Status = false;
                        response.Message.Add("File name is required.");
                        return response;
                    }
                    // Upload stream
                    if (upload.FileBytes == null) {
                        response.Status = false;
                        response.Message = new List<string>() { "Please select a file for upload." };
                        return response;
                    }

                    // Valid response
                    if (response.Status) {

                        var context = new SQLDB();
                        var file = new IDS.EBSTCRM.Base.File();
                        var binary = new IDS.EBSTCRM.Base.Binary();

                        //Save binary data
                        binary.Data = upload.FileBytes;
                        binary.Id = context.filearchive_insertBinary(binary);

                        //Set file
                        file.binary = binary.Id;
                        file.filename = upload.FileName;
                        file.contenttype = (upload.FileName.Contains(".msg") ? "Outlook/Email" : "File/Document");
                        file.description = null;
                        file.organisationId = user.OrganisationId;
                        file.contactId = upload.ContactId;
                        file.contentgroup = "";
                        file.contentlength = upload.FileBytes.Length;
                        file.folderType = 1;
                        file.userId = user.Id;

                        file.Id = context.filearchive_insertFile(user, file);

                        if (upload.Shared != null && upload.Shared.OrganisationId.Any()) {
                            upload.Shared.OrganisationId = upload.Shared.OrganisationId.Where(s => s > 0).Distinct().ToList();
                            var sharedOrgIds = string.Join(",", upload.Shared.OrganisationId);
                            if (!string.IsNullOrEmpty(sharedOrgIds)) {
                                context.APIShareFile(file.Id, upload.ContactId, sharedOrgIds);
                            }
                        }

                        response.Status = true;
                        response.Message = new List<string>() { "File Id - " + file.Id.ToString() };

                        context.Dispose();
                        context = null;
                    }
                } else {
                    response.Status = validation.Status;
                    response.Message = validation.Message;
                }
                return response;
            } catch (Exception ex) {
                return (new Response() {
                    Status = false,
                    Message = new List<string>() {
                        ex.Message
                    }
                });
            }
        }

        //** Ny metode til at slette filer, ESCRM-163
        [WebMethod]
        public DeleteFileResponse DeleteContactFile(Credential Credential, int FileId)
        {
            try
            {
                var sensitiveDocment = false;
                var user = new User();
                var response = new DeleteFileResponse();
                var validation = CredentialService.IsValid(Credential, out user);
                //Binary B = new Binary();
                //var binary = 0;

                if (validation.Status)
                {
                    var context = new SQLDB();
                    var file = context.APIGetFile(user, FileId, true);
                    if (file != null && file.Rows.Count > 0)
                    {
                        foreach (DataRow row in file.Rows)
                        {
                            response.FileId = FileId; // Convert.ToInt32(row["Id"]);
                            response.Filename = Convert.ToString(row["filename"]);
                            response.Data = (byte[])row["Data"];
                            sensitiveDocment = Convert.ToBoolean(row["Sensitive"]);
                        }

                        //** Delete file
                        var result = context.filearchive_DeleteFileWS(user, FileId, user.Id);

                        response.SprocStatus = result;
                    }
                    else
                    {
                        response.Status = false;
                        response.Message = new List<string>() { "Fil findes ikke" };
                    }

                    context.Dispose();
                    context = null;
                }
                else
                {
                    response.Status = validation.Status;
                    response.Message = validation.Message;
                }
                return (response);
            }
            catch (Exception ex)
            {
                return (new DeleteFileResponse()
                {
                    Status = false,
                    Message = new List<string>() {
                        ex.Message
                    }
                });
            }
        }

        [WebMethod]
        public DownloadFileResponse DownloadFile(Credential Credential, int FileId) {
            try {
                var sensitiveDocment = false;
                var user = new User();
                var response = new DownloadFileResponse();
                var validation = CredentialService.IsValid(Credential, out user);
                if (validation.Status) {
                    var context = new SQLDB();
                    var file = context.APIGetFile(user, FileId, true);
                    if (file != null && file.Rows.Count > 0) {
                        foreach (DataRow row in file.Rows) {
                            response.FileId = Convert.ToInt32(row["Id"]);
                            response.Filename = Convert.ToString(row["filename"]);
                            response.Data = (byte[])row["Data"];
                            sensitiveDocment = Convert.ToBoolean(row["Sensitive"]);
                        }

                        if (sensitiveDocment) {
                            response.Status = false;
                            response.Message = new List<string>() { "This function is not allowed to read sensitive file.Please use DownloadSensitiveFile method to read sensitive file." };
                        }
                    } else {
                        response.Status = false;
                        response.Message = new List<string>() { "File not exist" };
                    }

                    context.Dispose();
                    context = null;
                } else {
                    response.Status = validation.Status;
                    response.Message = validation.Message;
                }
                return (response);
            } catch (Exception ex) {
                return (new DownloadFileResponse() {
                    Status = false,
                    Message = new List<string>() {
                        ex.Message
                    }
                });
            }
        }

        [WebMethod]
        public DownloadFileResponse DownloadSensitiveFile(Credential Credential, int FileId) {
            try {
                var user = new User();
                var response = new DownloadFileResponse();
                var validation = CredentialService.IsValid(Credential, out user);
                if (validation.Status) {
                    var context = new SQLDB();
                    var file = context.APIGetFile(user, FileId, false);
                    if (file != null && file.Rows.Count > 0) {
                        foreach (DataRow row in file.Rows) {
                            response.FileId = Convert.ToInt32(row["Id"]);
                            response.Filename = Convert.ToString(row["filename"]);
                            response.Data = (byte[])row["Data"];
                        }
                    } else {
                        response.Status = false;
                        response.Message = new List<string>() { "File not exist" };
                    }

                    context.Dispose();
                    context = null;
                } else {
                    response.Status = validation.Status;
                    response.Message = validation.Message;
                }
                return (response);
            } catch (Exception ex) {
                return (new DownloadFileResponse() {
                    Status = false,
                    Message = new List<string>() {
                        ex.Message
                    }
                });
            }
        }

        [WebMethod]
        public SearchCompanyResponse SearchCompany(Credential Credential, SearchCompanyRequest Request) {
            try {
                var user = new User();
                var response = new SearchCompanyResponse();
                var validation = CredentialService.IsValid(Credential, out user);
                if (validation.Status) {
                    var context = new SQLDB();
                    var query = new StringBuilder();
                    query.Append("<Parameters>");
                    query.Append("      <Query>");
                    query.Append("          <CompanyID>{CompanyID}</CompanyID>");
                    query.Append("          <ContactID>{ContactID}</ContactID>");
                    query.Append("          <CompanyName>{CompanyName}</CompanyName>");
                    query.Append("          <Firstname>{Firstname}</Firstname>");
                    query.Append("          <Lastname>{Lastname}</Lastname>");
                    query.Append("          <CVR>{CVR}</CVR>");
                    query.Append("          <PNR>{PNR}</PNR>");
                    query.Append("          <Address>{Address}</Address>");
                    query.Append("          <Zipcode>{Zipcode}</Zipcode>");
                    query.Append("          <City>{City}</City>");
                    query.Append("          <Phone>{Phone}</Phone>");
                    query.Append("          <Email>{Email}</Email>");
                    query.Append("      </Query>");
                    query.Append("      <IncludeLocalFields>{IncludeLocalFields}</IncludeLocalFields>");
                    query.Append("      {AVN}");
                    query.Append("      <IncludeOwnNotes>{IncludeOwnNotes}</IncludeOwnNotes>");
                    query.Append("      <IncludeOtherNotes>{IncludeOtherNotes}</IncludeOtherNotes>");
                    query.Append("      <IncludeMeetings>{IncludeMeetings}</IncludeMeetings>");
                    query.Append("</Parameters>");

                    // Query configration
                    query.Replace("{CompanyID}", Convert.ToString(Request.Query.CompanyID) == "0" ? String.Empty : Convert.ToString(Request.Query.CompanyID));
                    query.Replace("{ContactID}", Convert.ToString(Request.Query.ContactID) == "0" ? String.Empty : Convert.ToString(Request.Query.ContactID));
                    query.Replace("{CompanyName}", !String.IsNullOrEmpty(Request.Query.CompanyName) ? Request.Query.CompanyName : String.Empty);
                    query.Replace("{Firstname}", !String.IsNullOrEmpty(Request.Query.Firstname) ? Request.Query.Firstname : String.Empty);
                    query.Replace("{Lastname}", !String.IsNullOrEmpty(Request.Query.Lastname) ? Request.Query.Lastname : String.Empty);
                    query.Replace("{CVR}", !String.IsNullOrEmpty(Request.Query.CVR) ? Request.Query.CVR : String.Empty);
                    query.Replace("{PNR}", Convert.ToString(Request.Query.PNR) == "0" ? String.Empty : Convert.ToString(Request.Query.PNR));
                    query.Replace("{Address}", !String.IsNullOrEmpty(Request.Query.Address) ? Request.Query.Address : String.Empty);
                    query.Replace("{Zipcode}", !String.IsNullOrEmpty(Request.Query.Zipcode) ? Request.Query.Zipcode : String.Empty);
                    query.Replace("{City}", !String.IsNullOrEmpty(Request.Query.City) ? Request.Query.City : String.Empty);
                    query.Replace("{Phone}", !String.IsNullOrEmpty(Request.Query.Phone) ? Request.Query.Phone : String.Empty);
                    query.Replace("{Email}", !String.IsNullOrEmpty(Request.Query.Email) ? Request.Query.Email : String.Empty);

                    // Including data
                    query.Replace("{IncludeLocalFields}", Request.IncludeLocalFields ? "true" : "false");
                    query.Replace("{IncludeOwnNotes}", Request.IncludeOwnNotes ? "true" : "false");
                    query.Replace("{IncludeOtherNotes}", Request.IncludeOtherNotes ? "true" : "false");
                    query.Replace("{IncludeMeetings}", Request.IncludeMeetings ? "true" : "false");

                    // AVN 
                    string avnXml = String.Empty;
                    if (Request.AVN != null) {
                        avnXml = "<AVN>";
                        avnXml = avnXml + "<Include>" + Request.AVN.Include.ToString() + "</Include>";
                        if (Request.AVN.List != null && Request.AVN.List.Any()) {
                            avnXml = avnXml + "<List>";
                            foreach (var id in Request.AVN.List) {
                                avnXml = avnXml + "<Id>" + id.Id + "</Id>";
                            }
                            avnXml = avnXml + "</List>";
                        }
                        avnXml = avnXml + "</AVN>";
                    }
                    query.Replace("{AVN}", avnXml);

                    // all fields
                    var companyfields = context.APICompanySearch(user.OrganisationId, user.Id, query.ToString());
                    if (companyfields != null) {
                        if (companyfields.Rows.Count > 0) {

                            var dataClassfication = context.GetDataClassifications();

                            // Dynamic companies fields
                            var lFields = context.dynamicFields_getFieldsForOrganisationFromSQL(user.Organisation, string.Empty);
                            var companyDynamicField = lFields.Where(w => w.DatabaseTable.Contains("companies")
                                                                     && !DynamicFieldService.InvalidUIFieldType.Contains(w.FieldType)).ToList();

                            foreach (DataRow row in companyfields.Rows) {
                                var company = new SearchCompany();
                                var companyId = (row.Table.Columns.Contains("CompanyId") ? Convert.ToInt32(row["CompanyId"]) : 0);

                                // Basic information
                                company.IsPOT = (row.Table.Columns.Contains("CompanyType") ? Convert.ToBoolean(row["CompanyType"]) : false);
                                if (row.Table.Columns.Contains("CompanyAbandonedDate")) {
                                    object value = row["CompanyAbandonedDate"];
                                    if (value == DBNull.Value) {
                                        company.CompanyAbandonedDate = null;
                                    } else {
                                        company.CompanyAbandonedDate = Convert.ToDateTime(row["CompanyAbandonedDate"]);
                                    }
                                } else {
                                    company.CompanyAbandonedDate = null;
                                }
                                company.CompanyAbandonedBy = (row.Table.Columns.Contains("CompanyAbandonedBy") ? Convert.ToString(row["CompanyAbandonedBy"]) : null);
                                company.Organisation = new Models.Organisation() {
                                    Id = Convert.ToInt32(row["CompanyOrganisationId"]),
                                    Name = Convert.ToString(row["OrganisationName"])
                                };

                                // Remove duplicate companies
                                if (!response.Companies.Any(w => w.CompanyId == companyId)) {

                                    // Dynamic Fields
                                    company.CompanyId = companyId;
                                    foreach (var f in companyDynamicField) {
                                        string[] tmp = f.DatabaseTable.Split('_');
                                        int ownerId = TypeCast.ToInt(tmp[tmp.Length - 1]);
                                        var column = f.DatabaseTable + "_" + f.DatabaseColumn + "_" + ownerId;

                                        // Determine data classification
                                        var fieldValue = "";
                                        var dataclassificationid = f.DataClassificationId;
                                        var isDataClassified = dataClassfication.FirstOrDefault(w => w.Id == dataclassificationid);
                                        if (isDataClassified != null) {
                                            var maskFormat = isDataClassified.ExportMask;
                                            if (string.IsNullOrEmpty(maskFormat)) {
                                                fieldValue = (row.Table.Columns.Contains(column)
                                                                ? Convert.ToString(row[column])
                                                                : string.Empty);
                                            } else {
                                                fieldValue = (row.Table.Columns.Contains(column)
                                                                ? Obfuscate(Convert.ToString(row[column]), maskFormat)
                                                                : string.Empty);
                                            }
                                        } else {
                                            fieldValue = (row.Table.Columns.Contains(column) ? Convert.ToString(row[column]) : string.Empty);
                                        }
                                        var restrictedList = SysField.GetRestrictedList(context, user.OrganisationId, f);

                                        company.Fields.Add(new SysField() {
                                            Name = f.DatabaseColumn,
                                            DataType = f.FieldType,
                                            IsRequired = (f.RequiredState == 1 ? true : false),
                                            Value = fieldValue,
                                            FieldView = f.ViewState,
                                            ValueRestrictedList = restrictedList
                                        });
                                    }

                                    var name = company.Fields.FirstOrDefault(w => w.Name == "Firmanavn");
                                    if (name != null) { company.CompanyName = name.Value; }
                                    response.Companies.Add(company);
                                }
                            }

                            // Distinct Companies
                            var companyIdList = response.Companies.Select(s => s.CompanyId).ToArray();

                            // Meeting
                            if (Request.IncludeMeetings) {
                                if (companyIdList.Any()) {
                                    var companyIds = string.Join(",", companyIdList);
                                    var meetings = context.APIGetMeeting(user.OrganisationId, companyIds);

                                    foreach (DataRow row in meetings.Rows) {
                                        var meeting = new Meeting();
                                        meeting.Id = Convert.ToInt32(row["Id"]);
                                        meeting.UserId = Convert.ToString(row["UserId"]);
                                        meeting.MeetingUrl = Convert.ToString(row["MeetingUrl"]);
                                        meeting.Subject = Convert.ToString(row["Subject"]);
                                        meeting.Location = Convert.ToString(row["Location"]);
                                        meeting.DateStart = Convert.ToDateTime(row["DateStart"]);
                                        meeting.DateEnd = Convert.ToDateTime(row["DateEnd"]);
                                        meeting.Timespent = Convert.ToDouble(row["Timespent"]);
                                        meeting.PrimaryProjectTypeId = Convert.ToInt32(row["PrimaryProjectTypeId"]);
                                        meeting.PrimaryProjectTypeName = Convert.ToString(row["PrimaryProjectTypeName"]);
                                        meeting.SecondaryProjectTypeId = Convert.ToInt32(row["SecondaryProjectTypeId"]);
                                        meeting.SecondaryProjectTypeName = Convert.ToString(row["SecondaryProjectTypeName"]);
                                        meeting.SecondaryProjectTypeSerialNo = Convert.ToInt32(row["SecondaryProjectTypeSerialNo"]);
                                        meeting.CaseNumberId = Convert.ToInt32(row["CaseNumberId"]);
                                        meeting.CaseNumberRelationId = Convert.ToInt32(row["CaseNumberRelationId"]);
                                        meeting.CaseNumber = Convert.ToInt32(row["CaseNumber"]);
                                        meeting.CaseNumberName = Convert.ToString(row["CaseNumberName"]);
                                        meeting.DateStamp = Convert.ToString(row["DateStamp"]);
                                        meeting.CreatedBy = Convert.ToString(row["CreatedBy"]);
                                        meeting.CreatedByUsername = Convert.ToString(row["CreatedByUsername"]);

                                        // Adding meeting to it's Company object
                                        var companyId = Convert.ToInt32(row["CompanyId"]);
                                        var company = response.Companies.FirstOrDefault(w => w.CompanyId == companyId);
                                        company.Meetings.Add(meeting);
                                    }
                                }
                            }

                            // AVNs
                            if (companyIdList.Any()) {
                                foreach (var companyId in companyIdList) {
                                    var avnData = context.APIGetAVNs(true, user.Id, companyId, avnXml);

                                    foreach (var avn in avnData) {
                                        // Extracting column names from datatable
                                        var columnName = new List<String>();
                                        for (var c = 0; c < avn.Columns.Count; c++) {
                                            columnName.Add(avn.Columns[c].ColumnName);
                                        }

                                        foreach (DataRow row in avn.Rows) {
                                            var avnCompanyId = (row.Table.Columns.Contains("SMVCompanyId") ? Convert.ToString(row["SMVCompanyId"]) : Convert.ToString(companyId));

                                            if (!string.IsNullOrEmpty(avnCompanyId) && companyId == Convert.ToInt32(avnCompanyId)) {

                                                var avnId = Convert.ToInt32(row["EntityId"]);

                                                var companyAVN = new Models.AVN();
                                                companyAVN.AVNId = avnId;
                                                companyAVN.AVNName = Convert.ToString(row["Name"]);
                                                companyAVN.AVNTypeId = Convert.ToInt32(row["AVNTypeId"]);

                                                var avnFields = context.AVN_GetFields(companyAVN.AVNTypeId);
                                                foreach (var column in columnName) {

                                                    var avnfield = avnFields.FirstOrDefault(w => w.TableColumnName == column);

                                                    // Determine data classification
                                                    var fieldValue = "";
                                                    if (avnfield != null) {
                                                        var dataclassificationid = avnfield.DataClassificationId;
                                                        var isDataClassified = dataClassfication.FirstOrDefault(w => w.Id == dataclassificationid);
                                                        if (isDataClassified != null) {
                                                            var maskFormat = isDataClassified.ExportMask;
                                                            if (string.IsNullOrEmpty(maskFormat)) {
                                                                fieldValue = (row.Table.Columns.Contains(column)
                                                                                ? Convert.ToString(row[column])
                                                                                : string.Empty);
                                                            } else {
                                                                fieldValue = (row.Table.Columns.Contains(column)
                                                                                ? Obfuscate(Convert.ToString(row[column]), maskFormat)
                                                                                : string.Empty);
                                                            }
                                                        } else {
                                                            fieldValue = (row.Table.Columns.Contains(column) ? Convert.ToString(row[column]) : string.Empty);
                                                        }
                                                    } else {
                                                        fieldValue = Convert.ToString(row[column]);
                                                    }

                                                    companyAVN.Fields.Add(new SysField() {
                                                        Name = column,
                                                        Value = fieldValue
                                                    });
                                                }

                                                // Adding AVNs to it's Company object
                                                var company = response.Companies.FirstOrDefault(w => w.CompanyId == companyId);
                                                company.AVNs.Add(companyAVN);
                                            }
                                        }
                                    }
                                }
                            }

                            // Notes
                            if (companyIdList.Any()) {
                                foreach (var companyId in companyIdList) {
                                    var notes = context.APIGetNote(true, user.OrganisationId, companyId);
                                    foreach (DataRow row in notes.Rows) {
                                        var note = new Models.Note();
                                        note.NoteId = Convert.ToInt32(row["id"]);
                                        note.Title = Convert.ToString(row["Name"]);
                                        if (!DBNull.Value.Equals(row["NoteDate"])) {
                                            note.NoteDate = Convert.ToDateTime(row["NoteDate"]);
                                        }
                                        if (!DBNull.Value.Equals(row["NoteType"])) {
                                            note.Category = Convert.ToString(row["NoteType"]);
                                        }
                                        if (!DBNull.Value.Equals(row["NoteType2"])) {
                                            note.Category2 = Convert.ToString(row["NoteType2"]);
                                        }
                                        note.Sensitive = Convert.ToBoolean(row["Sensitive"]);
                                        note.Text = (note.Sensitive ? Obfuscate(Convert.ToString(row["Note"]), "********") : Convert.ToString(row["Note"]));
                                        note.UserId = Convert.ToString(row["userId"]);
                                        note.Organisation = new Models.Organisation() {
                                            Id = Convert.ToInt32(row["organisationId"]),
                                            Name = Convert.ToString(row["OrganisationName"])
                                        };

                                        // Adding note to it's Company object
                                        var company = response.Companies.FirstOrDefault(w => w.CompanyId == companyId);
                                        company.Notes.Add(note);
                                    }
                                }
                            }

                            // Contact
                            if (companyIdList.Any()) {
                                foreach (var companyId in companyIdList) {

                                    var contactDynamicField = lFields.Where(w => w.DatabaseTable.Contains("contacts")
                                                                             && !DynamicFieldService.InvalidUIFieldType.Contains(w.FieldType)).ToList();

                                    var company = response.Companies.FirstOrDefault(w => w.CompanyId == companyId);
                                    foreach (DataRow row in companyfields.Rows) {

                                        // allow only selected company's contact into company instance
                                        if ((row.Table.Columns.Contains("CompanyId") ? Convert.ToInt32(row["CompanyId"]) : 0) == companyId) {

                                            var contact = new Models.Contact();
                                            contact.ContactId = (row.Table.Columns.Contains("ContactId") ? Convert.ToInt32(row["ContactId"]) : 0);
                                            if (row.Table.Columns.Contains("ContactAbandonedDate")) {
                                                object value = row["ContactAbandonedDate"];
                                                if (value == DBNull.Value) {
                                                    contact.ContactAbandonedDate = null;
                                                } else {
                                                    contact.ContactAbandonedDate = Convert.ToDateTime(row["ContactAbandonedDate"]);
                                                }
                                            } else {
                                                contact.ContactAbandonedDate = null;
                                            }
                                            contact.ContactAbandonedBy = (row.Table.Columns.Contains("ContactAbandonedBy") ? Convert.ToString(row["ContactAbandonedBy"]) : null);

                                            // Dynamic Fields
                                            foreach (var f in contactDynamicField) {
                                                string[] tmp = f.DatabaseTable.Split('_');
                                                int ownerId = TypeCast.ToInt(tmp[tmp.Length - 1]);
                                                var column = f.DatabaseTable + "_" + f.DatabaseColumn + "_" + ownerId;
                                                var restrictedList = SysField.GetRestrictedList(context, user.OrganisationId, f);

                                                var contactfield = new SysField();
                                                contactfield.Name = f.DatabaseColumn;
                                                contactfield.DataType = f.FieldType;
                                                contactfield.IsRequired = (f.RequiredState == 1 ? true : false);

                                                // Determine data classification
                                                var fieldValue = "";
                                                var dataclassificationid = f.DataClassificationId;
                                                var isDataClassified = dataClassfication.FirstOrDefault(w => w.Id == dataclassificationid);
                                                if (isDataClassified != null) {
                                                    var maskFormat = isDataClassified.ExportMask;
                                                    if (string.IsNullOrEmpty(maskFormat)) {
                                                        fieldValue = (row.Table.Columns.Contains(column)
                                                                        ? Convert.ToString(row[column])
                                                                        : string.Empty);
                                                    } else {
                                                        fieldValue = (row.Table.Columns.Contains(column)
                                                                        ? Obfuscate(Convert.ToString(row[column]), maskFormat)
                                                                        : string.Empty);
                                                    }
                                                } else {
                                                    fieldValue = (row.Table.Columns.Contains(column) ? Convert.ToString(row[column]) : string.Empty);
                                                }

                                                contactfield.Value = fieldValue;
                                                contactfield.FieldView = f.ViewState;
                                                contactfield.ValueRestrictedList = restrictedList;

                                                contact.Fields.Add(contactfield);
                                            }

                                            var fname = contact.Fields.FirstOrDefault(w => w.Name == "Fornavn");
                                            var lname = contact.Fields.FirstOrDefault(w => w.Name == "Efternavn");
                                            var email = contact.Fields.FirstOrDefault(w => w.Name == "Email");

                                            if (fname != null) { contact.Name = fname.Value; }
                                            if (lname != null) {
                                                if (fname != null) { contact.Name = fname.Value + " " + lname.Value; } else { contact.Name = lname.Value; }
                                            }
                                            if (email != null) { contact.Email = email.Value; }

                                            // Notes
                                            var notes = context.APIGetNote(false, user.OrganisationId, contact.ContactId);
                                            foreach (DataRow noterow in notes.Rows) {
                                                var note = new Models.Note();
                                                note.NoteId = Convert.ToInt32(noterow["id"]);
                                                note.Title = Convert.ToString(noterow["Name"]);
                                                if (!DBNull.Value.Equals(noterow["NoteDate"])) {
                                                    note.NoteDate = Convert.ToDateTime(noterow["NoteDate"]);
                                                }
                                                if (!DBNull.Value.Equals(noterow["NoteType"])) {
                                                    note.Category = Convert.ToString(noterow["NoteType"]);
                                                }
                                                if (!DBNull.Value.Equals(noterow["NoteType2"])) {
                                                    note.Category2 = Convert.ToString(noterow["NoteType2"]);
                                                }
                                                note.Sensitive = Convert.ToBoolean(noterow["Sensitive"]);
                                                note.Text = (note.Sensitive ? Obfuscate(Convert.ToString(noterow["Note"]), "********") : Convert.ToString(noterow["Note"]));
                                                note.UserId = Convert.ToString(noterow["userId"]);
                                                note.Organisation = new Models.Organisation() {
                                                    Id = Convert.ToInt32(noterow["organisationId"]),
                                                    Name = Convert.ToString(noterow["OrganisationName"])
                                                };

                                                // Adding note to contact object
                                                contact.Notes.Add(note);
                                            }

                                            // Documents
                                            var documents = context.APIGetDocument(user.OrganisationId, contact.ContactId);
                                            foreach (DataRow docrow in documents.Rows) {
                                                var document = new Models.Document();
                                                document.Id = Convert.ToInt32(docrow["Id"]);
                                                document.Name = Convert.ToString(docrow["filename"]);
                                                document.Type = Convert.ToString(docrow["contenttype"]);
                                                document.UserId = Convert.ToString(docrow["userId"]);
                                                document.OrganisationId = Convert.ToString(docrow["organisationId"]);
                                                document.DateCreated = Convert.ToDateTime(docrow["dateCreated"]);
                                                document.Sensitive = Convert.ToBoolean(docrow["Sensitive"]);

                                                // Adding document to contact object
                                                contact.Documents.Add(document);
                                            }

                                            // AVN
                                            var contactAVNs = context.APIGetAVNs(false, user.Id, contact.ContactId, avnXml);
                                            foreach (var avn in contactAVNs) {
                                                // Extracting column names from datatable
                                                var columnName = new List<String>();
                                                for (var c = 0; c < avn.Columns.Count; c++) {
                                                    columnName.Add(avn.Columns[c].ColumnName);
                                                }

                                                foreach (DataRow avnRow in avn.Rows) {

                                                    var avnContactId = (avnRow.Table.Columns.Contains("SMVContactId") ? Convert.ToString(avnRow["SMVContactId"]) : Convert.ToString(contact.ContactId));
                                                    var contactAvnId = Convert.ToInt32(avnRow["EntityId"]);

                                                    if (!string.IsNullOrEmpty(avnContactId) && contact.ContactId == Convert.ToInt32(avnContactId)) {
                                                        var contactAVN = new Models.AVN();
                                                        contactAVN.AVNId = Convert.ToInt32(avnRow["EntityId"]);
                                                        contactAVN.AVNName = Convert.ToString(avnRow["Name"]);
                                                        contactAVN.AVNTypeId = Convert.ToInt32(avnRow["AVNTypeId"]);
                                                        var contactAvnFields = context.AVN_GetFields(contactAVN.AVNTypeId);

                                                        foreach (var column in columnName) {

                                                            var contactAvnfield = contactAvnFields.FirstOrDefault(w => w.TableColumnName == column);

                                                            // Determine data classification
                                                            var fieldValue = "";
                                                            if (contactAvnfield != null) {
                                                                var dataclassificationid = contactAvnfield.DataClassificationId;
                                                                var isDataClassified = dataClassfication.FirstOrDefault(w => w.Id == dataclassificationid);
                                                                if (isDataClassified != null) {
                                                                    var maskFormat = isDataClassified.ExportMask;
                                                                    if (string.IsNullOrEmpty(maskFormat)) {
                                                                        fieldValue = (avnRow.Table.Columns.Contains(column)
                                                                                        ? Convert.ToString(avnRow[column])
                                                                                        : string.Empty);
                                                                    } else {
                                                                        fieldValue = (avnRow.Table.Columns.Contains(column)
                                                                                        ? Obfuscate(Convert.ToString(avnRow[column]), maskFormat)
                                                                                        : string.Empty);
                                                                    }
                                                                } else {
                                                                    fieldValue = (avnRow.Table.Columns.Contains(column) ? Convert.ToString(avnRow[column]) : string.Empty);
                                                                }
                                                            } else {
                                                                fieldValue = Convert.ToString(avnRow[column]);
                                                            }

                                                            contactAVN.Fields.Add(new SysField() {
                                                                Name = column,
                                                                Value = fieldValue
                                                            });
                                                        }

                                                        // Adding AVNs to it's contact object
                                                        contact.AVNs.Add(contactAVN);
                                                    }
                                                }
                                            }

                                            // Adding contact to it's Company object
                                            company.Contacts.Add(contact);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    context.Dispose();
                    context = null;
                }
                response.Status = validation.Status;
                response.Message = validation.Message;
                return (response);
            } catch (Exception ex) {
                return (new SearchCompanyResponse() {
                    Status = false,
                    Message = new List<string>() {
                        ex.Message
                    }
                });
            }
        }

        [WebMethod]
        public Response GetContactSensitiveFieldData(Credential Credential, int ContactId, string Fieldname) {
            try {
                var user = new User();
                var response = new Response();
                var validation = CredentialService.IsValid(Credential, out user);
                if (validation.Status) {
                    var context = new SQLDB();

                    var lFields = context.dynamicFields_getFieldsForOrganisationFromSQL(user.Organisation, string.Empty);
                    var contactDynamicField = lFields.Where(w => (w.DatabaseTable).Contains("contacts")
                                && !DynamicFieldService.InvalidUIFieldType.Contains(w.FieldType)
                                && (DynamicFieldService.ValidUIFieldViewState.Contains(w.ViewState))).ToList();

                    var baseField = contactDynamicField.FirstOrDefault(w => w.DatabaseColumn == Fieldname);
                    if (baseField != null) {
                        string[] tmp = baseField.DatabaseTable.Split('_');
                        string fn = (baseField.DatabaseTable + "_" + baseField.DatabaseColumn + "_" + tmp[tmp.Length - 1]);
                        response.CustomDataField_1 = context.Contact_Get(user, ContactId, fn);
                    }
                    context.Dispose();
                    context = null;
                } else {
                    response.Status = validation.Status;
                    response.Message = validation.Message;
                }
                return (response);
            } catch (Exception ex) {
                return (new Response() {
                    Status = false,
                    Message = new List<string>() {
                        ex.Message
                    }
                });
            }
        }

        [WebMethod]
        public Response GetCompanySensitiveFieldData(Credential Credential, int CompanyId, string Fieldname) {
            try {
                var user = new User();
                var response = new Response();
                var validation = CredentialService.IsValid(Credential, out user);
                if (validation.Status) {
                    var context = new SQLDB();

                    var lFields = context.dynamicFields_getFieldsForOrganisationFromSQL(user.Organisation, string.Empty);
                    var companyDynamicField = lFields.Where(w => (w.DatabaseTable).Contains("companies")
                                && !DynamicFieldService.InvalidUIFieldType.Contains(w.FieldType)
                                && (DynamicFieldService.ValidUIFieldViewState.Contains(w.ViewState))).ToList();

                    var baseField = companyDynamicField.FirstOrDefault(w => w.DatabaseColumn == Fieldname);
                    if (baseField != null) {
                        string[] tmp = baseField.DatabaseTable.Split('_');
                        string fn = (baseField.DatabaseTable + "_" + baseField.DatabaseColumn + "_" + tmp[tmp.Length - 1]);
                        response.CustomDataField_1 = context.Company_Get(user, CompanyId, fn);
                    }

                    context.Dispose();
                    context = null;
                } else {
                    response.Status = validation.Status;
                    response.Message = validation.Message;
                }
                return (response);
            } catch (Exception ex) {
                return (new Response() {
                    Status = false,
                    Message = new List<string>() {
                        ex.Message
                    }
                });
            }
        }

        [WebMethod]
        public Response GetNoteSensitiveFieldData(Credential Credential, int Id) {
            try {
                var user = new User();
                var response = new Response();
                var validation = CredentialService.IsValid(Credential, out user);
                if (validation.Status) {
                    var context = new SQLDB();
                    var note = context.notes_getNote(user, Id);
                    if (note != null) {
                        response.CustomDataField_1 = note.note;
                    }
                    context.Dispose();
                    context = null;
                } else {
                    response.Status = validation.Status;
                    response.Message = validation.Message;
                }
                return (response);
            } catch (Exception ex) {
                return (new Response() {
                    Status = false,
                    Message = new List<string>() {
                        ex.Message
                    }
                });
            }
        }

        [WebMethod]
        public Response GetAVNSensitiveFieldData(Credential Credential, int AvnTypeId, int Id, string Fieldname) {
            try {
                var user = new User();
                var response = new Response();
                var validation = CredentialService.IsValid(Credential, out user);
                if (validation.Status) {
                    var context = new SQLDB();

                    var avn = context.AVN_GetAVN(user, AvnTypeId, Id, false);
                    if (avn != null) {
                        var avnField = avn.Fields.FirstOrDefault(w => w.TableColumnName == Fieldname);
                        if (avnField != null) {
                            response.CustomDataField_1 = TypeCast.ToString(avnField.Value);
                        } else {
                            response.CustomDataField_1 = string.Empty;
                        }
                    }
                    context.Dispose();
                    context = null;
                } else {
                    response.Status = validation.Status;
                    response.Message = validation.Message;
                }
                return (response);
            } catch (Exception ex) {
                return (new Response() {
                    Status = false,
                    Message = new List<string>() {
                        ex.Message
                    }
                });
            }
        }

        public string Obfuscate(string value, string format) {
            if (string.IsNullOrEmpty(value)) { return string.Empty; }

            var distinctCharacter = format.Distinct().Count();
            if (distinctCharacter == 1) {
                return format;
            } else {
                if (value.Length > 4) {
                    return value.Substring(0, value.Length - 4) + "****";
                } else {
                    if (!string.IsNullOrEmpty(value)) {
                        return "****";
                    }
                }
            }
            return format;
        }

        /// <summary>
        /// Den skal skrive til AgreementContact, hvor den i ContractTypeId skal indsætte parameter AgrementId og den skal i ContactId skal indsætte parameter ContactId, ESCRM-273/274
        /// </summary>
        /// <param name="Credential"></param>
        /// <param name="ContactId"></param>
        /// <param name="AgreementId"></param>
        /// <returns></returns>
        [WebMethod]
        public ContactAgreementResponse ContactAgreementAdd(Credential credential, int contactId, int agreementId)
        {
            try
            {
                bool success = true;

                var user = new User();
                var response = new ContactAgreementResponse();
                var validation = CredentialService.IsValid(credential, out user);
                List<IDS.EBSTCRM.Base.Classes.Agreement> agreements = new List<Base.Classes.Agreement>();

                if (validation.Status && user != null)
                    agreements = AgreementServiceForAPI.GetAgreementsByUser(user);

                if (agreements.Count(a => a.Id == agreementId) != 1)
                {
                    validation.Status = false;
                    validation.Message.Add("Agreement not found or not accessible.");
                    response.Status = false;
                    response.Message.Add("Agreement not found or not accessible.");
                }

                if (validation.Status)
                {
                    var context = new SQLDB();

                    var contactAgreement = new Base.Classes.AgreementContact();
                    var content = new Base.Classes.AgreementContent();
                    int fAggrementId = 0;

                    var contactAgreements = context.ContactAgreementExists(contactId, agreementId, user.OrganisationId, out fAggrementId);
                    if (contactAgreements)
                    {
                        success = false;
                        response.Status = false;
                        response.Message.Add("Kontakt aftale findes allerede.");
                    }
                    else
                    {
                        var agreement = context.GetAgreements(user).FirstOrDefault(w => w.Id == agreementId);
                        if (agreement != null)
                        {
                            content.Subject = AgreementService.ParseToken(agreement.TemplateSubject, contactId);
                            content.Body = AgreementService.ParseToken(agreement.TemplateText, contactId);

                            contactAgreement.Id = 0;
                            contactAgreement.AgreementTypeId = agreement.Id;
                            contactAgreement.ContactId = TypeCast.ToInt(contactId);
                            contactAgreement.ExpiryDate = TimeService.GetDateTime(DateTime.Now, TypeCast.ToInt(agreement.Expiry), agreement.ExpiryDatePart);
                            contactAgreement.FollowUp = TimeService.GetDateTime(DateTime.Now, TypeCast.ToInt(agreement.FollowUp), "day");
                            if (agreement.RequireDocumentation) { contactAgreement.Active = false; } else { contactAgreement.Active = true; }
                            contactAgreement.CreatedById = user.Id;
                        }
                        else
                        {
                            success = false;
                            response.Status = false;
                            response.Message.Add("Kontakt aftale findes ikke for organisation " + user.OrganisationId);
                        }
                    }

                    if (success)
                    {
                        contactAgreement.AgreementXml = contactAgreement.GetAgreementXml(content);
                        var result = context.AddEditContactAgreement(contactAgreement);

                        response.Status = true;
                        response.Message.Add("Kontakt aftale oprettet.");
                    }

                    context.Dispose();
                    context = null;
                }
                else
                {
                    response.Status = validation.Status;
                    response.Message = validation.Message;
                }
                return (response);
            }
            catch (Exception ex)
            {
                return (new ContactAgreementResponse()
                {
                    Status = false,
                    Message = new List<string>() {
                        ex.Message
                    }
                });
            }
        }

        /// <summary>
        /// Den skal slette AgreementContact, udfra AgrementId og ContactId, ESCRM-273/275
        /// </summary>
        /// <param name="Credential"></param>
        /// <param name="ContactId"></param>
        /// <param name="AgreementId"></param>
        /// <returns></returns>
        [WebMethod]
        public ContactAgreementResponse ContactAgreementRemove(Credential credential, int contactId, int agreementId)
        {
            try
            {
                bool success = true;

                var user = new User();
                var response = new ContactAgreementResponse();
                var validation = CredentialService.IsValid(credential, out user);

                List<IDS.EBSTCRM.Base.Classes.Agreement> agreements = new List<Base.Classes.Agreement>();

                if (validation.Status && user != null)
                    agreements = AgreementServiceForAPI.GetAgreementsByUser(user);

                if (agreements.Count(a => a.Id == agreementId) != 1)
                {
                    response.Status = false;
                    validation.Message.Add("Agreement not found or not accessible.");
                    validation.Status = false;
                    response.Message.Add("Agreement not found or not accessible.");
                }

                if (validation.Status)
                {
                    var context = new SQLDB();

                    var contactAgreement = new Base.Classes.AgreementContact();
                    var content = new Base.Classes.AgreementContent();
                    int fAgreementId = 0;


                    var contactAgreements = context.ContactAgreementExists(contactId, agreementId, user.OrganisationId, out fAgreementId);
                    if (!contactAgreements)
                    {
                        success = false;
                        response.Status = false;
                        response.Message.Add("Kontakt aftale findes ikke.");
                    }

                    if (success)
                    {
                        context.ContactAgreement_Delete(user, fAgreementId);

                        response.Status = true;
                        response.Message.Add("Kontakt aftale slettet.");
                    }
                    else
                    {
                        success = false;
                        response.Status = false;
                        response.Message.Add("Kontakt aftale kunne ikke slettes for organisation " + user.OrganisationId);
                    }

                    context.Dispose();
                    context = null;
                }
                else
                {
                    response.Status = validation.Status;
                    response.Message = validation.Message;
                }
                return (response);
            }
            catch (Exception ex)
            {
                return (new ContactAgreementResponse()
                {
                    Status = false,
                    Message = new List<string>() {
                        ex.Message
                    }
                });
            }
        }

        /// <summary>
        /// Den henter alle, udfra AgrementId, ESCRM-273/274
        /// </summary>
        /// <param name="Credential"></param>
        /// <param name="AgreementId"></param>
        /// <returns></returns>
        [WebMethod]
        public ContactAgreementResponse ContactAgreementContactsById(Credential credential, int agreementId)
        {
            try
            {
                bool success = true;

                var user = new User();
                var response = new ContactAgreementResponse();
                var validation = CredentialService.IsValid(credential, out user);
                List<IDS.EBSTCRM.Base.Classes.Agreement> agreements = new List<Base.Classes.Agreement>();

                if (validation.Status && user != null)
                    agreements = AgreementServiceForAPI.GetAgreementsByUser(user);

                if(agreements.Count(a=>a.Id==agreementId)!=1)
                {
                    validation.Status = false;
                    validation.Message.Add("Agreement not found or not accessible.");
                    response.Status = false;
                    response.Message.Add("Agreement not found or not accessible.");
                }

                if (validation.Status)
                {
                    var context = new SQLDB();

                    List<Tuple<string, string, string, string, string, string, string>> records = new List<Tuple<string, string, string, string, string, string, string>>();

                    records = context.ContactAgreementContactsById(agreementId);
                    if (records.Count == 0)
                    {
                        success = false;
                        response.Status = false;
                        response.Message.Add("Kunne ikke finde nogle kontakt aftaler.");
                        response.AgreementContactList = null;
                    }

                    if (success)
                    {
                        response.Status = true;
                        response.Message.Add("Kontakt aftaler fundet.");

                        int x = 0;
                        response.AgreementContactList = new string[records.Count];
                        //** Returner data
                        foreach (var item in records)
                        {
                            string line = item.Item1 + ";" + item.Item2 + ";" + item.Item3 + ";" + item.Item4 + ";" + item.Item5 + ";" + item.Item6 + ";" + item.Item7;
                            response.AgreementContactList[x] = line;
                            x = x + 1;
                        }
                    }
                    else
                    {
                        success = false;
                        response.Status = false;
                        response.Message.Add("Kunne ikke finde kontakt aftaler for Id: " + agreementId);
                        response.AgreementContactList = null;
                    }

                    context.Dispose();
                    context = null;
                }
                else
                {
                    response.Status = validation.Status;
                    response.Message = validation.Message;
                    response.AgreementContactList = null;
                }
                return (response);
            }
            catch (Exception ex)
            {
                return (new ContactAgreementResponse()
                {
                    Status = false,
                    Message = new List<string>() {
                        ex.Message
                    },
                    AgreementContactList = null
                }); ;
            }
        }

        /// <summary>
        /// Den sletter mailgroupcontact, udfra ContactId, ESCRM-273/275
        /// </summary>
        /// <param name="Credential"></param>
        /// <param name="ContacttId"></param>
        /// <returns></returns>
        [WebMethod]
        public MailgroupContactResponse MailgroupContactRemove(Credential credential, int mailgroupId, int contactId)
        {
            try
            {
                bool success = true;

                var user = new User();
                var response = new MailgroupContactResponse();
                var validation = CredentialService.IsValid(credential, out user);
                // Get accesslevel for usergroup
                var accessLevel = MailgroupService.UserAccessLevelForMailgroup(user, mailgroupId);
                if (validation.Status && accessLevel>1)
                {
                    var context = new SQLDB();
                    
                    //** Undersøg om den findes i forvejen
                    if(!context.MailgroupContactExists(mailgroupId, contactId, out int fmailgroupId))
                    {
                        success = false;
                        response.Status = false;
                        response.Message.Add("Intressegruppe Kontakt findes ikke.");
                    }

                    if(success)
                    {
                        //** Slet mailgroupcontact
                        context.Mailgroup_Contact_deleteAssociation(mailgroupId, contactId);

                        response.Status = true;
                        response.Message.Add("Interessegruppe kontakt slettet.");
                    }
                    else
                    {
                        success = false;
                        response.Status = false;
                        response.Message.Add("Interessegruppe kontakt " + mailgroupId + " aftale kunne ikke slettes for bruger " + user.Firstname + " " + user.Lastname);
                    }

                    context.Dispose();
                    context = null;
                }
                else
                {
                    response.Status = validation.Status;
                    response.Message = validation.Message;
                }
                return (response);
            }
            catch (Exception ex)
            {
                return (new MailgroupContactResponse()
                {
                    Status = false,
                    Message = new List<string>() {
                        ex.Message
                    }
                }); ;
            }
        }

        /// <summary>
        /// Den opretter mailgroupcontact, udfra mailgroupid og contactid, ESCRM-273/275
        /// </summary>
        /// <param name="Credential"></param>
        /// <param name="MailgroupId"></param>
        /// <param name="ContacttId"></param>
        /// <returns></returns>
        [WebMethod]
        public MailgroupContactResponse MailgroupContactAdd(Credential credential, int mailgroupContactId, int contactId)
        {
            try
            {
                bool success = true;

                var user = new User();
                var response = new MailgroupContactResponse();
                var validation = CredentialService.IsValid(credential, out user);

                // Get accesslevel for usergroup
                var accessLevel = MailgroupService.UserAccessLevelForMailgroup(user, mailgroupContactId);
                if (validation.Status && accessLevel>1)
                {
                    var context = new SQLDB();

                    //** Undersøg om den findes i forvejen
                    if (context.MailgroupContactExists(mailgroupContactId, contactId, out int fmailgroupContactId))
                    {
                        success = false;
                        response.Status = false;
                        response.Message.Add("Intressegruppe Kontakt findes i forvejen.");
                    }

                    if (success)
                    {
                        //** Slet mailgroupcontact
                        var result = context.Mailgroup_Contact_createAssociation_V2(mailgroupContactId, contactId, user.Id, false);

                        response.Status = true;
                        response.Message.Add("Interessegruppe kontakt oprettet med ContactId:" + contactId);
                    }
                    else
                    {
                        success = false;
                        response.Status = false;
                        response.Message.Add("Interessegruppe kontakt " + mailgroupContactId + "  kunne ikke oprettes for bruger " + user.Firstname + " " + user.Lastname);
                    }

                    context.Dispose();
                    context = null;
                }
                else
                {
                    response.Status = validation.Status;
                    response.Message = validation.Message;
                }
                return (response);
            }
            catch (Exception ex)
            {
                return (new MailgroupContactResponse()
                {
                    Status = false,
                    Message = new List<string>() {
                        ex.Message
                    }
                }); ;
            }
        }

        /// <summary>
        /// Den henter alle, udfra MailgroupId, ESCRM-273/275
        /// </summary>
        /// <param name="Credential"></param>
        /// <param name="MailgroupId"></param>
        /// <returns></returns>
        [WebMethod]
        public MailgroupContactResponse MailgroupContactsById(Credential credential, int mailgroupId)
        {
            try
            {
                bool success = true;

                var user = new User();
                var response = new MailgroupContactResponse();
                var validation = CredentialService.IsValid(credential, out user);
                int accessLevel=-1;

                // Get accesslevel for usergroup if successful login
                if (validation.Status)
                    accessLevel = MailgroupService.UserAccessLevelForMailgroup(user, mailgroupId);
                
                // If login successfull and accesslevel = 0
                if (accessLevel == 0 && validation.Status)
                    response.Message.Add("Mailgroup not found or not accessible with credentials.");

                if (validation.Status && accessLevel>0)
                {
                    var context = new SQLDB();

                    List<Tuple<string, string, string, string, string>> records = new List<Tuple<string, string, string, string, string>>();

                    records = context.MailgroupContactsById(mailgroupId);
                    if (records.Count == 0)
                    {
                        success = false;
                        response.Status = false;
                        response.Message.Add("Kunne ikke finde nogle interressegruppe kontakt.");
                        response.MailgroupContactList = null;
                    }

                    if (success)
                    {
                        response.Status = true;
                        response.Message.Add("Interressegruppe kontakt fundet.");

                        int x = 0;
                        response.MailgroupContactList = new string[records.Count];
                        //** Returner data
                        foreach (var item in records)
                        {
                            string line = item.Item1 + ";" + item.Item2 + ";" + item.Item3 + ";" + item.Item4 + ";" + item.Item5;
                            response.MailgroupContactList[x] = line;
                            x = x + 1;
                        }
                    }
                    else
                    {
                        success = false;
                        response.Status = false;
                        response.Message.Add("Kunne ikke finde interressegruppe kontakt for Id: " + mailgroupId);
                        response.MailgroupContactList = null;
                    }

                    context.Dispose();
                    context = null;
                }
                else
                {
                    response.Status = validation.Status;
                    response.Message = validation.Message;
                    response.MailgroupContactList = null;
                }
                return (response);
            }
            catch (Exception ex)
            {
                return (new MailgroupContactResponse()
                {
                    Status = false,
                    Message = new List<string>() {
                        ex.Message
                    },
                    MailgroupContactList = null
                }); ;
            }
        }

        /// <summary>
        /// Den henter mailgroupId, udfra MailgroupName, ESCRM-273/275
        /// </summary>
        /// <param name="Credential"></param>
        /// <param name="MailgroupName"></param>
        /// <returns></returns>
        [WebMethod]
        public MailgroupContactResponse MailgroupsGetByName(Credential credential, string mailgroupName)
        {
            try
            {
                bool success = true;

                var user = new User();
                var response = new MailgroupContactResponse();
                var validation = CredentialService.IsValid(credential, out user);
                if (validation.Status)
                {
                    var context = new SQLDB();

                    int MailgroupId = 0;
                    //MailgroupId = context.MailgroupsGetByName(mailgroupName);
                    List<Mailgroup> mgs = context.Mailgroups_getMailgroupsByOrgId(user.OrganisationId, 0, false);
                    MailgroupId = mgs.First(m => m.name.ToLower() == mailgroupName.ToLower()).id;
                    if (MailgroupId == 0)
                    {
                        success = false;
                        response.Status = false;
                        response.Message.Add("Kunne ikke finde nogle interressegrupper.");
                        response.MailgroupContactList = null;
                    }

                    if (success)
                    {
                        response.Status = true;
                        response.Message.Add("Interressegruppe navn fundet.");
                        response.MailgroupId = MailgroupId;
                    }
                    else
                    {
                        success = false;
                        response.Status = false;
                        response.Message.Add("Kunne ikke finde interressegruppe id for navn " + mailgroupName);
                        response.MailgroupId = 0;
                    }

                    context.Dispose();
                    context = null;
                }
                else
                {
                    response.Status = validation.Status;
                    response.Message = validation.Message;
                    response.MailgroupId = 0;
                }
                return (response);
            }
            catch (Exception ex)
            {
                return (new MailgroupContactResponse()
                {
                    Status = false,
                    Message = new List<string>() {
                        ex.Message
                    },
                    MailgroupId = 0
            }); ;
            }
        }

        /// <summary>
        /// Den henter alle kontaktid, email oog navn, udfra Email, ESCRM-273/276
        /// </summary>
        /// <param name="Credential"></param>
        /// <param name="liste med emails i list"></param>
        /// <param name="liste med emails i array"></param>
        /// <returns></returns>
        [WebMethod]
        public ContactsByEmailResponse ContactsByEmail(Credential credential, List<string> emailsList = null, string[] emailArray = null)
        {
            try
            {
                bool success = true;

                var user = new User();
                var response = new ContactsByEmailResponse();
                var validation = CredentialService.IsValid(credential, out user);
                if (validation.Status)
                {
                    var context = new SQLDB();

                    List<Tuple<int, string, string>> records = new List<Tuple<int, string, string>>();

                    //** Find ud af om det er en liste eller array
                    List<string> SearchList = new List<string>();

                    //** Vi har modtaget en list
                    if (emailsList != null && emailArray == null)
                    {
                        SearchList = emailsList;
                    }

                    //** Vi har modtaget en array
                    if(emailArray != null && emailsList == null)
                    {
                        foreach(var item in emailArray)
                        {
                            SearchList.Add(item);
                        }
                    }

                    records = context.ContactsByEmail(SearchList);
                    if (records.Count == 0)
                    {
                        success = false;
                        response.Status = false;
                        response.Message.Add("Kunne ikke finde nogle kontakter.");
                        response.ContactByEmailList = null;
                    }

                    if (success)
                    {
                        response.Status = true;
                        response.Message.Add("kontakter fundet.");

                        int x = 0;
                        response.ContactByEmailList = new string[records.Count];

                        //** Returner data
                        foreach (var item in records)
                        {
                            string line = item.Item1 + ";" + item.Item2 + ";" + item.Item3;
                            response.ContactByEmailList[x] = line;
                            x = x + 1;
                        }
                    }
                    else
                    {
                        success = false;
                        response.Status = false;
                        response.Message.Add("Kunne ikke finde kontakter for listen.");
                        response.ContactByEmailList = null;
                    }

                    context.Dispose();
                    context = null;
                }
                else
                {
                    response.Status = validation.Status;
                    response.Message = validation.Message;
                    response.ContactByEmailList = null;
                }
                return response;
            }
            catch (Exception ex)
            {
                return (new ContactsByEmailResponse()
                {
                    Status = false,
                    Message = new List<string>() {
                        ex.Message
                    },
                    ContactByEmailList = null
                }); ;
            }
        }



        /// <summary>
        /// Get all contacts matching email in array
        /// </summary>
        /// <param name="credential"></param>
        /// <param name="email"></param>
        /// <returns></returns>
        [WebMethod]
        public EmailContact[] ContactsBySingleEmail(Credential credential, string email)
        {
            try
            {               
                var user = new User();
                var validation = CredentialService.IsValid(credential, out user);
                EmailContact[] result = null;

                if (validation.Status)
                {
                    var context = new SQLDB();

                    List<string> SearchList = new List<string>();
                    //** Vi har modtaget et array
                    if (!string.IsNullOrEmpty(email))
                    {
                        SearchList.Add(email);
                    }

                    // Get Contacts from database
                    List<Tuple<int, string, string>> records = context.ContactsByEmail(SearchList);


                    if (records.Count == 0)
                    {
                        result = null;
                    }
                    else
                    {
                        int x = 0;
                        result = new EmailContact[records.Count];
                        foreach (var item in records)
                        {                   
                            // Loop through the data and create EmailContacts from data
                            EmailContact con = new EmailContact(item.Item1,item.Item3, item.Item2);
                            result[x] = con;
                            x = x+1;
                        }
                    }
                    context.Dispose();
                    context = null;
                    return result;
                }
                else
                {
                    // User not validated
                    result = null;
                }

                return null;
            }
            catch (Exception ex)
            {
                // Something wrong return null
                return null;
            }
        }


        /// <summary>
        /// Get all contacts matching email in array
        /// </summary>
        /// <param name="credential"></param>
        /// <param name="emails"></param>
        /// <returns></returns>
        [WebMethod]
        public EmailContact[] ContactsByEmails(Credential credential, string[] emails)
        {
            try
            {
                var user = new User();
                var validation = CredentialService.IsValid(credential, out user);
                EmailContact[] result = null;

                if (validation.Status)
                {
                    var context = new SQLDB();

                    List<string> SearchList = new List<string>();
                    //** Vi har modtaget et array
                    if (emails.Length>0)
                    {
                        foreach (string email in emails)
                        {
                            SearchList.Add(email);
                        }
                    }

                    // Get Contacts from database
                    List<Tuple<int, string, string>> records = context.ContactsByEmail(SearchList);


                    if (records.Count == 0)
                    {
                        result = null;
                    }
                    else
                    {
                        int x = 0;
                        result = new EmailContact[records.Count];
                        foreach (var item in records)
                        {
                            // Loop through the data and create EmailContacts from data
                            EmailContact con = new EmailContact(item.Item1, item.Item3, item.Item2);
                            result[x] = con;
                            x = x + 1;
                        }
                    }
                    context.Dispose();
                    context = null;
                    return result;
                }
                else
                {
                    // User not validated
                    result = null;
                }

                return null;
            }
            catch (Exception ex)
            {
                // Something wrong return null
                return null;
            }
        }

    }
}
