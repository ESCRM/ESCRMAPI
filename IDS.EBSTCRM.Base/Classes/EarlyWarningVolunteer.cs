using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Collections;
using System.Web;

namespace IDS.EBSTCRM.Base
{
    /// <summary>
    /// Volunteers for Early Warning Companies
    /// </summary>
    public class EarlyWarningVolunteerDeleted : EarlyWarningVolunteer
    {
        public string DeletedByName;
        public string OrganisationName;

        public EarlyWarningVolunteerDeleted()
            : base()
        {

        }

        public EarlyWarningVolunteerDeleted(ref SqlDataReader dr)
            : base(ref dr)
        {
            DeletedByName = TypeCast.ToString(dr["DeletedByName"]);
            OrganisationName = TypeCast.ToString(dr["OrganisationName"]);
        }
    }
    public class EarlyWarningVolunteer
    {
        public int Id;
        public int OrganisationId;
        public string Firstname;
        public string Lastname;
        public string Phone;
        public string Mobile;
        public string Email;
        public string Address;
        public string Zipcode;
        public string City;
        public string CV;
        public string Memo;
        public bool Competence_Design;
        public bool Competence_Export;
        public bool Competence_HR;
        public bool Competence_Purchase;
        public bool Competence_IPR;
        public bool Competence_IT;
        public bool Competence_CommunicationsAndPR;
        public bool Competence_Logistics;
        public bool Competence_Advertising;
        public bool Competence_Production;
        public bool Competence_Sales;
        public bool Competence_Strategy;
        public bool Competence_ProductDevelopment;
        public bool Competence_Finance;
        public DateTime DateCreated;
        public string CreatedBy;
        public object LastUpdated;
        public string LastUpdatedBy;
        public object DateDeleted;
        public string DeletedBy;

        public string ContactToEWConsultant;
        public bool Active;
        public bool Radius_Hovedstaden;
        public bool Radius_Midtjylland;
        public bool Radius_Syddanmark;
        public bool Radius_Sjaelland;
        public bool Radius_Nordjylland;

        public string ActionRadiusText;

        public EarlyWarningVolunteer()
        {

        }

        public EarlyWarningVolunteer(HttpRequest Request)
        {
            Id = TypeCast.ToInt(Request.Form["txtId"]);

            Firstname = TypeCast.ToString(Request.Form["txtFirstname"]);
            Lastname = TypeCast.ToString(Request.Form["txtLastname"]);
            Phone = TypeCast.ToString(Request.Form["txtPhone"]);
            Mobile = TypeCast.ToString(Request.Form["txtMobile"]);
            Email = TypeCast.ToString(Request.Form["txtEmail"]);
            Address = TypeCast.ToString(Request.Form["txtAddress"]);
            Zipcode = TypeCast.ToString(Request.Form["txtZipcode"]);
            City = TypeCast.ToString(Request.Form["txtCity"]);
            CV = TypeCast.ToString(Request.Form["txtCV"]);
            Memo = TypeCast.ToString(Request.Form["txtMemo"]);
            Competence_Design = TypeCast.ToBool(Request.Form["cbCompetence_Design"]);
            Competence_Export = TypeCast.ToBool(Request.Form["cbCompetence_Export"]);
            Competence_HR = TypeCast.ToBool(Request.Form["cbCompetence_HR"]);
            Competence_Purchase = TypeCast.ToBool(Request.Form["cbCompetence_Purchase"]);
            Competence_IPR = TypeCast.ToBool(Request.Form["cbCompetence_IPR"]);
            Competence_IT = TypeCast.ToBool(Request.Form["cbCompetence_IT"]);
            Competence_CommunicationsAndPR = TypeCast.ToBool(Request.Form["cbCompetence_CommunicationsAndPR"]);
            Competence_Logistics = TypeCast.ToBool(Request.Form["cbCompetence_Logistics"]);
            Competence_Advertising = TypeCast.ToBool(Request.Form["cbCompetence_Advertising"]);
            Competence_Production = TypeCast.ToBool(Request.Form["cbCompetence_Production"]);
            Competence_Sales = TypeCast.ToBool(Request.Form["cbCompetence_Sales"]);
            Competence_Strategy = TypeCast.ToBool(Request.Form["cbCompetence_Strategy"]);
            Competence_ProductDevelopment = TypeCast.ToBool(Request.Form["cbCompetence_ProductDevelopment"]);
            Competence_Finance = TypeCast.ToBool(Request.Form["cbCompetence_Finance"]);

            ContactToEWConsultant = TypeCast.ToString(Request.Form["txtContactToEWConsultant"]);
            Active = TypeCast.ToBool(Request.Form["selStatus"]);
            Radius_Hovedstaden = TypeCast.ToBool(Request.Form["cbRadius_Hovedstaden"]);
            Radius_Midtjylland = TypeCast.ToBool(Request.Form["cbRadius_Midtjylland"]);
            Radius_Syddanmark = TypeCast.ToBool(Request.Form["cbRadius_Syddanmark"]);
            Radius_Sjaelland = TypeCast.ToBool(Request.Form["cbRadius_Sjaelland"]);
            Radius_Nordjylland = TypeCast.ToBool(Request.Form["cbRadius_Nordjylland"]);

        }

