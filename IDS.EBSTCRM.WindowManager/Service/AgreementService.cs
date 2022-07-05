using IDS.EBSTCRM.Base;
using System.Linq;

namespace IDS.EBSTCRM.WindowManager.Service {
    public class AgreementService {
        public static string ParseToken(string text, int contactid) {
            if (IDS.EBSTCRM.WindowManager.Security.UserAccess.GetCurrentUser() != null) {
                var user = (User)IDS.EBSTCRM.WindowManager.Security.UserAccess.GetCurrentUser();
                var sql = new SQLDB();

                var firstName = "";
                var lastName = "";
                var email = "";
                var companyId = 0;
                var companyName = "";

                // Contact information
                var contact = sql.Contact_Get(user.OrganisationId, contactid, user);
                if (contact != null) {
                    if (contact.DynamicTblColumns.FirstOrDefault(w => w.Name == "z_contacts_1_Fornavn_1") != null) {
                        firstName = TypeCast.ToString(contact.DynamicTblColumns.FirstOrDefault(w => w.Name == "z_contacts_1_Fornavn_1").Value);
                    }
                    if (contact.DynamicTblColumns.FirstOrDefault(w => w.Name == "z_contacts_1_Efternavn_1") != null) {
                        lastName = TypeCast.ToString(contact.DynamicTblColumns.FirstOrDefault(w => w.Name == "z_contacts_1_Efternavn_1").Value);
                    }
                    if (contact.DynamicTblColumns.FirstOrDefault(w => w.Name == "z_contacts_1_Email_1") != null) {
                        email = TypeCast.ToString(contact.DynamicTblColumns.FirstOrDefault(w => w.Name == "z_contacts_1_Email_1").Value);
                    }
                    companyId = TypeCast.ToInt(contact.CompanyId);
                }

                // Company information
                if (companyId > 0) {
                    var company = sql.Company_Get(user.OrganisationId, companyId, user);
                    if (company != null) {
                        if (company.DynamicTblColumns.FirstOrDefault(w => w.Name == "z_companies_1_Firmanavn_1") != null) {
                            companyName = TypeCast.ToString(company.DynamicTblColumns.FirstOrDefault(w => w.Name == "z_companies_1_Firmanavn_1").Value);
                        }
                    }
                }

                // English Keywords
                text = text.Replace("%FirstName%", firstName);
                text = text.Replace("%LastName%", lastName);
                text = text.Replace("%Email%", email);
                text = text.Replace("%CompanyName%", companyName);

                // Danish Keywords
                text = text.Replace("%Fornavn%", firstName);
                text = text.Replace("%Efternavn%", lastName);
                text = text.Replace("%Mail%", email);
                text = text.Replace("%Firmanavn%", companyName);

                return text;
            }
            return text;
        }
    }
}