using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;

namespace IDS.EBSTCRM.Base
{
    /// <summary>
    /// Gets evaluation data from an organisations
    /// Split into month counts
    /// </summary>
    public class Statistics_EvaluationOrganisationMonth
    {
        public int Total;
        public int Redirects;
        public int PrivateRedirects;
        public int Year;
        public int Month;

        public Statistics_EvaluationOrganisationMonth()
        {

        }

        public Statistics_EvaluationOrganisationMonth(ref SqlDataReader dr)
        {
            Total = TypeCast.ToInt(dr["total"]);
            Redirects = TypeCast.ToInt(dr["Redirects"]);
            PrivateRedirects = TypeCast.ToInt(dr["PrivateRedirects"]);
            Year = TypeCast.ToInt(dr["Year"]);
            Month = TypeCast.ToInt(dr["Month"]);
        }
    }
}
