using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;

namespace IDS.EBSTCRM.Base
{
    /// <summary>
    /// OBSOLOTE FROM CRM1.0
    /// </summary>
    [Serializable()]
    public class DynamicText : EventLogBase
    {
        private int Id;
        private string Headword;
        private string Description;
        private string Category;

        public string description
        {
            get { return Description; }
            set { Description = value; }
        }
	

        public string headword
        {
            get { return Headword; }
            set { Headword = value; }
        }

        public string category {
            get { return Category; }
            set { Category = value; }
        }

        public int id
        {
            get { return Id; }
            set { Id = value; }
        }

        public DynamicText()
        { 
        }

        public DynamicText(ref SqlDataReader dr)
        {
            Populate(ref dr);
        }

        private void Populate(ref SqlDataReader dr)
        {
            Id = TypeCast.ToInt(dr["Id"]);
            Headword = TypeCast.ToString(dr["Headword"]);
            Description = TypeCast.ToString(dr["Description"]);
            Category = TypeCast.ToString(dr["Category"]);
        }

        public override VisualItems GetVisualItemsForEventLog(string Event)
        {
            VisualItems retval = new VisualItems();

            retval.Text = eventToText(Event) + " emneordet " + Headword;
            retval.Icon = "images/listviewIcons/dynamicText.png";

            retval.JavaScript = (Event == "DELETE" ?
                                    "alert('Emneordet er blevet slettet');"
                                    :
                                    "top.frames['root'].frames['frameGlobalAdmin'].dynamictexts_EditThisDynamictexts(" + Id + ");");



            return retval;
        }
    }
}
