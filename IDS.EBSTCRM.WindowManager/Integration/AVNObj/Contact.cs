using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using IDS.EBSTCRM.Base;

namespace IDS.EBSTCRM.WindowManager.Integration
{
    [Serializable]
    public class Contact
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string CompanyName { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string CVR { get; set; }
        public string PNR { get; set; }
        public string Address { get; set; }
        public string Zipcode { get; set; }
        public string City { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }

        public Contact()
        {

        }

        public Contact(ref SqlDataReader dr)
        {
            Id = TypeCast.ToInt(dr["ContactId"]);
            CompanyId = TypeCast.ToInt(dr["CompanyId"]);
            CompanyName = TypeCast.ToString(dr["CompanyName"]);
            Firstname = TypeCast.ToString(dr["Firstname"]);
            Lastname = TypeCast.ToString(dr["Lastname"]);
            CVR = TypeCast.ToString(dr["CVR"]);
            PNR = TypeCast.ToString(dr["PNR"]);
            Address = TypeCast.ToString(dr["Address"]);
            Zipcode = TypeCast.ToString(dr["Zipcode"]);
            City = TypeCast.ToString(dr["City"]);
            Phone = TypeCast.ToString(dr["Phone"]);
            Email = TypeCast.ToString(dr["Email"]);
        }
    }
}