using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.Serialization;

namespace IDS.EBSTCRM.WindowManager.Integration.Models {

    [Serializable]
    [XmlRoot("parameters")]
    public class SearchCompanyRequest {

        public QueryParameters Query { get; set; }

        public QueryAVN AVN { get; set; }

        public bool IncludeLocalFields { get; set; }

        public bool IncludeOwnNotes { get; set; }

        public bool IncludeOtherNotes { get; set; }

        public bool IncludeMeetings { get; set; }

        public SearchCompanyRequest() {
            Query = new QueryParameters();
            AVN = new QueryAVN();
        }
    }
    public class QueryParameters {
        [XmlElement("CompanyID")]
        public int CompanyID { get; set; }

        [XmlElement("ContactID")]
        public int ContactID { get; set; }

        [XmlElement("CompanyName")]
        public string CompanyName { get; set; }

        [XmlElement("Firstname")]
        public string Firstname { get; set; }

        [XmlElement("Lastname")]
        public string Lastname { get; set; }

        [XmlElement("CVR")]
        public string CVR { get; set; }

        [XmlElement("PNR")]
        public int? PNR { get; set; }

        [XmlElement("Address")]
        public string Address { get; set; }

        [XmlElement("Zipcode")]
        public string Zipcode { get; set; }

        [XmlElement("City")]
        public string City { get; set; }

        [XmlElement("Phone")]
        public string Phone { get; set; }

        [XmlElement("Email")]
        public string Email { get; set; }
    }

    public class QueryAVN {
        public Type Include { get; set; }
        public List<AVNIds> List { get; set; }
        public QueryAVN() {
            List = new List<AVNIds>();
        }
    }
    public class AVNIds {
        [XmlElement("Id")]
        public int Id { get; set; }
    }

    public enum Type {
        All,
        Own
    }
}