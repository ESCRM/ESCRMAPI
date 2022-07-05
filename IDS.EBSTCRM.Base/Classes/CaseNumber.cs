using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;

namespace IDS.EBSTCRM.Base
{
    /// <summary>
    /// Casenumber class (Varenumre)
    /// Used with Primary- and Secondary ProjectTypes for Hour and Driving Registration
    /// </summary>
    public class CaseNumber : EventLogBase 
    {
        public int Id;
        public int OrganisationId;
        public int Number;
        public string Name;
        public string Description;

        public CaseNumber()
        {

        }

        public CaseNumber(ref SqlDataReader dr)
        {
            Id=TypeCast.ToInt(dr["id"]);
            OrganisationId=TypeCast.ToInt(dr["OrganisationId"]);
            Number = TypeCast.ToInt(dr["CaseNumber"]);
            Name = TypeCast.ToString(dr["Name"]);
            Description = TypeCast.ToString(dr["Description"]);
        }


        public override VisualItems GetVisualItemsForEventLog(string Event)
        {
            VisualItems retval = new VisualItems();

            retval.Text = eventToText(Event) + " varennr. [" + Number + "] " + Name;
            retval.Icon = "images/listviewIcons/casenumber.png";

            retval.JavaScript = (Event == "DELETE" ?
                                    "alert('Varenummer er blevet slettet');"
                                    :
                                    "top.frames['root'].frames['frameAdmin'].casenumbers_editThisCaseNumber(" + Id + ");");
                


            return retval;
        }
            
    }
}
