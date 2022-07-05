using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Mail;

namespace IDS.EBSTCRM.Base {
    /// <summary>
    /// Enables Secure Type Casts to/from object types
    /// Note: This Class Handles All Exceptions!
    /// </summary>
    public static class TypeCast {

        public enum ToSecondsFrom {
            minutes = 0,
            hours = 1,
            days = 2,
            weeks = 3,
            months = 4
        }

        /// <summary>
        /// Verify an emailaddress as valid
        /// </summary>
        /// <param name="emailaddress"></param>
        /// <returns>true if the mail is valid</returns>
        public static bool IsValidEmail(string emailaddress) {
            try {
                MailAddress m = new MailAddress(emailaddress);
                return true;
            } catch (FormatException) {
                return false;
            }
        }

        /// <summary>
        /// Fixes layout of CPR number
        /// </summary>
        /// <param name="CPR"></param>
        /// <returns></returns>
        public static string FixCPR(string CPR) {
            string ret = CPR;
            if (ret.Length > 7) {
                ret = ret.Replace("-", "");

                ret = ret.Substring(0, 6) + "-" + ret.Substring(6);
            }

            return ret;
        }

        public static string RemoveForbiddenFileCharacters(string file) {
            string[] r = new string[] { "?", ":", "\\", "/", "*", "<", ">", "|" };

            foreach (string s in r) {
                file = file.Replace(s, "");
            }

            return file;
        }

        /// <summary>
        /// Capitalize text
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string Capitalize(string value) {
            return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(value);
        }

        /// <summary>
        /// Converts a userrole to visible string, for use within listviews
        /// </summary>
        /// <param name="UserRole"></param>
        /// <returns></returns>
        public static string UserRoleToString(UserRoles UserRole) {
            switch (UserRole) {
                case UserRoles.Administrator:
                    return "Administrator";

                case UserRoles.CEO:
                    return "Direktør";

                case UserRoles.Consultant:
                    return "Konsulent";

                case UserRoles.GlobalAdministrator:
                    return "Global administrator";

                case UserRoles.GlobalStatistics:
                    return "Global statistiker";

                case UserRoles.SectionLeader:
                    return "Mellemleder";

                case UserRoles.SystemOwner:
                    return "Systemejer";
            }

            return "Ukendt";
        }

        /// <summary>
        /// Convert number to seconds
        /// </summary>
        /// <param name="counter"></param>
        /// <param name="from"></param>
        /// <returns></returns>
        public static long ToSeconds(int counter, ToSecondsFrom from) {
            long multiplier = 60;

            switch (from) {
                case ToSecondsFrom.minutes:
                    multiplier = 60;
                    break;

                case ToSecondsFrom.hours:
                    multiplier = 3600;
                    break;

                case ToSecondsFrom.days:
                    multiplier = 86400;
                    break;

                case ToSecondsFrom.weeks:
                    multiplier = 604800;
                    break;

                case ToSecondsFrom.months:
                    multiplier = 2678400;
                    break;
            }

            return counter * multiplier;
        }

        /// <summary>
        /// Convert object to string or DbValue.null (used for saving data to SQL Database)
        /// </summary>
        /// <param name="Data"></param>
        /// <returns></returns>
        public static object ToStringOrDBNull(object Data) {
            string retval = "";
            try {
                retval = ToString(Data);
            } catch {
                retval = "";
            }

            if (retval == "")
                return DBNull.Value;
            else
                return retval;

        }

        /// <summary>
        /// Converts an object to a string representation
        /// </summary>
        /// <param name="Data"></param>
        /// <returns></returns>
        public static string ToString(object Data) {
            try {
                if (Data == DBNull.Value || Data == null)
                    return "";
                else
                    return Data.ToString();
            } catch {
                return "";
            }
        }

        /// <summary>
        /// Converts and object to an integer representation - if conversion fails, zero is returned
        /// </summary>
        /// <param name="Data"></param>
        /// <returns></returns>
        public static double? ToDoubleLoose(object Data) {
            try {
                double result;
                if (double.TryParse(Data.ToString(), out result))
                    return result;
                else
                    return null;
            } catch {
                return null;
            }
        }

