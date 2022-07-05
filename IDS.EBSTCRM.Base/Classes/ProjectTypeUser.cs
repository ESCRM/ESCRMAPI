using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;

namespace IDS.EBSTCRM.Base
{
    /// <summary>
    /// Secondary ProjectTypes paired with user.
    /// When the user is paired with the project type,
    /// the user is NOT allowed to register data on any of the Case Numbers Nested within.
    /// </summary>
    public class ProjectTypeUser
    {
        public string UserId;
        public string Firstname;
        public string Lastname;
        public string Username;
        public bool IsUserExcluded;

        public ProjectTypeUser(ref SqlDataReader dr)
        {
            UserId = TypeCast.ToString(dr["UserId"]);
            Firstname = TypeCast.ToString(dr["Firstname"]);
            Lastname = TypeCast.ToString(dr["Lastname"]);
            Username = TypeCast.ToString(dr["Username"]);
            IsUserExcluded = TypeCast.ToInt(dr["IsUserExcluded"])>0;
        }
    }
}
