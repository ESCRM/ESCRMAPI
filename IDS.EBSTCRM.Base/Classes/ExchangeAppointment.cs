using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Collections;

namespace IDS.EBSTCRM.Base
{
    /// <summary>
    /// Exchange Integration Appointment
    /// Saved locally in Database, with reference using URL to the Exchange Server
    /// </summary>
    public class ExchangeAppointment : EventLogBase 
    {
        public int id;
        public int organisationId;
        public string userId;
        public int contactId;
        public string contactEmail;
        public string url;
        public string subject;
        public string location;
        public DateTime dateStart;
        public DateTime dateEnd;
        public string body;

        public ExchangeAppointment()
        {

        }

        public ExchangeAppointment(ref SqlDataReader dr)
        {
            id = TypeCast.ToInt(dr["id"]);
            organisationId = TypeCast.ToInt(dr["organisationId"]);
            userId = TypeCast.ToString(dr["userId"]);
            contactId = TypeCast.ToInt(dr["contactId"]);
            contactEmail = TypeCast.ToString(dr["contactEmail"]);
            url = TypeCast.ToString(dr["url"]);
            subject = TypeCast.ToString(dr["subject"]);
            location = TypeCast.ToString(dr["location"]);
            dateStart = TypeCast.ToDateTime(dr["dateStart"]);
            dateEnd = TypeCast.ToDateTime(dr["dateEnd"]);
            body = TypeCast.ToString(dr["body"]);

        }


    }
}