        /// <summary>
        /// Converts and object to an integer representation - if conversion fails, zero is returned
        /// </summary>
        /// <param name="Data"></param>
        /// <returns></returns>
        public static int? ToIntLoose(object Data) {
            try {
                int result;
                if (int.TryParse(Data.ToString(), out result))
                    return result;
                else
                    return null;
            } catch {
                return null;
            }
        }

        /// <summary>
        /// Converts and object to an integer representation - if conversion fails, zero is returned
        /// </summary>
        /// <param name="Data"></param>
        /// <returns></returns>
        public static int ToInt(object Data) {
            try {
                int result;
                if (Data == null)
                    return 0;
                if (int.TryParse(Data.ToString(), out result))
                    return result;
                else
                    return 0;
            } catch {
                return 0;
            }
        }

        /// <summary>
        /// Converts an object to long
        /// </summary>
        /// <param name="Data"></param>
        /// <returns></returns>
        public static long ToLong(object Data) {
            try {
                long result;
                if (long.TryParse(Data.ToString(), out result))
                    return result;
                else
                    return 0;
            } catch {
                return 0;
            }
        }

        /// <summary>
        /// Converts and object to an integer representation - if conversion fails, zero is returned
        /// </summary>
        /// <param name="Data"></param>
        /// <returns></returns>
        public static long? ToLongLoose(object Data) {
            try {
                long result;
                if (long.TryParse(Data.ToString(), out result))
                    return result;
                else
                    return null;
            } catch {
                return null;
            }
        }

        /// <summary>
        /// Converts an object to decimals
        /// </summary>
        /// <param name="Data"></param>
        /// <param name="Culture"></param>
        /// <returns></returns>
        public static decimal ToDecimal(object Data, System.Globalization.CultureInfo Culture) {
            try {
                return decimal.Parse(ToString(Data), Culture.NumberFormat);
            } catch {
                return 0;
            }
        }

