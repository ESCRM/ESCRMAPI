using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace IDS.EBSTCRM.Base.Helper {
    /// <summary>
    /// List of text method
    /// </summary>
   public class TextMethod {
        /// <summary>
        /// Mask
        /// </summary>
        /// <param name="value"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static string Mask(string value, string format) {
            if (format.IndexOf('#') == -1) {
                return format;
            } else {
                var maskvalue = "";
                var formatArray = format.ToArray();
                var valueArray = value.ToArray();
                if (formatArray.Length == valueArray.Length) {
                    for (int i = 0; i < formatArray.Length; i++) {
                        if (formatArray[i].ToString() == "#") {
                            maskvalue = maskvalue + valueArray[i].ToString();
                        } else {
                            maskvalue = maskvalue + formatArray[i].ToString();
                        }
                    }
                }
                if (formatArray.Length > valueArray.Length) {
                    for (int i = 0; i < valueArray.Length; i++) {
                        if (formatArray[i].ToString() == "#") {
                            maskvalue = maskvalue + valueArray[i].ToString();
                        } else {
                            maskvalue = maskvalue + formatArray[i].ToString();
                        }
                    }
                }
                if (formatArray.Length < valueArray.Length) {
                    for (int i = 0; i < formatArray.Length; i++) {

                        // when format array reaches to an end, we would like to add same format of last array character to rest of the VALUE
                        if (i == formatArray.Length - 1) {
                            for (int k = (formatArray.Length - 1); k < valueArray.Length; k++) {
                                if (formatArray[i].ToString() == "#") {
                                    maskvalue = maskvalue + valueArray[k].ToString();
                                } else {
                                    maskvalue = maskvalue + formatArray[i].ToString();
                                }
                            }
                        } else {
                            if (formatArray[i].ToString() == "#") {
                                maskvalue = maskvalue + valueArray[i].ToString();
                            } else {
                                maskvalue = maskvalue + formatArray[i].ToString();
                            }
                        }
                    }
                }
                return maskvalue;
            }
        }

        public static bool HasColumn(SqlDataReader dr, string columnName) {
            for (int i = 0; i < dr.FieldCount; i++) {
                if (dr.GetName(i).Equals(columnName, StringComparison.InvariantCultureIgnoreCase))
                    return true;
            }
            return false;
        }
    }
}
