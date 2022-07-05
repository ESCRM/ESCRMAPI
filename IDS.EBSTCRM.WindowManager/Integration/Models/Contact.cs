using System;
using System.Collections.Generic;

namespace IDS.EBSTCRM.WindowManager.Integration.Models {

    [Serializable]
    public class SysContact {
        public ContactDetail ContactDetail { get; set; }
        public SysContact() {
            ContactDetail = new ContactDetail();
        }
    }

    [Serializable]
    public class ContactDetail : ContactBase {
        /// <summary>
        /// If ID is present its an update
        /// </summary>
        public int ContactId { get; set; }
        public int CompanyId { get; set; }
        /// <summary>
        /// DDMMYYYY or MMDDYYYY or YYYYMMDD
        /// </summary>
        public string DateFormat { get; set; }
        /// <summary>
        /// Dot or Comma
        /// </summary>
        public string DecimalSeparator { get; set; }
    }

    [Serializable]
    public class ContactBase {
        public bool IsPOT { get; set; }
        //public string Firstname { get; set; }
        //public string Lastname { get; set; }
        //public string Email { get; set; }
        public List<SysField> Fields { get; set; }
        public ContactBase() {
            Fields = new List<SysField>();
        }
    }
}