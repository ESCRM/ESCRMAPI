using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Data.SqlClient;

namespace IDS.EBSTCRM.Base
{
    /// <summary>
    /// Deleted Early Warning Companies and Contacts in listview
    /// Inherits EarlyWarningCompanyAndContact
    /// </summary>
    public class EarlyWarningCompanyAndContactDeleted : EarlyWarningCompanyAndContact
    {
        public string DeletedByName;
        public string OrganisationName;

        public EarlyWarningCompanyAndContactDeleted()
            : base()
        {

        }

        public EarlyWarningCompanyAndContactDeleted(ref SqlDataReader dr)
            : base(ref dr)
        {
            DeletedByName = TypeCast.ToString(dr["DeletedByName"]);
            OrganisationName = TypeCast.ToString(dr["OrganisationName"]);
        }
    }

    /// <summary>
    /// Early Warning Listview item
    /// For displaying lists of Early Warning Company and Contact Data
    /// </summary>
    public class EarlyWarningCompanyAndContact
    {
        public int CompanyId;
        public string CompanyName;
        public string CVR;
        public string Address;
        public string Zipcode;
        public string City;
        public string County;
        public string CompanyEmail;
        public string CompanyPhone;
        public string CompanyType;
        public int Status;

        public int ContactId;
        public string Firstname;
        public string Lastname;
        public string ContactMobilePhone;
        public string ContactEmail;
        public string ContactPhone;

        // New Properties from ÆØ4
		public object DateEnded { get;set;}
		public object DateStarted { get;set;}
		public object DateSentToVolunteer { get;set;}
		public string Recommendation { get;set;}
		public string CurrentConsultant { get;set;}
        public string CurrentConsultantInfo { get; set; }

        public EarlyWarningCompanyAndContact()
        {

        }

        public EarlyWarningCompanyAndContact(ref SqlDataReader dr)
        {
            CompanyId=TypeCast.ToInt(dr["CompanyId"]);
            CompanyName=TypeCast.ToString(dr["CompanyName"]);
            CVR=TypeCast.ToString(dr["CVR"]);
            Address=TypeCast.ToString(dr["Address"]);
            Zipcode=TypeCast.ToString(dr["Zipcode"]);
            City=TypeCast.ToString(dr["City"]);
            County = TypeCast.ToString(dr["County"]);
            CompanyEmail=TypeCast.ToString(dr["CompanyEmail"]);
            CompanyPhone=TypeCast.ToString(dr["CompanyPhone"]);
            CompanyType=TypeCast.ToString(dr["CompanyType"]);
            Status = TypeCast.ToInt(dr["Status"]);

            ContactId = TypeCast.ToInt(dr["ContactId"]);
            Firstname=TypeCast.ToString(dr["Firstname"]);
            Lastname=TypeCast.ToString(dr["Lastname"]);
            ContactMobilePhone=TypeCast.ToString(dr["ContactMobilePhone"]);
            ContactEmail=TypeCast.ToString(dr["ContactEmail"]);
            ContactPhone=TypeCast.ToString(dr["ContactPhone"]);

            // New Properties from ÆØ4
            DateEnded = TypeCast.ToDateTimeOrNull(dr["DateEnded"]);
            DateStarted = TypeCast.ToDateTimeOrNull(dr["DateStarted"]);
            DateSentToVolunteer = TypeCast.ToDateTimeOrNull(dr["DateSentToVolunteer"]);
            Recommendation = TypeCast.ToString(dr["Recommendation"]);
            CurrentConsultant = TypeCast.ToString(dr["CurrentConsultant"]);
            CurrentConsultantInfo = TypeCast.ToString(dr["CurrentConsultantInfo"]);
        }
    }
}
