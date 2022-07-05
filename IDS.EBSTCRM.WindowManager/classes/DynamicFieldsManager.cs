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

namespace IDS.EBSTCRM.WindowManager {
    public static class DynamicFieldsManager {
        public static string[] CountryList = new string[] { "Denmark", "-", "Albania", "Algeria", "American Samoa", "Andorra", "Angola", "Anguilla", "Antigua & Barbuda", "Argentina", "Armenia", "Aruba", "Australia", "Austria", "Azerbaijan", "Azores", "Bahamas", "Bahrain", "Bangladesh", "Barbados", "Belarus", "Belgium", "Belize", "Benin", "Bermuda", "Bhutan", "Bolivia", "Bonaire", "Bosnia", "Botswana", "Brazil", "British Virgin Isles", "Brunei", "Bulgaria", "Burkina Faso", "Burundi", "Cambodia", "Cameroon", "Canada", "Canary Islands", "Cape Verde", "Cayman Islands", "Central African Republic", "Chad", "Channel Islands", "Chile", "China", "Colombia", "Cook Islands", "Costa Rica", "Cote DIvoire", "Croatia", "Curacao", "Cyprus", "Czech Republic", "Dem Rep of Congo", "Denmark", "Djibouti", "Dominica", "Dominican Republic", "Ecuador", "Egypt", "El Salvador", "England", "Equitorial Guinea", "Eritrea", "Estonia", "Ethiopia", "Faeroe Islands", "Fiji", "Finland", "France", "French Guiana", "French Polynesia", "Gabon", "Gambia", "Georgia", "Germany", "Ghana", "Gibraltar", "Greece", "Greenland", "Grenada", "Guadeloupe", "Guam", "Guatemala", "Guinea", "Guinea-Bissau", "Guyana", "Haiti", "Holland", "Honduras", "Hong Kong", "Hungary", "Iceland", "India", "Indonesia", "Iraq", "Israel", "Italy", "IvoryCoast", "Jamaica", "Japan", "Jordan", "Kazakhstan", "Kenya", "Kiribati", "Kosrae", "Kuwait", "Kyrgyzstan", "Laos", "Latvia", "Lebanon", "Lesotho", "Liberia", "Libya", "Liechtenstein", "Lithuania", "Luxembourg", "Macau", "Macedonia", "Madagascar", "Madeira", "Malawi", "Malaysia", "Maldives", "Mali", "Malta", "Marshall Islands", "Martinique", "Mauritania", "Mauritius", "Mexico", "Micronesia", "Moldova", "Monaco", "Mongolia", "Montserrat", "Morocco", "Mozambique", "Myanmar", "N. Mariana Islands", "Namibia", "Nauru", "Nepal", "Netherlands", "Netherlands Antilles", "New Caledonia", "New Zealand", "Nicaragua", "Niger", "Nigeria", "Norfolk Island", "Northern Ireland", "Norway", "Oman", "Other-Not Shown", "Pakistan", "Palau", "Panama", "Papua New Guinea", "Paraguay", "Peoples Rep of China", "Peru", "Philippines", "Pitcairn Island", "Poland", "Ponape", "Portugal", "Puerto Rico", "Qatar", "Republic of Congo", "Republic of Ireland", "Republic of Yemen", "Reunion", "Romania", "Rota", "Russia", "Rwanda", "Saba", "Saipan", "San Marino", "SaoTome and Principe", "Saudi Arabia", "Scotland", "Senegal", "Seychelles", "Sierra Leone", "Singapore", "Slovakia", "Slovenia", "Solomon Islands", "South Africa", "South Korea", "Spain", "Sri Lanka", "St. Barthelemy", "St. Christopher", "St. Croix", "St. Eustatius", "St. John", "St. Kitts & Nevis", "St. Lucia", "St. Maarten", "St. Martin", "St. Thomas", "St. Vincent and Grenadine", "Sudan", "Suriname", "Swaziland", "Sweden", "Switzerland", "Syria", "Tahiti", "Taiwan", "Tajikistan", "Tanzania", "Thailand", "Tinian", "Togo", "Tonga", "Tortola", "Trinidad & Tobago", "Truk", "Tunisia", "Turkey", "Turkmenistan", "Turks & Caicos Islands", "Tuvalu", "Uganda", "Ukraine", "Union Island", "United Arab Emirates", "United Kingdom", "United States", "Uruguay", "US Virgin Islands", "Uzbekistan", "Vanuatu", "Vatican City State", "Venezuela", "Vietnam", "Virgin Gorda", "Wake Island", "Wales", "Wallis & Futuna Islands", "Western Samoa", "Yap", "Yugoslavia", "Zambia", "Zimbabwe" };

