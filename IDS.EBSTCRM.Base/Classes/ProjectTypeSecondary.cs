using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;

namespace IDS.EBSTCRM.Base {
    /// <summary>
    /// Secondary project types
    /// Used to organize Case Numbers into folders.
    /// Secondary Project Types are Individual to each Organisation,
    /// but nested within a Primary Project Type
    /// </summary>
    public class ProjectTypeSecondary : EventLogBase {
        public int Id;
        public int PrimaryProjectTypeId;
        public int OrganisationId;
        public int SerialNo;
        public string Name;
        public string Description;
        public int UserGroupId;
        public bool ControlledActivity;

        public ProjectTypeSecondary() { }

        public ProjectTypeSecondary(ref SqlDataReader dr) {
            Id = TypeCast.ToInt(dr["Id"]);
            PrimaryProjectTypeId = TypeCast.ToInt(dr["PrimaryProjectTypeId"]);
            OrganisationId = TypeCast.ToInt(dr["OrganisationId"]);
            SerialNo = TypeCast.ToInt(dr["SerialNo"]);
            Name = TypeCast.ToString(dr["Name"]);
            Description = TypeCast.ToString(dr["Description"]);
            UserGroupId = TypeCast.ToInt(dr["UserGroupId"]);
            ControlledActivity = TypeCast.ToBool(dr["ControlledActivity"]);
        }

        public override VisualItems GetVisualItemsForEventLog(string Event) {
            var retval = new VisualItems();
            retval.Text = eventToText(Event) + " aktivitet [" + SerialNo + "] " + Name;
            retval.Icon = "images/listviewIcons/projectType.png";
            retval.JavaScript = (Event == "DELETE" ?
                                    "alert('Aktiviten er blevet slettet');"
                                    :
                                    "top.frames['root'].frames['frameAdmin'].ProjectTypes_editSubProjectType(" + Id + ");");
            return retval;
        }
    }
}
