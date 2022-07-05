using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;

namespace IDS.EBSTCRM.Base
{
    /// <summary>
    /// MonthValue used for statistics displaying Graphs
    /// OBSOLETE
    /// </summary>
    public class MonthValue
    {
        public int Month;
        public int Value;

        public MonthValue()
        {

        }

        public MonthValue(ref SqlDataReader dr)
        {
            Month = TypeCast.ToInt(dr["Month"]);
            Value = TypeCast.ToInt(dr["Value"]);
        }
    }
}
