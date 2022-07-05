using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;

namespace IDS.EBSTCRM.Base
{
    /// <summary>
    /// Gets evaluation data from an organisations
    /// Split into day counts
    /// </summary>
    public class Statistics_EvaluationOrganisationDay
    {
        public int Total;
        public int Redirects;
        public int PrivateRedirects;
        public DateTime Datestamp;

        public Statistics_EvaluationOrganisationDay()
        {

        }

        public Statistics_EvaluationOrganisationDay(ref SqlDataReader dr)
        {
            Total = TypeCast.ToInt(dr["total"]);
            Redirects = TypeCast.ToInt(dr["Redirects"]);
            PrivateRedirects = TypeCast.ToInt(dr["PrivateRedirects"]);
            Datestamp = TypeCast.ToDateTime(dr["Datestamp"]);
        }
    }
}
