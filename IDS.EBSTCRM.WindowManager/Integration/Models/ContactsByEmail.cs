using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IDS.EBSTCRM.WindowManager.Integration.Models
{
    public class ContactsByEmailResponse : Response
    {
        public string[] ContactByEmailList { get; set; }
    }

    [Serializable]
    public class EmailContact
    {
        public int ContactId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }

        public EmailContact()
        {

        }

        public EmailContact(int contactId, string name, string email)
        {
            this.ContactId = contactId;
            this.Name = name;
            this.Email = email;
        }
    }

    //[Serializable]
    //public class ContactResponse
    //{
    //    public bool Status { get; set; }
    //    public List<string> Message { get; set; }

    //    public List<ContactsStructured> ContactsList { get; set; }
    //    public ContactResponse()
    //    {
    //        Status = true;
    //        Message = new List<string>();
    //        ContactsList = new List<ContactsStructured>();
    //    }
    //}
}