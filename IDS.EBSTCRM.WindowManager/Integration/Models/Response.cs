using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IDS.EBSTCRM.WindowManager.Integration.Models {

    [Serializable]
    public class Response {
        public bool Status { get; set; }
        public List<string> Message { get; set; }
        public string CustomDataField_1 { get; set; }
        public Response() {
            Status = true;
            Message = new List<string>();
        }
    }
}