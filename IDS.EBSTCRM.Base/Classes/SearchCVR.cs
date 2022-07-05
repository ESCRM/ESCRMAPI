using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IDS.EBSTCRM.Base.Classes {
    /// <summary>
    /// Search CVR object
    /// </summary>
    public class SearchCVR {
        public int ID { get; set; }
        public string CompanyName { get; set; }
        public string CVR { get; set; }
        public string PNummer { get; set; }
        public string Street { get; set; }
        public string Zipcode { get; set; }
        public string City { get; set; }
        public string Municipality { get; set; }
        public string Region { get; set; }
    }
}