        public EarlyWarningVolunteer(ref SqlDataReader Dr)
        {
            Id = TypeCast.ToInt(Dr["Id"]);
            OrganisationId = TypeCast.ToInt(Dr["OrganisationId"]);
            Firstname = TypeCast.ToString(Dr["Firstname"]);
            Lastname = TypeCast.ToString(Dr["Lastname"]);
            Phone = TypeCast.ToString(Dr["Phone"]);
            Mobile = TypeCast.ToString(Dr["Mobile"]);
            Email = TypeCast.ToString(Dr["Email"]);
            Address = TypeCast.ToString(Dr["Address"]);
            Zipcode = TypeCast.ToString(Dr["Zipcode"]);
            City = TypeCast.ToString(Dr["City"]);
            CV = TypeCast.ToString(Dr["CV"]);
            Memo = TypeCast.ToString(Dr["Memo"]);
            Competence_Design = TypeCast.ToBool(Dr["Competence_Design"]);
            Competence_Export = TypeCast.ToBool(Dr["Competence_Export"]);
            Competence_HR = TypeCast.ToBool(Dr["Competence_HR"]);
            Competence_Purchase = TypeCast.ToBool(Dr["Competence_Purchase"]);
            Competence_IPR = TypeCast.ToBool(Dr["Competence_IPR"]);
            Competence_IT = TypeCast.ToBool(Dr["Competence_IT"]);
            Competence_CommunicationsAndPR = TypeCast.ToBool(Dr["Competence_CommunicationsAndPR"]);
            Competence_Logistics = TypeCast.ToBool(Dr["Competence_Logistics"]);
            Competence_Advertising = TypeCast.ToBool(Dr["Competence_Advertising"]);
            Competence_Production = TypeCast.ToBool(Dr["Competence_Production"]);
            Competence_Sales = TypeCast.ToBool(Dr["Competence_Sales"]);
            Competence_Strategy = TypeCast.ToBool(Dr["Competence_Strategy"]);
            Competence_ProductDevelopment = TypeCast.ToBool(Dr["Competence_ProductDevelopment"]);
            Competence_Finance = TypeCast.ToBool(Dr["Competence_Finance"]);
            DateCreated = TypeCast.ToDateTime(Dr["DateCreated"]);
            CreatedBy = TypeCast.ToString(Dr["CreatedBy"]);
            LastUpdated = TypeCast.ToDateTimeOrNull(Dr["LastUpdated"]);
            LastUpdatedBy = TypeCast.ToString(Dr["LastUpdatedBy"]);
            DateDeleted = TypeCast.ToDateTimeOrNull(Dr["DateDeleted"]);
            DeletedBy = TypeCast.ToString(Dr["DeletedBy"]);


            ContactToEWConsultant = TypeCast.ToString(Dr["ContactToEWConsultant"]);
            Active = TypeCast.ToBool(Dr["Active"]);
            Radius_Hovedstaden = TypeCast.ToBool(Dr["Radius_Hovedstaden"]);
            Radius_Midtjylland = TypeCast.ToBool(Dr["Radius_Midtjylland"]);
            Radius_Syddanmark = TypeCast.ToBool(Dr["Radius_Syddanmark"]);
            Radius_Sjaelland = TypeCast.ToBool(Dr["Radius_Sjaelland"]);
            Radius_Nordjylland = TypeCast.ToBool(Dr["Radius_Nordjylland"]);

            ActionRadiusText = TypeCast.ToString(Dr["ActionRadiusText"]);

        }

    }
}
