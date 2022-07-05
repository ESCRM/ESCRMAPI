using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;

namespace IDS.EBSTCRM.Base
{
    public enum FolderTypes
    {
        DelteDokumenterForAlle = 0,
        DelteFilerForOrg = 1,
        MineDokumenter = 2
    }

    /// <summary>
    /// Deleted File folder
    /// OBSOLETE FROM CRM1.0
    /// </summary>
    public class FileFolderDeleted
    {
        public int Id;
        public string Name;
        public string Path;
        public string DeletedByUser;
        public DateTime DateDeleted;

        public FileFolderDeleted()
        {
        }

        public FileFolderDeleted(ref SqlDataReader dr)
        {
            Id = TypeCast.ToInt(dr["id"]);
            Name = TypeCast.ToString(dr["Name"]);
            Path = TypeCast.ToString(dr["Path"]);
            DeletedByUser = TypeCast.ToString(dr["DeletedByUser"]);
            DateDeleted = TypeCast.ToDateTime(dr["deletedDate"]);
        }
    }

    /// <summary>
    /// File folder
    /// OBSOLETE FROM CRM1.0
    /// </summary>
    public class FileFolder : EventLogBase
    {
        public int Id;
        public object ParentId;
        public String Name;
        public String userId;
        public DateTime DateCreated;
        public int organisationId;
        public int ChildCount;
        public int FolderType;
        public object deletedDate;
        public object deletedBy;

        public FileFolder()
        {

        }

        
        public FileFolder(ref SqlDataReader dr)
        {
            Id = TypeCast.ToInt(dr["Id"]);
            ParentId = TypeCast.ToInt(dr["ParentId"]);
            Name = TypeCast.ToString(dr["Name"]);
            userId = TypeCast.ToString(dr["userId"]);
            DateCreated = TypeCast.ToDateTime(dr["DateCreated"]);
            organisationId = TypeCast.ToInt(dr["organisationId"]);
            ChildCount = TypeCast.ToInt(dr["ChildCount"]);
            FolderType = TypeCast.ToInt(dr["FolderType"]);
            deletedDate = TypeCast.ToStringOrDBNull(dr["deletedDate"]);
            deletedBy = TypeCast.ToStringOrDBNull(dr["deletedBy"]);

        }
    }
}
