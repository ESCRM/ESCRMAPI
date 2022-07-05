using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace IDS.EBSTCRM.Base.Classes {
    public class AgreementContact {
        public int Id { get; set; }
        public int AgreementTypeId { get; set; }
        public string AgreementName { get; set; }
        public int ContactId { get; set; }
        public string ContactName { get; set; }

        public string FileName { get; set; }
        public byte[] Binary { get; set; }

        public string AgreementXml { get; set; }
        public string Comment { get; set; }
        public DateTime ExpiryDate { get; set; }
        public DateTime FollowUp { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool Active { get; set; }
        public string Type { get; set; }
        public bool RequireDocumentation { get; set; }
        public string Description { get; set; }

        public string CreatedById { get; set; }
        public string CreatedBy { get; set; }

        public AgreementContent Agreement { get; set; }
        public AgreementContent GetAgreement() {
            AgreementContent content = null;
            try {
                if (!string.IsNullOrEmpty(AgreementXml)) {
                    XmlSerializer xmlSerializer = null;
                    var xmldoc = new XmlDocument();
                    xmldoc.LoadXml(AgreementXml);
                    if (xmldoc.DocumentElement != null) {
                        var xmlNodeReader = new XmlNodeReader(xmldoc.DocumentElement);
                        xmlSerializer = new XmlSerializer(typeof(AgreementContent));
                        content = (AgreementContent)xmlSerializer.Deserialize(xmlNodeReader);
                    }
                }
            } catch {
                return content;
            }
            return content;
        }
        public string GetAgreementXml(AgreementContent content) {
            try {
                var stringwriter = new System.IO.StringWriter();
                var serializer = new XmlSerializer(typeof(AgreementContent));
                serializer.Serialize(stringwriter, content);
                return stringwriter.ToString();
            } catch {
                return "";
            }
        }
    }

    [Serializable]
    [XmlRoot("Agreement")]
    public class AgreementContent {

        [XmlElement("Email")]
        public bool Email { get; set; }

        [XmlElement("To")]
        public string To { get; set; }

        [XmlElement("From")]
        public string From { get; set; }

        [XmlElement("SentOn")]
        public string SentOn { get; set; }

        [XmlElement("Subject")]
        public string Subject { get; set; }

        [XmlElement("Body")]
        public string Body { get; set; }

        public AgreementContent() {
            Email = false;
        }
    }
}
