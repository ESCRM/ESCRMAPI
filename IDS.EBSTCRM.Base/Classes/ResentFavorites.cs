using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IDS.EBSTCRM.Base.Classes
{
    public class ResentFavorites
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string CompanyName { get; set; }
        public string Action { get; set; }
        public string Description { get; set; }
        public string ChangedDate { get; set; }
        public string OrganisationId { get; set; }
        public string UserId { get; set; }
        public string Name { get; set; }
        public string Profile { get; set; }
        public string CompanyType { get; set; }
    }
}
