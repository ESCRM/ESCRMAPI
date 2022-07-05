using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IDS.EBSTCRM.WindowManager.Integration.Models
{
    public class ContactAgreementResponse : Response
    {
        public int AgreementContactId { get; set; }
        public string[] AgreementContactList { get; set; }
    }
}