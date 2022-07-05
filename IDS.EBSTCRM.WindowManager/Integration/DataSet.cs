using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IDS.EBSTCRM.WindowManager.Integration
{
    [Serializable]
    public enum DataType
    {
        Any,
        String,
        DateTime,
        Integer,
        Float,
        Boolean
    }

    [Serializable]
    public class DataSet
    {
        public string UserId { get; set; }
        
        public Row[] Rows { get; set; }
    }

    [Serializable()]
    public class Row
    {
        public PropertyValue[] Columns { get; set; }
    }

    [Serializable]
    public class PropertyValue
    {
        public string Name { get; set; }
        public DataType DataType { get; set; }
        public string Value { get; set; }

        public PropertyValue()
        {
        }

        public PropertyValue(string name, string dataType, object value)
        {
            this.Name = name.Replace("@@unify:","@");
            
            switch (dataType)
            {
                case "bit":
                    DataType = Integration.DataType.Boolean;
                    Value = value == DBNull.Value ? "{NULL}" : IDS.EBSTCRM.Base.TypeCast.ToBool(value) == true ? "TRUE" : "FALSE";
                    break;

                case "int":
                    DataType = Integration.DataType.Integer;
                    Value = value == DBNull.Value ? "{NULL}" : IDS.EBSTCRM.Base.TypeCast.ToInt(value).ToString("#");
                    break;

                case "float":
                    DataType = Integration.DataType.Float;
                    Value = value == DBNull.Value ? "{NULL}" : IDS.EBSTCRM.Base.TypeCast.ToDecimal(value).ToString("#,##0.00");
                    break;

                case "varchar":
                case "nvarchar":
                    DataType = Integration.DataType.String;
                    Value = value == DBNull.Value ? "{NULL}" : IDS.EBSTCRM.Base.TypeCast.ToString(value);
                    break;

                case "datetime":
                    DataType = Integration.DataType.DateTime;
                    Value = value == DBNull.Value ? "{NULL}" : IDS.EBSTCRM.Base.TypeCast.ToDateTime(value).ToString("dd-MM-yyyy HH:mm:ss");
                    break;

                default:
                    DataType = Integration.DataType.Any;
                    Value = value == DBNull.Value ? "{NULL}" : IDS.EBSTCRM.Base.TypeCast.ToString(value);
                    break;
            }

        }
    }
}