using System;
using System.Collections.Generic;
namespace IDS.EBSTCRM.WindowManager.Integration.Models {
    public class SearchCompanyResponse : Response {
        public List<SearchCompany> Companies { get; set; }
        public SearchCompanyResponse() {
            Companies = new List<SearchCompany>();
        }
    }

    public class SearchCompany {
        public int CompanyId { get; set; }
        public string CompanyName { get; set; }
        public bool IsPOT { get; set; }
        public DateTime? CompanyAbandonedDate { get; set; }
        public string CompanyAbandonedBy { get; set; }
        public Organisation Organisation { get; set; }
        public List<SysField> Fields { get; set; }
        public List<Note> Notes { get; set; }
        public List<Meeting> Meetings { get; set; }
        public List<AVN> AVNs { get; set; }
        public List<Contact> Contacts { get; set; }
        public SearchCompany() {
            Organisation = new Organisation();
            Fields = new List<SysField>();
            Notes = new List<Note>();
            Meetings = new List<Meeting>();
            AVNs = new List<AVN>();
            Contacts = new List<Contact>();
        }
    }

    public class Note {
        public int NoteId { get; set; }
        public string Title { get; set; }
        public DateTime NoteDate { get; set; }
        public string Category { get; set; }
        public string Category2 { get; set; }
        public string Text { get; set; }
        public Organisation Organisation { get; set; }
        public string UserId { get; set; }
        public bool Sensitive { get; set; }
        public Note() {
            Organisation = new Organisation();
            Sensitive = false;
        }
    }

    public class Meeting {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string MeetingUrl { get; set; }
        public string Subject { get; set; }
        public string Location { get; set; }
        public DateTime DateStart { get; set; }
        public DateTime DateEnd { get; set; }
        public double Timespent { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedByUsername { get; set; }

        // Project
        public int PrimaryProjectTypeId { get; set; }
        public string PrimaryProjectTypeName { get; set; }
        public int SecondaryProjectTypeId { get; set; }
        public string SecondaryProjectTypeName { get; set; }
        public int SecondaryProjectTypeSerialNo { get; set; }

        // Case
        public int CaseNumberId { get; set; }
        public int CaseNumberRelationId { get; set; }
        public int CaseNumber { get; set; }
        public string CaseNumberName { get; set; }
        public string DateStamp { get; set; }
    }

    public class Organisation {
        public int Id { get; set; }
        public string Name { get; set; }
        public int AccessType { get; set; }
    }

    public class AVN {
        public int AVNId { get; set; }
        public string AVNName { get; set; }
        public int AVNTypeId { get; set; }
        public List<SysField> Fields { get; set; }
        public AVN() {
            Fields = new List<SysField>();
        }
    }

    public class Contact {
        public int ContactId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public DateTime? ContactAbandonedDate { get; set; }
        public string ContactAbandonedBy { get; set; }
        public List<SysField> Fields { get; set; }
        public List<Note> Notes { get; set; }
        public List<Document> Documents { get; set; }
        public List<AVN> AVNs { get; set; }
        public Contact() {
            Fields = new List<SysField>();
            Notes = new List<Note>();
            Documents = new List<Document>();
            AVNs = new List<AVN>();
        }
    }

    public class Document {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Url { get; set; }
        public string UserId { get; set; }
        public string OrganisationId { get; set; }
        public DateTime DateCreated { get; set; }
        public bool Sensitive { get; set; }
    }
}