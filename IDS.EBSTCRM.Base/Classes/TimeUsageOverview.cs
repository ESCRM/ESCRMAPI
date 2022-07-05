using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;

namespace IDS.EBSTCRM.Base
{
    /// <summary>
    /// Overview for a full year view of all Hour registratrions made by all users
    /// </summary>
    public class TimeUsageOverview
    {
        public int Id;
        public string UserId;
        public string Initials;
        public int WeekNumber;
        public string Status;
        public decimal Total;
        public string Username;
        public string Firstname;
        public string Lastname;
        public int Year;
        public object DeletedDate;

        public TimeUsageOverview()
        {

        }

        public TimeUsageOverview(ref SqlDataReader dr)
        {
            Id = TypeCast.ToInt(dr["id"]);
            UserId = TypeCast.ToString(dr["userID"]);
            Initials = TypeCast.ToString(dr["Initials"]);
            WeekNumber = TypeCast.ToInt(dr["WeekNumber"]);
            Status = TypeCast.ToString(dr["status"]);
            Total = TypeCast.ToDecimal(dr["Total"]);
            Username = TypeCast.ToString(dr["Username"]);
            Firstname = TypeCast.ToString(dr["Firstname"]);
            Lastname = TypeCast.ToString(dr["Lastname"]);
            Year = TypeCast.ToInt(dr["Year"]);

            DeletedDate = TypeCast.ToDateTimeOrNull(dr["DeletedDate"]);
        }
    }
}
