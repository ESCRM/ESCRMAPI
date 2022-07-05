using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;

namespace IDS.EBSTCRM.Base
{
    /// <summary>
    /// Evaluation information
    /// Used to display user evaluation information, 
    /// when a SMV/POT contact is sent to evaluation
    /// </summary>
    public class EvaluationInfo
    {
        public int Id;
        public DateTime SentDate;
        public string SentById;
        public string Firstname;
        public string Lastname;
        public string Username;
        public string Email;
        public string Initials;

        public EvaluationInfo()
        {

        }

        public EvaluationInfo(ref SqlDataReader dr)
        {
            Id = TypeCast.ToInt(dr["Id"]);
            SentDate = TypeCast.ToDateTime(dr["SentDate"]);
            Firstname = TypeCast.ToString(dr["Firstname"]);
            Lastname = TypeCast.ToString(dr["Lastname"]);
            Username = TypeCast.ToString(dr["Username"]);
            Email = TypeCast.ToString(dr["Email"]);
            Initials = TypeCast.ToString(dr["Initials"]);
        }
    }
}