        public static HtmlTable createElement(IDS.EBSTCRM.Base.DynamicField f, bool inDesignMode, string type, ref SQLDB sql, ref User U, bool mayUseSuggestedFields, int baseTabIndex1, int baseTabIndex2, bool IsIE, out string JavaScript, List<SQLLabelReplaceableValue> SQLLabelReplaceables, List<Base.Classes.DataClassification> dataClassfications = null) {
            JavaScript = "";
            //Fix for local units, making sure they are encouraged to fill in all values.
            if (f.RequiredState == 1 &&
                    mayUseSuggestedFields &&
                    (U.Organisation.Type == Organisation.OrganisationType.County ||
                        U.Organisation.Type == Organisation.OrganisationType.Sattelite) &&
                        f.BaseOrganisationId != f.OrganisationId)
                f.RequiredState = 2;

            if (f.NoInherit(U.OrganisationId) && f.BaseOrganisationId != f.OrganisationId)
                f.ShowInListview = 0;

            HtmlTable tbl = new HtmlTable();
            HtmlTableRow tr = new HtmlTableRow();
            HtmlTableCell td = new HtmlTableCell();

            tbl.CellPadding = 0;
            tbl.CellSpacing = 0;
            tbl.Border = 0;

            tbl.Controls.Add(tr);
            tr.Controls.Add(td);



            string[] tmp = f.DatabaseTable.Split('_');
            int ownerId = TypeCast.ToInt(tmp[tmp.Length - 1]);


            tbl.Attributes["dbTable"] = f.DatabaseTable;
            tbl.Attributes["dbBaseOrg"] = f.BaseOrganisationId.ToString();
            tbl.Attributes["dbOrg"] = f.OrganisationId.ToString();

            tbl.Attributes["dbName"] = f.DatabaseColumn;
            tbl.Attributes["dbDefaultValue"] = f.DefaultFieldValue;

            tbl.Attributes["dbFieldType"] = f.ViewState == "" ? "null" : f.ViewState;
            tbl.Attributes["dbContentType"] = f.FieldType;
            tbl.Attributes["dbId"] = f.Id.ToString();
            tbl.Attributes["dbOwnerId"] = (f.OwnerId > 0 ? f.OwnerId.ToString() : "");
            tbl.Attributes["UseInReports"] = f.UseInReports.ToString();

            var cssclass = "";
            if (inDesignMode) {
                cssclass = "FieldDesignControl";
            } else {
                cssclass = "FieldRealTimeControl";

                string[] validSharedFieldType = new string[] { "datetime", "textbox", "integer", "absinteger", "float", "absfloat", "emailaddress", "memo", "checkbox", "listview", "dropdown" };
                if ((f.SharedWith == DynamicField.ShareMethod.FullShared || f.SharedWith == DynamicField.ShareMethod.PartialShared)
                    && Array.IndexOf(validSharedFieldType, f.FieldType) > -1
                    && !f.InnerCSS.ToLower().Replace(" ", "").Contains("display:none")) {

                    if (f.FieldType == "checkbox") {
                        cssclass = cssclass + " sharedCheckbox";
                    } else {
                        cssclass = cssclass + " sharedData";
                    }
                }
            }
            tbl.Attributes["class"] = cssclass;

            tbl.Attributes["style"] += "left:" + (type == "SMV" ? f.X1 : f.X2) + ";" +
                        "top:" + (type == "SMV" ? f.Y1 : f.Y2) + ";" +
                        "width:" + (type == "SMV" ? f.Width1 : f.Width2) + ";" +
                        "height:" + (type == "SMV" ? f.Height1 : f.Height2) + ";";

            td.Attributes["style"] = "width:" + (type == "SMV" ? f.Width1 : f.Width2) + ";" +
                        "height:" + (type == "SMV" ? f.Height1 : f.Height2) + ";";

            if (inDesignMode) {
                tbl.Attributes["locked"] = (ownerId == f.OrganisationId ? "" : "Locked");
                tbl.Attributes["style"] += "cursor:hand;";
                tbl.Attributes["FollowAny"] = f.FollowAny == "true" ? "true" : "null";
                tbl.Attributes["NoInherit"] = f.NoInherit_Obsolete ? "1" : "0";

                if (f.SharedWith == DynamicField.ShareMethod.NotShared)
                    tbl.Attributes["SharedWith"] = "Ingen nedarvning";
                else if (f.SharedWith == DynamicField.ShareMethod.FullShared)
                    tbl.Attributes["SharedWith"] = "Fuld nedarvning";
                else {
                    string sharedWith = "";
                    if (f.SharedWithOrganisations != null) {
                        foreach (int i in f.SharedWithOrganisations) {
                            sharedWith += (sharedWith == "" ? "" : ",") + i.ToString();
                        }
                    }
                    tbl.Attributes["SharedWith"] = "Delvis nedarvning\t" + sharedWith;
                }

                tbl.Attributes["onclick"] += "selectObject(this);";
                tbl.Attributes["onmousedown"] += "mouseDown(this);";
                tbl.Attributes["onmouseup"] += "mouseUp(this);";
                tbl.Attributes["alternateText"] = f.AlternateText;

                if (f.ViewState != "" && f.ViewState != type)
                    tbl.Attributes["style"] += "visibility:hidden;";

                tbl.Attributes["style"] += f.OuterCSS.Replace("visibility:hidden;", "");
            } else {
                tbl.Attributes["title"] = f.AlternateText;
                tbl.Attributes["style"] += f.OuterCSS;
            }

            if (f.DatabaseColumn == "AfkrydsningsboksMedDeling") {
                System.Diagnostics.Debug.WriteLine("No Inherit check");
            }

            if (f.BaseOrganisationId != U.OrganisationId && f.NoInherit(U.OrganisationId)) {
                tbl.Attributes["style"] += "visibility:hidden;";
            }

            tbl.Attributes["dbX1"] += f.X1;
            tbl.Attributes["dbY1"] += f.Y1;
            tbl.Attributes["dbW1"] += f.Width1;
            tbl.Attributes["dbH1"] += f.Height1;

            tbl.Attributes["dbX2"] += f.X2;
            tbl.Attributes["dbY2"] += f.Y2;
            tbl.Attributes["dbW2"] += f.Width2;
            tbl.Attributes["dbH2"] += f.Height2;

            //tbl.Attributes["style"] += "padding:3px;";

            // Data classification changes
            tbl.Attributes["dbClassificationId"] = f.DataClassificationId.ToString();
            tbl.Attributes["dbAnonymizationId"] = f.AnonymizationId.ToString();

            tbl.Attributes["reasontype"] = f.ReasonType.ToString();
            tbl.Attributes["reasondesc"] = f.Reason.ToString();

            var anonymizationRequired = 0;
            var dcFieldType = sql.GetDataClassificationByFieldType().FirstOrDefault(w => w.FieldType == f.FieldType && w.Id == f.DataClassificationId);
            if (dcFieldType != null) {
                anonymizationRequired = (dcFieldType.EnforceAnonymization ? 1 : 0);
            }
            tbl.Attributes["dbAnonymizationRequired"] = anonymizationRequired.ToString();


            var showContactPicker = false;
            if (f.DataClassificationId == 200) { showContactPicker = true; }

            switch (f.FieldType) {
                case "button":
                    HtmlInputButton cmdButton = new HtmlInputButton();
                    td.Controls.Add(cmdButton);

                    if (!inDesignMode) {
                        cmdButton.Attributes["class"] = "designerButton";

                        cmdButton.Value = f.DatabaseColumn;
                        cmdButton.Attributes["tabIndex"] = (type == "SMV" ? f.TabIndex1 : f.TabIndex2).ToString();
                        //tbl.Attributes["onchange"] = "locateDoublesFromField(this);";
                        cmdButton.Attributes["oldValue"] = cmdButton.Value;
                        cmdButton.Attributes["style"] += "width:" + (type == "SMV" ? f.Width1 : f.Width2) + "px;";

                        cmdButton.ID = f.DatabaseColumn;
                        cmdButton.Attributes["title"] = f.AlternateText;

                        //string onClick = "";
                        JavaScript = "\t\tfunction OnClick_" + f.Id + "(sender) {\n";

                        foreach (string s in sql.dynamicFields_getCustomValues(f.Id, false)) {
                            if (s.Trim() != "")
                                JavaScript += "\t\t\t" + s + "\r\n";
                        }

                        JavaScript += "\n\t\t};\r\n";

                        cmdButton.Attributes["onclick"] = "OnClick_" + f.Id + "(this);";
                    } else {
                        cmdButton.Attributes["class"] = "designerButton";

                        cmdButton.Attributes["readonly"] += "true";
                        cmdButton.Value = f.DatabaseColumn;

                        foreach (string s in sql.dynamicFields_getCustomValues(f.Id, false)) {
                            tbl.Attributes["SelDropDownItems"] += s + "\n";
                        }

                        var positiveList = f.PositiveFieldValue.Split(',').Where(w => w != "").ToList();
                        foreach (string p in positiveList) {
                            tbl.Attributes["PositiveDropDownItems"] += p + "\n";
                        }
                    }

                    cmdButton.Name = cmdButton.ID;

                    if (inDesignMode)
                        tbl.Attributes["tabIndex"] = (type == "SMV" ? f.TabIndex1 : f.TabIndex2).ToString();
                    tbl.Attributes["RequiredState"] = f.RequiredState.ToString();

                    cmdButton.Attributes["style"] += f.InnerCSS;

                    break;

                case "map":
                    tbl.Attributes["tabIndex1"] = f.TabIndex1.ToString();
                    tbl.Attributes["tabIndex2"] = f.TabIndex2.ToString();
                    tbl.Attributes["ShowInListview"] = f.ShowInListview.ToString();
                    tbl.Attributes["ListviewIndex"] = f.ListviewIndex.ToString();
                    tbl.Attributes["DataLink"] = f.DataLink.ToString();
                    tbl.Attributes["RequiredState"] = f.RequiredState.ToString();

                    //Set properties
                    List<string> mapProps = sql.dynamicFields_getCustomValues(f.Id, false);
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
                        divMap.Attributes["style"] += f.InnerCSS + ";width:" + (type == "SMV" ? f.Width1 : f.Width2) + "px;" +
                        "height:" + (type == "SMV" ? f.Height1 : f.Height2) + "px;";
                        divMap.Attributes["googlemaps"] = "true";
                    }


                    break;

                case "datetime":
                    HtmlInputText txtDateTime = new HtmlInputText();
                    td.Controls.Add(txtDateTime);

                    if (!inDesignMode) {

                        txtDateTime.Attributes["class"] = "designerTextbox";
                        txtDateTime.Attributes["tabIndex"] = (type == "SMV" ? (baseTabIndex1 + f.TabIndex1).ToString() : (baseTabIndex2 + f.TabIndex2).ToString());
                        //tbl.Attributes["onchange"] = "locateDoublesFromField(this);";
                        txtDateTime.Attributes["oldValue"] = txtDateTime.Value;
                        txtDateTime.Attributes["style"] += "width:" + (f.Width1 - 22) + "px;";
                        txtDateTime.ID = f.DatabaseTable + "_" + f.DatabaseColumn + "_" + ownerId;
                        txtDateTime.Attributes["title"] = f.AlternateText;

                        HtmlTableCell tdBtn = new HtmlTableCell();
                        tr.Controls.Add(tdBtn);
                        tdBtn.VAlign = "middle";
                        tbl.Align = "left";

                        HtmlGenericControl calBtn = new HtmlGenericControl("DIV");
                        calBtn.Attributes["class"] = "designerDatePickerButton";
                        calBtn.Attributes["onmouseover"] = "this.className='designerDatePickerButtonHover';";
                        calBtn.Attributes["onmouseout"] = "this.className='designerDatePickerButton';";
                        calBtn.Attributes["onclick"] = "pickDateToControl('" + txtDateTime.ID + "');";

                        if (!IsIE)
                            calBtn.Attributes["style"] += "height:16px;";
                        tdBtn.Controls.Add(calBtn);
                        if (f.InnerCSS.ToLower().Contains("display:none")) {
                            tbl.Attributes["style"] += ";display:none;";
                        }
                    } else {

                        txtDateTime.Attributes["class"] = "designerDatePicker";
                        txtDateTime.Attributes["readonly"] += "true";
                        txtDateTime.Value = f.DatabaseColumn;
                    }

                    txtDateTime.Name = txtDateTime.ID;

                    tbl.Attributes["tabIndex1"] = f.TabIndex1.ToString();
                    tbl.Attributes["tabIndex2"] = f.TabIndex2.ToString();
                    tbl.Attributes["RequiredState"] = f.RequiredState.ToString();

                    txtDateTime.Attributes["style"] += f.InnerCSS;

                    // When field is not rendered for Field designer page
                    if (!inDesignMode) {

                        var isSensitive = false;
                        if (dataClassfications != null && dataClassfications.Any()) {
                            var classification = dataClassfications.FirstOrDefault(w => w.Id == f.DataClassificationId);
                            if (classification != null && !string.IsNullOrEmpty(classification.ExportMask)) {

                                isSensitive = true;

                                // Override textbox width
                                txtDateTime.Attributes["class"] = "designerTextbox Locked";
                                txtDateTime.Attributes["disabled"] = "disabled";
                                txtDateTime.Attributes["sensitive"] = "1";
                                txtDateTime.Attributes["style"] = "width:" + ((f.Width1 - 22) - 18) + "px;";

                                // Routine to add image button for classified data
                                HtmlInputImage img = new HtmlInputImage();
                                img.Attributes["title"] = "Vis indhold";
                                img.Attributes["src"] = "/images/eye32.png";
                                img.Attributes["width"] = "16";
                                img.Attributes["height"] = "16";
                                img.Attributes["style"] = "margin-left:3px;";
                                img.Attributes["data-target-id"] = f.Id.ToString();
                                img.Attributes["data-target-name"] = f.DatabaseTable + "_" + f.DatabaseColumn + "_" + ownerId;
                                img.Attributes["data-target-type"] = (f.DatabaseTable.IndexOf("contacts", StringComparison.OrdinalIgnoreCase) > 0 ? "contact" : "company");
                                img.Attributes["onclick"] = "Show(this);";

                                HtmlTableCell tdSensitive = new HtmlTableCell();
                                tr.Controls.Add(tdSensitive);
                                tdSensitive.VAlign = "middle";
                                tbl.Align = "left";
                                tdSensitive.Controls.Add(img);
                            }
                        }

                        if (showContactPicker && f.DatabaseTable.IndexOf("companies", StringComparison.OrdinalIgnoreCase) > -1) {
                            if (isSensitive) {
                                txtDateTime.Attributes["style"] = "width:" + ((Convert.ToInt32(type == "SMV" ? f.Width1 : f.Width2) - 22) - 45) + "px;";
                            } else {
                                txtDateTime.Attributes["style"] = "width:" + ((Convert.ToInt32(type == "SMV" ? f.Width1 : f.Width2) - 22) - 25) + "px;";
                            }

                            // Routine to add image button for positive field
                            HtmlInputImage img = new HtmlInputImage();
                            img.Attributes["title"] = "Vælg den/de kontaktpersoner som feltet vedrører.";
                            img.Attributes["src"] = "/images/checkNames.png";
                            img.Attributes["width"] = "16";
                            img.Attributes["height"] = "16";
                            img.Attributes["style"] = "margin-left:3px;";
                            img.Attributes["data-field-id"] = f.Id.ToString();
                            img.Attributes["data-object-type"] = "120";
                            img.Attributes["data-screen-ref-key"] = Guid.NewGuid().ToString();
                            img.Attributes["onclick"] = "Expiration(this);";

                            HtmlTableCell tdContactPicker = new HtmlTableCell();
                            tr.Controls.Add(tdContactPicker);
                            tdContactPicker.VAlign = "middle";
                            tbl.Align = "left";
                            tdContactPicker.Controls.Add(img);
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
                    txt.Attributes["class"] = "designerTextbox" + (ownerId == f.OrganisationId || !inDesignMode ? "" : "Locked");
                    if (inDesignMode) txt.Attributes["readonly"] += "true";
                    if (!inDesignMode) {
                        txt.Attributes["tabIndex"] = (type == "SMV" ? (baseTabIndex1 + f.TabIndex1).ToString() : (baseTabIndex2 + f.TabIndex2).ToString());
                        //tbl.Attributes["onchange"] = "locateDoublesFromField(this);";
                        txt.Attributes["oldValue"] = txt.Value;

                        txt.Attributes["style"] += "width:" + (f.Width1 - 4) + "px;";
                    } else
                        txt.Value = f.DatabaseColumn;

                    tbl.Attributes["tabIndex1"] = f.TabIndex1.ToString();
                    tbl.Attributes["tabIndex2"] = f.TabIndex2.ToString();
                    tbl.Attributes["ShowInListview"] = f.ShowInListview.ToString();
                    tbl.Attributes["ListviewIndex"] = f.ListviewIndex.ToString();
                    tbl.Attributes["DataLink"] = f.DataLink.ToString();
                    tbl.Attributes["RequiredState"] = f.RequiredState.ToString();

                    if (!inDesignMode) txt.ID = f.DatabaseTable + "_" + f.DatabaseColumn + "_" + ownerId;
                    txt.Name = txt.ID;
                    if (!inDesignMode) txt.Attributes["title"] = f.AlternateText;

                    txt.Attributes["style"] += f.InnerCSS;


                    // When field is not rendered for Field designer page
                    if (!inDesignMode) {

                        var isSensitive = false;
                        if (dataClassfications != null && dataClassfications.Any()) {
                            var classification = dataClassfications.FirstOrDefault(w => w.Id == f.DataClassificationId);
                            if (classification != null && !string.IsNullOrEmpty(classification.ExportMask)) {

                                isSensitive = true;

                                // Override textbox width
                                txt.Attributes["class"] = "designerTextbox Locked";
                                txt.Attributes["disabled"] = "disabled";
                                txt.Attributes["sensitive"] = "1";
                                txt.Attributes["style"] = "width:" + ((f.Width1 - 4) - 18) + "px;";

                                // Routine to add image button for classified data
                                HtmlInputImage img = new HtmlInputImage();
                                img.Attributes["title"] = "Vis indhold";
                                img.Attributes["src"] = "/images/eye32.png";
                                img.Attributes["width"] = "16";
                                img.Attributes["height"] = "16";
                                img.Attributes["style"] = "margin-left:3px;";
                                img.Attributes["data-target-id"] = f.Id.ToString();
                                img.Attributes["data-target-name"] = f.DatabaseTable + "_" + f.DatabaseColumn + "_" + ownerId;
                                img.Attributes["data-target-type"] = (f.DatabaseTable.IndexOf("contacts", StringComparison.OrdinalIgnoreCase) > 0 ? "contact" : "company");
                                img.Attributes["onclick"] = "Show(this);";
                                td.Controls.Add(img);
                            }
                        }

                        if (showContactPicker && f.DatabaseTable.IndexOf("companies", StringComparison.OrdinalIgnoreCase) > -1) {
                            if (isSensitive) {
                                txt.Attributes["style"] = "width:" + (Convert.ToInt32(type == "SMV" ? f.Width1 : f.Width2) - 45) + "px;";
                            } else {
                                txt.Attributes["style"] = "width:" + (Convert.ToInt32(type == "SMV" ? f.Width1 : f.Width2) - 25) + "px;";
                            }

                            // Routine to add image button for positive field
                            HtmlInputImage img = new HtmlInputImage();
                            img.Attributes["title"] = "Vælg den/de kontaktpersoner som feltet vedrører.";
                            img.Attributes["src"] = "/images/checkNames.png";
                            img.Attributes["width"] = "16";
                            img.Attributes["height"] = "16";
                            img.Attributes["style"] = "margin-left:3px;";
                            img.Attributes["data-field-id"] = f.Id.ToString();
                            img.Attributes["data-object-type"] = "120";
                            img.Attributes["data-screen-ref-key"] = Guid.NewGuid().ToString();
                            img.Attributes["onclick"] = "Expiration(this);";
                            td.Controls.Add(img);
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

                        foreach (string s in sql.dynamicFields_getCustomValues(f.Id, false)) {
                            tbl.Attributes["SelDropDownItems"] += s + "\n";
                        }

                        var positiveList = f.PositiveFieldValue.Split(',').Where(w => w != "").ToList();
                        foreach (string p in positiveList) {
                            tbl.Attributes["PositiveDropDownItems"] += p + "\n";
                        }

                    } else {
                        if (f.DatabaseColumn == "VHSJ kontakter") {

                        }

                        if (f.OrganisationId == U.OrganisationId || f.OrganisationId == f.BaseOrganisationId) {
                            string sql2Exec = "";
                            foreach (string s in sql.dynamicFields_getCustomValues(f.Id, true)) {
                                sql2Exec += s + "\r\n";
                            }

                            if (sql2Exec == "" && f.OwnerId != f.Id && f.OwnerId > 0) {
                                foreach (string s in sql.dynamicFields_getCustomValues(f.OwnerId, true)) {
                                    sql2Exec += s + "\r\n";
                                }
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

                                    sql2Exec = sql2Exec.Replace("%currentuser%", U.Id);

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
                                            sqllbl.Attributes["style"] += "width:" + ((type == "SMV" ? f.Width1 : TypeCast.ToInt(f.Width2)) - 2) + ";" + "height:" + ((type == "SMV" ? f.Height1 : TypeCast.ToInt(f.Height2)) - 2) + ";";
                                            sqllbl.Attributes["style"] += "overflow:auto;";
                                        }
                                    }
                                } catch (Exception sqlexp) {
                                    sqllbl.InnerHtml = "<b>Fejl!</b><br>" + sqlexp.Message.Replace("\n", "<br>") + "<br>" + sqlexp.StackTrace.Replace("\n", "<br>");

                                    sqllbl.Attributes["style"] += "width:" + (type == "SMV" ? f.Width1 : f.Width2) + ";" + "height:" + (type == "SMV" ? f.Height1 : f.Height2) + ";";
                                    sqllbl.Attributes["style"] += "overflow:auto;";
                                }
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
                    //txa.Value = f.DatabaseColumn; //inDesignMode ? f.DatabaseColumn : grabValue(values, f.DatabaseTable + "_" + f.DatabaseColumn, f.BaseOrganisationId);
                    txa.Attributes["class"] = "designerTextarea" + (ownerId == f.OrganisationId || !inDesignMode ? "" : "Locked");
                    if (inDesignMode) txa.Attributes["readonly"] += "true";
                    txa.Attributes["tabIndex"] = (type == "SMV" ? (baseTabIndex1 + f.TabIndex1).ToString() : (baseTabIndex2 + f.TabIndex2).ToString());

                    tbl.Attributes["tabIndex1"] = f.TabIndex1.ToString();
                    tbl.Attributes["tabIndex2"] = f.TabIndex2.ToString();
                    tbl.Attributes["ShowInListview"] = f.ShowInListview.ToString();
                    tbl.Attributes["ListviewIndex"] = f.ListviewIndex.ToString();
                    tbl.Attributes["DataLink"] = f.DataLink.ToString();
                    tbl.Attributes["RequiredState"] = f.RequiredState.ToString();

                    if (!inDesignMode) txa.ID = f.DatabaseTable + "_" + f.DatabaseColumn + "_" + ownerId;
                    txa.Name = txa.ID;
                    if (!inDesignMode) txa.Attributes["title"] = f.AlternateText;
                    txa.Attributes["style"] += f.InnerCSS;

                    // When field is not rendered for Field designer page
                    if (!inDesignMode) {

                        var isSensitive = false;
                        if (dataClassfications != null && dataClassfications.Any()) {
                            var classification = dataClassfications.FirstOrDefault(w => w.Id == f.DataClassificationId);
                            if (classification != null && !string.IsNullOrEmpty(classification.ExportMask)) {

                                isSensitive = true;
                                // Override textbox width
                                txa.Attributes["class"] = "designerTextbox Locked";
                                txa.Attributes["disabled"] = "disabled";
                                txa.Attributes["sensitive"] = "1";
                                txa.Attributes["style"] = "width:" + (Convert.ToInt32(type == "SMV" ? f.Width1 : f.Width2) - 22) + "px;";

                                // Routine to add image button for classified data
                                HtmlInputImage img = new HtmlInputImage();
                                img.Attributes["title"] = "Vis indhold";
                                img.Attributes["src"] = "/images/eye32.png";
                                img.Attributes["width"] = "16";
                                img.Attributes["height"] = "16";
                                img.Attributes["style"] = "margin-left:3px;";
                                img.Attributes["data-target-id"] = f.Id.ToString();
                                img.Attributes["data-target-name"] = f.DatabaseTable + "_" + f.DatabaseColumn + "_" + ownerId;
                                img.Attributes["data-target-type"] = (f.DatabaseTable.IndexOf("contacts", StringComparison.OrdinalIgnoreCase) > 0 ? "contact" : "company");
                                img.Attributes["onclick"] = "Show(this);";
                                td.Controls.Add(img);
                            }
                        }

                        if (showContactPicker && f.DatabaseTable.IndexOf("companies", StringComparison.OrdinalIgnoreCase) > -1) {
                            if (isSensitive) {
                                txa.Attributes["style"] = "width:" + (Convert.ToInt32(type == "SMV" ? f.Width1 : f.Width2) - 45) + "px;";
                            } else {
                                txa.Attributes["style"] = "width:" + (Convert.ToInt32(type == "SMV" ? f.Width1 : f.Width2) - 25) + "px;";
                            }

                            // Routine to add image button for positive field
                            HtmlInputImage img = new HtmlInputImage();
                            img.Attributes["title"] = "Vælg den/de kontaktpersoner som feltet vedrører.";
                            img.Attributes["src"] = "/images/checkNames.png";
                            img.Attributes["width"] = "16";
                            img.Attributes["height"] = "16";
                            img.Attributes["style"] = "margin-left:3px;";
                            img.Attributes["data-field-id"] = f.Id.ToString();
                            img.Attributes["data-object-type"] = "120";
                            img.Attributes["data-screen-ref-key"] = Guid.NewGuid().ToString();
                            img.Attributes["onclick"] = "Expiration(this);";
                            td.Controls.Add(img);
                        }
                    }

                    break;

                case "checkbox":
                    HtmlInputCheckBox chb = new HtmlInputCheckBox();
                    td.Controls.Add(chb);
                    chb.Attributes["tabIndex"] = (type == "SMV" ? (baseTabIndex1 + f.TabIndex1).ToString() : (baseTabIndex2 + f.TabIndex2).ToString());

                    tbl.Attributes["tabIndex1"] = f.TabIndex1.ToString();
                    tbl.Attributes["tabIndex2"] = f.TabIndex2.ToString();
                    tbl.Attributes["ShowInListview"] = f.ShowInListview.ToString();
                    tbl.Attributes["ListviewIndex"] = f.ListviewIndex.ToString();
                    tbl.Attributes["DataLink"] = f.DataLink.ToString();
                    tbl.Attributes["RequiredState"] = f.RequiredState.ToString();

                    if (!inDesignMode) {
                        chb.ID = f.DatabaseTable + "_" + f.DatabaseColumn + "_" + ownerId;
                        //chb.Checked = grabValue(values, f.DatabaseTable + "_" + f.DatabaseColumn, f.BaseOrganisationId) == "True";
                    }
                    chb.Name = chb.ID;
                    if (!inDesignMode) chb.Attributes["title"] = f.AlternateText;
                    chb.Attributes["style"] += f.InnerCSS;

                    // When field is not rendered for Field designer page
                    if (!inDesignMode) {
                        if (showContactPicker && f.DatabaseTable.IndexOf("companies", StringComparison.OrdinalIgnoreCase) > -1) {
                            //if (isSensitive) {
                            //    sel.Attributes["style"] = "width:" + (Convert.ToInt32(type == "SMV" ? f.Width1 : f.Width2) - 45) + "px;";
                            //} else {
                            //    sel.Attributes["style"] = "width:" + (Convert.ToInt32(type == "SMV" ? f.Width1 : f.Width2) - 25) + "px;";
                            //}

                            // Routine to add image button for positive field
                            HtmlInputImage img = new HtmlInputImage();
                            img.Attributes["title"] = "Vælg den/de kontaktpersoner som feltet vedrører.";
                            img.Attributes["src"] = "/images/checkNames.png";
                            img.Attributes["width"] = "16";
                            img.Attributes["height"] = "16";
                            img.Attributes["style"] = "margin-left:3px;";
                            img.Attributes["data-field-id"] = f.Id.ToString();
                            img.Attributes["data-object-type"] = "120";
                            img.Attributes["data-screen-ref-key"] = Guid.NewGuid().ToString();
                            img.Attributes["onclick"] = "Expiration(this);";

                            HtmlTableCell tdContactPicker = new HtmlTableCell();
                            tr.Controls.Add(tdContactPicker);
                            tdContactPicker.VAlign = "middle";
                            tbl.Align = "left";
                            tdContactPicker.Controls.Add(img);
                        }
                    }

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

                case "externaldatalink":
                    if (inDesignMode) {
                        HtmlImage i = new HtmlImage();
                        HtmlGenericControl d = new HtmlGenericControl("DIV");

                        i.Src = "images/designFields/externaldatalink" + (ownerId == f.OrganisationId ? "" : "Locked") + ".gif";
                        i.Attributes["style"] = "width:16px;height:16px;";
                        i.Align = "left";
                        tbl.Attributes["DataSource"] = f.DataSource;
                        tbl.Attributes["tabIndex1"] = f.TabIndex1.ToString();
                        tbl.Attributes["tabIndex2"] = f.TabIndex2.ToString();
                        tbl.Attributes["ShowInListview"] = f.ShowInListview.ToString();
                        tbl.Attributes["ListviewIndex"] = f.ListviewIndex.ToString();
                        tbl.Attributes["DataLink"] = f.DataLink.ToString();
                        tbl.Attributes["RequiredState"] = f.RequiredState.ToString();

                        d.InnerHtml = f.DatabaseColumn;
                        td.Controls.Add(i);
                        td.Controls.Add(d);
                    } else
                        tbl.Visible = false; // tbl.Attributes["style"] += "display:none;";

                    break;

                case "listview":
                    HtmlSelect lst = new HtmlSelect();
                    lst.Multiple = true;
                    td.Controls.Add(lst);
                    lst.Attributes["class"] = "designerTextarea" + (ownerId == f.OrganisationId || !inDesignMode ? "" : "Locked");
                    lst.Attributes["integralheight"] = "true";

                    if (inDesignMode) lst.Attributes["readonly"] += "true";
                    if (!inDesignMode) {
                        lst.Attributes["tabIndex"] = (type == "SMV" ? (baseTabIndex1 + f.TabIndex1).ToString() : (baseTabIndex2 + f.TabIndex2).ToString());
                        //tbl.Attributes["onchange"] = "locateDoublesFromField(this);";
                        lst.Attributes["oldValue"] = lst.Value;
                    }

                    tbl.Attributes["tabIndex1"] = f.TabIndex1.ToString();
                    tbl.Attributes["tabIndex2"] = f.TabIndex2.ToString();
                    tbl.Attributes["ShowInListview"] = f.ShowInListview.ToString();
                    tbl.Attributes["ListviewIndex"] = f.ListviewIndex.ToString();
                    tbl.Attributes["DataLink"] = f.DataLink.ToString();
                    tbl.Attributes["RequiredState"] = f.RequiredState.ToString();
                    if (!inDesignMode) lst.Attributes["title"] = f.AlternateText;



                    if (!inDesignMode) {
                        lst.Attributes["style"] = "width:" + (type == "SMV" ? f.Width1 : f.Width2) + ";" +
                        "height:" + (type == "SMV" ? f.Height1 : f.Height2) + ";";

                        lst.ID = f.DatabaseTable + "_" + f.DatabaseColumn + "_" + ownerId;
                        //string oldValues = grabValue(values, f.DatabaseTable + "_" + f.DatabaseColumn, f.BaseOrganisationId);
                        //string[] items = oldValues.Split('\n');
                        //lst.Attributes["oldValue"] = oldValues;
                        //lst.Attributes["style"] = "width:100%;height:100%;";

                        //foreach (string s in sql.dynamicFields_getCustomValues(f.Id, inDesignMode))
                        //{
                        //    lst.Items.Add(s.Replace("\r", "").Replace("\n", ""));
                        //    //ListItem l = new ListItem(s);
                        //    ////l.Selected = inListItems(items, s);
                        //    //lst.Items.Add(l);
                        //}

                        List<string> vals2 = sql.dynamicFields_getCustomValues(f.Id, inDesignMode);
                        if (vals2.Count > 0) {
                            if (vals2[0].Replace("\r", "").Replace("\n", "") != "" || vals2.Count > 1) {
                                foreach (string s in vals2) {
                                    lst.Items.Add(s.Replace("\r", "").Replace("\n", ""));
                                }
                            }
                        }

                        switch (f.DataLink) {
                            case "Counties":
                                if (lst.Items.Count > 0)
                                    lst.Items.Add(new ListItem("-", ""));
                                foreach (County c in sql.Organisation_County_getAssociatedConties(U.OrganisationId)) {
                                    lst.Items.Add(c.name);
                                }
                                lst.Items.Add("-");
                                foreach (County c in sql.County_getCounties("", ""))
                                    lst.Items.Add(c.name);

                                break;

                            case "Countries":

                                foreach (string s in CountryList) {
                                    if (s == "-")
                                        lst.Items.Add(new ListItem("-", ""));
                                    else
                                        lst.Items.Add(s);
                                }
                                break;
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
                        foreach (string s in sql.dynamicFields_getCustomValues(f.Id, inDesignMode)) {
                            tbl.Attributes["SelDropDownItems"] += s + "\n";
                        }

                        var positiveList = f.PositiveFieldValue.Split(',').Where(w => w != "").ToList();
                        foreach (string p in positiveList) {
                            tbl.Attributes["PositiveDropDownItems"] += p + "\n";
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
                    sel.Attributes["class"] = "designerTextbox" + (ownerId == f.OrganisationId || !inDesignMode ? "" : "Locked");
                    if (inDesignMode) sel.Attributes["readonly"] += "true";
                    if (!inDesignMode) {
                        sel.Attributes["tabIndex"] = (type == "SMV" ? (baseTabIndex1 + f.TabIndex1).ToString() : (baseTabIndex2 + f.TabIndex2).ToString());
                        //tbl.Attributes["onchange"] = "locateDoublesFromField(this);";
                        sel.Attributes["oldValue"] = sel.Value;
                    }

                    tbl.Attributes["tabIndex1"] = f.TabIndex1.ToString();
                    tbl.Attributes["tabIndex2"] = f.TabIndex2.ToString();
                    tbl.Attributes["ShowInListview"] = f.ShowInListview.ToString();
                    tbl.Attributes["ListviewIndex"] = f.ListviewIndex.ToString();
                    tbl.Attributes["DataLink"] = f.DataLink.ToString();
                    tbl.Attributes["RequiredState"] = f.RequiredState.ToString();
                    if (!inDesignMode) sel.Attributes["title"] = f.AlternateText;

                    if (!inDesignMode) {
                        sel.ID = f.DatabaseTable + "_" + f.DatabaseColumn + "_" + ownerId;

                        List<string> vals = sql.dynamicFields_getCustomValues(f.Id, inDesignMode);
                        if (vals.Count > 0) {
                            if (vals[0].Replace("\r", "").Replace("\n", "") != "" || vals.Count > 1) {
                                foreach (string s in vals) {
                                    sel.Items.Add(s.Replace("\r", "").Replace("\n", ""));
                                }
                            }
                        }

                        switch (f.DataLink) {
                            case "Counties":
                                if (sel.Items.Count > 0)
                                    sel.Items.Add(new ListItem("-", ""));
                                foreach (County c in sql.Organisation_County_getAssociatedConties(U.OrganisationId)) {
                                    sel.Items.Add(c.name);
                                }


                                sel.Items.Add("-");
                                foreach (County c in sql.County_getCounties("", ""))
                                    sel.Items.Add(c.name);

                                break;

                            case "Countries":

                                foreach (string s in CountryList) {
                                    if (s == "-")
                                        sel.Items.Add(new ListItem("-", ""));
                                    else
                                        sel.Items.Add(s);
                                }
                                break;
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
                        foreach (string s in sql.dynamicFields_getCustomValues(f.Id, inDesignMode)) {
                            tbl.Attributes["SelDropDownItems"] += s + "\n";
                        }

                        var positiveList = f.PositiveFieldValue.Split(',').Where(w => w != "").ToList();
                        foreach (string p in positiveList) {
                            tbl.Attributes["PositiveDropDownItems"] += p + "\n";
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

                        var isSensitive = false;
                        if (dataClassfications != null && dataClassfications.Any()) {
                            var classification = dataClassfications.FirstOrDefault(w => w.Id == f.DataClassificationId);
                            if (classification != null && !string.IsNullOrEmpty(classification.ExportMask)) {

                                //td.Attributes["style"] = "width:" + (type == "SMV" ? f.Width1 : f.Width2) + ";"

                                isSensitive = true;

                                // Override textbox width
                                sel.Attributes["class"] = "designerTextbox Locked";
                                sel.Attributes["disabled"] = "disabled";
                                sel.Attributes["sensitive"] = "1";
                                sel.Attributes["style"] = "width:" + (Convert.ToInt32(type == "SMV" ? f.Width1 : f.Width2) - 22) + "px;";

                                // Routine to add image button for classified data
                                HtmlInputImage img = new HtmlInputImage();
                                img.Attributes["title"] = "Vis indhold";
                                img.Attributes["src"] = "/images/eye32.png";
                                img.Attributes["width"] = "16";
                                img.Attributes["height"] = "16";
                                img.Attributes["style"] = "margin-left:3px;";
                                img.Attributes["data-target-id"] = f.Id.ToString();
                                img.Attributes["data-target-name"] = f.DatabaseTable + "_" + f.DatabaseColumn + "_" + ownerId;
                                img.Attributes["data-target-type"] = (f.DatabaseTable.IndexOf("contacts", StringComparison.OrdinalIgnoreCase) > 0 ? "contact" : "company");
                                img.Attributes["onclick"] = "Show(this);";
                                td.Controls.Add(img);
                            }
                        }


                        if (string.IsNullOrEmpty(f.PositiveFieldValue)) { showContactPicker = false; }
                        if (showContactPicker && f.DatabaseTable.IndexOf("companies", StringComparison.OrdinalIgnoreCase) > -1) {
                            if (isSensitive) {
                                sel.Attributes["style"] = "width:" + (Convert.ToInt32(type == "SMV" ? f.Width1 : f.Width2) - 45) + "px;";
                            } else {
                                sel.Attributes["style"] = "width:" + (Convert.ToInt32(type == "SMV" ? f.Width1 : f.Width2) - 25) + "px;";
                            }

                            // Routine to add image button for positive field
                            HtmlInputImage img = new HtmlInputImage();
                            img.Attributes["title"] = "Vælg den/de kontaktpersoner som feltet vedrører.";
                            img.Attributes["src"] = "/images/checkNames.png";
                            img.Attributes["width"] = "16";
                            img.Attributes["height"] = "16";
                            img.Attributes["style"] = "margin-left:3px;";
                            img.Attributes["data-field-id"] = f.Id.ToString();
                            img.Attributes["data-object-type"] = "120";
                            img.Attributes["data-screen-ref-key"] = Guid.NewGuid().ToString();
                            img.Attributes["onclick"] = "Expiration(this);";
                            td.Controls.Add(img);
                        }
                    }

                    break;
            }

            return tbl;
        }

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

        public static HtmlGenericControl CreateMobileElement(this IDS.EBSTCRM.Base.TableColumnWithValue t, ref SQLDB sql, ref User U) {
            HtmlGenericControl div = new HtmlGenericControl("DIV");

            if (t.DynamicField != null) {
                HtmlGenericControl divText = new HtmlGenericControl("DIV");
                divText.InnerText = t.DynamicField.DatabaseColumn;

                //TODO: Switch correct type
                switch (t.DynamicField.FieldType) {
                    case "datetime":
                    case "textbox":
                    case "integer":
                    case "absinteger":
                    case "float":
                    case "absfloat":
                    case "emailaddress":
                        HtmlInputText ipText = new HtmlInputText();
                        ipText.Attributes["name"] = t.Name;
                        ipText.Attributes["id"] = t.Name;
                        ipText.Attributes["class"] = "textField";
                        ipText.Value = t.ValueFormatted;

                        div.Controls.Add(divText);
                        div.Controls.Add(ipText);
                        break;

                    case "memo":
                        HtmlTextArea ipTextarea = new HtmlTextArea();
                        ipTextarea.Attributes["name"] = t.Name;
                        ipTextarea.Attributes["id"] = t.Name;
                        ipTextarea.Attributes["class"] = "textAreaField";
                        ipTextarea.Value = t.ValueFormatted;

                        div.Controls.Add(divText);
                        div.Controls.Add(ipTextarea);
                        break;

                    case "checkbox":
                        HtmlInputCheckBox ipCheck = new HtmlInputCheckBox();
                        ipCheck.Attributes["name"] = t.Name;
                        ipCheck.Attributes["id"] = t.Name;
                        ipCheck.Value = t.ValueFormatted;
                        ipCheck.Checked = t.ValueFormatted == "Ja";

                        HtmlGenericControl cbLabel = new HtmlGenericControl("LABEL");
                        cbLabel.Attributes["for"] = t.Name;
                        cbLabel.InnerText = t.DynamicField.DatabaseColumn;

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

                        string[] lwValues = t.ValueFormatted.Split('\n');

                        List<string> vals2 = sql.dynamicFields_getCustomValues(t.DynamicField.Id, false);
                        if (vals2.Count > 0) {
                            if (vals2[0].Replace("\r", "").Replace("\n", "") != "" || vals2.Count > 1) {
                                foreach (string s in vals2) {
                                    lst.Items.Add(s.Replace("\r", "").Replace("\n", ""));
                                }
                            }
                        }

                        switch (t.DynamicField.DataLink) {
                            case "Counties":
                                if (lst.Items.Count > 0)
                                    lst.Items.Add(new ListItem("-", ""));
                                foreach (County c in sql.Organisation_County_getAssociatedConties(U.OrganisationId)) {
                                    lst.Items.Add(c.name);
                                }
                                lst.Items.Add("-");
                                foreach (County c in sql.County_getCounties("", ""))
                                    lst.Items.Add(c.name);

                                break;

                            case "Countries":

                                foreach (string s in CountryList) {
                                    if (s == "-")
                                        lst.Items.Add(new ListItem("-", ""));
                                    else
                                        lst.Items.Add(s);
                                }
                                break;
                        }

                        //Make sure the chosen item is in the listview/combobox
                        bool valuePresent = false;

                        if (lwValues != null && lwValues.Length > 0) {
                            for (int i = 0; i < lst.Items.Count; i++) {
                                for (int x = 0; x < lwValues.Length; x++) {
                                    System.Diagnostics.Debug.WriteLine(lst.Items[i].Value.Trim().ToLower() + "==" + lwValues[x].Trim().Replace("\r", "").Replace("\n", "").ToLower() + "|");
                                    if (lst.Items[i].Value.Trim().ToLower() == lwValues[x].Trim().Replace("\r", "").Replace("\n", "").ToLower()) {
                                        lst.Items[i].Selected = true;
                                        valuePresent = true;
                                    }
                                }
                            }
                        }

                        System.Diagnostics.Debug.WriteLine("");

                        if (!valuePresent && t.ValueFormatted != "") {
                            if (lst.Items.Count > 0)
                                lst.Items.Insert(0, new ListItem(lst.Value));
                            else
                                lst.Items.Add(new ListItem(lst.Value));
                        }
                        break;

                    case "dropdown":
                        HtmlSelect sel = new HtmlSelect();
                        sel.Attributes["class"] = "selectDataField";

                        div.Controls.Add(divText);
                        div.Controls.Add(sel);

                        List<string> vals = sql.dynamicFields_getCustomValues(t.DynamicField.Id, false);
                        if (vals.Count > 0) {
                            if (vals[0].Replace("\r", "").Replace("\n", "") != "" || vals.Count > 1) {
                                foreach (string s in vals) {
                                    sel.Items.Add(s.Replace("\r", "").Replace("\n", ""));
                                }
                            }
                        }

                        switch (t.DynamicField.DataLink) {
                            case "Counties":
                                if (sel.Items.Count > 0)
                                    sel.Items.Add(new ListItem("-", ""));
                                foreach (County c in sql.Organisation_County_getAssociatedConties(U.OrganisationId)) {
                                    sel.Items.Add(c.name);
                                }


                                sel.Items.Add("-");
                                foreach (County c in sql.County_getCounties("", ""))
                                    sel.Items.Add(c.name);

                                break;

                            case "Countries":

                                foreach (string s in CountryList) {
                                    if (s == "-")
                                        sel.Items.Add(new ListItem("-", ""));
                                    else
                                        sel.Items.Add(s);
                                }
                                break;
                        }


                        //Make sure the chosen item is in the listview/combobox
                        bool selValuePresent = false;

                        for (int i = 0; i < sel.Items.Count; i++) {
                            if (sel.Items[i].Value == t.ValueFormatted) {
                                sel.Items[i].Selected = true;
                                selValuePresent = true;
                                break;
                            } else
                                sel.Items[i].Selected = false;
                        }

                        if (!selValuePresent && t.ValueFormatted != "") {
                            if (sel.Items.Count > 0)
                                sel.Items.Insert(0, new ListItem(sel.Value));
                            else
                                sel.Items.Add(new ListItem(sel.Value));
                        }

                        break;
                }

            } else {
                div.InnerText = "unknown field";
            }

            if (t.DynamicField.InnerCSS.ToLower().Contains("display:none"))
                div.Attributes["style"] = "display:none;";

            return div;
        }
    }
}