        /// <summary>
        /// Converts an object to decimals
        /// </summary>
        /// <param name="Data"></param>
        /// <param name="Culture"></param>
        /// <returns></returns>
        public static decimal ToDecimal(object Data) {
            return ToDecimal(Data, System.Globalization.CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Converts an object to boolean value
        /// </summary>
        /// <param name="Data"></param>
        /// <returns></returns>
        public static bool ToBool(object Data) {
            try {
                string tmp = TypeCast.ToString(Data).ToLower();
                if (tmp == "true" || tmp == "1" || tmp == "-1" || tmp == "ja" || tmp == "sand")
                    return true;
                else
                    return (bool)Data;
            } catch {
                return false;
            }
        }

        public static bool? ToBoolLoose(object Data) {
            try {
                bool result;
                if (bool.TryParse(Data.ToString(), out result))
                    return result;
                else
                    return null;
            } catch {
                return null;
            }
        }

        /// <summary>
        /// Converts an object to a boolean value - if conversion fails, true is returned
        /// </summary>
        /// <param name="Data"></param>
        /// <returns></returns>
        public static bool ToBoolOrTrue(object Data) {
            try {
                string tmp = TypeCast.ToString(Data).ToLower();
                if (tmp == "true" || tmp == "1" || tmp == "-1")
                    return true;
                else {
                    if (tmp == "") return true;
                    return (bool)Data;
                }
            } catch {
                return true;
            }
        }

        /// <summary>
        /// Converts an object to an integer or dbValue.null
        /// </summary>
        /// <param name="Data"></param>
        /// <returns></returns>
        public static object ToIntOrDBNull(object Data) {
            try {
                int result;
                if (int.TryParse(Data.ToString(), out result))
                    return result;
                else
                    return DBNull.Value;
            } catch {
                return DBNull.Value;
            }
        }

        /// <summary>
        /// Converts an object to an integer or null - if conversion failes, null is returned
        /// </summary>
        /// <param name="Data"></param>
        /// <returns></returns>
        public static object ToIntOrNull(object Data) {
            try {
                int result;
                if (int.TryParse(Data.ToString(), out result))
                    return result;
                else
                    return null;
            } catch {
                return null;
            }
        }

        /// <summary>
        /// Convert an object to DataTime or null
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static object makeDateOrNothing(object value) {
            object retval;
            try {
                if (value == DBNull.Value) {
                    retval = null;
                } else {
                    retval = System.DateTime.Parse(System.Convert.ToString(value));
                }
            } catch {
                retval = null;
            }
            return retval;
        }

        public static DateTime? ToNullableDateTime(object value) {
            object d = makeDateOrNothing(value);
            if (d != null)
                return (DateTime)makeDateOrNothing(value);
            else
                return null;
        }

        /// <summary>
        /// Convert an object to DataTime or null
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static object ToDateTimeOrNull(object value) {
            return makeDateOrNothing(value);
        }

        /// <summary>
        /// Convert an object to DataTime or null
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static DateTime? ToDateTimeLoose(object value) {
            return (DateTime?)makeDateOrNothing(value);
        }

        /// <summary>
        /// Convert an object to DataTime - if conversion fails, today is returned as a Date
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static DateTime ToDateTime(object value) {
            DateTime retval;
            try {
                if (value == DBNull.Value) {
                    retval = DateTime.Now;
                } else {
                    retval = System.DateTime.Parse(System.Convert.ToString(value));
                }
            } catch {
                retval = DateTime.Now;
            }
            return retval;
        }

        /// <summary>
        /// Convert an object to DataTime - if conversion fails, today is returned as a Date. Ny metode, ESCRM-196/195
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static DateTime ToDateTime2(object value)
        {
            DateTime retval;
            try
            {
                if (value == DBNull.Value)
                {
                    //retval = DateTime.Now;
                    //** Vi sætter en stor dato ind i stedet for dags dato, ESCRM-196/195
                    retval = DateTime.Parse("2999-12-31");
                }
                else
                {
                    retval = System.DateTime.Parse(System.Convert.ToString(value));
                }
            }
            catch
            {
                retval = DateTime.Now;
            }
            return retval;
        }

        /// <summary>
        /// Convert an object to DataTime or dbvalue.null
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static object ToDateTimeOrDBNull(object value) {
            object tmp = makeDateOrNothing(value);
            return tmp == null ? DBNull.Value : tmp;
        }

        /// <summary>
        /// Prepares a dynamicfield name for use as a stored procedure argument
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string PrepareArgument(string s) {
            string text = s;
            string pattern = @"[^A-Za-z0-9æøåÆØÅ]";
            string newText = System.Text.RegularExpressions.Regex.Replace(text, pattern, "_");
            return newText.Replace(" ", "_");
        }

        /// <summary>
        /// Correct dynamic table names, enforcing correct SQL naming syntax
        /// </summary>
        /// <param name="tc"></param>
        /// <returns></returns>
        public static string fixTableColumnName(string tc) {
            string[] tcs = tc.Split('_');
            string retval = "";

            if (tcs.Length > 4) {
                for (int i = 3; i < tcs.Length - 1; i++) {
                    retval += tcs[i];
                    if (i < tcs.Length - 2) retval += "_";
                }
            } else {
                retval = tcs[0];
            }

            return retval;
        }

        /// <summary>
        /// Replace text using Regular Expressions
        /// </summary>
        /// <param name="original"></param>
        /// <param name="pattern"></param>
        /// <param name="replacement"></param>
        /// <returns></returns>
        public static string ReplaceEx(string original,
                            string pattern, string replacement) {
            int count, position0, position1;
            count = position0 = position1 = 0;
            string upperString = original.ToUpper();
            string upperPattern = pattern.ToUpper();
            int inc = (original.Length / pattern.Length) *
                      (replacement.Length - pattern.Length);
            char[] chars = new char[original.Length + Math.Max(0, inc)];
            while ((position1 = upperString.IndexOf(upperPattern,
                                              position0)) != -1) {
                for (int i = position0; i < position1; ++i)
                    chars[count++] = original[i];
                for (int i = 0; i < replacement.Length; ++i)
                    chars[count++] = replacement[i];
                position0 = position1 + pattern.Length;
            }
            if (position0 == 0) return original;
            for (int i = position0; i < original.Length; ++i)
                chars[count++] = original[i];
            return new string(chars, 0, count);
        }

        /// <summary>
        /// Highligsts text within an searchresult
        /// </summary>
        /// <param name="query"></param>
        /// <param name="resultItems"></param>
        /// <param name="foreColor"></param>
        /// <param name="backColor"></param>
        public static void HighlightSearchResult(string query, ref List<string> resultItems, string foreColor, string backColor) {
            string[] words = query.Split(' ');
            for (int i = 0; i < resultItems.Count; i++) {
                foreach (string w in words) {
                    if (w != "" && w != " " && w != null) {
                        string colorReplace = "<font style=\"color:" + foreColor + ";background-color:" + backColor + ";\">$&</font>";
                        resultItems[i] = System.Text.RegularExpressions.Regex.Replace(resultItems[i], w, colorReplace, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    }
                }
            }
        }

        /// <summary>
        /// Get the desired width of a tablecolumn from a dynamic field
        /// </summary>
        /// <param name="list"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static int getTableColumnWithValueInListIndex(ref List<TableColumnWithValue> list, string name) {
            int counter = 0;
            foreach (TableColumnWithValue t in list) {
                System.Diagnostics.Debug.WriteLine("getTableColumnWithValueInListIndex: " + (t.Name == name && name != "" && name != null).ToString() + " " + t.Name + " = " + name);

                if (t.Name == name && name != "" && name != null) return counter;
                counter++;
            }
            return -1;
        }

        /// <summary>
        /// Get if a table column has been modified within dynamic fields
        /// </summary>
        /// <param name="list"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static int getTableColumnModifiedInListIndex(ref List<TableColumnModified> list, string name) {
            int counter = 0;
            foreach (TableColumnModified t in list) {
                System.Diagnostics.Debug.WriteLine("getTableColumnModifiedInListIndex: " + (t.Name == name && name != "" && name != null).ToString() + " " + t.Name + " = " + name);

                if (t.Name == name && name != "" && name != null) return counter;
                counter++;
            }
            return -1;
        }

        /// <summary>
        /// Find correct tablecolumn within listview
        /// </summary>
        /// <param name="list"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static int getTableColumnInListIndex(ref List<TableColumn> list, string name) {
            int counter = 0;
            foreach (TableColumn t in list) {
                System.Diagnostics.Debug.WriteLine("getTableColumnInListIndex: " + (t.Name == name && name != "" && name != null).ToString() + " " + t.Name + " = " + name);

                if (t.Name == name && name != "" && name != null) return counter;
                counter++;
            }
            return -1;
        }

