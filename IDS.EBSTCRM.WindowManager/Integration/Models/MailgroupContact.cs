using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IDS.EBSTCRM.WindowManager.Integration.Models
{
    public class MailgroupContactResponse : Response
    {
        public int MailgroupContactId { get; set; }
        public int MailgroupId { get; set; }
        public string[] MailgroupContactList { get; set; }
    }
}