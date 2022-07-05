using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IDS.EBSTCRM.WindowManager.Integration.Models {

    [Serializable]
    public class SysCompany {
        public CompanyDetail CompanyDetail { get; set; }
    }

    [Serializable]
    public class CompanyDetail {
        public int CompanyId { get; set; }
        public string CompanyName { get; set; }
        public bool IsPOT { get; set; }
        public string CVR { get; set; }
        public string PNR { get; set; }
        public int NNEID { get; set; }
        public string Country { get; set; }
        /// <summary>
        /// DDMMYYYY or MMDDYYYY or YYYYMMDD
        /// </summary>
        public string DateFormat { get; set; }
        /// <summary>
        /// Dot or Comma
        /// </summary>
        public string DecimalSeparator { get; set; }
        public List<SysField> Fields { get; set; }
        public ContactBase Contact { get; set; }
    }
}