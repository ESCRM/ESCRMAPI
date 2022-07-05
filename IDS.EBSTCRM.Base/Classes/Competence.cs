using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Collections;

namespace IDS.EBSTCRM.Base
{
    /// <summary>
    /// Competence for SAM Companies
    /// </summary>
    public class Competence : EventLogBase 
    {
        private int competenceId;
        private string competenceString;
        private bool check;
        private string  description;

        public int ParentId;

        public string  Description
        {
            get { return description; }
            set { description = value; }
        }
	

        public string CompetenceString
        {
            get { return competenceString; }
            set { competenceString = value; }
        }
	

        public int CompetenceId
        {
            get { return competenceId; }
            set { competenceId = value; }
        }

        public bool Check
        {
            get { return check; }
            set { check = value; }
        }

        public Competence()
        { 
        }

        public Competence(ref SqlDataReader dr)
        {
            this.competenceId = TypeCast.ToInt(dr["CompetenceId"]);
            this.competenceString = TypeCast.ToString(dr["competence"]);
            this.description = TypeCast.ToString(dr["description"]);
            this.ParentId = TypeCast.ToInt(dr["ParentId"]);
        }

        public override VisualItems GetVisualItemsForEventLog(string Event)
        {
            VisualItems retval = new VisualItems();

            retval.Text = eventToText(Event) + " kompetence " + competenceString;
            retval.Icon = "images/listviewIcons/county.png";

            retval.JavaScript = (Event == "DELETE" ?
                                    "alert('Kompetencen er blevet slettet');"
                                    :
                                    "top.frames['root'].frames['frameGlobalAdmin'].competences_EditThisCompetence(" + competenceId + ");");



            return retval;
        }

    }
}
