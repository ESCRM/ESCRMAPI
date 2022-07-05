using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IDS.EBSTCRM.WindowManager.Integration
{
    [Serializable]
    public class CompanyQuery
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public string CVR { get; set; }
        public string PNR { get; set; }
        public string Address { get; set; }
        public string Zipcode { get; set; }
        public string City { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }

        public CompanyQuery()
        {

        }
    }
}