using System;
namespace IDS.EBSTCRM.Base {
    [Serializable()]
    public class LogType {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ExpiryDatePart { get; set; }
        public int ExpiryValue { get; set; }
        public int LogLevel { get; set; }
        public string PageName { get; set; }
        public string Title { get; set; }
    }
}