        /// <summary>
        /// Converts bytes to kilobytes
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string ToKiloBytes(int bytes) {
            return (bytes / 1024).ToString("#,##0") + " KB";
        }

        /// <summary>
        /// Add spaces to a string, giving starting spaces, for use with i.e. dropdowns
        /// </summary>
        /// <param name="NumberOfSpaces"></param>
        /// <returns></returns>
        public static string DecodedSpaces(int NumberOfSpaces) {
            //used to add "white spaces" in dropdownbox
            string strSpaces = string.Empty;

            for (int i = NumberOfSpaces; i > 0; i--) {
                char c = (char)160;
                strSpaces += c;

            }

            return strSpaces;

        }

        //public static DateTime GetDateFromWeek(int year, int weekOfYear, DayOfWeek dayOfWeek)
        //{
        //    DateTime jan1 = new DateTime(year, 1, 1);

        //    int daysOffset = (int)System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek - (int)jan1.DayOfWeek;

        //    DateTime firstMonday = jan1.AddDays(daysOffset);

        //    int firstWeek = System.Globalization.CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(jan1, System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.CalendarWeekRule, System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek);

        //    if (firstWeek <= 1)
        //    {
        //        weekOfYear -= 1;
        //    }

        //    return firstMonday.AddDays(weekOfYear * 7);
        //}

        /// <summary>
        /// Gets date from a week number and year
        /// </summary>
        /// <param name="year"></param>
        /// <param name="weekOfYear"></param>
        /// <returns></returns>
        public static DateTime GetDateFromWeek(int year, int weekOfYear) {
            DateTime jan1 = new DateTime(year, 1, 1);
            int daysOffset = DayOfWeek.Thursday - jan1.DayOfWeek;

            DateTime firstThursday = jan1.AddDays(daysOffset);
            var cal = System.Globalization.CultureInfo.CurrentCulture.Calendar;
            int firstWeek = cal.GetWeekOfYear(firstThursday, System.Globalization.CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);

            var weekNum = weekOfYear;
            if (firstWeek <= 1) {
                weekNum -= 1;
            }
            var result = firstThursday.AddDays(weekNum * 7);
            return result.AddDays(-3);
        }



