using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;

namespace IDS.EBSTCRM.Base
{
    /// <summary>
    /// Mentors
    /// OBSOLETE
    /// </summary>
    public class Mentor : EventLogBase
    {
        public int Id;
        public int OrganisationId;
        public int ContactId;
        public int CompanyId;
        public string Firstname;
        public string Lastname;

        public Mentor()
        {

        }

        public Mentor(ref SqlDataReader dr)
        {
            Id = TypeCast.ToInt(dr["id"]);
            OrganisationId = TypeCast.ToInt(dr["id"]);
            ContactId = TypeCast.ToInt(dr["contactId"]);
            CompanyId = TypeCast.ToInt(dr["companyId"]);
            Firstname = TypeCast.ToString(dr["firstname"]);
            Lastname = TypeCast.ToString(dr["Lastname"]);
        }
    }
}
