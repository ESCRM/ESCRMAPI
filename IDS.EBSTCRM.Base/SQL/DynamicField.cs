using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Linq;

namespace IDS.EBSTCRM.Base {
    /// <summary>
    /// Dynamic Fields containing data, position datatype ect.
    /// For all SMV/POT Contact and Company Fields.
    /// </summary>
    public class DynamicField {
        public int Id;
        public int OwnerId;
        public int OrganisationId;
        public int BaseOrganisationId;
        public string DatabaseTable;
        public string DatabaseColumn;
        public int X1;
        public int Y1;
        public int Width1;
        public int Height1;
        public object X2;
        public object Y2;
        public object Width2;
        public object Height2;
        public string FieldType;
        public int RequiredState;
        public int SearchAble;
        public int UseInReports;
        public string ViewState;
        public int TabIndex1 = 0;
        public int TabIndex2 = 0;
        public object ShowInListview = null;
        public int ListviewIndex = 0;
        public string DataLink = "";
        public string DataSource = "";
        public string FollowAny = "";
        public string AlternateText = "";
        public bool NoInherit_Obsolete = false;

        public string InnerCSS = "";
        public string OuterCSS = "";
        public string DefaultFieldValue = "";

        public string OriginalFieldType = "";

        public ShareMethod SharedWith;
        public List<int> SharedWithOrganisations;

        public List<string> CustomFieldValues = new List<string>();

        public int DataClassificationId = 0;
        public int AnonymizationId = 0;
        public bool LogAccess = false;

        public string PositiveFieldValue { get; set; }

        public int ReasonType { get; set; }
        public string Reason { get; set; }

        public enum DynamicTableName {
            z_companies = 1,
            z_contacts = 2
        }

        public enum ShareMethod {
            NotShared = 0,
            FullShared = 1,
            PartialShared = 2
        }

        public bool NoInherit(int OrganisationId) {
            if (this.BaseOrganisationId != OrganisationId) {
                if (SharedWith == ShareMethod.FullShared)
                    return false;
                else if (SharedWith == ShareMethod.NotShared)
                    return true;
                else {
                    if (SharedWithOrganisations != null)
                        return !SharedWithOrganisations.Contains(OrganisationId);
                    else
                        return true;
                }
            } else
                return false;
        }

        public DynamicField() {

        }

        /// OLD CONSTRUCTOR REMOVED
        //public DynamicField(Organisation o, string parseThis, string dbColumn)
        //{
        //    string[] items = parseThis.Split(',');

        //    if (items.Length == 1) return;

        //    this.DatabaseColumn=dbColumn;
        //    this.Id = TypeCast.ToInt(items[1]);

        //    if(items[14]!="null")
        //        this.DatabaseTable = items[14];
        //    else
        //        this.DatabaseTable = items[0] + o.Id;

        //    this.X1 = TypeCast.ToInt(items[2]);
        //    this.Y1 = TypeCast.ToInt(items[3]);
        //    this.Width1 = TypeCast.ToInt(items[4]);
        //    this.Height1 = TypeCast.ToInt(items[5]);

        //    this.X2 = TypeCast.ToInt(items[6]);
        //    this.Y2 = TypeCast.ToInt(items[7]);
        //    this.Width2 = TypeCast.ToInt(items[8]);
        //    this.Height2 = TypeCast.ToInt(items[9]);

        //    this.ViewState = items[10];
        //    if (this.ViewState != "SMV" && this.ViewState != "POT")
        //        this.ViewState = "";

        //    this.FieldType = items[11];

        //    this.OrganisationId = o.Id;
        //    this.TabIndex1 = TypeCast.ToInt(items[12]);
        //    this.TabIndex2 = TypeCast.ToInt(items[13]);

        //    this.ShowInListview = TypeCast.ToIntOrNull(items[15]);
        //    this.ListviewIndex = TypeCast.ToInt(items[16]);

        //    this.DataLink = TypeCast.ToString(items[17]);
        //    this.RequiredState = TypeCast.ToInt(items[18]);

        //    this.DataSource = TypeCast.ToString(items[19]);
        //    this.FollowAny = TypeCast.ToString(items[20]);

