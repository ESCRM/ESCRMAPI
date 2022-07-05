using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Collections;

namespace IDS.EBSTCRM.Base
{
    /// <summary>
    /// Early Warning Note
    /// </summary>
    public class EarlyWarningNote
    {

        public int Id;
        public int CompanyId;
        public int ContactId;
        public int VolunteerId;
        public string Title;
        public string Body;
        public DateTime DateCreated;
        public string CreatedBy;
        public object LastUpdated;
        public string LastUpdatedBy;
        public object DateDeleted;
        public string DeletedBy;

        public string UserFirstname;
        public string UserLastname;
        public string UserEmail;
        public string Username;


        public EarlyWarningNote()
        {

        }

        public EarlyWarningNote(ref SqlDataReader dr)
        {
            Id = TypeCast.ToInt(dr["Id"]);
            CompanyId = TypeCast.ToInt(dr["CompanyId"]);
            ContactId = TypeCast.ToInt(dr["ContactId"]);
            VolunteerId = TypeCast.ToInt(dr["VolunteerId"]);
            Title = TypeCast.ToString(dr["Title"]);
            Body = TypeCast.ToString(dr["Body"]);
            DateCreated = TypeCast.ToDateTime(dr["DateCreated"]);
            CreatedBy = TypeCast.ToString(dr["CreatedBy"]);
            LastUpdated = TypeCast.ToDateTimeOrNull(dr["LastUpdated"]);
            LastUpdatedBy = TypeCast.ToString(dr["LastUpdatedBy"]);
            DateDeleted = TypeCast.ToDateTimeOrNull(dr["DateDeleted"]);
            DeletedBy = TypeCast.ToString(dr["DeletedBy"]);

            UserFirstname = TypeCast.ToString(dr["userFirstname"]);
            UserLastname = TypeCast.ToString(dr["userLastname"]);
            UserEmail = TypeCast.ToString(dr["userEmail"]);
            Username = TypeCast.ToString(dr["Username"]);
        }
    }
}
