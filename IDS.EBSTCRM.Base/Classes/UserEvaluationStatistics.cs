using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Collections;

namespace IDS.EBSTCRM.Base
{
    /// <summary>
    /// Statitical class for User Evaluations
    /// </summary>
    public class UserEvaluationStatistics
    {
        public int CountWithSAM;
        public int CountTotal;
        public string Username;
        public string Id;

        public decimal Percentage;

        public UserEvaluationStatistics()
        {

        }

        public UserEvaluationStatistics(ref SqlDataReader dr)
        {
            CountWithSAM = TypeCast.ToInt(dr["CountWithSAM"]);
            CountTotal = TypeCast.ToInt(dr["CountTotal"]);

            Username = TypeCast.ToString(dr["Username"]);
            Id = TypeCast.ToString(dr["Id"]);

            if(CountTotal>0)
                Percentage = (decimal)CountWithSAM / (decimal)CountTotal * 100;
        }
    }
}