        //    this.OrganisationId = TypeCast.ToInt(items[21]);
        //    this.BaseOrganisationId = TypeCast.ToInt(items[22]);
        //    this.OwnerId = TypeCast.ToInt(items[23]);
        //    this.UseInReports = TypeCast.ToInt(items[24]);
        //    this.NoInherit = TypeCast.ToBool(items[25]);

        //    this.AlternateText = items[26].Replace("<comma>", ",");
        //    items[27] = items[27].Replace("<comma>", ",");
        //    CustomFieldValues.AddRange(items[27].Split('\n'));

        //    for (int i = 0; i < CustomFieldValues.Count - 1; i++)
        //    {
        //        CustomFieldValues[i] = CustomFieldValues[i].Replace("\r", "");
        //    }

        //}

        public DynamicField(int id,
                            int ownerId,
                            int organisationId,
                            int baseOrganisationId,
                            string databaseTable,
                            string databaseColumn,
                            int x1,
                            int y1,
                            int width1,
                            int height1,
                            object x2,
                            object y2,
                            object width2,
                            object height2,
                            string fieldType,
                            int requiredState,
                            int searchAble,
                            int useInReports,
                            string viewState,
                            int tabIndex1,
                            int tabIndex2,
                            object showInListview,
                            int listviewIndex,
                            string dataLink,
                            string dataSource,
                            string followAny,
                            string alternateText,
                            string originalFieldType,
                            string defaultFieldValue,
                            ShareMethod sharedWith) {
            Id = id;
            OwnerId = ownerId;
            OrganisationId = organisationId;
            BaseOrganisationId = baseOrganisationId;
            DatabaseTable = databaseTable;
            DatabaseColumn = databaseColumn;
            X1 = x1;
            Y1 = y1;
            Width1 = width1;
            Height1 = height1;
            X2 = x2;
            Y2 = y2;
            Width2 = width2;
            Height2 = height2;
            FieldType = fieldType;
            RequiredState = requiredState;
            SearchAble = searchAble;
            UseInReports = useInReports;
            ViewState = viewState;
            TabIndex1 = tabIndex1;
            TabIndex2 = tabIndex2;
            ShowInListview = showInListview;
            ListviewIndex = listviewIndex;
            DataLink = dataLink;
            DataSource = dataSource;
            FollowAny = followAny;
            AlternateText = alternateText;
            OriginalFieldType = originalFieldType;
            DefaultFieldValue = defaultFieldValue;
            SharedWith = sharedWith;

            if (this.ViewState != "SMV" && this.ViewState != "POT")
                this.ViewState = "";
        }

