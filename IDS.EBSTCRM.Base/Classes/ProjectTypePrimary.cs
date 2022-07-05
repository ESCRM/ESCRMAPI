using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;

namespace IDS.EBSTCRM.Base
{
    /// <summary>
    /// Primary Project Types
    /// Only used by Erhvers- & Byggestyrelsens Organisation
    /// </summary>
    public class ProjectTypePrimary : EventLogBase
    {
        public int Id;
        public string Name;
        public int AccountNo;
        public int FinalSerialNo;
        public string Description;
        public int ChildCount;
        public int UserGroupId;

        public ProjectTypePrimary()
        {
        
        }

        public ProjectTypePrimary(ref SqlDataReader dr)
        {
            Id = TypeCast.ToInt(dr["Id"]);
            Name = TypeCast.ToString(dr["Name"]);
            AccountNo = TypeCast.ToInt(dr["AccountNo"]);
            FinalSerialNo = TypeCast.ToInt(dr["FinalSerialNo"]);
            Description = TypeCast.ToString(dr["Description"]);
            ChildCount = TypeCast.ToInt(dr["ChildCount"]);
        }

        public override VisualItems GetVisualItemsForEventLog(string Event)
        {
            VisualItems retval = new VisualItems();
            retval.Text = eventToText(Event) + " aktivitet [" + AccountNo + "-" + FinalSerialNo + "] " + Name;
            retval.Icon = "images/listviewIcons/projectType.png";
            retval.JavaScript = (Event == "DELETE" ? "alert('Aktiviten er blevet slettet');"
                                    : "top.frames['root'].frames['frameGlobalAdmin'].ProjectTypes_EditThisProjectType(" + Id + ");");

            return retval;
        }
    }
}
