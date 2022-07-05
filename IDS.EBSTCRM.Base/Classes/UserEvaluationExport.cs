using System;
using System.Collections.Generic;
using System.Text;
using IDS.EBSTCRM.Base;
using System.Data.SqlClient;

namespace IDS.EBSTCRM.Base
{
    /// <summary>
    /// Defines Export Items of Evaluated SMV/POT Contacts along with which user, whom sent the SMV/POT to evaluation
    /// </summary>
    public class UserEvaluationExport
    {
        public int ContactId;
        public int OrganisationId;
        public string OrganisationName;
        public string Username;
        public string ContactType;
        public List<string> Fields = new List<string>();
        public DateTime DateForEvaluation;
        public string Email;
        public bool RecieveEmailOnExportToUserEvaluation;
        public string EvaluationType;

        public UserEvaluationExport(ref SqlDataReader dr)
        {
            ContactId = TypeCast.ToInt(dr["contactId"]);
            OrganisationName = TypeCast.ToString(dr["OrganisationName"]);
            Username = TypeCast.ToString(dr["Username"]);
            ContactType = TypeCast.ToInt(dr["ContactType"]) == 0 ? "SMV" : "POT";
            DateForEvaluation = TypeCast.ToDateTime(dr["DateForEvaluation"]);
            EvaluationType = TypeCast.ToString(dr["EvaluationType"]);

            Email = TypeCast.ToString(dr["email"]);
            RecieveEmailOnExportToUserEvaluation = TypeCast.ToBool(dr["RecieveEmailOnExportToUserEvaluation"]);
            OrganisationId = TypeCast.ToInt(dr["OrganisationId"]);

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