        public DynamicField(ref System.Data.SqlClient.SqlDataReader dr) {
            Id = TypeCast.ToInt(dr["id"]);
            OwnerId = TypeCast.ToInt(dr["OwnerId"]);
            OrganisationId = TypeCast.ToInt(dr["organisationId"]);
            BaseOrganisationId = TypeCast.ToInt(dr["BaseOrganisationId"]);
            DatabaseTable = TypeCast.ToString(dr["databaseTable"]);
            DatabaseColumn = TypeCast.ToString(dr["databaseColumn"]);
            X1 = TypeCast.ToInt(dr["x1"]);
            Y1 = TypeCast.ToInt(dr["y1"]);
            Width1 = TypeCast.ToInt(dr["width1"]);
            Height1 = TypeCast.ToInt(dr["height1"]);
            X2 = TypeCast.ToIntOrNull(dr["x2"]);
            Y2 = TypeCast.ToIntOrNull(dr["y2"]);
            Width2 = TypeCast.ToIntOrNull(dr["width2"]);
            Height2 = TypeCast.ToIntOrNull(dr["height2"]);
            FieldType = TypeCast.ToString(dr["fieldType"]);
            RequiredState = TypeCast.ToInt(dr["requiredState"]);
            SearchAble = TypeCast.ToInt(dr["searchAble"]);
            UseInReports = TypeCast.ToInt(dr["useInReports"]);
            ViewState = dr["viewState"] == DBNull.Value ? "" : TypeCast.ToString(dr["viewState"]);
            TabIndex1 = TypeCast.ToInt(dr["tabIndex1"]);
            TabIndex2 = TypeCast.ToInt(dr["tabIndex2"]);

            ShowInListview = TypeCast.ToInt(dr["ShowInListview"]);
            ListviewIndex = TypeCast.ToInt(dr["ListviewIndex"]);

            DataLink = TypeCast.ToString(dr["DataLink"]);

            DataSource = TypeCast.ToString(dr["DataSource"]);
            FollowAny = TypeCast.ToString(dr["FollowAny"]);
            AlternateText = TypeCast.ToString(dr["alternateText"]);
            NoInherit_Obsolete = TypeCast.ToBool(dr["noInheritance"]);
            SharedWith = (ShareMethod)TypeCast.ToInt(dr["SharedWith"]);


            if (SharedWith == ShareMethod.PartialShared) {
                if (SharedWithOrganisations == null)
                    SharedWithOrganisations = new List<int>();
                else
                    SharedWithOrganisations.Clear();

                string[] orgs = TypeCast.ToString(dr["SharedWithOrganisations"]).Split(',');
                foreach (string o in orgs) {
                    int org = TypeCast.ToInt(o);
                    if (org > 0 && !SharedWithOrganisations.Contains(org))
                        SharedWithOrganisations.Add(org);
                }
            }

            InnerCSS = TypeCast.ToString(dr["InnerCSS"]);
            OuterCSS = TypeCast.ToString(dr["OuterCSS"]);
            DefaultFieldValue = TypeCast.ToString(dr["DefaultFieldValue"]);

            OriginalFieldType = TypeCast.ToString(dr["OriginalFieldType"]);

            if (ColumnExists(dr, "DataClassificationId")) { DataClassificationId = TypeCast.ToInt(dr["DataClassificationId"]); }
            if (ColumnExists(dr, "AnonymizationId")) { AnonymizationId = TypeCast.ToInt(dr["AnonymizationId"]); }
            if (ColumnExists(dr, "LogAccess")) { LogAccess = TypeCast.ToBool(dr["LogAccess"]); }
            if (ColumnExists(dr, "PositiveFieldValue")) { PositiveFieldValue = TypeCast.ToString(dr["PositiveFieldValue"]); }
            if (ColumnExists(dr, "ReasonType")) { ReasonType = TypeCast.ToInt(dr["ReasonType"]); }
            if (ColumnExists(dr, "Reason")) { Reason = TypeCast.ToString(dr["Reason"]); }

            if (this.ViewState != "SMV" && this.ViewState != "POT")
                this.ViewState = "";
        }

        /// <summary>
        /// Checking if column exist in the datareader
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public bool ColumnExists(IDataReader reader, string columnName) {
            for (int i = 0; i < reader.FieldCount; i++) {
                if (reader.GetName(i).Equals(columnName, StringComparison.InvariantCultureIgnoreCase)) {
                    return true;
                }
            }
            return false;
        }

        public System.Web.UI.HtmlControls.HtmlGenericControl ToColumnHeader() {
            System.Web.UI.HtmlControls.HtmlGenericControl ch = new System.Web.UI.HtmlControls.HtmlGenericControl("I");
            ch.InnerText = this.DatabaseColumn;
            int w = TypeCast.ToInt(this.ShowInListview);
            ch.Attributes["width"] = (w < 20 ? 80 : w).ToString();
            ch.Attributes["datatype"] = getColumnDataType();
            ch.Attributes["mask"] = getMask();

            return ch;
        }

        private string getMask() {
            if (this.FieldType.ToLower() == "externaldatalink")
                return _getMask(this.OriginalFieldType);
            else
                return _getMask(this.FieldType);

            //switch (this.FieldType.ToLower())
            //{
            //    case "absinteger":
            //    case "absfloat":
            //    case "integer":
            //    case "float":
            //        return ",";

            //    case "externaldatalink":

            //}
            //return "";
        }

        private string _getMask(string fieldtype) {
            switch (fieldtype.ToLower()) {
                case "absinteger":
                case "absfloat":
                case "integer":
                case "float":
                    return ",";
            }
            return "";
        }

        private string getColumnDataType() {
            if (this.FieldType.ToLower() == "externaldatalink")
                return _getColumnDataType(this.OriginalFieldType);
            else
                return _getColumnDataType(this.FieldType);

            //switch (this.FieldType.ToLower())
            //{
            //    case "absinteger":
            //    case "absfloat":
            //    case "integer":
            //    case "float":
            //        return "numeric";

            //}
            //return "";
        }

