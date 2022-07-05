using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Web.UI.HtmlControls;

namespace IDS.EBSTCRM.Base
{
    /// <summary>
    /// Early Warning History Item:
    /// Used for listview, which can contain:
    /// Notes,
    /// Files,
    /// SAM Contact Person
    /// </summary>
    [Serializable()]
    public class EarlyWarningHistoryItem
    {
        public string Type;
        public string SavedOn;
        public int Id;
        public int ContactId;
        public int CompanyId;
        public string Title;
        public DateTime DateCreated;
        public object LastUpdated;
        public string CreatedByUser;
        public string UpdatedByUser;

        public EarlyWarningHistoryItem()
        {

        }

        public EarlyWarningHistoryItem(ref SqlDataReader dr)
        {
            Type = TypeCast.ToString(dr["Type"]);
            SavedOn = TypeCast.ToString(dr["SavedOn"]);
            Id = TypeCast.ToInt(dr["Id"]);
            ContactId = TypeCast.ToInt(dr["ContactId"]);
            CompanyId = TypeCast.ToInt(dr["CompanyId"]);
            Title = TypeCast.ToString(dr["Title"]);
            DateCreated = TypeCast.ToDateTime(dr["DateCreated"]);
            LastUpdated = TypeCast.ToDateTimeOrNull(dr["LastUpdated"]);
            CreatedByUser = TypeCast.ToString(dr["CreatedByUser"]);
            UpdatedByUser = TypeCast.ToString(dr["UpdatedByUser"]);

        }

        private string GetIcon()
        {
            switch (Type)
            {
                case "Note":
                    return "images/note.png";

                case "Fil":
                    return "images/file.png";
                    
                case "SAM":
                    return "images/sam.png";

                case "Frivillig":
                    return "images/ewVolunteer.png";

            }
            return "";
        }

        public System.Web.UI.HtmlControls.HtmlGenericControl ToListviewItem()
        {
            HtmlGenericControl b = new HtmlGenericControl("b");
            b.Attributes["args"] = Id.ToString();
            b.Attributes["icon16"] = this.GetIcon();
            b.Attributes["uid"] = b.Attributes["args"];

            HtmlGenericControl u1 = new HtmlGenericControl("u");
            HtmlGenericControl u2 = new HtmlGenericControl("u");
            HtmlGenericControl u3 = new HtmlGenericControl("u");
            HtmlGenericControl u4 = new HtmlGenericControl("u");
            HtmlGenericControl u5 = new HtmlGenericControl("u");
            HtmlGenericControl u6 = new HtmlGenericControl("u");
            HtmlGenericControl u7 = new HtmlGenericControl("u");

            b.Controls.Add(u1);
            b.Controls.Add(u2);
            b.Controls.Add(u3);
            b.Controls.Add(u4);
            b.Controls.Add(u5);
            b.Controls.Add(u6);
            b.Controls.Add(u7);

            u1.InnerText = this.Title;
            u2.InnerText = this.SavedOn;
            u3.InnerText = this.Type;
            u4.InnerText = this.DateCreated.ToString("dd-MM-yyyy HH:mm");
            u5.InnerText = this.CreatedByUser;
            u6.InnerText = this.LastUpdated != null ? TypeCast.ToDateTime(this.LastUpdated).ToString("dd-MM-yyyy HH:mm") : "";
            u7.InnerText = this.UpdatedByUser;

            return b;

        }
    }
}
