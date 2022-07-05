using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;

namespace IDS.EBSTCRM.Base
{
    /// <summary>
    /// Used for Exporting Partner Evaluations with financials pools and other data
    /// see https://[site]/Exports/FinansieringsPuljer.aspx
    /// </summary>
    public class PartnerEvaluationExport
    {
        public int ContactId;
        public int OrganisationId;
        public string OrganisationName;
        public string Username;
        public string UserId;
        public int EvaluationId;

        public string SMVPOTContactType;

        public DateTime DateForEvaluation;
        public string UserEmail;
        public bool RecieveEmailOnExportToUserEvaluation;

		public string Firstname;
		public string Lastname;
		public string Email;
		public string Phone1;
		public string CompanyName;
		public string CVR;
		public string Address;
		public string Zipcode;
        public string City;

        public int SMVPOTConcatId;
        public int SMVPOTCompanyId;

        public List<string> Fields = new List<string>();

        public PartnerEvaluationExport(ref SqlDataReader dr)
        {
            ContactId = TypeCast.ToInt(dr["contactId"]);
            OrganisationName = TypeCast.ToString(dr["OrganisationName"]);
            UserId = TypeCast.ToString(dr["UserId"]);
            Username = TypeCast.ToString(dr["Username"]);
            DateForEvaluation = TypeCast.ToDateTime(dr["DateForEvaluation"]);
            EvaluationId = TypeCast.ToInt(dr["EvaluationId"]);

            SMVPOTContactType = TypeCast.ToInt(dr["ContactType"]) == 0 ? "SMV" : "POT";

            UserEmail = TypeCast.ToString(dr["UserEmail"]);
            RecieveEmailOnExportToUserEvaluation = TypeCast.ToBool(dr["RecieveEmailOnExportToUserEvaluation"]);
            OrganisationId = TypeCast.ToInt(dr["OrganisationId"]);

            Firstname = TypeCast.ToString(dr["SAMFirstname"]);
            Lastname = TypeCast.ToString(dr["SAMLastname"]);
            Email = TypeCast.ToString(dr["SAMemail"]);
            Phone1 = TypeCast.ToString(dr["SAMPhone1"]);
            CompanyName = TypeCast.ToString(dr["SAMCompanyName"]);
            CVR = TypeCast.ToString(dr["SAMCVR"]);
            Address = TypeCast.ToString(dr["SAMAddress"]);
            Zipcode = TypeCast.ToString(dr["SAMZipcode"]);
            City = TypeCast.ToString(dr["SAMCity"]);

            SMVPOTConcatId = TypeCast.ToInt(dr["SMVPOTConcatId"]);
            SMVPOTCompanyId = TypeCast.ToInt(dr["SMVPOTCompanyId"]);

            for (int i = 0; i < dr.FieldCount; i++)
            {
                if (dr.GetName(i).StartsWith("z_") == true)
                {
                    Fields.Add(TypeCast.ToString(dr[i]));
                }
            }
            
        }
    }
}
