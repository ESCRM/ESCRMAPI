using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IDS.EBSTCRM.WindowManager.Service {
    public class TimeService {
        public static DateTime GetDateTime(DateTime date, int interval, string datepart) {
            if (datepart == "day") {
                return date.AddDays(interval);
            } else if (datepart == "month") {
                return date.AddMonths(interval);
            } else if (datepart == "year") {
                return date.AddYears(interval);
            }
            return date;
        }
    }
}