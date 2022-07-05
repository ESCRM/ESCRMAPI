using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IDS.EBSTCRM.Base.Classes {
    public class Field {
        public int RowID { get; set; }
        public string ColumnName { get; set; }
        public string DataType { get; set; }
        public string DataLength { get; set; }
    }
}
