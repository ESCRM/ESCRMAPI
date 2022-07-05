using IDS.EBSTCRM.Base;
using IDS.EBSTCRM.Base.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IDS.EBSTCRM.WindowManager.Security {
    public class SessionManager {
        private static string _FlagDataObject = "flagdataobject";
        public static void initSession() {
            if (HttpContext.Current.Session[_FlagDataObject] == null) {
                HttpContext.Current.Session.Timeout = 60;
                HttpContext.Current.Session.Add(_FlagDataObject, new List<Flag>());
            }
        }
        public static void SetFlagDataObject(Flag value) {
            var flag = new List<Flag>();
            if (HttpContext.Current.Session[_FlagDataObject] != null) {
                flag = GetAllFlagDataObject();
                if (flag == null) { flag = new List<Flag>(); }
            }
            flag.Add(value);
            HttpContext.Current.Session[_FlagDataObject] = flag;
        }
        public static List<Flag> GetAllFlagDataObject() {
            if (HttpContext.Current.Session[_FlagDataObject] != null) {
                return (List<Flag>)HttpContext.Current.Session[_FlagDataObject];
            }
            return null;
        }
        public static Flag GetFlagDataObjectByRefKey(string refkey) {
            if (HttpContext.Current.Session[_FlagDataObject] != null) {
                var flags = (List<Flag>)HttpContext.Current.Session[_FlagDataObject];
                //return flags.FirstOrDefault(f => f.ScreenRefKey == refkey);
            }
            return null;
        }
    }
}