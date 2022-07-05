using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace IDS.EBSTCRM.WindowManager.Integration
{
    [Serializable]
    public class AVNField
    {
        #region Properties

        [XmlIgnore]
        public int Id { get; set; }

        public string Name { get; set; }
        public string DataType { get; set; }
        public bool IsRequired { get; set; }
        public string Value { get; set; }

        public string[] ValueRestrictedList { get; set; }

        #endregion

        #region Constructors

        public AVNField()
        {

        }

        public AVNField(IDS.EBSTCRM.Base.AVNField f)
        {
            this.Id = f.Id;
            this.Name = f.DatabaseColumn;
            this.DataType = f.FieldType;
            this.IsRequired = f.RequiredState == 1;
            this.ValueRestrictedList = f.CustomFieldValues != null ? f.CustomFieldValues.ToArray() : null;
        }

        public AVNField(IDS.EBSTCRM.Base.AVNFieldWithValue f)
        {
            this.Id = f.Id;
            this.Name = f.DatabaseColumn;
            this.DataType = f.FieldType;
            this.IsRequired = f.RequiredState == 1;
            this.Value = f.FormattedValue;
            this.ValueRestrictedList = f.CustomFieldValues != null ? f.CustomFieldValues.ToArray() : null;
        }

        #endregion
    }
}