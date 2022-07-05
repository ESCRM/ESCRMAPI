using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;


namespace IDS.EBSTCRM.Base
{
    /// <summary>
    /// Deleted file on SMV/POT contact
    /// Inherits File
    /// </summary>
    public class FileDeleted
    {
        public int Id;
        public string FileName;
        public string Path;
        public string DeletedByUser;
        public DateTime DateDeleted;

        public FileDeleted()
        {
        }

        public FileDeleted(ref SqlDataReader dr)
        {
            Id = TypeCast.ToInt(dr["id"]);
            FileName = TypeCast.ToString(dr["FileName"]);
            Path = TypeCast.ToString(dr["Path"]);
            DeletedByUser = TypeCast.ToString(dr["DeletedByUser"]);
            DateDeleted = TypeCast.ToDateTime(dr["deletedDate"]);
        }
    }

    /// <summary>
    /// File for CRM2 version
    /// Inherits File
    /// </summary>
    public class FileCRM2 : File
    {
        public string VisibleToTexts;
        public string SharedWith;

        public FileCRM2()
        {

        }

        public FileCRM2(ref SqlDataReader dr) : base(ref dr)
        {
            VisibleToTexts = TypeCast.ToString(dr["VisibleToTexts"]);
            SharedWith = TypeCast.ToString(dr["SharedWith"]);
        }
    }

    /// <summary>
    /// Basic File saved on a SMV/POT Contact
    /// References to a Binary class for byte[] data
    /// </summary>
    public class File : EventLogBase
    {
        public int Id;
        public string filename;
        public string contenttype;
        public int binary;
        public object description;
        public string userId;
        public object FileFolder;
        public object contentgroup;
        public int contentlength;
        public object deletedDate;
        public object deletedBy;
        public object folderType;
        public int organisationId;
        public string dynamicText;
        private string path;
        public string userName;
        public DateTime dateCreated;
        public object contactId;
        public List<TableColumnWithValue> ContactData = new List<TableColumnWithValue>();

        public string Category;

        public string getContactData()
        {
            string ret = "";
            int count = 0;
            foreach (TableColumnWithValue t in ContactData)
            {
                if (t.ValueFormatted != "")
                {
                    ret += (count == 0 ? "" : (count == 1 ? " / " : " ")) + t.ValueFormatted;
                    count++;
                }
            }
            return ret == "" ? "Ingen" : ret;
        }
        
        public string Path
        {
            get { return path; }
            set { path = value; }
        }
	        

        public File()
        {
        }

        public File(ref SqlDataReader dr)
        {
            Id = TypeCast.ToInt(dr["Id"]);
            filename = TypeCast.ToString(dr["filename"]);
            contenttype = TypeCast.ToString(dr["contenttype"]);
            binary = TypeCast.ToInt(dr["binary"]);
            description = TypeCast.ToStringOrDBNull(dr["description"]);
            userId = TypeCast.ToString(dr["userId"]);
            FileFolder = TypeCast.ToIntOrNull(dr["FileFolder"]);
            contentgroup = TypeCast.ToStringOrDBNull(dr["contentgroup"]);
            contentlength = TypeCast.ToInt(dr["contentlength"]);
            deletedDate = TypeCast.ToStringOrDBNull(dr["deletedDate"]);
            deletedBy = TypeCast.ToStringOrDBNull(dr["deletedBy"]);
            folderType = TypeCast.ToIntOrDBNull(dr["folderType"]);
            organisationId = TypeCast.ToInt(dr["organisationId"]);
            dynamicText = TypeCast.ToString(dr["DynamicText"]);
            userName = TypeCast.ToString(dr["userName"]);
            dateCreated = TypeCast.ToDateTime(dr["dateCreated"]);
            contactId = TypeCast.ToIntOrDBNull(dr["contactId"]);
            Category = TypeCast.ToString(dr["Category"]);

            for (int i = 17; i < dr.FieldCount; i++)
            {
                if (dr.GetName(i) != "path")
                    ContactData.Add(new TableColumnWithValue(dr.GetName(i), dr.GetDataTypeName(i), 0, dr[i]));
                else
                    path = TypeCast.ToString(dr["path"]);
               
            }

        }
    }
}
