    using System;
using System.Collections.Generic;
using System.Text;

namespace IDS.EBSTCRM.Base
{
    public class SQLLabelDataItem
    {
        public string Name { get; set; }
        public object Value { get; set; }
        public string DataType {get;set;}

        public string ValueFormatted
        {
            get
            {
                switch (DataType.ToLower())
                {
                    case "int":
                        return TypeCast.ToDecimal(Value).ToString("#,##0");

                    case "float":
                        return TypeCast.ToDecimal(Value).ToString("#,##0.00");

                    case "bit":
                        return TypeCast.ToBool(Value) == true ? "X" : "";

                    case "datetime":
                        DateTime? dt = TypeCast.ToDateTimeLoose(Value);
                        if (dt != null)
                            return dt.Value.ToString("dd-MM-yyyy HH:mm").Replace(" 00:00","");
                        else
                            return "";

                    default:
                        string v = TypeCast.ToString(Value);

                        if (v.StartsWith("[@") && v.IndexOf("]") > 2)
                        {
                            return v.Substring(v.IndexOf("]")+1);
                        }
                        else
                            return v;
                }
                
            }
        }

        public string ValueFormatting
        {
            get
            {
                string v = TypeCast.ToString(Value);

                if (v.StartsWith("[@") && v.IndexOf("]")>2)
                {
                    return v.Substring(2,v.IndexOf("]")-2);
                }
                else
                    return "";
            }

        }

        public string ValueAlignment
        {
            get
            {
                switch (DataType.ToLower())
                {
                    case "float":
                    case "int":
                        return "right";

                    default:
                        return "left";
                }
            }
        }

        public SQLLabelDataItem(string name, object value, string dataType)
        {
            this.Name = name;
            this.Value = value;
            this.DataType = dataType;
        }


    }
}
