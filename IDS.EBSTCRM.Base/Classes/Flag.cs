using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace IDS.EBSTCRM.Base.Classes {
    public class FlagAndDataObject {
        public List<FlagType> flagtype { get; set; }
        public List<DataObject> dataobject { get; set; }
        public FlagAndDataObject() {
            flagtype = new List<FlagType>();
            dataobject = new List<DataObject>();
        }
    }

    public class Flag {
        public string Id { get; set; }
        public int Type { get; set; }
        public DateTime Date { get; set; }
        public int ObjectType { get; set; }
        public int ObjectIdInt { get; set; }
        public string ObjectIdGuid { get; set; }
        public int ContactId { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public string ApprovedBy { get; set; }
        public string FlagAttributes { get; set; }
        public int ReasonType { get; set; }
        public string Reason { get; set; }
        public bool Delete { get; set; }

        public int ImportantFieldId { get; set; }

        // AVN variables
        public int AVNId { get; set; }
        public int DateAddInterval { get; set; }
        public string DateAddPart { get; set; }

        // List variables
        public string ReasonText { get; set; }
        public string OrganisationName { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }

        public Flag() {
            Delete = false;
            DateAddInterval = 0;
            DateAddPart = "";
        }
    }

    public class FlagType {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string DefaultValue { get; set; }
        public Dropdown DropdownDefaultValue {
            get {
                if (!string.IsNullOrEmpty(DefaultValue)) {
                    var xmldoc = new XmlDocument();
                    xmldoc.LoadXml(DefaultValue);
                    if (xmldoc.DocumentElement != null) {
                        var xmlNodeReader = new XmlNodeReader(xmldoc.DocumentElement);
                        var xmlSerializer = new XmlSerializer(typeof(Dropdown));
                        return (Dropdown)xmlSerializer.Deserialize(xmlNodeReader);
                    }
                }
                return (new Dropdown());
            }
        }
    }

    [XmlRoot("Dropdown")]
    public class Dropdown {
        [XmlElement("Line")]
        public List<DropdownValues> Values { get; set; }
        public Dropdown() {
            Values = new List<DropdownValues>();
        }
    }

    public class DropdownValues {
        [XmlElement("Id")]
        public int Id { get; set; }
        [XmlElement("Value")]
        public string Value { get; set; }
    }

    public class DataObject {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ValidFlagTypes { get; set; }
        public string ContactValidFlagTypes { get; set; }
        public string FriendlyName { get; set; }
    }

    public enum FlagEnum {
        Keepuntill = 1,
        NoExpiry = 2,
        Anonymize = 3,
        Lock = 4,
        Sensitive = 5,
        Delete = 6,
    }
}