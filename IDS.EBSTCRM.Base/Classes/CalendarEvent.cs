using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IDS.EBSTCRM.Base.Classes {
    public class CalEvent {
        public int Id { get; set; }
        public string Title { get; set; }
        public string ShortDescription { get; set; }


        public int TypeId { get; set; }
        public string Type { get; set; }
        public string Color { get; set; }


        public string Place { get; set; }
        public string MunicipalityName { get; set; }
        public string RegionName { get; set; }



        public int Municipality { get; set; }
        public int Region { get; set; }


        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Link { get; set; }
        public int CompanyId { get; set; }
        public string CompanyName { get; set; }
        public int ContactId { get; set; }


        public string UserId { get; set; }
        public int OrganisationId { get; set; }
        public string OrganisationName { get; set; }
    }
}

