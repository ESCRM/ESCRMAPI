using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace IDS.EBSTCRM.Base {

    /// <summary>
    /// File for AVN
    /// </summary>
    [Serializable()]
    public class AVNFile : EventLogBase {

        public int id;
        public int avnId;
        public string name;
        public string description;
        public string fileName;
        public string contentType;
        public byte[] fileData;
        public string userId;
        public int contentLength;
        public object deletedDate;
        public object deletedBy;
        public int organisationId;
        public string dateCreated;
        public int sort;

        public AVNFile() { }

        public AVNFile(ref SqlDataReader dr) {
            Populate(ref dr);
        }

        private void Populate(ref SqlDataReader dr) {
            id = TypeCast.ToInt(dr["Id"]);
            organisationId = TypeCast.ToInt(dr["OrganisationId"]);
            fileName = TypeCast.ToString(dr["CategoryName"]);
            dateCreated = TypeCast.ToDateTime(dr["DateCreated"]).ToString("dd-MM-yyyy HH:mm");
        }
    }
}
