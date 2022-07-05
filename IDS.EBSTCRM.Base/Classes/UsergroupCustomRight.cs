using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;

namespace IDS.EBSTCRM.Base
{
    public class UsergroupCustomRight
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Icon16 { get; set; }
        public string Icon32 { get; set; }
        public string CreatedBy { get;set;}
        public DateTime Created { get; set; }
        public string LastUpdatedBy { get; set; }
        public DateTime? LastUpdated { get; set; }
        public string DeletedBy { get; set; }
        public DateTime? Deleted { get; set; }

        public UsergroupCustomRight()
        {

        }

        public UsergroupCustomRight(ref SqlDataReader dr)
        {
            Id = TypeCast.ToInt(dr["Id"]);
            Name = TypeCast.ToString(dr["Name"]);
            Description = TypeCast.ToString(dr["Description"]);
            Icon16 = TypeCast.ToString(dr["Icon16"]);
            Icon32 = TypeCast.ToString(dr["Icon32"]);
            CreatedBy = TypeCast.ToString(dr["CreatedBy"]);
            Created = TypeCast.ToDateTime(dr["Created"]);
            LastUpdatedBy = TypeCast.ToString(dr["LastUpdatedBy"]);
            LastUpdated = TypeCast.ToDateTimeLoose(dr["LastUpdated"]);
            DeletedBy = TypeCast.ToString(dr["DeletedBy"]);
            Deleted = TypeCast.ToDateTimeLoose(dr["Deleted"]);
        }
    }
}
