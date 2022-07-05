using System;

namespace IDS.EBSTCRM.Base.Classes {
    public class HourlyTimeUsage {
        public int Id { get; set; } 
        public DateTime WeekDate { get; set; } 
        public int SecondaryProjectTypeId { get; set; }
        public string SecondaryProjectTypeName { get; set; }
        public string Varenavn { get; set; }
        
        //** Har skiftet til decimal. ESCRM-155/156
        //public int TimeSpent { get; set; }
        public decimal TimeSpent { get; set; }
        
        public string Description { get; set; }
        public string Kommentar { get; set; }
        public string OriginalDescription { get; set; }

        public string Status { get; set; }
        public string Employee { get; set; }
        public string Email { get; set; }
    }
}