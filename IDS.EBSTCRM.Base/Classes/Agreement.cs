using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IDS.EBSTCRM.Base.Classes {
    public class Agreement {
        public int Id { get; set; }
        public string Name { get; set; }
        public int OrganisationId { get; set; }
        public int Expiry { get; set; }
        public string ExpiryDatePart { get; set; }
        public DateTime? ExpiryDateFixed { get; set; }
        public int FollowUp { get; set; }
        public string Description { get; set; }
        public string TemplateText { get; set; }
        public string TemplateSubject { get; set; }
        public string Type { get; set; }
        public DateTime Active { get; set; }
        public bool RequireDocumentation { get; set; }

        /*public string ExpiryDate {
            get {
                var date = DateTime.Now;
                if (ExpiryDatePart == "day") {
                    return date.AddDays(Expiry).ToString("dd-MM-yyyy");
                } else if (ExpiryDatePart == "month") {
                    return date.AddMonths(Expiry).ToString("dd-MM-yyyy");
                } else if (ExpiryDatePart == "year") {
                    return date.AddYears(Expiry).ToString("dd-MM-yyyy");
                }
                return date.ToString("dd-MM-yyyy");
            }
        }
        public string FollowUpDate {
            get {
                var date = DateTime.Now;
                return date.AddDays(FollowUp).ToString("dd-MM-yyyy");
            }
        }*/

        public static string DatePartToDanish(int number, string datepart) {
            if (datepart == "day") {
                return "dag" + (number > 1 ? "e" : "");
            } else if (datepart == "month") {
                return "måned" + (number > 1 ? "er" : "");
            } else if (datepart == "year") {
                return "år";
            }
            return string.Empty;
        }
    }
}
