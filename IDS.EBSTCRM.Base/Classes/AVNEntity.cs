using System.Xml.Serialization;
using System.Collections.Generic;
using System;

namespace IDS.EBSTCRM.Base.Classes {

    [XmlRoot("AVN")]
    [Serializable()]
    public class AVNEntities {

        [XmlElement("Columns")]
        public Columns Columns { get; set; }

        [XmlElement("Rows")]
        public List<Entity> Rows { get; set; }
        public AVNEntities() {
            Columns = new Columns();
            Rows = new List<Entity>();
        }
    }

    [Serializable()]
    public class Columns {

        [XmlElement("Field")]
        public List<_AVNField> Fields { get; set; }
        public Columns() {
            Fields = new List<_AVNField>();
        }
    }

    [Serializable()]
    public class _AVNField {

        [XmlIgnoreAttribute]
        public string Name { get; set; }

        [XmlElement("Name")]
        public string FriendlyName { get; set; }

        [XmlElement("Datatype")]
        public string Datatype { get; set; }
    }

    [Serializable()]
    public class Entity {

        [XmlElement("Value")]
        public List<string> Data { get; set; }
        public Entity() {
            Data = new List<string>();
        }
    }
}