using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;

namespace IDS.EBSTCRM.Base
{
    /// <summary>
    /// Creates a listview history item for SMV/POT Contacts
    /// </summary>
    public class ContactHistoryItem
    {
        public string Type { get; set; }
        public int Id { get; set; }
        public DateTime DateCreated { get; set; }
        public string Title { get; set; }
        public string SavedTo { get; set; }
        public string User { get; set; }
        public string Shared { get; set; }
        public decimal TimeUsage { get; set; }
        public string Location { get; set; }
        public string Pool { get; set; }
        public DateTime? ReminderDate { get; set; }
        public string RelationId { get; set; }
        public string UserId { get; set; }
        public string CategoryName { get; set; }

        public int ContactId { get; set; }
        public int OrganisationId { get; set; }

        public string Icon
        {
            get
            {
                switch (Type)
                {
                    case "CONTACTTOEVALUATION":
                        return "Images/userEvaluation.png";

                    case "CONTACTTRANSFERRED":
                    case "COMPANYTRANSFERRED":
                        return "Images/transfer.png";

                    case "COMPANYUPDATED":
                        return "Images/smvSaved.png";

                    case "COMPANYCREATED":
                        return "Images/smvNew.png";

                    case "Tilskudsinformation": //** Har tilføjet icon til tilskudsinformation,  ESCRM-9/42
                        return "images/reports/bookmark-new-3.png";

                    case "CONTACTUPDATED":
                        return "Images/smvContactSaved.png";

                    case "CONTACTCREATED":
                        return "Images/smvContactNew.png";

                    case "NOTE":
                        return "Images/Note.png";

                    case "SAM":
                        return "Images/Sam.png";

                    case "MAILGROUP":
                        return "Images/mailgroup.png";

                    case "FILE":
                        try
                        {
                            if (Title.LastIndexOf(".") > -1)
                            {
                                string ext = Title.Substring(Title.LastIndexOf(".") + 1);
                                return "Images/Media/file_" + ext + ".png";                                
                            }
                            else return "Images/Media/file_unknown.png";
                        }
                        catch
                        {
                            return "Images/Media/file_unknown.png";
                        }

                    case "MEETING":
                        return "Images/7calendar.png";

                    case "MEETINGREPORTING":
                        return "Images/calendar.png";
                }

                return "";
            }
        }

        public string FormattedType
        {
            get
            {
                switch (Type)
                {
                    case "CONTACTTOEVALUATION":
                        return "Evalueret";

                    case "CONTACTTRANSFERRED":
                    case "COMPANYTRANSFERRED":
                        return "Anvist";

                    case "COMPANYUPDATED":
                        return "Opdateret";

                    case "COMPANYCREATED":
                        return "Oprettet";


                    case "CONTACTUPDATED":
                        return "Opdateret";

                    case "CONTACTCREATED":
                        return "Oprettet";


                    case "NOTE":
                        return "Note";

                    case "SAM":
                        return "SAM Relation";

                    case "MAILGROUP":
                        return "Interessegruppe";

                    case "FILE":
                        return "Dokument";

                    case "MEETING":
                        return "Møde";

                    case "MEETINGREPORTING":
                        return "Indrapporteret møde";
                }

                return this.Type;

            }
        }

        public ContactHistoryItem()
        {

        }

        public ContactHistoryItem(ref SqlDataReader dr)
        {
            Type = TypeCast.ToString(dr["Type"]);
            Id = TypeCast.ToInt(dr["Id"]);
            DateCreated = TypeCast.ToDateTime(dr["DateCreated"]);
            Title = TypeCast.ToString(dr["Title"]);
            CategoryName = TypeCast.ToString(dr["CategoryName"]);

            SavedTo = TypeCast.ToString(dr["SavedTo"]);
            User = TypeCast.ToString(dr["User"]);
            Shared = TypeCast.ToString(dr["Shared"]);
            TimeUsage = TypeCast.ToDecimal(dr["TimeUsage"]);
            Location = TypeCast.ToString(dr["Location"]);
            Pool = TypeCast.ToString(dr["Pool"]);
            ReminderDate = TypeCast.ToNullableDateTime(dr["ReminderDate"]);
            RelationId = TypeCast.ToString(dr["RelationId"]);
            UserId = TypeCast.ToString(dr["UserId"]);

            ContactId = TypeCast.ToInt(dr["ContactId"]);
            OrganisationId = TypeCast.ToInt(dr["OrganisationId"]);
        }
    }
}
