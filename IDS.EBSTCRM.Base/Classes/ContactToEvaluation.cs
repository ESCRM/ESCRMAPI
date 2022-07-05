using System;
using System.Collections.Generic;
namespace IDS.EBSTCRM.Base.Classes {

    /// <summary>
    /// Class for ContactToEvaluation
    /// </summary>
    [Serializable]
    public class ContactToEvaluation {

        public int Id { get; set; }

        public int OrgId { get; set; }

        public int ContactId { get; set; }

        public int CompanyId { get; set; }

        public string UserId { get; set; }

        public string Datestamp { get; set; }

        public string Exported { get; set; }

        public string Type { get; set; }

        public List<ContactToEvaluationArc> ContactToEvaluationArc { get; set; }

        public ContactToEvaluation() {
            ContactToEvaluationArc = new List<Classes.ContactToEvaluationArc>();
        }
    }

    /// <summary>
    /// Class for Archieved ContactToEvaluation
    /// </summary>
    [Serializable]
    public class ContactToEvaluationArc {

        public int Id { get; set; }

        public int IdOrgId { get; set; }

        public int OrgId { get; set; }

        public int ContactId { get; set; }

        public int CompanyId { get; set; }

        public string UserId { get; set; }

        public string Datestamp { get; set; }

        public string Exported { get; set; }

        public string Reason { get; set; }

        public string Type { get; set; }
    }
}