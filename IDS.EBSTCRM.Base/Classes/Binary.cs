using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;

namespace IDS.EBSTCRM.Base
{
    /// <summary>
    /// Binary file stored in database
    /// Childclass of IDS.EBSTCRM.Base.File
    /// </summary>
    public class Binary
    {
        public int Id;
        public byte[] Data;
        public object deletedDate;
        public object deletedBy;

        public Binary()
        {
        }

        /// <summary>
        /// Constructor of new Binary file from DataReader
        /// </summary>
        /// <param name="dr"></param>
        public Binary(ref SqlDataReader dr)
        {
            Id = TypeCast.ToInt(dr["Id"]);
            Data = (byte[])dr["data"];
            deletedDate = TypeCast.ToStringOrDBNull(dr["deletedDate"]);
            deletedBy = TypeCast.ToStringOrDBNull(dr["deletedBy"]);
        }
    }

    
}
