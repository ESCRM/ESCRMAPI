using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IDS.EBSTCRM.WindowManager.Integration.Models {

    [Serializable]
    public class SysDynamicFields : Response {
        public List<SysField> Company { get; set; }
        public List<SysField> Contact { get; set; }
        public SysDynamicFields() {
            Company = new List<SysField>();
            Contact = new List<SysField>();
        }
    }
}