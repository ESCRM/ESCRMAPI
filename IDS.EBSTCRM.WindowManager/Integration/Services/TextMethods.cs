using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.Serialization;

namespace IDS.EBSTCRM.WindowManager.Integration.Services {
    public static class TextMethods {
        // Private method for use of only This Web API
        public static bool ContainsAny(this string haystack, params string[] needles) {
            foreach (string needle in needles) {
                if (haystack.ToLower().Contains(needle.ToLower()))
                    return true;
            }
            return false;
        }

        public static string ConvertToXml<T>(T obj) {
            XmlSerializer xsSubmit = new XmlSerializer(typeof(T));
            using (var sww = new StringWriter()) {
                using (XmlWriter writer = XmlWriter.Create(sww)) {
                    xsSubmit.Serialize(writer, obj);
                    return sww.ToString();
                }
            }
        }
    }
}