        private string _getColumnDataType(string fieldtype) {
            switch (this.FieldType.ToLower()) {
                case "absinteger":
                case "absfloat":
                case "integer":
                case "float":
                    return "numeric";
            }
            return "";
        }

        public DynamicField(string[] dr) {
            Id = TypeCast.ToInt(dr[0]);
            OwnerId = TypeCast.ToInt(dr[1]);
            OrganisationId = TypeCast.ToInt(dr[2]);
            BaseOrganisationId = TypeCast.ToInt(dr[3]);
            DatabaseTable = TypeCast.ToString(dr[4]);
            DatabaseColumn = TypeCast.ToString(dr[5]);
            X1 = TypeCast.ToInt(dr[6]);
            Y1 = TypeCast.ToInt(dr[7]);
            Width1 = TypeCast.ToInt(dr[8]);
            Height1 = TypeCast.ToInt(dr[9]);
            X2 = TypeCast.ToIntOrNull(dr[10]);
            Y2 = TypeCast.ToIntOrNull(dr[11]);
            Width2 = TypeCast.ToIntOrNull(dr[12]);
            Height2 = TypeCast.ToIntOrNull(dr[13]);
            FieldType = TypeCast.ToString(dr[14]);
            RequiredState = TypeCast.ToInt(dr[15]);
            SearchAble = TypeCast.ToInt(dr[16]);
            UseInReports = TypeCast.ToInt(dr[17]);
            ViewState = dr[18];
            TabIndex1 = TypeCast.ToInt(dr[19]);
            TabIndex2 = TypeCast.ToInt(dr[20]);

            ShowInListview = TypeCast.ToInt(dr[21]);
            ListviewIndex = TypeCast.ToInt(dr[22]);

            DataLink = TypeCast.ToString(dr[23]);

            DataSource = TypeCast.ToString(dr[24]);
            FollowAny = TypeCast.ToString(dr[25]);
            AlternateText = TypeCast.ToString(dr[26]);
            NoInherit_Obsolete = TypeCast.ToBool(dr[27]);

            OuterCSS = TypeCast.ToString(dr[28]);
            InnerCSS = TypeCast.ToString(dr[29]);

            //Added default value to array (2011-12-08)
            DefaultFieldValue = TypeCast.ToString(dr[30]);

            //Shared with options
            string[] shared = TypeCast.ToString(dr[31]).Split(':');
            if (shared[0].IndexOf("Fuld") > -1) {
                SharedWith = ShareMethod.FullShared;
                if (SharedWithOrganisations != null)
                    SharedWithOrganisations.Clear();
            } else if (shared[0].IndexOf("Delvis") > -1) {
                SharedWith = ShareMethod.PartialShared;
                if (SharedWithOrganisations != null)
                    SharedWithOrganisations.Clear();
                else
                    SharedWithOrganisations = new List<int>();

                if (shared.Length > 1) {
                    foreach (string s in shared[1].Split(',')) {
                        int orgId = TypeCast.ToInt(s);
                        if (orgId > 0 && !SharedWithOrganisations.Contains(orgId))
                            SharedWithOrganisations.Add(orgId);
                    }
                }
            } else {
                SharedWith = ShareMethod.NotShared;
                if (SharedWithOrganisations != null)
                    SharedWithOrganisations.Clear();
            }

            //DataClassification
            DataClassificationId = TypeCast.ToInt(dr[32]);

            //Anonymization
            AnonymizationId = TypeCast.ToInt(dr[33]);

            //Push
            CustomFieldValues.AddRange(TypeCast.ToString(dr[34]).Split('\n'));

            //PositiveList
            var positivelist = TypeCast.ToString(dr[35]);
            var positiveCommaValue = string.Empty;
            if (positivelist != "") {
                positiveCommaValue = string.Join(",", positivelist.Split('\n').Where(w => w != "").ToList());
            }
            PositiveFieldValue = positiveCommaValue;

            //ReasonType
            ReasonType = TypeCast.ToInt(dr[36]);

            //Reason
            Reason = TypeCast.ToString(dr[37]);
        }
    }
}

