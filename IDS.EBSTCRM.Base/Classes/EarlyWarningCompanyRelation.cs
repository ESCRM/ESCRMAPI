using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
namespace IDS.EBSTCRM.Base
{
    /// <summary>
    /// Early Warning Volunteers Relations to Companies
    /// 
    /// </summary>
    public class EarlyWarningCompanyRelation
    {
        public int CompanyId;
        public string CompanyName;
        public string CVR;
        public object DateCreated;
        public string CreatedBy;
        public int ContactId;

		public DateTime? DateStarted { get;set;}
		public DateTime? DateSentToVolunteer { get;set;}
        public DateTime? DateEnded { get; set; }

        public EarlyWarningCompanyRelation()
        {

        }

        public EarlyWarningCompanyRelation(ref SqlDataReader dr)
        {
            CompanyId = TypeCast.ToInt(dr["CompanyId"]);
            CompanyName = TypeCast.ToString(dr["CompanyName"]);
            ContactId = TypeCast.ToInt(dr["ContactId"]);
            CVR = TypeCast.ToString(dr["CVR"]);
            DateCreated = TypeCast.ToDateTimeOrNull(dr["DateCreated"]);
            CreatedBy = TypeCast.ToString(dr["CreatedBy"]);

            DateStarted = TypeCast.ToDateTimeLoose(dr["DateStarted"]);
            DateSentToVolunteer = TypeCast.ToDateTimeLoose(dr["DateSentToVolunteer"]);
            DateEnded = TypeCast.ToDateTimeLoose(dr["DateEnded"]);

        }
    }
}
