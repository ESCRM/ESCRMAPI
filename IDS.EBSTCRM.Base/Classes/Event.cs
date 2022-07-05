using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;


namespace IDS.EBSTCRM.Base
{
    /// <summary>
    /// Event Logger class
    /// Used for recent events in CRM1.0, now OBSOLETE
    /// </summary>
    public class Event
    {
        public int OrganisationId;
        public string UserId;
        public string EventType;
        public string EventOwner;
        public string EventId;
        public string Username;
        public string UserEmail;
        public string UserFirstname;
        public string UserLastname;
        public DateTime DateStamp;
        public string Icon;
        public string Text;
        public string JavaScript;

        public Event()
        {

        }

        public Event(ref SqlDataReader dr)
        {
            OrganisationId = TypeCast.ToInt(dr["OrganisationId"]);
            UserId = TypeCast.ToString(dr["UserId"]);
            EventType = TypeCast.ToString(dr["EventType"]);
            EventOwner = TypeCast.ToString(dr["EventOwner"]);
            EventId = TypeCast.ToString(dr["EventId"]);
            Username = TypeCast.ToString(dr["Username"]);
            UserEmail = TypeCast.ToString(dr["UserEmail"]);
            UserFirstname = TypeCast.ToString(dr["UserFirstname"]);
            UserLastname = TypeCast.ToString(dr["UserLastname"]);
            DateStamp = TypeCast.ToDateTime(dr["DateStamp"]);

            Icon = TypeCast.ToString(dr["Icon"]);
            Text = TypeCast.ToString(dr["Text"]);
            JavaScript = TypeCast.ToString(dr["JavaScript"]);
        }

    }
}
