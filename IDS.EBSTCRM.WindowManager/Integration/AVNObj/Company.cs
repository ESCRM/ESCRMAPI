using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using IDS.EBSTCRM.Base;

namespace IDS.EBSTCRM.WindowManager.Integration
{
    [Serializable]
    public class Company
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string CVR { get; set; }
        public string PNR { get; set; }
        public string Address { get; set; }
        public string Zipcode { get; set; }
        public string City { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }

        public Company()
        {

        }

        public Company(ref SqlDataReader dr)
        {
            Id =TypeCast.ToInt(dr["CompanyId"]);
            Name = TypeCast.ToString(dr["Name"]);
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