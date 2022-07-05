using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;

namespace IDS.EBSTCRM.Base {
    /// <summary>
    /// 
    /// </summary>
    [Serializable()]
    public class Category : EventLogBase {
        public int Id { get; set; }
        public int OrganisationId { get; set; }
        public string Name { get; set; }
        public int Type { get; set; }
        public int SortOrder { get; set; }
        public string CreatedBy { get; set; }
        public DateTime DateCreated { get; set; }
        public string CreatedByName { get; set; }
        public Category() {
        }
        public Category(ref SqlDataReader dr) {
            Populate(ref dr);
        }
        private void Populate(ref SqlDataReader dr) {
            Id = TypeCast.ToInt(dr["Id"]);
            OrganisationId = TypeCast.ToInt(dr["OrganisationId"]);
            Name = TypeCast.ToString(dr["Name"]);
            CreatedBy = TypeCast.ToString(dr["CreatedBy"]);
            DateCreated = TypeCast.ToDateTime(dr["DateCreated"]);
            CreatedByName = TypeCast.ToString(dr["CreatedByName"]);
            SortOrder = TypeCast.ToInt(dr["SortOrder"]);
        }
    }
}
