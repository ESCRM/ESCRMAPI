using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IDS.EBSTCRM.Base.Classes {
    public class DataClassification {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ExportMask { get; set; }
        public bool LogAccess { get; set; }
        public bool EnforceAnonymization { get; set; }
        public bool LockInGUI { get; set; }
    }
}

/*
Id	        int
Name	    varchar
Description	varchar
ExportMask	varchar
LogAccess	bit
EnforceAnonymization	bit
LockInGUI	bit
*/