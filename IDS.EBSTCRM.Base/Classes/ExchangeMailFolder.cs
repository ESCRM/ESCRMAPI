using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;

namespace IDS.EBSTCRM.Base
{
    /// <summary>
    /// Exchange Email Folder Reference
    /// OBSOLETE
    /// </summary>
    public class ExchangeMailFolder
    {
        public int Id;
        public string UserId;
        public int OrganisationId;
        public string FolderName;
        public string FullPath;
        public string ParentFolderName;
        public int ParentFolderId;
        public int ChildCount = 0;

        public ExchangeMailFolder()
        {

        }

        public ExchangeMailFolder(ref SqlDataReader dr)
        {
            Id = TypeCast.ToInt(dr["Id"]);
            UserId = TypeCast.ToString(dr["UserId"]);
            OrganisationId = TypeCast.ToInt(dr["OrganisationId"]);
            FolderName = TypeCast.ToString(dr["FolderName"]);
            FullPath = TypeCast.ToString(dr["FullPath"]);
            ParentFolderName = TypeCast.ToString(dr["ParentFolderName"]);
            ParentFolderId = TypeCast.ToInt(dr["ParentFolderId"]);

            ChildCount = TypeCast.ToInt(dr["ChildCount"]);
        }
    }
}
