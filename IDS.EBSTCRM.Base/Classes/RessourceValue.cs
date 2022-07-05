using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;

namespace IDS.EBSTCRM.Base
{
    /// <summary>
    /// Resporce Value
    /// Used for drawing statistical graphs
    /// </summary>
    public class RessourceValue
    {
        public string Name;
        public decimal Value;

        public RessourceValue()
        {

        }

        public RessourceValue(ref SqlDataReader dr)
        {
            Name = TypeCast.ToString(dr["name"]);
            Value = TypeCast.ToDecimal(dr["value"]);
        }
    }
}