        /// <summary>
        /// Gets weeknumber from date
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static int GetWeekFromDate(DateTime date) {
            System.Globalization.CultureInfo ciCurr = System.Globalization.CultureInfo.CurrentCulture;
            int weekNum = ciCurr.Calendar.GetWeekOfYear(date, System.Globalization.CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            return weekNum;
        }

        //public static int GetWeekFromDate(DateTime date)
        //{
        //    // Updated 2004.09.27. Cleaned the code and fixed a bug. Compared the algorithm with
        //    // code published here . Tested code successfully against the other algorithm 
        //    // for all dates in all years between 1900 and 2100.
        //    // Thanks to Marcus Dahlberg for pointing out the deficient logic.
        //    // Calculates the ISO 8601 Week Number
        //    // In this scenario the first day of the week is monday, 
        //    // and the week rule states that:
        //    // [...] the first calendar week of a year is the one 
        //    // that includes the first Thursday of that year and 
        //    // [...] the last calendar week of a calendar year is 
        //    // the week immediately preceding the first 
        //    // calendar week of the next year.
        //    // The first week of the year may thus start in the 
        //    // preceding year

        //    const int JAN = 1;
        //    const int DEC = 12;
        //    const int LASTDAYOFDEC = 31;
        //    const int FIRSTDAYOFJAN = 1;
        //    const int THURSDAY = 4;
        //    bool ThursdayFlag = false;

        //    // Get the day number since the beginning of the year
        //    int DayOfYear = date.DayOfYear;

        //    // Get the numeric weekday of the first day of the 
        //    // year (using sunday as FirstDay)
        //    int StartWeekDayOfYear =
        //         (int)(new DateTime(date.Year, JAN, FIRSTDAYOFJAN)).DayOfWeek;
        //    int EndWeekDayOfYear =
        //         (int)(new DateTime(date.Year, DEC, LASTDAYOFDEC)).DayOfWeek;

        //    // Compensate for the fact that we are using monday
        //    // as the first day of the week
        //    if (StartWeekDayOfYear == 0)
        //        StartWeekDayOfYear = 7;
        //    if (EndWeekDayOfYear == 0)
        //        EndWeekDayOfYear = 7;

        //    // Calculate the number of days in the first and last week
        //    int DaysInFirstWeek = 8 - (StartWeekDayOfYear);
        //    int DaysInLastWeek = 8 - (EndWeekDayOfYear);

        //    // If the year either starts or ends on a thursday it will have a 53rd week
        //    if (StartWeekDayOfYear == THURSDAY || EndWeekDayOfYear == THURSDAY)
        //        ThursdayFlag = true;

        //    // We begin by calculating the number of FULL weeks between the start of the year and
        //    // our date. The number is rounded up, so the smallest possible value is 0.
        //    int FullWeeks = (int)Math.Ceiling((DayOfYear - (DaysInFirstWeek)) / 7.0);

        //    int WeekNumber = FullWeeks;

        //    // If the first week of the year has at least four days, then the actual week number for our date
        //    // can be incremented by one.
        //    if (DaysInFirstWeek >= THURSDAY)
        //        WeekNumber = WeekNumber + 1;

        //    // If week number is larger than week 52 (and the year doesn't either start or end on a thursday)
        //    // then the correct week number is 1. 
        //    if (WeekNumber > 52 && !ThursdayFlag)
        //        WeekNumber = 1;

        //    // If week number is still 0, it means that we are trying to evaluate the week number for a
        //    // week that belongs in the previous year (since that week has 3 days or less in our date's year).
        //    // We therefore make a recursive call using the last day of the previous year.
        //    if (WeekNumber == 0)
        //        WeekNumber = GetWeekFromDate(
        //             new DateTime(date.Year - 1, DEC, LASTDAYOFDEC));
        //    return WeekNumber;
        //}


        /// <summary>
        /// Converts string to array of byte
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static byte[] StrToByteArray(string str) {
            System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
            return encoding.GetBytes(str);
        }


    }
}
