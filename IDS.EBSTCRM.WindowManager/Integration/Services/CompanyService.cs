using System;
using System.Linq;
using IDS.EBSTCRM.Base;
using System.Collections.Generic;
using IDS.EBSTCRM.WindowManager.Integration.Models;
namespace IDS.EBSTCRM.WindowManager.Integration.Services {
    public class CompanyService {
        public static Response IsValid(SysCompany companyRequest, List<DynamicField> companyDynamicField, List<DynamicField> contactDynamicField) {

            var result = new Response();

            // First Level : Validation
            // Contact validation
            if (companyRequest.CompanyDetail.CompanyId == 0) { // validate contact only when request for create a company
                result = ContactService.IsValid(companyRequest.CompanyDetail.Contact.Fields, contactDynamicField);
            } else {
                if (companyRequest.CompanyDetail.Contact != null && companyRequest.CompanyDetail.Contact.Fields != null &&
                    companyRequest.CompanyDetail.Contact.Fields.Any()) {

                    result.Status = false;
                    result.Message.Add("Contact fields are not allowed for update company request.");
                }
            }

            if (result.Status) {

                // Second Level : Validation
                // Company fields should be from "[DynamicFields]" table
                var messageCompany = "Kan ikke gemmes, fordi følgende felter for virksomheden ikke er fra systemet:";
                foreach (var companyField in companyRequest.CompanyDetail.Fields) {
                    //** Check for null, hvis ja så spring over, ESCRM-21/35
                    if (companyField == null)
                        continue;

                    var validField = companyDynamicField.Any(w => w.DatabaseColumn == companyField.Name);

                    // add this error when there's invalid field sent through api object
                    if (!validField) {
                        result.Status = false;
                        messageCompany = messageCompany + "\n" + companyField.Name;
                    }
                }
                if (!result.Status) { result.Message.Add(messageCompany); }


                // Third Level : Validation
                // Checking required company fields
                if (result.Status) {
                    messageCompany = "Der kan ikke gemmes, da følgende felter for virksomheden ikke er udfyldt korrekt:";

                    //** Sæt tæller index, ESCRM-21/35
                    int counterIndex = 0;
                    foreach (var companyField in companyDynamicField.Where(w => w.RequiredState == 1).ToList()) {
                        //** Check for null. hvis ja så spring over, ESCRM-21/35
                        if (companyRequest.CompanyDetail.Fields[counterIndex] == null)
                        {
                            counterIndex++;
                            continue;
                        }
                        else
                        {
                            counterIndex++;
                        }
                        if (counterIndex == companyRequest.CompanyDetail.Fields.Count)
                            break;

                        var field = companyRequest.CompanyDetail.Fields.FirstOrDefault(w => w.Name == companyField.DatabaseColumn);
                        if (field != null) {
                            if (string.IsNullOrEmpty(field.Value)) {
                                result.Status = false;
                                messageCompany = messageCompany + "\n" + field.Name;
                            }
                        } else {

                            // when required field not found in company request
                            result.Status = false;
                            messageCompany = messageCompany + "\n" + companyField.DatabaseColumn;
                        }
                    }

                    if (!result.Status) { result.Message.Add(messageCompany); }
                }
            }
            return result;

        }

        public static string GetDataType(List<DynamicField> fields, string name, ref object value) {
            foreach (DynamicField f in fields) {

                if (f.DatabaseTable + "_" + f.DatabaseColumn + "_" + f.BaseOrganisationId == name) {

                    if (f.FieldType == "integer" || f.FieldType == "absinteger") {
                        value = TypeCast.ToString(value);
                        value = ((string)value).Trim();
                        value = ((string)value).Replace(" ", "");
                        value = TypeCast.ToInt(value);
                        return "int";
                    } else if (f.FieldType == "float" || f.FieldType == "absfloat") {
                        value = TypeCast.ToString(value);
                        value = ((string)value).Trim();
                        value = ((string)value).Replace(" ", "");
                        value = ((string)value).Replace(".", "");
                        value = TypeCast.ToDecimal(value);
                        return "float";
                    } else if (f.FieldType == "datetime") {
                        object v = TypeCast.ToDateTimeOrNull(value);
                        if (v != null) { value = ((DateTime)v).ToString("yyyy-MM-dd"); } else { value = null; }
                        return "datetime";
                    }
                }
            }
            return "ntext";
        }
    }
}