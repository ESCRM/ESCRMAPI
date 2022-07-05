using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IDS.EBSTCRM.Base.Classes {
    public class CompanyContactByPositiveField {

        // Company fields
        public int CompanyId { get; set; }
        public string CompanyName { get; set; }
        public string CompanyCVRNumber { get; set; }
        public string CompanyKommune { get; set; }
        public string PositiveFieldValue { get; set; }

        // Contact fields
        public int ContactId { get; set; }
        public string ContactFirstname { get; set; }
        public string ContactLastname { get; set; }
        public string ContactEmail { get; set; }

        public bool Flag { get; set; }
    }
}
