using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;

namespace IDS.EBSTCRM.Base
{
    /// <summary>
    /// File saved to Early Warning Contact person
    /// </summary>
    public class EarlyWarningFileWithData : EarlyWarningFile
    {
        public byte[] Data = null;

        public EarlyWarningFileWithData()
            : base()
        {

        }

        public EarlyWarningFileWithData(ref SqlDataReader dr)
            : base(ref dr)
        {
            Data = (byte[])dr["Data"];
        }
    }

    public class EarlyWarningFile
    {
        public int Id;
        public int OwnerId;
        public string Filename;
        public string CreatedBy;
        public DateTime DateCreated;
        public string CreatedByFirstname;
        public string CreatedByLastname;
        public string CreatedByUsername;
        public string CreatedByEmail;
        public string CreatedByOrganisationName;

        public EarlyWarningFile()
        {

        }

        public EarlyWarningFile(ref SqlDataReader dr)
        {
            Id = TypeCast.ToInt(dr["id"]);
            OwnerId = TypeCast.ToInt(dr["OwnerId"]);

            Filename = TypeCast.ToString(dr["Filename"]);
            CreatedBy = TypeCast.ToString(dr["CreatedBy"]);

            DateCreated = TypeCast.ToDateTime(dr["DateCreated"]);

            CreatedByFirstname = TypeCast.ToString(dr["CreatedByFirstname"]);
            CreatedByLastname = TypeCast.ToString(dr["CreatedByLastname"]);
            CreatedByUsername = TypeCast.ToString(dr["CreatedByUsername"]);
            CreatedByEmail = TypeCast.ToString(dr["CreatedByEmail"]);
            CreatedByOrganisationName = TypeCast.ToString(dr["CreatedByOrganisationName"]);
        }
    }
}
