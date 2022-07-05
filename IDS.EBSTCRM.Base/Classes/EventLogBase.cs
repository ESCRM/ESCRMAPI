using System;
using System.Collections.Generic;
using System.Text;

namespace IDS.EBSTCRM.Base
{
    /// <summary>
    /// OBSOLETE: Was used by CRM V1, for displaying users events in a html frame.
    /// Kept while CRM V1 is running.
    /// </summary>
    [Serializable()]
    public class EventLogBase
    {
        public struct VisualItems
        {
            public string Icon;
            public string Text;
            public string JavaScript;

            public VisualItems(string icon, string text, string javaScript)
            {
                Icon = icon;
                Text = text;
                JavaScript = javaScript;
            }
        }

        public string eventToText(string Event)
        {
            switch (Event)
            {
                case "CREATE":
                    return "Oprettet";
                case "UPDATE":
                    return "Opdateret";
                case "DELETE":
                    return "Slettet";
                case "APPROVED":
                    return "Godkendt";
                case "NOTAPPROVED":
                    return "Afvist";

            }

            return "Ukendt";
        }

        public virtual VisualItems GetVisualItemsForEventLog(string Event)
        {
            return new VisualItems("images/listviewIcons/null.png", eventToText(Event) + " EventLogBase", "");
        }
    }
}
