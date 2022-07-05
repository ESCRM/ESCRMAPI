using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using IDS.EBSTCRM.Base;
using System.Collections.Generic;
using System.Linq;

namespace IDS.EBSTCRM.Base {
    /// <summary>
    /// AVN single field for design canvas
    /// </summary>
    public class AVNField {

        #region Properties

        public int Id { get; set; }
        public int AvnId { get; set; }
        public string DatabaseColumn { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string FieldType { get; set; }
        public int RequiredState { get; set; }
        public int TabIndex { get; set; }
        public string AlternateText { get; set; }
        public string OuterCSS { get; set; }
        public string InnerCSS { get; set; }
        public string Icon { get; set; }
        public string Statistics { get; set; }
        public string ListIndex { get; set; }
        public string ListWidth { get; set; }

        public List<string> CustomFieldValues { get; set; }

        public string DataType { get; set; }
        public int DataClassificationId = 0;
        public int AnonymizationId = 0;
        public bool LogAccess = false;

        public string TableColumnName { get {
                return (TypeCast.ToString(Id) + "_" + DatabaseColumn);
            }
        }

        #endregion

        #region Constructors

        public AVNField() {

        }

        /// <summary>
        /// Construct a field posted back from CRM Web site, for save
        /// </summary>
        /// <param name="AvnId">Current AVN Id</param>
        /// <param name="dr">Field properties as string array</param>
        public AVNField(int AvnId, string[] dr) {
            this.Id = TypeCast.ToInt(dr[0]);
            this.AvnId = AvnId; // TypeCast.ToInt(dr[1]);

            this.DatabaseColumn = TypeCast.ToString(dr[2]);

            this.X = TypeCast.ToInt(dr[3]);
            this.Y = TypeCast.ToInt(dr[4]);
            this.Width = TypeCast.ToInt(dr[5]);
            this.Height = TypeCast.ToInt(dr[6]);

            this.FieldType = TypeCast.ToString(dr[7]);
            this.RequiredState = TypeCast.ToInt(dr[8]);
            this.TabIndex = TypeCast.ToInt(dr[9]);

            this.AlternateText = TypeCast.ToString(dr[10]);
            this.OuterCSS = TypeCast.ToString(dr[11]);
            this.InnerCSS = TypeCast.ToString(dr[12]);
            this.Icon = TypeCast.ToString(dr[13]);
            this.Statistics = TypeCast.ToString(dr[14]);
            this.ListIndex = TypeCast.ToString(dr[15]);
            this.ListWidth = TypeCast.ToString(dr[16]);

            this.DataClassificationId = TypeCast.ToInt(dr[17]);
            this.AnonymizationId = TypeCast.ToInt(dr[18]);

            CustomFieldValues = new List<string>();
            this.CustomFieldValues.AddRange(TypeCast.ToString(dr[19]).Split('\n'));
        }

        /// <summary>
        /// Construct a field using a SQL DataReader, loading the field from the database
        /// </summary>
        /// <param name="dr">SQL DataReader</param>
        public AVNField(ref System.Data.SqlClient.SqlDataReader dr) {
            Id = TypeCast.ToInt(dr["Id"]);
            AvnId = TypeCast.ToInt(dr["AvnId"]);
            DatabaseColumn = TypeCast.ToString(dr["databaseColumn"]);
            X = TypeCast.ToInt(dr["X"]);
            Y = TypeCast.ToInt(dr["Y"]);
            Width = TypeCast.ToInt(dr["Width"]);
            Height = TypeCast.ToInt(dr["Height"]);
            FieldType = TypeCast.ToString(dr["FieldType"]);
            RequiredState = TypeCast.ToInt(dr["RequiredState"]);
            TabIndex = TypeCast.ToInt(dr["TabIndex"]);
            AlternateText = TypeCast.ToString(dr["AlternateText"]);
            OuterCSS = TypeCast.ToString(dr["OuterCSS"]);
            InnerCSS = TypeCast.ToString(dr["InnerCSS"]);
            Icon = TypeCast.ToString(dr["Icon"]);
            Statistics = TypeCast.ToString(dr["StatisticsType"]);
            ListIndex = TypeCast.ToString(dr["ListIndex"]);
            ListWidth = TypeCast.ToString(dr["ListWidth"]);

            if (ColumnExists(dr, "DataClassificationId")) {
                DataClassificationId = TypeCast.ToInt(dr["DataClassificationId"]);
            }
            if (ColumnExists(dr, "AnonymizationId")) {
                AnonymizationId = TypeCast.ToInt(dr["AnonymizationId"]);
            }
            if (ColumnExists(dr, "LogAccess")) {
                LogAccess = TypeCast.ToBool(dr["LogAccess"]);
            }
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

        #endregion

        #region HTML Layout Building

        //Countries in the world
        public static string[] CountryList = new string[] { "Denmark", "-", "Albania", "Algeria", "American Samoa", "Andorra", "Angola", "Anguilla", "Antigua & Barbuda", "Argentina", "Armenia", "Aruba", "Australia", "Austria", "Azerbaijan", "Azores", "Bahamas", "Bahrain", "Bangladesh", "Barbados", "Belarus", "Belgium", "Belize", "Benin", "Bermuda", "Bhutan", "Bolivia", "Bonaire", "Bosnia", "Botswana", "Brazil", "British Virgin Isles", "Brunei", "Bulgaria", "Burkina Faso", "Burundi", "Cambodia", "Cameroon", "Canada", "Canary Islands", "Cape Verde", "Cayman Islands", "Central African Republic", "Chad", "Channel Islands", "Chile", "China", "Colombia", "Cook Islands", "Costa Rica", "Cote DIvoire", "Croatia", "Curacao", "Cyprus", "Czech Republic", "Dem Rep of Congo", "Denmark", "Djibouti", "Dominica", "Dominican Republic", "Ecuador", "Egypt", "El Salvador", "England", "Equitorial Guinea", "Eritrea", "Estonia", "Ethiopia", "Faeroe Islands", "Fiji", "Finland", "France", "French Guiana", "French Polynesia", "Gabon", "Gambia", "Georgia", "Germany", "Ghana", "Gibraltar", "Greece", "Greenland", "Grenada", "Guadeloupe", "Guam", "Guatemala", "Guinea", "Guinea-Bissau", "Guyana", "Haiti", "Holland", "Honduras", "Hong Kong", "Hungary", "Iceland", "India", "Indonesia", "Iraq", "Israel", "Italy", "IvoryCoast", "Jamaica", "Japan", "Jordan", "Kazakhstan", "Kenya", "Kiribati", "Kosrae", "Kuwait", "Kyrgyzstan", "Laos", "Latvia", "Lebanon", "Lesotho", "Liberia", "Libya", "Liechtenstein", "Lithuania", "Luxembourg", "Macau", "Macedonia", "Madagascar", "Madeira", "Malawi", "Malaysia", "Maldives", "Mali", "Malta", "Marshall Islands", "Martinique", "Mauritania", "Mauritius", "Mexico", "Micronesia", "Moldova", "Monaco", "Mongolia", "Montserrat", "Morocco", "Mozambique", "Myanmar", "N. Mariana Islands", "Namibia", "Nauru", "Nepal", "Netherlands", "Netherlands Antilles", "New Caledonia", "New Zealand", "Nicaragua", "Niger", "Nigeria", "Norfolk Island", "Northern Ireland", "Norway", "Oman", "Other-Not Shown", "Pakistan", "Palau", "Panama", "Papua New Guinea", "Paraguay", "Peoples Rep of China", "Peru", "Philippines", "Pitcairn Island", "Poland", "Ponape", "Portugal", "Puerto Rico", "Qatar", "Republic of Congo", "Republic of Ireland", "Republic of Yemen", "Reunion", "Romania", "Rota", "Russia", "Rwanda", "Saba", "Saipan", "San Marino", "SaoTome and Principe", "Saudi Arabia", "Scotland", "Senegal", "Seychelles", "Sierra Leone", "Singapore", "Slovakia", "Slovenia", "Solomon Islands", "South Africa", "South Korea", "Spain", "Sri Lanka", "St. Barthelemy", "St. Christopher", "St. Croix", "St. Eustatius", "St. John", "St. Kitts & Nevis", "St. Lucia", "St. Maarten", "St. Martin", "St. Thomas", "St. Vincent and Grenadine", "Sudan", "Suriname", "Swaziland", "Sweden", "Switzerland", "Syria", "Tahiti", "Taiwan", "Tajikistan", "Tanzania", "Thailand", "Tinian", "Togo", "Tonga", "Tortola", "Trinidad & Tobago", "Truk", "Tunisia", "Turkey", "Turkmenistan", "Turks & Caicos Islands", "Tuvalu", "Uganda", "Ukraine", "Union Island", "United Arab Emirates", "United Kingdom", "United States", "Uruguay", "US Virgin Islands", "Uzbekistan", "Vanuatu", "Vatican City State", "Venezuela", "Vietnam", "Virgin Gorda", "Wake Island", "Wales", "Wallis & Futuna Islands", "Western Samoa", "Yap", "Yugoslavia", "Zambia", "Zimbabwe" };

        /// <summary>
        /// Create the field as a HTML Element
        /// </summary>
        /// <param name="f"></param>
        /// <param name="inDesignMode"></param>
        /// <param name="sql"></param>
        /// <param name="U"></param>
        /// <param name="IsIE"></param>
        /// <param name="JavaScript"></param>
        /// <returns></returns>
        public static HtmlTable createElement(AVNField f, bool inDesignMode, ref SQLDB sql, ref User U, bool IsIE, out string JavaScript, List<SQLLabelReplaceableValue> SQLLabelReplaceables, List<Base.Classes.DataClassification> dataClassfications = null) {
            return createElement(new AVNFieldWithValue(f, null), inDesignMode, ref sql, ref U, IsIE, out JavaScript, SQLLabelReplaceables, dataClassfications);
        }

        /// <summary>
        /// Create the field as a HTML Element
        /// </summary>
        /// <param name="f"></param>
        /// <param name="inDesignMode"></param>
        /// <param name="sql"></param>
        /// <param name="U"></param>
        /// <param name="IsIE"></param>
        /// <param name="JavaScript"></param>
        /// <returns></returns>
        public static HtmlTable createElement(AVNFieldWithValue f, bool inDesignMode, ref SQLDB sql, ref User U, bool IsIE, out string JavaScript, List<SQLLabelReplaceableValue> SQLLabelReplaceables, List<Base.Classes.DataClassification> dataClassfications = null) {
            JavaScript = "";

            HtmlTable tbl = new HtmlTable();
            HtmlTableRow tr = new HtmlTableRow();
            HtmlTableCell td = new HtmlTableCell();

            tbl.CellPadding = 0;
            tbl.CellSpacing = 0;
            tbl.Border = 0;

            tbl.Controls.Add(tr);
            tr.Controls.Add(td);

            tbl.Attributes["dbTable"] = f.AvnId.ToString();

            tbl.Attributes["dbName"] = f.DatabaseColumn;

            tbl.Attributes["dbContentType"] = f.FieldType;
            tbl.Attributes["dbId"] = f.Id.ToString();


            tbl.Attributes["class"] = inDesignMode ? "FieldDesignControl" : "FieldRealTimeControl";

            tbl.Attributes["style"] += "left:" + f.X + "px;" +
                        "top:" + f.Y + "px;" +
                        "width:" + f.Width + "px;" +
                        "height:" + f.Height + "px;";

            td.Attributes["style"] = "width:" + f.Width + "px;" +
                        "height:" + f.Height + "px;";

            if (inDesignMode) {
                if (f.Id == 3950) {

                }

                if (f.ListIndex != "" && f.ListIndex != null) {

                }
                tbl.Attributes["Icon"] = f.Icon;
                tbl.Attributes["Statistics"] = f.Statistics;
                tbl.Attributes["ListIndex"] = f.ListIndex;
                tbl.Attributes["ListWidth"] = f.ListWidth;
                tbl.Attributes["onclick"] += "selectObject(this);";
                tbl.Attributes["onmousedown"] += "mouseDown(this);";
                tbl.Attributes["onmouseup"] += "mouseUp(this);";
                tbl.Attributes["alternateText"] = f.AlternateText;

                tbl.Attributes["style"] += f.OuterCSS.Replace("visibility:hidden;", "");
            } else {
                tbl.Attributes["title"] = f.AlternateText;
                tbl.Attributes["style"] += f.OuterCSS;
            }

            tbl.Attributes["dbX"] += f.X;
            tbl.Attributes["dbY"] += f.Y;
            tbl.Attributes["dbW"] += f.Width;
            tbl.Attributes["dbH"] += f.Height;

            // Data classification changes
            tbl.Attributes["dbClassificationId"] = f.DataClassificationId.ToString();
            tbl.Attributes["dbAnonymizationId"] = f.AnonymizationId.ToString();

            var anonymizationRequired = 0;
            var dcFieldType = sql.GetDataClassificationByFieldType().FirstOrDefault(w => w.FieldType == f.FieldType && w.Id == f.DataClassificationId);
            if (dcFieldType != null) {
                anonymizationRequired = (dcFieldType.EnforceAnonymization ? 1 : 0);
            }
            tbl.Attributes["dbAnonymizationRequired"] = anonymizationRequired.ToString();

            switch (f.FieldType) {
                case "map":
                    tbl.Attributes["RequiredState"] = f.RequiredState.ToString();

                    //Set properties
                    List<string> mapProps = sql.AVNFields_getCustomValues(f.Id);
                    foreach (string mapProp in mapProps) {

                        string[] mp = System.Text.RegularExpressions.Regex.Split(mapProp, "#%\\|%#");
                        if (mp.Length == 2 && mp[0] != "" && mp[1] != "") {
                            tbl.Attributes[mp[0]] = mp[1];
                        }
                    }

                    if (inDesignMode) {
                        HtmlImage img = new HtmlImage();
                        td.Controls.Add(img);
                        img.Src = "images/googleMaps32.png";
                        img.Alt = "Kort";
                        img.Attributes["title"] = "Kort";
                        img.Attributes["class"] = "designerTextarea";
                        img.Attributes["style"] += f.InnerCSS;
                    } else {
                        HtmlGenericControl divMap = new HtmlGenericControl("div");
                        td.Controls.Add(divMap);
                        divMap.Attributes["class"] = "designerTextarea";
                        divMap.Attributes["style"] += f.InnerCSS + ";width:" + f.Width + "px;" +
                        "height:" + f.Height + "px;";
                        divMap.Attributes["googlemaps"] = "true";
                    }


                    break;

                case "button":
                    HtmlInputButton cmdButton = new HtmlInputButton();
                    td.Controls.Add(cmdButton);

                    if (!inDesignMode) {
                        cmdButton.Attributes["class"] = "designerButton";

                        cmdButton.Value = f.DatabaseColumn;
                        cmdButton.Attributes["tabIndex"] = f.TabIndex.ToString();
                        //tbl.Attributes["onchange"] = "locateDoublesFromField(this);";
                        cmdButton.Attributes["oldValue"] = cmdButton.Value;
                        cmdButton.Attributes["style"] += "width:" + f.Width + "px;";

                        cmdButton.ID = f.DatabaseColumn;
                        cmdButton.Attributes["title"] = f.AlternateText;

                        //string onClick = "";
                        JavaScript = "\t\tfunction OnClick_" + f.Id + "(sender) {\n";

                        foreach (string s in sql.AVNFields_getCustomValues(f.Id)) {
                            if (s.Trim() != "")
                                JavaScript += "\t\t\t" + s + "\r\n";
                        }

                        JavaScript += "\n\t\t};\r\n";

                        cmdButton.Attributes["onclick"] = "OnClick_" + f.Id + "(this);";
                    } else {
                        cmdButton.Attributes["class"] = "designerButton";

                        cmdButton.Attributes["readonly"] += "true";
                        cmdButton.Value = f.DatabaseColumn;

                        foreach (string s in sql.AVNFields_getCustomValues(f.Id)) {
                            tbl.Attributes["SelDropDownItems"] += s + "\n";
                        }
                    }

                    cmdButton.Name = cmdButton.ID;

                    if (inDesignMode)
                        tbl.Attributes["tabIndex"] = f.TabIndex.ToString();
                    tbl.Attributes["RequiredState"] = f.RequiredState.ToString();

                    cmdButton.Attributes["style"] += f.InnerCSS;

                    break;


                case "datetime":
                    HtmlInputText txtDateTime = new HtmlInputText();
                    td.Controls.Add(txtDateTime);

                    if (!inDesignMode) {
                        txtDateTime.Attributes["class"] = "designerTextbox";

                        txtDateTime.Value = f.FormattedValue;
                        txtDateTime.Attributes["tabIndex"] = f.TabIndex.ToString();
                        //tbl.Attributes["onchange"] = "locateDoublesFromField(this);";
                        txtDateTime.Attributes["oldValue"] = txtDateTime.Value;
                        txtDateTime.Attributes["style"] += "width:" + (f.Width - 22) + "px;";

                        txtDateTime.ID = f.DatabaseColumn;
                        txtDateTime.Attributes["title"] = f.AlternateText;

                        HtmlTableCell tdBtn = new HtmlTableCell();
                        tr.Controls.Add(tdBtn);
                        tdBtn.VAlign = "middle";
                        tbl.Align = "left";

                        HtmlGenericControl calBtn = new HtmlGenericControl("DIV");
                        calBtn.Attributes["class"] = "designerDatePickerButton";
                        calBtn.Attributes["onmouseover"] = "this.className='designerDatePickerButtonHover';";
                        calBtn.Attributes["onmouseout"] = "this.className='designerDatePickerButton';";

                        calBtn.Attributes["onclick"] = "pickDateToControl('" + f.DatabaseColumn + "');";

                        if (!IsIE)
                            calBtn.Attributes["style"] += "height:16px;";

                        if (f.InnerCSS.ToLower().Contains("display:none")) {
                            tbl.Attributes["style"] += ";display:none;";
                        }

                        tdBtn.Controls.Add(calBtn);
                    } else {
                        txtDateTime.Attributes["class"] = "designerDatePicker";

                        txtDateTime.Attributes["readonly"] += "true";
                        txtDateTime.Value = f.DatabaseColumn;
                    }

                    txtDateTime.Name = txtDateTime.ID;

                    if (inDesignMode)
                        tbl.Attributes["tabIndex"] = f.TabIndex.ToString();
                    tbl.Attributes["RequiredState"] = f.RequiredState.ToString();

                    txtDateTime.Attributes["style"] += f.InnerCSS;

                    // When field is not rendered for Field designer page
                    if (!inDesignMode) {
                        if (dataClassfications != null && dataClassfications.Any()) {
                            var classification = dataClassfications.FirstOrDefault(w => w.Id == f.DataClassificationId);
                            if (classification != null && !string.IsNullOrEmpty(classification.ExportMask)) {

                                // Override textbox width
                                txtDateTime.Attributes["class"] = "designerTextbox Locked";
                                txtDateTime.Attributes["disabled"] = "disabled";
                                txtDateTime.Attributes["sensitive"] = "1";
                                txtDateTime.Attributes["style"] = "width:" + ((f.Width - 22) - 18) + "px;";

                                // Routine to add image button for classified data
                                HtmlInputImage img = new HtmlInputImage();
                                img.Attributes["title"] = "View";
                                img.Attributes["src"] = "/images/eye32.png";
                                img.Attributes["width"] = "16";
                                img.Attributes["height"] = "16";
                                img.Attributes["style"] = "margin-left:3px;";
                                img.Attributes["data-target-id"] = f.Id.ToString();
                                img.Attributes["data-target-name"] = f.DatabaseColumn;
                                img.Attributes["data-target-type"] = "avn";
                                img.Attributes["onclick"] = "Show(this);";
                                td.Controls.Add(img);
                            }
                        }
                    }

                    break;

                case "textbox":
                case "integer":
                case "absinteger":
                case "float":
                case "absfloat":
                case "emailaddress":
                    HtmlInputText txt = new HtmlInputText();
                    td.Controls.Add(txt);
                    //txt.Value = f.DatabaseColumn; // : grabValue(values, f.DatabaseTable + "_" + f.DatabaseColumn, f.BaseOrganisationId);
                    txt.Attributes["class"] = "designerTextbox";
                    if (inDesignMode) txt.Attributes["readonly"] += "true";
                    if (!inDesignMode) {
                        txt.Value = f.FormattedValue;
                        txt.Attributes["tabIndex"] = f.TabIndex.ToString();
                        //tbl.Attributes["onchange"] = "locateDoublesFromField(this);";
                        txt.Attributes["oldValue"] = txt.Value;
                        txt.Attributes["style"] += "width:" + (f.Width - 4) + "px;";
                    } else
                        txt.Value = f.DatabaseColumn;

                    if (inDesignMode)
                        tbl.Attributes["tabIndex"] = f.TabIndex.ToString();
                    tbl.Attributes["RequiredState"] = f.RequiredState.ToString();

                    if (!inDesignMode) txt.ID = f.DatabaseColumn;
                    txt.Name = txt.ID;
                    if (!inDesignMode) txt.Attributes["title"] = f.AlternateText;

                    txt.Attributes["style"] += f.InnerCSS;


                    // When field is not rendered for Field designer page
                    if (!inDesignMode) {
                        if (dataClassfications != null && dataClassfications.Any()) {
                            var classification = dataClassfications.FirstOrDefault(w => w.Id == f.DataClassificationId);
                            if (classification != null && !string.IsNullOrEmpty(classification.ExportMask)) {

                                // Override textbox width
                                txt.Attributes["class"] = "designerTextbox Locked";
                                txt.Attributes["disabled"] = "disabled";
                                txt.Attributes["sensitive"] = "1";
                                txt.Attributes["style"] = "width:" + ((f.Width - 4) - 18) + "px;";

                                // Routine to add image button for classified data
                                HtmlInputImage img = new HtmlInputImage();
                                img.Attributes["title"] = "View";
                                img.Attributes["src"] = "/images/eye32.png";
                                img.Attributes["width"] = "16";
                                img.Attributes["height"] = "16";
                                img.Attributes["style"] = "margin-left:3px;";
                                img.Attributes["data-target-id"] = f.Id.ToString();
                                img.Attributes["data-target-name"] = f.DatabaseColumn;
                                img.Attributes["data-target-type"] = "avn";
                                img.Attributes["onclick"] = "Show(this);";
                                td.Controls.Add(img);
                            }
                        }
                    }


                    break;

                case "label":
                    HtmlGenericControl lbl = new HtmlGenericControl("DIV");
                    lbl.InnerHtml = f.DatabaseColumn;
                    td.Controls.Add(lbl);
                    lbl.Attributes["class"] = "FieldDesignLabel";
                    if (!inDesignMode) lbl.Attributes["title"] = f.AlternateText;

                    lbl.Attributes["style"] += f.InnerCSS;
                    break;

                case "sqllabel":
                    HtmlGenericControl sqllbl = new HtmlGenericControl("DIV");
                    sqllbl.Attributes["class"] = "FieldDesignSQLLabel";

                    if (inDesignMode) {
                        sqllbl.InnerHtml = f.DatabaseColumn;

                        foreach (string s in sql.AVNFields_getCustomValues(f.Id)) {
                            tbl.Attributes["SelDropDownItems"] += s + "\n";
                        }
                    } else {
                        string sql2Exec = "";
                        foreach (string s in sql.AVNFields_getCustomValues(f.Id)) {
                            sql2Exec += s + "\r\n";
                        }

                        if (sql2Exec != "") {
                            try {
                                //Replace sqllabel text with replaceables
                                if (SQLLabelReplaceables != null) {
                                    foreach (SQLLabelReplaceableValue r in SQLLabelReplaceables) {
                                        string regEx = "\\%" + r.Key + "\\%";
                                        sql2Exec = System.Text.RegularExpressions.Regex.Replace(sql2Exec, regEx, r.ReplaceWith, System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Multiline | System.Text.RegularExpressions.RegexOptions.Singleline | System.Text.RegularExpressions.RegexOptions.IgnorePatternWhitespace | System.Text.RegularExpressions.RegexOptions.CultureInvariant);
                                    }
                                }
                                //sql2Exec = sql2Exec.Replace("%currentuser%", U.Id);

                                SQLDB sqle = new SQLDB(System.Configuration.ConfigurationManager.AppSettings["ReadOnlyConnectionString"]);
                                List<IDS.EBSTCRM.Base.SQLLabelDataItem[]> res = sqle.generic_executeSQLForResultSet(sql2Exec);

                                if (res.Count > 0) {
                                    //Single result
                                    if (res.Count == 1 && res[0].Length == 1) {
                                        sqllbl.InnerText = res[0][0].ValueFormatted;
                                    } else {
                                        HtmlTable tblsql = new HtmlTable();
                                        tblsql.Attributes["class"] = "avnSQLLabel_Table";
                                        tblsql.CellPadding = 2;
                                        tblsql.CellSpacing = 0;
                                        tblsql.Border = 0;

                                        bool AtHeader = true;

                                        foreach (IDS.EBSTCRM.Base.SQLLabelDataItem[] ss in res) {
                                            if (AtHeader) {
                                                AtHeader = false;
                                                HtmlTableRow trsqlH = new HtmlTableRow();
                                                tblsql.Controls.Add(trsqlH);

                                                for (int i = 0; i < ss.Length; i++) {
                                                    HtmlTableCell tdsqlH = new HtmlTableCell();
                                                    tdsqlH.InnerText = ss[i].Name;

                                                    tdsqlH.Attributes["class"] = i == ss.Length - 1 ? "avnSQLLabel_TdHeader_EOL" : "avnSQLLabel_TdHeader";

                                                    trsqlH.Controls.Add(tdsqlH);
                                                }
                                            }

                                            HtmlTableRow trsql = new HtmlTableRow();
                                            tblsql.Controls.Add(trsql);

                                            for (int i = 0; i < ss.Length; i++) {
                                                HtmlTableCell tdsql = new HtmlTableCell();



                                                tdsql.InnerText = ss[i].ValueFormatted;

                                                tdsql.Attributes["class"] = i == ss.Length - 1 ? "avnSQLLabel_Td_EOL" : "avnSQLLabel_Td";
                                                tdsql.Attributes["align"] = ss[i].ValueAlignment;

                                                string format = ss[i].ValueFormatting;
                                                if (format != "")
                                                    tdsql.Attributes["style"] += ";" + format;

                                                trsql.Controls.Add(tdsql);
                                            }
                                        }

                                        sqllbl.Controls.Add(tblsql);

                                        sqllbl.Attributes["class"] = "avnSQLLabel_Frame";
                                        sqllbl.Attributes["style"] += "width:" + (f.Width - 2) + ";" + "height:" + (f.Height - 2) + ";";
                                        sqllbl.Attributes["style"] += "overflow:auto;";
                                    }
                                }
                            } catch (Exception sqlexp) {
                                sqllbl.InnerHtml = "<b>Fejl!</b><br>" + sqlexp.Message.Replace("\n", "<br>") + "<br>" + sqlexp.StackTrace.Replace("\n", "<br>");

                                sqllbl.Attributes["style"] += "width:" + f.Width + ";" + "height:" + f.Height + ";";
                                sqllbl.Attributes["style"] += "overflow:auto;";
                            }
                        }

                    }

                    td.Controls.Add(sqllbl);

                    if (!inDesignMode) sqllbl.Attributes["title"] = f.AlternateText;

                    sqllbl.Attributes["style"] += f.InnerCSS;
                    break;

                case "title":
                    HtmlGenericControl ttl = new HtmlGenericControl("DIV");
                    ttl.InnerHtml = f.DatabaseColumn;
                    ttl.Attributes["class"] = "FieldDesignTitle";
                    td.Controls.Add(ttl);
                    if (!inDesignMode) ttl.Attributes["title"] = f.AlternateText;

                    ttl.Attributes["style"] += f.InnerCSS;
                    break;

                case "memo":
                    HtmlTextArea txa = new HtmlTextArea();
                    td.Controls.Add(txa);
                    txa.Value = f.DatabaseColumn; //inDesignMode ? f.DatabaseColumn : grabValue(values, f.DatabaseTable + "_" + f.DatabaseColumn, f.BaseOrganisationId);
                    txa.Attributes["class"] = "designerTextarea";
                    if (inDesignMode) txa.Attributes["readonly"] += "true";
                    txa.Attributes["tabIndex"] = f.TabIndex.ToString();

                    if (inDesignMode)
                        tbl.Attributes["tabIndex"] = f.TabIndex.ToString();
                    tbl.Attributes["RequiredState"] = f.RequiredState.ToString();

                    if (!inDesignMode) {
                        txa.ID = f.DatabaseColumn;
                        txa.Attributes["title"] = f.AlternateText;
                        txa.Value = f.FormattedValue;
                        txa.Attributes["oldValue"] = txa.Value;
                    }

                    txa.Name = txa.ID;
                    txa.Attributes["style"] += f.InnerCSS;

                    // When field is not rendered for Field designer page
                    if (!inDesignMode) {
                        if (dataClassfications != null && dataClassfications.Any()) {
                            var classification = dataClassfications.FirstOrDefault(w => w.Id == f.DataClassificationId);
                            if (classification != null && !string.IsNullOrEmpty(classification.ExportMask)) {

                                // Override textbox width
                                txa.Attributes["class"] = "designerTextbox Locked";
                                txa.Attributes["disabled"] = "disabled";
                                txa.Attributes["sensitive"] = "1";
                                txa.Attributes["style"] = "width:" + (f.Width - 22) + "px;";

                                // Routine to add image button for classified data
                                HtmlInputImage img = new HtmlInputImage();
                                img.Attributes["title"] = "View";
                                img.Attributes["src"] = "/images/eye32.png";
                                img.Attributes["width"] = "16";
                                img.Attributes["height"] = "16";
                                img.Attributes["style"] = "margin-left:3px;";
                                img.Attributes["data-target-id"] = f.Id.ToString();
                                img.Attributes["data-target-name"] = f.DatabaseColumn;
                                img.Attributes["data-target-type"] = "avn";
                                img.Attributes["onclick"] = "Show(this);";
                                td.Controls.Add(img);
                            }
                        }
                    }

                    break;

                case "checkbox":
                    HtmlInputCheckBox chb = new HtmlInputCheckBox();
                    td.Controls.Add(chb);
                    chb.Attributes["tabIndex"] = f.TabIndex.ToString();

                    if (inDesignMode)
                        tbl.Attributes["tabIndex"] = f.TabIndex.ToString();
                    tbl.Attributes["RequiredState"] = f.RequiredState.ToString();

                    if (!inDesignMode) {
                        chb.ID = f.DatabaseColumn;
                        chb.Checked = f.FormattedValue == "Ja";
                    }
                    chb.Name = chb.ID;
                    if (!inDesignMode) chb.Attributes["title"] = f.AlternateText;
                    chb.Attributes["style"] += f.InnerCSS;
                    break;

                case "hr":

                    HtmlImage hrImg = new HtmlImage();
                    hrImg.Src = "images/spacer.gif";
                    hrImg.Attributes["class"] = "FieldDesignHR";
                    hrImg.Alt = "";
                    hrImg.Border = 0;
                    if (!inDesignMode) hrImg.Attributes["title"] = f.AlternateText;
                    td.Controls.Add(hrImg);
                    hrImg.Attributes["style"] += f.InnerCSS;
                    break;

                case "vr":
                    td.Align = "center";
                    HtmlImage vrImg = new HtmlImage();
                    vrImg.Src = "images/spacer.gif";
                    vrImg.Attributes["class"] = "FieldDesignVR";
                    vrImg.Alt = "";
                    vrImg.Border = 0;
                    if (!inDesignMode) vrImg.Attributes["title"] = f.AlternateText;
                    td.Controls.Add(vrImg);
                    vrImg.Attributes["style"] += f.InnerCSS;
                    break;


                case "listview":
                    HtmlSelect lst = new HtmlSelect();
                    lst.Multiple = true;
                    td.Controls.Add(lst);
                    lst.Attributes["class"] = "designerTextarea";
                    lst.Attributes["integralheight"] = "true";

                    if (inDesignMode) lst.Attributes["readonly"] += "true";
                    if (!inDesignMode) {
                        lst.Attributes["tabIndex"] = f.TabIndex.ToString();
                        //tbl.Attributes["onchange"] = "locateDoublesFromField(this);";
                        lst.Value = f.FormattedValue;
                        lst.Attributes["oldValue"] = lst.Value;
                    }

                    if (inDesignMode)
                        tbl.Attributes["tabIndex"] = f.TabIndex.ToString();
                    tbl.Attributes["RequiredState"] = f.RequiredState.ToString();
                    if (!inDesignMode) lst.Attributes["title"] = f.AlternateText;



                    if (!inDesignMode) {
                        lst.Attributes["style"] = "width:" + f.Width + ";" +
                        "height:" + f.Height + ";";

                        lst.ID = f.DatabaseColumn;

                        List<string> vals2 = sql.AVNFields_getCustomValues(f.Id);
                        if (vals2.Count > 0) {
                            if (vals2[0].Replace("\r", "").Replace("\n", "") != "" || vals2.Count > 1) {
                                List<string> listviewSelectedValues = new List<string>();
                                listviewSelectedValues.AddRange(f.FormattedValue.Split('\n'));

                                bool litemexists = false;

                                foreach (string s in vals2) {
                                    ListItem l = new ListItem(s.Replace("\r", "").Replace("\n", ""));
                                    if (listviewSelectedValues.Contains(s.Replace("\r", "").Replace("\n", ""))) {
                                        l.Selected = true;
                                    }
                                    lst.Items.Add(l);
                                }

                                foreach (string selectThis in listviewSelectedValues) {
                                    ListItem l = new ListItem(selectThis.Replace("\r", "").Replace("\n", ""));
                                    l.Selected = true;
                                    if (!lst.Items.Contains(l)) {
                                        lst.Items.Add(l);
                                    }
                                }
                            }
                        }


                        //Make sure the chosen item is in the listview/combobox
                        if (lst.Value != "") {
                            bool valuePresent = false;

                            for (int i = 0; i < lst.Items.Count; i++) {
                                if (lst.Items[i].Value == lst.Value) {
                                    valuePresent = true;
                                    break;
                                }
                            }

                            if (!valuePresent) {
                                if (lst.Items.Count > 0)
                                    lst.Items.Insert(0, new ListItem(lst.Value));
                                else
                                    lst.Items.Add(new ListItem(lst.Value));
                            }
                        }

                    } else {
                        foreach (string s in sql.AVNFields_getCustomValues(f.Id)) {
                            tbl.Attributes["SelDropDownItems"] += s + "\n";
                        }

                        ListItem l = new ListItem(f.DatabaseColumn);
                        lst.Items.Add(l);

                    }
                    lst.Name = lst.ID;

                    if (inDesignMode)
                        lst.Value = f.DatabaseColumn;

                    lst.Attributes["style"] += f.InnerCSS;
                    break;

                case "dropdown":
                    HtmlSelect sel = new HtmlSelect();
                    td.Controls.Add(sel);
                    sel.Attributes["class"] = "designerTextbox";
                    if (inDesignMode) sel.Attributes["readonly"] += "true";
                    if (!inDesignMode) {
                        sel.Attributes["tabIndex"] = f.TabIndex.ToString();
                        //tbl.Attributes["onchange"] = "locateDoublesFromField(this);";
                        sel.Value = f.FormattedValue;
                        sel.Attributes["oldValue"] = sel.Value;
                    }

                    if (inDesignMode)
                        tbl.Attributes["tabIndex"] = f.TabIndex.ToString();
                    tbl.Attributes["RequiredState"] = f.RequiredState.ToString();
                    if (!inDesignMode) sel.Attributes["title"] = f.AlternateText;

                    if (!inDesignMode) {
                        sel.ID = f.DatabaseColumn;

                        List<string> vals = sql.AVNFields_getCustomValues(f.Id);
                        if (vals.Count > 0) {
                            if (vals[0].Replace("\r", "").Replace("\n", "") != "" || vals.Count > 1) {
                                List<string> dropdownSelectedValues = new List<string>();
                                dropdownSelectedValues.AddRange(f.FormattedValue.Split('\n'));
                                bool gotSelected = false;

                                foreach (string s in vals) {
                                    ListItem l = new ListItem(s.Replace("\r", "").Replace("\n", ""));
                                    if (dropdownSelectedValues.Contains(s.Replace("\r", "").Replace("\n", "")) && !gotSelected) {
                                        l.Selected = true;
                                        gotSelected = true;
                                    }

                                    sel.Items.Add(l);
                                }

                                if (!gotSelected && dropdownSelectedValues != null && dropdownSelectedValues.Count > 0) {
                                    ListItem l = new ListItem(dropdownSelectedValues[0].Replace("\r", "").Replace("\n", ""));
                                    l.Selected = true;
                                    sel.Items.Add(l);
                                }
                            }
                        }

                        //Make sure the chosen item is in the listview/combobox
                        if (sel.Value != "") {
                            bool valuePresent = false;

                            for (int i = 0; i < sel.Items.Count; i++) {
                                if (sel.Items[i].Value == sel.Value) {
                                    valuePresent = true;
                                    break;
                                }
                            }

                            if (!valuePresent) {
                                if (sel.Items.Count > 0)
                                    sel.Items.Insert(0, new ListItem(sel.Value));
                                else
                                    sel.Items.Add(new ListItem(sel.Value));
                            }
                        }
                    } else {
                        foreach (string s in sql.AVNFields_getCustomValues(f.Id)) {
                            tbl.Attributes["SelDropDownItems"] += s + "\n";
                        }
                        ListItem l = new ListItem(f.DatabaseColumn);
                        sel.Items.Add(l);
                        sel.Disabled = true;

                    }

                    sel.Name = sel.ID;
                    //sel.Value = inDesignMode ? f.DatabaseColumn : grabValue(values, f.DatabaseTable + "_" + f.DatabaseColumn, f.BaseOrganisationId);
                    sel.Attributes["style"] += f.InnerCSS;

                    // When field is not rendered for Field designer page
                    if (!inDesignMode) {
                        if (dataClassfications != null && dataClassfications.Any()) {
                            var classification = dataClassfications.FirstOrDefault(w => w.Id == f.DataClassificationId);
                            if (classification != null && !string.IsNullOrEmpty(classification.ExportMask)) {

                                //td.Attributes["style"] = "width:" + (type == "SMV" ? f.Width1 : f.Width2) + ";"

                                // Override textbox width
                                sel.Attributes["class"] = "designerTextbox Locked";
                                sel.Attributes["disabled"] = "disabled";
                                sel.Attributes["sensitive"] = "1";
                                sel.Attributes["style"] = "width:" + (f.Width - 22) + "px;";

                                // Routine to add image button for classified data
                                HtmlInputImage img = new HtmlInputImage();
                                img.Attributes["title"] = "View";
                                img.Attributes["src"] = "/images/eye32.png";
                                img.Attributes["width"] = "16";
                                img.Attributes["height"] = "16";
                                img.Attributes["style"] = "margin-left:3px;";
                                img.Attributes["data-target-id"] = f.Id.ToString();
                                img.Attributes["data-target-name"] = f.DatabaseColumn;
                                img.Attributes["data-target-type"] = "avn";
                                img.Attributes["onclick"] = "Show(this);";
                                td.Controls.Add(img);
                            }
                        }
                    }

                    break;
            }

            return tbl;
        }


        public static HtmlGenericControl createMobileElement(AVNFieldWithValue f, bool inDesignMode, ref SQLDB sql, ref User U, bool IsIE, out string JavaScript, List<SQLLabelReplaceableValue> SQLLabelReplaceables) {
            JavaScript = "";


            HtmlGenericControl div = new HtmlGenericControl("DIV");

            HtmlGenericControl divText = new HtmlGenericControl("DIV");
            divText.InnerText = f.DatabaseColumn;

            switch (f.FieldType) {
                case "sqllabel":
                    string sql2Exec = "";
                    foreach (string s in sql.AVNFields_getCustomValues(f.Id)) {
                        sql2Exec += s + "\r\n";
                    }

                    if (sql2Exec != "") {
                        try {
                            //Replace sqllabel text with replaceables
                            if (SQLLabelReplaceables != null) {
                                foreach (SQLLabelReplaceableValue r in SQLLabelReplaceables) {
                                    string regEx = "\\%" + r.Key + "\\%";
                                    sql2Exec = System.Text.RegularExpressions.Regex.Replace(sql2Exec, regEx, r.ReplaceWith, System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Multiline | System.Text.RegularExpressions.RegexOptions.Singleline | System.Text.RegularExpressions.RegexOptions.IgnorePatternWhitespace | System.Text.RegularExpressions.RegexOptions.CultureInvariant);
                                }
                            }
                            //sql2Exec = sql2Exec.Replace("%currentuser%", U.Id);

                            SQLDB sqle = new SQLDB(System.Configuration.ConfigurationManager.AppSettings["ReadOnlyConnectionString"]);
                            List<IDS.EBSTCRM.Base.SQLLabelDataItem[]> res = sqle.generic_executeSQLForResultSet(sql2Exec);

                            if (res.Count > 0) {
                                //Single result
                                if (res.Count == 1 && res[0].Length == 1) {
                                    div.InnerText = res[0][0].ValueFormatted;
                                } else {
                                    HtmlTable tblsql = new HtmlTable();
                                    tblsql.Attributes["class"] = "avnSQLLabel_Table";
                                    tblsql.CellPadding = 2;
                                    tblsql.CellSpacing = 0;
                                    tblsql.Border = 0;

                                    bool AtHeader = true;

                                    foreach (IDS.EBSTCRM.Base.SQLLabelDataItem[] ss in res) {
                                        if (AtHeader) {
                                            AtHeader = false;
                                            HtmlTableRow trsqlH = new HtmlTableRow();
                                            tblsql.Controls.Add(trsqlH);

                                            for (int i = 0; i < ss.Length; i++) {
                                                HtmlTableCell tdsqlH = new HtmlTableCell();
                                                tdsqlH.InnerText = ss[i].Name;

                                                tdsqlH.Attributes["class"] = i == ss.Length - 1 ? "avnSQLLabel_TdHeader_EOL" : "avnSQLLabel_TdHeader";

                                                trsqlH.Controls.Add(tdsqlH);
                                            }
                                        }

                                        HtmlTableRow trsql = new HtmlTableRow();
                                        tblsql.Controls.Add(trsql);

                                        for (int i = 0; i < ss.Length; i++) {
                                            HtmlTableCell tdsql = new HtmlTableCell();



                                            tdsql.InnerText = ss[i].ValueFormatted;

                                            tdsql.Attributes["class"] = i == ss.Length - 1 ? "avnSQLLabel_Td_EOL" : "avnSQLLabel_Td";
                                            tdsql.Attributes["align"] = ss[i].ValueAlignment;

                                            string format = ss[i].ValueFormatting;
                                            if (format != "")
                                                tdsql.Attributes["style"] += ";" + format;

                                            trsql.Controls.Add(tdsql);
                                        }
                                    }

                                    div.Controls.Add(tblsql);

                                    div.Attributes["class"] = "avnSQLLabel_Frame";
                                    div.Attributes["style"] += "width:" + (f.Width - 2) + ";" + "height:" + (f.Height - 2) + ";";
                                    div.Attributes["style"] += "overflow:auto;";
                                }
                            }
                        } catch (Exception sqlexp) {
                            div.InnerHtml = "<b>Fejl!</b><br>" + sqlexp.Message.Replace("\n", "<br>") + "<br>" + sqlexp.StackTrace.Replace("\n", "<br>");

                            div.Attributes["style"] += "width:" + f.Width + ";" + "height:" + f.Height + ";";
                            div.Attributes["style"] += "overflow:auto;";
                        }
                    }
                    break;

                case "datetime":
                case "textbox":
                case "integer":
                case "absinteger":
                case "float":
                case "absfloat":
                case "emailaddress":
                    HtmlInputText ipText = new HtmlInputText();
                    ipText.Attributes["name"] = f.DatabaseColumn;
                    ipText.Attributes["id"] = f.DatabaseColumn;
                    ipText.Attributes["class"] = "textField";
                    ipText.Value = f.FormattedValue;

                    div.Controls.Add(divText);
                    div.Controls.Add(ipText);
                    break;


                case "memo":
                    HtmlTextArea ipTextarea = new HtmlTextArea();
                    ipTextarea.Attributes["name"] = f.DatabaseColumn;
                    ipTextarea.Attributes["id"] = f.DatabaseColumn;
                    ipTextarea.Attributes["class"] = "textAreaField";
                    ipTextarea.Value = f.FormattedValue;

                    div.Controls.Add(divText);
                    div.Controls.Add(ipTextarea);
                    break;

                case "checkbox":
                    HtmlInputCheckBox ipCheck = new HtmlInputCheckBox();
                    ipCheck.Attributes["name"] = f.DatabaseColumn;
                    ipCheck.Attributes["id"] = f.DatabaseColumn;
                    ipCheck.Value = f.FormattedValue;
                    ipCheck.Checked = f.FormattedValue == "Ja";

                    HtmlGenericControl cbLabel = new HtmlGenericControl("LABEL");
                    cbLabel.Attributes["for"] = f.DatabaseColumn;
                    cbLabel.InnerText = f.DatabaseColumn;

                    div.Controls.Add(ipCheck);
                    div.Controls.Add(cbLabel);

                    div.Attributes["class"] = "checkboxInput";

                    break;





                case "listview":
                    HtmlSelect lst = new HtmlSelect();
                    lst.Attributes["class"] = "selectListview";
                    lst.Multiple = true;
                    //lst.Attributes["class"] = "";
                    lst.Attributes["integralheight"] = "true";

                    div.Controls.Add(divText);
                    div.Controls.Add(lst);

                    string[] lwValues = f.FormattedValue.Split('\n');

                    List<string> vals2 = sql.AVNFields_getCustomValues(f.Id);
                    if (vals2.Count > 0) {
                        if (vals2[0].Replace("\r", "").Replace("\n", "") != "" || vals2.Count > 1) {
                            List<string> listviewSelectedValues = new List<string>();
                            listviewSelectedValues.AddRange(f.FormattedValue.Split('\n'));

                            bool litemexists = false;

                            foreach (string s in vals2) {
                                ListItem l = new ListItem(s.Replace("\r", "").Replace("\n", ""));
                                if (listviewSelectedValues.Contains(s.Replace("\r", "").Replace("\n", ""))) {
                                    l.Selected = true;
                                }
                                lst.Items.Add(l);
                            }

                            foreach (string selectThis in listviewSelectedValues) {
                                ListItem l = new ListItem(selectThis.Replace("\r", "").Replace("\n", ""));
                                l.Selected = true;
                                if (!lst.Items.Contains(l)) {
                                    lst.Items.Add(l);
                                }
                            }
                        }

                        //Make sure the chosen item is in the listview/combobox
                        if (lst.Value != "") {
                            bool valuePresent = false;

                            for (int i = 0; i < lst.Items.Count; i++) {
                                if (lst.Items[i].Value == lst.Value) {
                                    valuePresent = true;
                                    break;
                                }
                            }

                            if (!valuePresent) {
                                if (lst.Items.Count > 0)
                                    lst.Items.Insert(0, new ListItem(lst.Value));
                                else
                                    lst.Items.Add(new ListItem(lst.Value));
                            }
                        }
                    }

                    break;

                case "dropdown":
                    HtmlSelect sel = new HtmlSelect();
                    sel.Attributes["class"] = "selectDataField";

                    div.Controls.Add(divText);
                    div.Controls.Add(sel);

                    List<string> vals = sql.AVNFields_getCustomValues(f.Id);
                    if (vals.Count > 0) {
                        if (vals[0].Replace("\r", "").Replace("\n", "") != "" || vals.Count > 1) {
                            List<string> dropdownSelectedValues = new List<string>();
                            dropdownSelectedValues.AddRange(f.FormattedValue.Split('\n'));
                            bool gotSelected = false;

                            foreach (string s in vals) {
                                ListItem l = new ListItem(s.Replace("\r", "").Replace("\n", ""));
                                if (dropdownSelectedValues.Contains(s.Replace("\r", "").Replace("\n", "")) && !gotSelected) {
                                    l.Selected = true;
                                    gotSelected = true;
                                }

                                sel.Items.Add(l);
                            }

                            if (!gotSelected && dropdownSelectedValues != null && dropdownSelectedValues.Count > 0) {
                                ListItem l = new ListItem(dropdownSelectedValues[0].Replace("\r", "").Replace("\n", ""));
                                l.Selected = true;
                                sel.Items.Add(l);
                            }
                        }
                    }

                    //Make sure the chosen item is in the listview/combobox
                    if (sel.Value != "") {
                        bool valuePresent = false;

                        for (int i = 0; i < sel.Items.Count; i++) {
                            if (sel.Items[i].Value == sel.Value) {
                                valuePresent = true;
                                break;
                            }
                        }

                        if (!valuePresent) {
                            if (sel.Items.Count > 0)
                                sel.Items.Insert(0, new ListItem(sel.Value));
                            else
                                sel.Items.Add(new ListItem(sel.Value));
                        }
                    }

                    break;
            }


            return div;
        }


        /// <summary>
        /// Get the value for a field
        /// </summary>
        /// <param name="items"></param>
        /// <param name="columnName"></param>
        /// <param name="organisationId"></param>
        /// <returns></returns>
        public static string grabValue(List<TableColumnWithValue> items, string columnName, int organisationId) {
            int index = TypeCast.getTableColumnWithValueInListIndex(ref items, columnName + "_" + organisationId);
            if (index >= 0) {
                return TypeCast.ToString(items[index].Value);
            } else
                return "";
        }

        public static bool inListItems(string[] items, string value) {
            foreach (string s in items) {
                if (s.ToLower().Trim() == value.ToLower().Trim())
                    return true;
            }

            return false;
        }

        #endregion
    }

    /// <summary>
    /// AVN Field with paired saved value from database
    /// </summary>
    public class AVNFieldWithValue : AVNField {
        public AVNFieldWithValue() {

        }

        public AVNFieldWithValue(ref System.Data.SqlClient.SqlDataReader dr) : base(ref dr) {

        }

        /// <summary>
        /// Constructs a new AVN field with value
        /// </summary>
        /// <param name="f"></param>
        /// <param name="value"></param>
        public AVNFieldWithValue(AVNField f, object value) {
            this.AlternateText = f.AlternateText;
            this.AvnId = f.AvnId;
            this.CustomFieldValues = f.CustomFieldValues;
            this.DatabaseColumn = f.DatabaseColumn;
            this.FieldType = f.FieldType;
            this.Height = f.Height;
            this.InnerCSS = f.InnerCSS;
            this.OuterCSS = f.OuterCSS;
            this.RequiredState = f.RequiredState;
            this.TabIndex = f.TabIndex;
            this.Value = value;
            this.Width = f.Width;
            this.X = f.X;
            this.Y = f.Y;
            this.Icon = f.Icon;
            this.Statistics = f.Statistics;
            this.ListIndex = f.ListIndex;
            this.ListWidth = f.ListWidth;

            this.DataClassificationId = f.DataClassificationId;
            this.AnonymizationId = f.AnonymizationId;

            this.Id = f.Id;
        }

        /// <summary>
        /// Constructs a new AVN field with value
        /// </summary>
        /// <param name="avnId"></param>
        /// <param name="databaseColumn"></param>
        /// <param name="value"></param>
        /// <param name="FieldPairing"></param>
        public AVNFieldWithValue(int avnId, string databaseColumn, object value, List<AVNField> FieldPairing) {
            this.AvnId = avnId;
            this.DatabaseColumn = databaseColumn;
            this.Value = value;

            this.Id = TypeCast.ToInt(databaseColumn.Split('_')[0]);

            foreach (AVNField f in FieldPairing) {
                if (f.Id == this.Id) {
                    this.AlternateText = f.AlternateText;
                    this.CustomFieldValues = f.CustomFieldValues;
                    this.FieldType = f.FieldType;
                    this.Height = f.Height;
                    this.InnerCSS = f.InnerCSS;
                    this.OuterCSS = f.OuterCSS;
                    this.RequiredState = f.RequiredState;
                    this.TabIndex = f.TabIndex;
                    this.Width = f.Width;
                    this.X = f.X;
                    this.Y = f.Y;
                    this.Icon = f.Icon;

                    break;
                }
            }
        }

        public object Value { get; set; }

        /// <summary>
        /// Gets the database value from a field
        /// </summary>
        public object DatabaseValue {
            get {
                if (this.Value == null)
                    return DBNull.Value;

                switch (this.FieldType) {
                    case "datetime":
                        if (this.Value != null && TypeCast.ToString(this.Value) != "") {
                            object v = TypeCast.ToDateTimeOrNull(this.Value);
                            if (v != null) {
                                return ((DateTime)v).ToString("yyyy-MM-dd HH:mm:ss");
                            } else
                                return DBNull.Value;
                        } else
                            return DBNull.Value;

                    case "float":
                    case "absfloat":
                        if (this.Value != null && TypeCast.ToString(this.Value) != "") {
                            string v = TypeCast.ToString(this.Value);
                            v = v.Replace(".", "").Replace(",", ".");
                            return v;
                        } else
                            return DBNull.Value;

                    case "integer":
                    case "absinteger":
                        if (this.Value != null && TypeCast.ToString(this.Value) != "")
                            return TypeCast.ToLong(this.Value).ToString();
                        else
                            return DBNull.Value;

                    case "textbox":
                    case "emailaddress":
                    case "memo":
                    case "dropdown":
                        return TypeCast.ToString(this.Value);

                    case "checkbox":
                        return TypeCast.ToBool(this.Value) == true;

                    case "listview":
                        return TypeCast.ToString(this.Value);
                }

                return DBNull.Value;
            }
        }

        /// <summary>
        /// Gets a nicely formatted database value from a field (i.e. datetimes will be formatted into (dd-MM-åååå)
        /// </summary>
        public string FormattedValue {
            get {
                if (this.Value == null)
                    return "";

                switch (this.FieldType) {
                    case "datetime":
                        if (this.Value != null) {
                            object v = TypeCast.ToDateTimeOrNull(this.Value);
                            if (v != null)
                                return ((DateTime)v).ToString("dd-MM-yyyy");
                            else
                                return "";
                        } else
                            return "";

                    case "float":
                    case "absfloat":
                        if (this.Value != null)
                            return TypeCast.ToDecimal(this.Value).ToString("#,##0.00");
                        else
                            return "";

                    case "integer":
                    case "absinteger":
                        return TypeCast.ToString(this.Value);

                    case "textbox":
                    case "emailaddress":
                    case "memo":
                    case "dropdown":
                        return TypeCast.ToString(this.Value);

                    case "checkbox":
                        return TypeCast.ToBool(this.Value) == true ? "Ja" : "Nej";

                    case "listview":
                        return TypeCast.ToString(this.Value);
                }

                return "";

            }
        }
    }


    public class SQLLabelReplaceableValue {
        public string Key { get; set; }
        public string ReplaceWith { get; set; }

        public SQLLabelReplaceableValue() {

        }

        public SQLLabelReplaceableValue(string key, string replaceWith) {
            Key = key;
            ReplaceWith = replaceWith;
        }
    }
}
