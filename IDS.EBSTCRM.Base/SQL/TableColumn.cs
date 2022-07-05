using System;
using System.Collections.Generic;
using System.Text;

namespace IDS.EBSTCRM.Base {
    /// <summary>
    /// Dynamic Field Table Column (Contains SQL Data Type, Name and Length)
    /// </summary>
    public class TableColumn {
        #region Properties / Declarations

        public string Name = "";
        public string DataType = "";
        public int Length = 0;

        #endregion

        #region Constructors

        public TableColumn(string name, string dataType, int length) {
            Name = name;
            DataType = dataType;
            Length = length;
        }

        #endregion

    }

    /// <summary>
    /// Dynamic Field for SMV/POT Contacts and Companies
    /// This item is paired with the DynamicField Class along with the actual stored value from the SQL Server
    /// </summary>
    public class TableColumnWithValue : TableColumn {
        #region Declarations / Properties

        public int Id;
        public object Value;
        public DynamicField DynamicField;
        public int ClassificationId;
        public int AnonymizationId;

        #endregion

        #region Constructors

        /// <summary>
        /// Construct a table column with value, and match it to a DynamicField.
        /// This for displaying a field within a SMV/POT Contact/Company edit window.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="dataType"></param>
        /// <param name="length"></param>
        /// <param name="value"></param>
        /// <param name="Fields">All dynamic fields for the current users organisation</param>
        public TableColumnWithValue(string name, string dataType, int length, object value, List<DynamicField> Fields)
            : base(name, dataType, length) {
            Value = value;

            MatchDynamicField(Fields);

        }

        public TableColumnWithValue(string name, string dataType, int length, object value)
            : base(name, dataType, length) {
            if (dataType == "bit") {
                Value = IDS.EBSTCRM.Base.TypeCast.ToBool(value);
            } else
                Value = value;
        }

        public TableColumnWithValue(string name, string dataType, int length, object value, int classificationId, int anonymizationId)
            : base(name, dataType, length) {
            if (dataType == "bit") {
                Value = IDS.EBSTCRM.Base.TypeCast.ToBool(value);
            } else {
                Value = value;
            }

            ClassificationId = classificationId;
            AnonymizationId = anonymizationId;
        }

        #endregion

        #region Methods

        /// <summary>
        /// This function pairs the Table Column with a dynamic field,
        /// extending the data with Field Properties;
        /// 
        /// X
        /// Y
        /// Width
        /// Height
        /// Description
        /// FieldType (Listview, Textbox, Email, etc.
        /// etc.
        /// 
        /// </summary>
        /// <param name="Fields"></param>
        public void MatchDynamicField(List<DynamicField> Fields) {
            //Exit if field already Matched
            if (DynamicField != null) return;

            //Loop thru all fields for the users organisation
            foreach (DynamicField f in Fields) {
                //Match a field NOT being a link to another field
                //(Note: A link to another field is typically, when a listview should contain data from a Company, i.e. Company Name)
                if (f.FieldType != "externaldatalink") {
                    MatchField(f);
                }
            }

            //If not field is located re-do match allowing identifying External Data links
            if (DynamicField == null) {
                foreach (DynamicField f in Fields) {
                    MatchField(f);
                }
            }
        }

        /// <summary>
        /// Matches a dynamic field to a table column with value
        /// </summary>
        /// <param name="f">Dynamic field</param>
        private void MatchField(DynamicField f) {
            string[] tmp = f.DatabaseTable.Split('_');
            string ds = "";
            if (f.DataSource != "" && f.DataSource != null && f.DataSource.IndexOf("_") > 0) {
                string[] dss = f.DataSource.Split('_');
                ds = dss[2];
            }

            // Extract the correct table and column name based on type and original organisation Id
            // - Or current Organisation Id, if the field is not inherited from another Organisation
            // (Note: A correct match is defined by matching the entire base.Name of the class, i.e.: 
            // z_contacts_101_Markedsinteresse_101
            // 
            // Parts: z contacts 101 Markedsinteresse 101
            //
            // [0] = Dynamic field indicator
            // [1] = Table source (Contacts/Companies)
            // [2] = Table organisation Id
            // [3] = Column name
            // [4] = Column organisation Id
            if (f.DatabaseTable + "_" + f.DatabaseColumn + "_" + tmp[tmp.Length - 1] == base.Name) {
                DynamicField = f;
            } else if (f.DataSource + "_" + (ds == "" ? tmp[tmp.Length - 1] : ds) == base.Name) {
                DynamicField = f;
            }
        }


        /// <summary>
        /// Returns the "Value" property as a formatted value
        /// This is (among others) used with the Report Generator
        /// </summary>
        public string ValueFormatted {
            get {
                switch (base.DataType) {
                    case "bit":
                        return TypeCast.ToBool(Value) == false ? "Nej" : "Ja";

                    case "int":
                        return TypeCast.ToInt(Value).ToString("#");

                    case "float":
                        return TypeCast.ToDecimal(Value).ToString("#,##0.00");

                    case "varchar":
                    case "nvarchar":
                        return TypeCast.ToString(Value);

                    case "datetime":
                        if (Value == null)
                            return "";
                        else {
                            object v = TypeCast.ToDateTimeOrNull(Value);
                            if (v != null)
                                return ((DateTime)v).ToString("dd-MM-yyyy");
                            else
                                return "";
                        }
                }

                return TypeCast.ToString(Value);
            }
        }

        #endregion

    }

    /// <summary>
    /// Renamed/Modified Tablecolumn from Field Design
    /// </summary>
    public class TableColumnModified : TableColumn {
        #region Declarations / Properties

        public object Value;
        public string OldName;

        #endregion

        #region Constructors

        public TableColumnModified(string name, string dataType, int length, object value, string oldName)
            : base(name, dataType, length) {
            Value = value;
            OldName = oldName;
        }

        #endregion
    }

}
