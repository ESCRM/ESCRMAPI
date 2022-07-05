using System;
using System.Collections.Generic;
using System.Text;

namespace IDS.EBSTCRM.Base
{
    /// <summary>
    /// AVN Entity with reminder(s)
    /// This class is used to display any reminders automatically in the CRM System
    /// </summary>
    public class AVNEntityWithReminder : AVNEntity
    {
        public string Firstname { get;set;}
        public string Lastname { get;set;}
        public string Email { get;set;}
        public string Username { get;set;}
        public UserRoles UserRole { get;set;}

        public int SecondsToInvoke { get; set; }

        public DateTime ReminderDate { get; set; }
        public int ReminderId { get; set; }

        /// <summary>
        /// Constructs a new Reminder from the database
        /// </summary>
        /// <param name="dr"></param>
        public AVNEntityWithReminder(ref System.Data.SqlClient.SqlDataReader dr)
            : base(ref dr)
        {
            Firstname = TypeCast.ToString(dr["Firstname"]);
            Lastname = TypeCast.ToString(dr["Lastname"]);
            Email = TypeCast.ToString(dr["Email"]);
            Username = TypeCast.ToString(dr["Username"]);
            UserRole = (UserRoles)TypeCast.ToInt(dr["UserRole"]);
            SecondsToInvoke = TypeCast.ToInt(dr["SecondsToInvoke"]);

            ReminderDate = TypeCast.ToDateTime(dr["ReminderDate"]);
            ReminderId = TypeCast.ToInt(dr["ReminderId"]);
        }
    }

    /// <summary>
    /// AVN Entity with reminder(s)
    /// This class is used to display any reminders automatically in the CRM System
    /// This class also containes "saved to" details
    /// </summary>
    public class AVNEntityWithReminderWithContactData : AVNEntityWithReminder
    {
		public string SMV_Fornavn { get; set; }
		public string SMV_Efternavn { get; set; }
		public string SMV_Firmanavn { get; set; }
		public string SMV_Telefon1 { get; set; }
		public string SMV_Telefon2 { get; set; }
        public string SMV_Email { get; set; }

        /// <summary>
        /// Construct a new Reminder with contact data from the database
        /// </summary>
        /// <param name="dr"></param>
        public AVNEntityWithReminderWithContactData(ref System.Data.SqlClient.SqlDataReader dr)
            : base(ref dr)
        {
            SMV_Fornavn = TypeCast.ToString(dr["SMV_Fornavn"]);
            SMV_Efternavn = TypeCast.ToString(dr["SMV_Efternavn"]);
            SMV_Firmanavn = TypeCast.ToString(dr["SMV_Firmanavn"]);
            SMV_Telefon1 = TypeCast.ToString(dr["SMV_Telefon1"]);
            SMV_Telefon2 = TypeCast.ToString(dr["SMV_Telefon2"]);
            SMV_Email = TypeCast.ToString(dr["SMV_Email"]);
        }
    }
}

