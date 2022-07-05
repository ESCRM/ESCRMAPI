using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

namespace IDS.EBSTCRM.WindowManager.Integration
{
    /// <summary>
    /// SMV Search result
    /// result is returned to Outlook2CRM addin for outlook
    /// </summary>
    [Serializable()]
    public class Outlook_SMVPOTContact
    {
        /// <summary>
        /// Lazyness - public variables
        /// </summary>
        #region Declarations

        public int ContactId;
        public string CompanyName;
        public string Firstname;
        public string Lastname;
        public string Email;

        public bool IsSMV;
        public bool IsEarlyWarning;

        #endregion

        #region Constructors

        public Outlook_SMVPOTContact()
        {

        }

        public Outlook_SMVPOTContact(ref System.Data.SqlClient.SqlDataReader dr, bool isEarlyWarning)
        {
            if (isEarlyWarning)
            {
                ContactId = IDS.EBSTCRM.Base.TypeCast.ToInt(dr["CompanyId"]);
                CompanyName = IDS.EBSTCRM.Base.TypeCast.ToString(dr["CompanyName"]);
                Firstname = IDS.EBSTCRM.Base.TypeCast.ToString(dr["Firstname"]);
                Lastname = IDS.EBSTCRM.Base.TypeCast.ToString(dr["Lastname"]);
                Email = IDS.EBSTCRM.Base.TypeCast.ToString(dr["Email"]);

                IsSMV = false;
                IsEarlyWarning = true;
            }
            else
            {
                ContactId = IDS.EBSTCRM.Base.TypeCast.ToInt(dr["ContactId"]);
                CompanyName = IDS.EBSTCRM.Base.TypeCast.ToString(dr["z_companies_1_Firmanavn_1"]);
                Firstname = IDS.EBSTCRM.Base.TypeCast.ToString(dr["z_contacts_1_Fornavn_1"]);
                Lastname = IDS.EBSTCRM.Base.TypeCast.ToString(dr["z_contacts_1_Efternavn_1"]);
                Email = IDS.EBSTCRM.Base.TypeCast.ToString(dr["z_contacts_1_Email_1"]);

                IsSMV = IDS.EBSTCRM.Base.TypeCast.ToInt(dr["ContactType"]) == 0;
                IsEarlyWarning = false;
            }
        }

        #endregion

    }
}
