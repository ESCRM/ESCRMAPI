using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;

namespace IDS.EBSTCRM.Base
{
    /// <summary>
    /// Mailgroup folder for organization (Interessegruppe)
    /// </summary>
    public class MailgroupFolder
    {
        public int Id;
        public int OrganisationId;
        public string Name;
        public int ParentId;

        public DateTime DateCreated;
        public string CreatedBy;
        public object DateDeleted;
        public string DeletedBy;
        public object LastUpdated;
        public string lastUpdatedBy;

        public int ChildCount;
        public int ChildFolderCount;
        public int ChildGroupCount;

        public MailgroupFolder()
        {

        }

        public MailgroupFolder(ref SqlDataReader dr)
        {
            Id = TypeCast.ToInt(dr["Id"]);
            OrganisationId = TypeCast.ToInt(dr["OrganisationId"]);
            Name = TypeCast.ToString(dr["Name"]);
            DateCreated = TypeCast.ToDateTime(dr["DateCreated"]);
            CreatedBy = TypeCast.ToString(dr["CreatedBy"]);
            DateDeleted = TypeCast.ToDateTimeOrNull(dr["DateDeleted"]);
            DeletedBy = TypeCast.ToString(dr["DeletedBy"]);
            LastUpdated = TypeCast.ToDateTimeOrNull(dr["LastUpdated"]);
            lastUpdatedBy = TypeCast.ToString(dr["lastUpdatedBy"]);
            ParentId = TypeCast.ToInt(dr["ParentId"]);

            ChildCount = TypeCast.ToInt(dr["ChildCount"]);
            ChildFolderCount = TypeCast.ToInt(dr["ChildFolderCount"]);
            ChildGroupCount = TypeCast.ToInt(dr["ChildGroupCount"]);

        }
    }
}
