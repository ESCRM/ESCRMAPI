using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;

namespace IDS.EBSTCRM.Base
{
    /// <summary>
    /// Financial Pool
    /// Financial pool for use with SAM -> SMV/POT Releations
    /// </summary>
    public class FinancialPool
    {
        public int Id;
        public int OrganisationId;
        public string OrganisationName;
        public string CreatedBy;
        public string Username;
        public string Name;
        public bool Shared;
        public DateTime DateCreated;
        public bool ForceEvaluation;
        public bool LimitedToSMV;

        public string VisibleToTexts;

        public List<int> SharedWithOrganisations = new List<int>();

        public FinancialPool()
        { 
        }

        public FinancialPool(ref SqlDataReader dr, bool isSingle)
        {
            this.Id = TypeCast.ToInt(dr["Id"]);
            this.OrganisationId = TypeCast.ToInt(dr["OrganisationId"]);
            this.OrganisationName = TypeCast.ToString(dr["OrganisationName"]);
            this.CreatedBy = TypeCast.ToString(dr["CreatedBy"]);
            this.Username = TypeCast.ToString(dr["Username"]);
            this.Name = TypeCast.ToString(dr["Name"]);
            this.Shared = TypeCast.ToBool(dr["Shared"]);
            this.DateCreated = TypeCast.ToDateTime(dr["DateCreated"]);
            this.ForceEvaluation = TypeCast.ToBool(dr["ForceEvaluation"]);
            this.VisibleToTexts = TypeCast.ToString(dr["VisibleToTexts"]);
            this.LimitedToSMV = TypeCast.ToBool(dr["LimitedToSMV"]);

            if (isSingle)
            {
                dr.NextResult();

                while (dr.Read())
                {
                    SharedWithOrganisations.Add(TypeCast.ToInt(dr["SharedWithOrganisationId"]));
                }
            }
        }
    }
}
