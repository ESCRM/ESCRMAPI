using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;

namespace IDS.EBSTCRM.Base
{
    /// <summary>
    /// List of users paired with CaseNumbers.
    /// Users within this list, does not have access to use the CaseNumber in question
    /// </summary>
    public class CaseNumberUser
    {
        public string UserId;
        public string Firstname;
        public string Lastname;
        public string Username;
        public bool IsUserExcluded;

        /// <summary>
        /// Constructors a new CaseNumber using a DataReader
        /// </summary>
        /// <param name="dr"></param>
        public CaseNumberUser(ref SqlDataReader dr)
        {
            UserId = TypeCast.ToString(dr["UserId"]);
            Firstname = TypeCast.ToString(dr["Firstname"]);
            Lastname = TypeCast.ToString(dr["Lastname"]);
            Username = TypeCast.ToString(dr["Username"]);
            IsUserExcluded = TypeCast.ToInt(dr["IsUserExcluded"])>0;
        }
    }
}
