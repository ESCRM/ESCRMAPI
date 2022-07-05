using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Services;

namespace IDS.EBSTCRM.WindowManager.Integration
{
    /// <summary>
    /// Summary description for AVN
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class AVN : System.Web.Services.WebService {

        #region Declarations

        string[] NonSavableFields = new string[] { "vr", "label", "sqllabel", "map", "title", "hr", "button" };

        #endregion

        #region Authorization

        [WebMethod]
        public bool Authorize(string Username, string Password) {
            IDS.EBSTCRM.Base.SQLDB sql = new IDS.EBSTCRM.Base.SQLDB();
            IDS.EBSTCRM.Base.User u = sql.Users_Login(Username, Password);
            sql.Dispose();
            sql = null;
            return u != null;
        }

        #endregion

        #region Search Company and Contacts

        [WebMethod]
        public Company[] CompanySearch(string Username, string Password, CompanyQuery Query) {
            IDS.EBSTCRM.Base.SQLDB sql = new IDS.EBSTCRM.Base.SQLDB();
            IDS.EBSTCRM.Base.User u = sql.Users_Login(Username, Password);

            if (u == null) {
                sql.Dispose();
                sql = null;
                throw new Exception("Forkert brugernavn eller password.");
            }

            if (Query == null) {
                sql.Dispose();
                sql = null;
                throw new Exception("Forespørgslen skal sendes med");
            }

            sql.Base.commandText = "Integration_AVN_SearchCompany";
            if (Query.Id != null && Query.Id > 0) sql.Base.parameters.AddWithValue("@Id", Query.Id);
            if (Query.Name != null && Query.Name != "") sql.Base.parameters.AddWithValue("@Name", Query.Name);
            if (Query.CVR != null && Query.CVR != "") sql.Base.parameters.AddWithValue("@CVR", Query.CVR);
            if (Query.PNR != null && Query.PNR != "") sql.Base.parameters.AddWithValue("@PNR", Query.PNR);
            if (Query.Address != null && Query.Address != "") sql.Base.parameters.AddWithValue("@Address", Query.Address);
            if (Query.Zipcode != null && Query.Zipcode != "") sql.Base.parameters.AddWithValue("@Zipcode", Query.Zipcode);
            if (Query.City != null && Query.City != "") sql.Base.parameters.AddWithValue("@City", Query.City);
            if (Query.Phone != null && Query.Phone != "") sql.Base.parameters.AddWithValue("@Phone", Query.Phone);
            if (Query.Email != null && Query.Email != "") sql.Base.parameters.AddWithValue("@Email", Query.Email);

            List<Company> retval = new System.Collections.Generic.List<Company>();

            System.Data.SqlClient.SqlDataReader dr = sql.Base.executeReader;
            while (dr.Read()) {
                retval.Add(new Company(ref dr));
            }

            dr.Close();
            sql.Base.reset();

            sql.Dispose();
            sql = null;

            return retval.ToArray();
        }

        [WebMethod]
        public Contact[] ContactSearch(string Username, string Password, ContactQuery Query) {
            IDS.EBSTCRM.Base.SQLDB sql = new IDS.EBSTCRM.Base.SQLDB();
            IDS.EBSTCRM.Base.User u = sql.Users_Login(Username, Password);

            if (u == null) {
                sql.Dispose();
                sql = null;
                throw new Exception("Forkert brugernavn eller password.");
            }

            if (Query == null) {
                sql.Dispose();
                sql = null;
                throw new Exception("Forespørgslen skal sendes med");
            }

            sql.Base.commandText = "Integration_AVN_SearchContact";
            if (Query.Id != null && Query.Id > 0) sql.Base.parameters.AddWithValue("@Id", Query.Id);
            if (Query.CompanyName != null && Query.CompanyName != "") sql.Base.parameters.AddWithValue("@CompanyName", Query.CompanyName);
            if (Query.Firstname != null && Query.Firstname != "") sql.Base.parameters.AddWithValue("@Firstname", Query.Firstname);
            if (Query.Lastname != null && Query.Lastname != "") sql.Base.parameters.AddWithValue("@Lastname", Query.Lastname);
            if (Query.CVR != null && Query.CVR != "") sql.Base.parameters.AddWithValue("@CVR", Query.CVR);
            if (Query.PNR != null && Query.PNR != "") sql.Base.parameters.AddWithValue("@PNR", Query.PNR);
            if (Query.Address != null && Query.Address != "") sql.Base.parameters.AddWithValue("@Address", Query.Address);
            if (Query.Zipcode != null && Query.Zipcode != "") sql.Base.parameters.AddWithValue("@Zipcode", Query.Zipcode);
            if (Query.City != null && Query.City != "") sql.Base.parameters.AddWithValue("@City", Query.City);
            if (Query.Phone != null && Query.Phone != "") sql.Base.parameters.AddWithValue("@Phone", Query.Phone);
            if (Query.Email != null && Query.Email != "") sql.Base.parameters.AddWithValue("@Email", Query.Email);

            List<Contact> retval = new System.Collections.Generic.List<Contact>();

            System.Data.SqlClient.SqlDataReader dr = sql.Base.executeReader;
            while (dr.Read()) {
                retval.Add(new Contact(ref dr));
            }

            dr.Close();
            sql.Base.reset();

            sql.Dispose();
            sql = null;

            return retval.ToArray();
        }

        #endregion

        #region AVN Validation

        private bool canUserAccessAVN(IDS.EBSTCRM.Base.User u, int avnTypeId) {
            bool result = false;
            IDS.EBSTCRM.Base.SQLDB sql = new IDS.EBSTCRM.Base.SQLDB();
            foreach (IDS.EBSTCRM.Base.AdminAVN avn in sql.AVN_GetCreateAbleAVNs(u)) {
                if (avn.Id == avnTypeId) {
                    result = true;
                    break;
                }
            }
            sql.Dispose();
            sql = null;

            return result;

        }

        #endregion

        #region AVN Update and Create

        #region Business case (udvikling fyn)

        [WebMethod]
        public Businesscase Businesscase_Get(string Username, string Password, int Id) {
            IDS.EBSTCRM.Base.SQLDB sql = new IDS.EBSTCRM.Base.SQLDB();
            IDS.EBSTCRM.Base.User u = sql.Users_Login(Username, Password);

            if (u == null) {
                sql.Dispose();
                sql = null;
                throw new Exception("Forkert brugernavn eller password.");
            }

            if (!canUserAccessAVN(u, 214)) {
                sql.Dispose();
                sql = null;
                throw new Exception("Brugeren har ikke rettigheder til at anvende denne projektnote.");
            }

            sql.Base.commandText = "z_avn_GetAVN_214";
            sql.Base.parameters.AddWithValue("@Id", Id);

            Businesscase retval = null;

            System.Data.SqlClient.SqlDataReader dr = sql.Base.executeReader;
            if (dr.Read()) {
                retval = new Businesscase(ref dr, sql.Base.ConnectionString.ToUpper().Contains("SANDKASSE"));
            }

            dr.Close();
            sql.Base.reset();

            sql.Dispose();
            sql = null;

            return retval;
        }

        [WebMethod]
        public int? Businesscase_Update(string Username, string Password, Businesscase Case) {
            IDS.EBSTCRM.Base.SQLDB sql = new IDS.EBSTCRM.Base.SQLDB();
            IDS.EBSTCRM.Base.User u = sql.Users_Login(Username, Password);

            if (u == null) {
                sql.Dispose();
                sql = null;
                throw new Exception("Forkert brugernavn eller password.");
            }

            if (!canUserAccessAVN(u, 214)) {
                sql.Dispose();
                sql = null;
                throw new Exception("Brugeren har ikke rettigheder til at anvende denne projektnote.");
            }

            if (Case.Id == null || Businesscase_Get(Username, Password, Case.Id.Value) == null) {
                sql.Dispose();
                sql = null;
                throw new Exception("Id'et til projektnoten er kunne ikke findes, og der kan derfor ikke opdateres.");
            }

            if ((Case.SMVContactId == null || Case.SMVContactId <= 0) &&
                (Case.SMVCompanyId == null || Case.SMVCompanyId <= 0)) {
                sql.Dispose();
                sql = null;
                throw new Exception("Der skal angives enten et virksomheds- eller kontaktpersonsid, hvor projektnoten skal gemmes.");
            }

            int? retval = _Businesscase_Update(ref u, ref sql, Case);

            sql.Dispose();
            sql = null;

            return retval;
        }

        [WebMethod]
        public int? Businesscase_Create(string Username, string Password, Businesscase Case) {
            IDS.EBSTCRM.Base.SQLDB sql = new IDS.EBSTCRM.Base.SQLDB();
            IDS.EBSTCRM.Base.User u = sql.Users_Login(Username, Password);

            if (u == null) {
                sql.Dispose();
                sql = null;
                throw new Exception("Forkert brugernavn eller password.");
            }

            if (!canUserAccessAVN(u, 214)) {
                sql.Dispose();
                sql = null;
                throw new Exception("Brugeren har ikke rettigheder til at anvende denne projektnote.");
            }

            if (Case.Id != null) {
                sql.Dispose();
                sql = null;
                throw new Exception("Der må ikke angives et Id til projektnoten under oprettelse.");
            }

            if ((Case.SMVContactId == null || Case.SMVContactId <= 0) &&
                (Case.SMVCompanyId == null || Case.SMVCompanyId <= 0)) {
                sql.Dispose();
                sql = null;
                throw new Exception("Der skal angives enten et virksomheds- eller kontaktpersonsid, hvor projektnoten skal gemmes.");
            }

            int? retval = _Businesscase_Update(ref u, ref sql, Case);

            sql.Dispose();
            sql = null;

            return retval;
        }

        private int? _Businesscase_Update(ref IDS.EBSTCRM.Base.User u, ref IDS.EBSTCRM.Base.SQLDB sql, Businesscase Case) {
            sql.Base.commandText = "z_avn_UpdateAVN_214";
            if (Case.Id != null && Case.Id > 0) sql.Base.parameters.AddWithValue("@Id", Case.Id);
            sql.Base.parameters.AddWithValue("@OrganisationId", u.OrganisationId);
            sql.Base.parameters.AddWithValue("@Name", Case.Name);
            sql.Base.parameters.AddWithValue("@UserId", u.Id);
            if (Case.SMVCompanyId != null && Case.SMVCompanyId > 0) sql.Base.parameters.AddWithValue("@SMVCompanyId", Case.SMVCompanyId);
            if (Case.SMVContactId != null && Case.SMVContactId > 0) sql.Base.parameters.AddWithValue("@SMVContactId", Case.SMVContactId);
            sql.Base.parameters.AddWithValue("@SharedWith", Case.SharedWith);

            sql.Base.parameters.AddWithValue("@12060_SharePointUrl", Case.SharePointUrl);
            sql.Base.parameters.AddWithValue("@12059_Pipeline", Case.Pipeline);
            sql.Base.parameters.AddWithValue("@12061_Navn", Case.Navn);
            sql.Base.parameters.AddWithValue("@12063_Kort_beskrivelse", Case.Kort_beskrivelse);
            sql.Base.parameters.AddWithValue("@12064_Deltagende_virksomheder", Case.Deltagende_virksomheder);
            sql.Base.parameters.AddWithValue("@12066_Ansvarlig", Case.Ansvarlig);
            sql.Base.parameters.AddWithValue("@12067_Startdato", Case.Startdato);
            sql.Base.parameters.AddWithValue("@12068_Slutdato", Case.Slutdato);
            sql.Base.parameters.AddWithValue("@12069_Opfølgningsdato", Case.Opfølgningsdato);

            if (sql.Base.ConnectionString.ToUpper().Contains("SANDKASSE"))
                sql.Base.parameters.AddWithValue("@17667_Subbrand", Case.Subbrands);
            else
                sql.Base.parameters.AddWithValue("@18965_Subbrand", Case.Subbrands);

            int? retval = null;

            System.Data.SqlClient.SqlDataReader dr = sql.Base.executeReader;
            if (dr.Read()) {
                retval = IDS.EBSTCRM.Base.TypeCast.ToInt(dr["ID"]);
            }

            dr.Close();
            sql.Base.reset();

            return retval;
        }

        #endregion

        #endregion

        #region Generic AVN Requst, Create and Update

        #region Requests

        #region Get Createable AVNs from User

        [WebMethod]
        public List<AVNType> GetCreateableAVNs(string Username, string Password) {
            IDS.EBSTCRM.Base.SQLDB sql = new IDS.EBSTCRM.Base.SQLDB();
            IDS.EBSTCRM.Base.User u = sql.Users_Login(Username, Password);
            List<AVNType> result = new System.Collections.Generic.List<AVNType>();

            #region Security check

            if (u == null) {
                sql.Dispose();
                sql = null;
                throw new Exception("Forkert brugernavn eller password.");
            }

            #endregion

            #region Get Data

            foreach (IDS.EBSTCRM.Base.AdminAVN a in sql.AVN_GetCreateAbleAVNs(u)) {
                result.Add(new AVNType(sql.AdminAVN_GetAVN(u, a.Id)));
            }

            #endregion

            return result;
        }

        #endregion

        #region Get Fields from AVN

        [WebMethod]
        public List<AVNField> GetFieldsFromAVN(string Username, string Password, int AvnId) {
            IDS.EBSTCRM.Base.SQLDB sql = new IDS.EBSTCRM.Base.SQLDB();
            IDS.EBSTCRM.Base.User u = sql.Users_Login(Username, Password);
            List<AVNField> result = new System.Collections.Generic.List<AVNField>();

            #region Security check

            if (u == null) {
                sql.Dispose();
                sql = null;
                throw new Exception("Forkert brugernavn eller password.");
            }

            if (!canUserAccessAVN(u, AvnId)) {
                sql.Dispose();
                sql = null;
                throw new Exception("Brugeren har ikke rettigheder til at anvende denne projektnote.");
            }

            #endregion

            #region Get Data

            foreach (IDS.EBSTCRM.Base.AVNField f in sql.AVN_GetFields(AvnId)) {
                //Only add database savable fields to result
                if (!NonSavableFields.Contains(f.FieldType)) {
                    AVNField ff = new AVNField(f);
                    if (ff.DataType == "dropdown" || ff.DataType == "listview") {
                        ff.ValueRestrictedList = sql.AVNFields_getCustomValues(f.Id).ToArray();
                    }
                    result.Add(ff);
                }
            }

            #endregion

            return result;
        }

        #endregion

        #region Get Organisations for sharing

        [WebMethod]
        public List<Sharing.Organisation> GetOrganisations(string Username, string Password) {
            return getOrganisations(Username, Password, null);
        }

        [WebMethod]
        public List<Sharing.Organisation> GetChildOrganisations(string Username, string Password, Sharing.Organisation ParentOrganisation) {
            return getOrganisations(Username, Password, ParentOrganisation.Id);
        }

        private List<Sharing.Organisation> getOrganisations(string Username, string Password, int? ParentOrganisationId) {
            List<Sharing.Organisation> retval = new List<Sharing.Organisation>();
            IDS.EBSTCRM.Base.SQLDB sql = new Base.SQLDB();
            IDS.EBSTCRM.Base.User u = sql.Users_Login(Username, Password);

            if (u == null) {
                sql.Dispose();
                sql = null;
                throw new Exception("Forkert brugernavn eller password.");
            }

            foreach (IDS.EBSTCRM.Base.Organisation org in sql.Organisations_getOrganisations(ParentOrganisationId)) {
                retval.Add(new Sharing.Organisation(org));
            }

            sql.Dispose();
            sql = null;

            return retval;
        }

        #endregion

        #region Get available usergroups for sharing

        [WebMethod]
        public List<Sharing.Usergroup> GetUsergroups(string Username, string Password) {
            List<Sharing.Usergroup> retval = new System.Collections.Generic.List<Sharing.Usergroup>();
            IDS.EBSTCRM.Base.SQLDB sql = new Base.SQLDB();
            IDS.EBSTCRM.Base.User u = sql.Users_Login(Username, Password);

            if (u == null) {
                sql.Dispose();
                sql = null;
                throw new Exception("Forkert brugernavn eller password.");
            }

            foreach (IDS.EBSTCRM.Base.Usergroup ug in sql.UserGroups_GetGroups(u)) {
                retval.Add(new Sharing.Usergroup(ug));
            }

            return retval;
        }

        #endregion

        #endregion

        #region Delete

        [WebMethod]
        public void DeleteAVN(string Username, string Password, AVNEntity AVN) {
            DeleteAVNFromId(Username, Password, AVN.AVNTypeId, AVN.EntityId.Value);
        }

        [WebMethod]
        public void DeleteAVNFromId(string Username, string Password, int AVNTypeId, int EntityId) {
            IDS.EBSTCRM.Base.SQLDB sql = new IDS.EBSTCRM.Base.SQLDB();
            IDS.EBSTCRM.Base.User u = sql.Users_Login(Username, Password);

            #region Security check

            if (u == null) {
                sql.Dispose();
                sql = null;
                throw new Exception("Forkert brugernavn eller password.");
            }

            if (sql.AdminAVN_GetAVN(u, AVNTypeId) == null) {
                sql.Dispose();
                sql = null;
                throw new Exception("Egenskaben \"AVNEntity.AVNTypeId=" + AVNTypeId + "\" findes ikke i CRM Systemet. Angiv et gyldigt AVNTypeId.");
            }

            if (!canUserAccessAVN(u, AVNTypeId)) {
                sql.Dispose();
                sql = null;
                throw new Exception("Brugeren har ikke rettigheder til at slette denne projektnote.");
            }

            var entity = sql.AVN_GetAVN(u, AVNTypeId, EntityId);
            if (entity == null) {
                sql.Dispose();
                sql = null;
                throw new Exception("projektnoten med \"AVNEntity.AVNTypeId=" + AVNTypeId + "\" og EntityId=" + EntityId + " findes ikke.");
            } else if (entity.CreatedBy != u.Id) {
                sql.Dispose();
                sql = null;
                throw new Exception("Brugeren har ikke rettigheder til at slette denne projektnote.");
            }

            #endregion

            sql.AVN_DeleteAVNEntity(u, AVNTypeId, EntityId);

            sql.Dispose();
            sql = null;
        }

        #endregion

        #region Update / Create

        [WebMethod]
        public void UpdateAVN(string Username, string Password, AVNEntity AVN) {
            CreateUpdateAVN(Username, Password, AVN, false);
        }

        [WebMethod]
        public int CreateAVN(string Username, string Password, AVNEntity AVN) {
            //** Løb felter igenem for påkrævede og afvis hvis de ikke er udfyldte.
            return CreateUpdateAVN(Username, Password, AVN, true);
        }

        private int CreateUpdateAVN(string Username, string Password, AVNEntity AVN, bool Create) {
            int retval = 0;
            IDS.EBSTCRM.Base.SQLDB sql = new IDS.EBSTCRM.Base.SQLDB();
            IDS.EBSTCRM.Base.User u = sql.Users_Login(Username, Password);
            List<AVNField> result = new System.Collections.Generic.List<AVNField>();

            #region Security check

            if (u == null) {
                sql.Dispose();
                sql = null;
                throw new Exception("Forkert brugernavn eller password.");
            }

            if (AVN == null) {
                sql.Dispose();
                sql = null;
                throw new Exception("Der skal angives en AVNEntity.");
            }

            if (sql.AdminAVN_GetAVN(u, AVN.AVNTypeId) == null) {
                sql.Dispose();
                sql = null;
                throw new Exception("Egenskaben \"AVNEntity.AVNTypeId=" + AVN.AVNTypeId + "\" findes ikke i CRM Systemet. Angiv et gyldigt AVNTypeId.");
            }

            if (!canUserAccessAVN(u, AVN.AVNTypeId)) {
                sql.Dispose();
                sql = null;
                throw new Exception("Brugeren har ikke rettigheder til at anvende denne projektnote.");
            }

            #endregion

            #region Generic Save Validation

            if (Create && AVN.EntityId != null) {
                sql.Dispose();
                sql = null;
                throw new Exception("Egenskaben \"AVNEntity.EntityId\" skal ikke udfyldes, når der skal oprettes en ny AVN.");
            }
            if (!Create) {
                if (sql.AVN_GetAVN(u, AVN.AVNTypeId, IDS.EBSTCRM.Base.TypeCast.ToInt(AVN.EntityId)) == null) {
                    sql.Dispose();
                    sql = null;
                    throw new Exception("Egenskaben \"AVNEntity.EntityId\" skal udfyldes med en gyldig nøgle for at opdatere en eksisiterende AVN.");
                }
            }

            if (AVN.Name == null || AVN.Name == "") {
                sql.Dispose();
                sql = null;
                throw new Exception("Egenskaben \"AVNEntity.Name\" skal udfyldes.");
            }

            if (AVN.ParentId == null) {
                sql.Dispose();
                sql = null;
                throw new Exception("Egenskaben \"AVNEntity.ParentId\" skal udfyldes.");
            }

            if (AVN.ParentType == AVNType.parentType.Company && sql.Company_Get(u.OrganisationId, AVN.ParentId != null ? AVN.ParentId.Value : 0, u) == null) {
                sql.Dispose();
                sql = null;
                throw new Exception("Virksomheden med nøglen \"" + AVN.ParentId.Value + "\" findes ikke i CRM systemet.");
            }

            if (AVN.ParentType == AVNType.parentType.Contact && sql.Contact_Get(u.OrganisationId, AVN.ParentId != null ? AVN.ParentId.Value : 0, u) == null) {
                sql.Dispose();
                sql = null;
                throw new Exception("Kontaktpersonen med nøglen \"" + AVN.ParentId.Value + "\" findes ikke i CRM systemet.");
            }

            #endregion

            #region Match Entity Fields

            List<AVNField> fields = GetFieldsFromAVN(Username, Password, AVN.AVNTypeId);

            //** Har fjernet check på antal felter, ESCRM-21/35
            //if (fields.Count != AVN.Fields.Length) {
            //    sql.Dispose();
            //    sql = null;
            //    throw new Exception("Antallet af felter til opdatering af AVN matcher ikke specifikationen.\r\n\r\nDer er indsendt " + AVN.Fields.Length + " felter, hvor der blev forventet " + fields.Count + " felter.");
            //}

            foreach (AVNField f in AVN.Fields) {
                //** Check if f is null. Hvis ja så fortsæt, ESCRM-21/35
                if (f == null)
                    continue;

                //Get matching field
                AVNField[] comparer = fields.Where(fsrc => fsrc.Name == f.Name && fsrc.DataType == f.DataType).Take(1).ToArray();
                if (comparer.Length == 1) {
                    AVNField fc = comparer[0];

                    //Check for required state
                    if (f.Value == "" && fc.IsRequired) {
                        sql.Dispose();
                        sql = null;
                        throw new Exception("Feltet \"" + f.Name + "\" er påkrævet og skal udfyldes!");
                    } else {
                        //Match content with data
                        if (!IsFieldContentValid(f, AVN, fc.DataType)) {
                            sql.Dispose();
                            sql = null;
                            throw new Exception("Feltet \"" + f.Name + "\"'s indhold er ikke formatteret korrekt. Der blev forventet data af typen: \"" + fc.DataType + "\", hvor værdien \"" + f.Value + "\" ikke passer." +
                            (f.DataType == "dropdown" || f.DataType == "listview" ? "\r\n\r\nTilladte værdier:\r\n\r\n" + StringArrayToCommaSepList(f.ValueRestrictedList) : ""));
                        }
                    }

                    f.Id = fc.Id;
                } else {
                    sql.Dispose();
                    sql = null;
                    throw new Exception("Feltet \"" + f.Name + "\" matcher ikke definitionen for feltet!");
                }
            }

            #endregion

            #region Save AVN to DB

            sql.Base.commandText = "z_avn_UpdateAVN_" + AVN.AVNTypeId;
            if (AVN.EntityId != null && AVN.EntityId > 0) sql.Base.parameters.AddWithValue("@id", AVN.EntityId);
            sql.Base.parameters.AddWithValue("@OrganisationId", u.OrganisationId);
            sql.Base.parameters.AddWithValue("@UserId", u.Id);
            sql.Base.parameters.AddWithValue("@Name", AVN.Name);
            if (AVN.ParentType == AVNType.parentType.Contact) sql.Base.parameters.AddWithValue("@SMVContactId", AVN.ParentId);
            if (AVN.ParentType == AVNType.parentType.Company) sql.Base.parameters.AddWithValue("@SMVCompanyId", AVN.ParentId);

            int shareState = 0;
            if (AVN.SharedWithOrganisations != null)
                shareState += AVN.SharedWithOrganisations.Length;
            if (AVN.SharedWithOrganisations != null)
                shareState += AVN.SharedWithOrganisations.Length;

            sql.Base.parameters.AddWithValue("@SharedWith", shareState > 0 ? 1 : 0);

            foreach (AVNField f in AVN.Fields) {
                //** Check if f is null. Hvis ja så fortsæt, ESCRM-21/35
                if (f == null)
                    continue;

                sql.Base.parameters.AddWithValue("@" + f.Id + "_" + f.Name.Replace(" ", "_").Replace("[", "_").Replace("]", "_").Replace("(", "_").Replace("-", "_").Replace(")", "_").Replace(".", "_").Replace(":", "_").Replace("/", "_").Replace("\\", "_").Replace(",", "_"), GetFieldContent(f, AVN));
            }

            System.Data.SqlClient.SqlDataReader dr = sql.Base.executeReader;
            if (dr.Read()) {
                retval = IDS.EBSTCRM.Base.TypeCast.ToInt(dr["id"]);
            }
            dr.Close();
            sql.Base.reset();

            #endregion

            #region Set AVN sharing properties

            if (retval > 0) {
                if (AVN.SharedWithOrganisations != null) {
                    foreach (Sharing.Organisation so in AVN.SharedWithOrganisations) {
                        sql.AVNEntity_UpdateSharedWith(AVN.AVNTypeId, retval, so.Id, 0, so.SharingState == Sharing.sharing.None ? 0 : so.SharingState == Sharing.sharing.ReadWrite ? 2 : 1);
                    }
                }
                if (AVN.SharedWithUsergroups != null) {
                    foreach (Sharing.Usergroup ug in AVN.SharedWithUsergroups) {
                        sql.AVNEntity_UpdateSharedWith(AVN.AVNTypeId, retval, 0, ug.Id, ug.SharingState == Sharing.sharing.None ? 0 : ug.SharingState == Sharing.sharing.ReadWrite ? 2 : 1);
                    }
                }
            }

            #endregion

            #region Set AVN Reminders

            if (retval > 0 && AVN.Reminders != null) {
                foreach (DateTime dt in AVN.Reminders) {
                    sql.AVNEntity_UpdateReminder(AVN.AVNTypeId, retval, dt, u.Id);
                }
            }

            #endregion

            #region Clean up

            sql.Dispose();
            sql = null;

            #endregion

            return retval;
        }

        private object GetFieldContent(AVNField field, AVNEntity AVN) {
            switch (field.DataType) {
                case "emailaddress":
                case "memo":
                case "dropdown":
                case "textbox":
                case "listview":
                    return field.Value;

                case "datetime":
                    switch (AVN.DateFormat) {
                        case AVNEntity.dateFormat.DDMMYYYY:
                            return ParseDateTime(field.Value, "dd-MM-yyyy");

                        case AVNEntity.dateFormat.MMDDYYYY:
                            return ParseDateTime(field.Value, "MM-dd-yyyy");

                        case AVNEntity.dateFormat.YYYYMMDD:
                            return ParseDateTime(field.Value, "yyyy-MM-dd");
                    }
                    return false;

                case "checkbox":
                    string[] booleanTypeNames = new string[] { "1", "ja", "true", "yes", "-1" };
                    return booleanTypeNames.Contains(field.Value.ToLower());

                case "float":
                case "absfloat":
                    string fv1 = field.Value.Replace(AVN.DecimalSeparator == AVNEntity.decimalSeparator.Comma ? "," : ".", ".");
                    return IDS.EBSTCRM.Base.TypeCast.ToDoubleLoose(fv1);

                case "integer":
                case "absinteger":
                    return IDS.EBSTCRM.Base.TypeCast.ToLongLoose(field.Value);
            }

            return null;
        }

        private bool IsFieldContentValid(AVNField field, AVNEntity AVN, string dataType) {
            switch (dataType) {
                case "emailaddress":
                    return (!field.IsRequired && string.IsNullOrEmpty(field.Value)) || System.Text.RegularExpressions.Regex.IsMatch(field.Value, @"^([a-zæøåA-ZÆØÅ0-9_\.\-])+\@(([a-zæøåA-ZÆØÅ0-9\-])+\.)+([a-zA-Z0-9]{2,4})+$");

                case "memo":
                case "textbox":
                    return (!field.IsRequired && string.IsNullOrEmpty(field.Value)) || !string.IsNullOrEmpty(field.Value);

                case "dropdown":
                case "listview":
                    return (!field.IsRequired && string.IsNullOrEmpty(field.Value)) || field.ValueRestrictedList != null ? field.ValueRestrictedList.Contains(field.Value) : true;

                case "datetime":

                    switch (AVN.DateFormat) {
                        case AVNEntity.dateFormat.DDMMYYYY:
                            return (!field.IsRequired && string.IsNullOrEmpty(field.Value)) || ParseDateTime(field.Value, "dd-MM-yyyy") != null;

                        case AVNEntity.dateFormat.MMDDYYYY:
                            return (!field.IsRequired && string.IsNullOrEmpty(field.Value)) || ParseDateTime(field.Value, "MM-dd-yyyy") != null;

                        case AVNEntity.dateFormat.YYYYMMDD:
                            return (!field.IsRequired && string.IsNullOrEmpty(field.Value)) || ParseDateTime(field.Value, "yyyy-MM-dd") != null;
                    }
                    return false;

                case "checkbox":
                    string[] booleanTypeNames = new string[] { "0", "1", "ja", "nej", "true", "false", "yes", "no", "-1" };
                    return booleanTypeNames.Contains(field.Value.ToLower());

                case "float":
                    string fv1 = field.Value.Replace(AVN.DecimalSeparator == AVNEntity.decimalSeparator.Comma ? "," : ".", ".");
                    return (!field.IsRequired && string.IsNullOrEmpty(field.Value)) || IDS.EBSTCRM.Base.TypeCast.ToDoubleLoose(fv1) != null;

                case "absfloat":
                    string fv2 = field.Value.Replace(AVN.DecimalSeparator == AVNEntity.decimalSeparator.Comma ? "," : ".", ".");
                    double? fv2v = IDS.EBSTCRM.Base.TypeCast.ToDoubleLoose(fv2);
                    return (!field.IsRequired && string.IsNullOrEmpty(field.Value)) || (fv2v != null && fv2v.Value >= 0);

                case "integer":
                    return (!field.IsRequired && string.IsNullOrEmpty(field.Value)) || System.Text.RegularExpressions.Regex.IsMatch(field.Value, @"^(-\d{1,24}|\d{1,24})$");

                case "absinteger":
                    return (!field.IsRequired && string.IsNullOrEmpty(field.Value)) || System.Text.RegularExpressions.Regex.IsMatch(field.Value, @"^\d{1,24}$");
            }

            return false;
        }

        private DateTime? ParseDateTime(string value, string format) {
            try {
                return DateTime.ParseExact(value, format + " HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
            } catch (Exception exp1) {
                try {
                    return DateTime.ParseExact(value, format, System.Globalization.CultureInfo.InvariantCulture);
                } catch {

                }
            }

            return null;
        }

        private string StringArrayToCommaSepList(string[] array) {
            string r = "";
            foreach (string s in array) {
                r += (r == "" ? "" : ", ") + s;
            }
            return r;
        }

        #endregion

        #endregion
    }
}
