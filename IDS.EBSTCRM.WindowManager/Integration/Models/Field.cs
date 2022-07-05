using IDS.EBSTCRM.Base;
using System;
using System.Collections.Generic;
namespace IDS.EBSTCRM.WindowManager.Integration.Models {

    [Serializable]
    public class SysField {
        public string Name { get; set; }
        public string DataType { get; set; }
        public bool IsRequired { get; set; }
        public string Value { get; set; }

        private string _FieldView;
        public string FieldView {
            get {
                if (_FieldView == "null" || _FieldView == "" || _FieldView == null) {
                    _FieldView = "General";
                }
                return _FieldView;
            }
            set {
                _FieldView = value;
            }
        }
        public List<String> ValueRestrictedList { get; set; }
        public SysField() {
            ValueRestrictedList = new List<String>();
        }

        public static List<string> GetRestrictedList(SQLDB context, int OrganisationId, DynamicField f) {
            var restrictedList = new List<string>();
            if (f.FieldType == "dropdown") {
                restrictedList = context.dynamicFields_getCustomValues(f.Id, false);
                switch (f.DataLink) {
                    case "Counties":
                        if (restrictedList.Count > 0) { restrictedList.Add(""); }
                        foreach (var county in context.Organisation_County_getAssociatedConties(OrganisationId)) {
                            restrictedList.Add(county.name);
                        }
                        restrictedList.Add("-");
                        foreach (var county in context.County_getCounties("", "")) {
                            restrictedList.Add(county.name);
                        }
                        break;
                    case "Countries":
                        foreach (string country in DynamicFieldsManager.CountryList) {
                            if (country == "-") { restrictedList.Add(""); } else { restrictedList.Add(country); }
                        }
                        break;
                }
            }
            return restrictedList;
        }
    }
}