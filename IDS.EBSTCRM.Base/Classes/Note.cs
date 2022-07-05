using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Collections;

namespace IDS.EBSTCRM.Base
{
    /// <summary>
    /// Deleted notes with reference to the deleted Contact (for a better listview in Trashcan)
    /// </summary>
    public class NoteDeleted : ContactDeleted 
    {
        public int NoteId;
        public string NoteName;
        public DateTime NoteDate;
        public DateTime DeletedDate;
        public bool NoteIsHighPriority;
        public int noteContactId;
        public int noteCompanyId;

        public NoteDeleted()
        {

        }

        public NoteDeleted(ref SqlDataReader dr)
            : base(ref dr)
        {
            NoteId = TypeCast.ToInt(dr["NoteId"]);
            NoteName = TypeCast.ToString(dr["NoteName"]);
            NoteDate = TypeCast.ToDateTime(dr["NoteDate"]);
            DeletedDate = TypeCast.ToDateTime(dr["DeletedDate"]);
            NoteIsHighPriority = TypeCast.ToBool(dr["NoteIsHighPriority"]);

            noteContactId = TypeCast.ToInt(dr["noteContactId"]);
            noteCompanyId = TypeCast.ToInt(dr["noteCompanyId"]);
        }
    }

    /// <summary>
    /// Note saved on a SMV/POT Contact or Company, visible when viewing a SMV/POT Contact or Company
    /// </summary>
    public class Note : EventLogBase
    {
        #region Declarations / Properties

        public int id;
        public int organisationId;
        public int contactId;
        public int companyId;
        public int VisibleTo;
        public bool isHighPriority;
        public string name;
        public string note;
        public DateTime dateCreated;

        public string userId;
        public string firstname;
        public string lastname;
        public string email;
        public string username;
        public int userRole;

        public object ReminderDate;
        public bool ReminderDismissed;
        public string NoteType;
        public string NoteType2;
        public object NoteDate;

        public string VisibleToTexts;

        public List<int> SharedWithOrganisations = new List<int>();

        public bool NoteFromBatchJob;

        #endregion


        public Note()
        {
            dateCreated = DateTime.Now;

        }

        /// <summary>
        /// Constructs a note object
        /// </summary>
        /// <param name="dr">Data reader</param>
        /// <param name="isSingle">When opening a single Note, a reference to whom the Notes is shared with is required</param>
        public Note(ref SqlDataReader dr, bool isSingle)
        {
            id = TypeCast.ToInt(dr["id"]);
            organisationId = TypeCast.ToInt(dr["organisationId"]);
            contactId = TypeCast.ToInt(dr["contactId"]);
            companyId = TypeCast.ToInt(dr["companyId"]);
            VisibleTo = TypeCast.ToInt(dr["VisibleTo"]);
            isHighPriority = TypeCast.ToBool(dr["isHighPriority"]);
            name = TypeCast.ToString(dr["name"]);
            note = TypeCast.ToString(dr["note"]);
            dateCreated = TypeCast.ToDateTime(dr["dateCreated"]);

            userId = TypeCast.ToString(dr["userId"]);
            firstname = TypeCast.ToString(dr["firstname"]);
            lastname = TypeCast.ToString(dr["lastname"]);
            email = TypeCast.ToString(dr["email"]);
            username = TypeCast.ToString(dr["username"]);

            ReminderDate = TypeCast.ToDateTimeOrNull(dr["ReminderDate"]);
            ReminderDismissed = TypeCast.ToBool(dr["ReminderDismissed"]);
            NoteType = TypeCast.ToString(dr["NoteType"]);
            NoteType2 = TypeCast.ToString(dr["NoteType2"]);

            NoteDate = TypeCast.ToDateTimeOrNull(dr["NoteDate"]);

            VisibleToTexts = TypeCast.ToString(dr["VisibleToTexts"]);

            userRole = TypeCast.ToInt(dr["userRole"]);

            NoteFromBatchJob = TypeCast.ToBool(dr["NoteFromBatchJob"]);

            //If the note is opened as a single object, read shared with from the DataReader
            //This is used, when the note is re-saved, making sure no shares are lost.
            if (isSingle)
            {
                dr.NextResult();

                while (dr.Read())
                {
                    SharedWithOrganisations.Add(TypeCast.ToInt(dr["SharedWithOrganisationId"]));
                }
            }
        }
    }

    /// <summary>
    /// Loads a note with a reminder, used by root.aspx in CRMv2.
    /// Ensuring an alert will appear, when Reminder date/time is reached.
    /// </summary>
    public class NoteWithReminder : Note
    {
        public int SecondsToInvoke;

        public NoteWithReminder()
        {

        }

        public NoteWithReminder(ref SqlDataReader dr, bool isSingle)
            : base(ref dr, isSingle)
        {
            SecondsToInvoke = TypeCast.ToInt(dr["secondsToInvoke"]);
        }
    }

    /// <summary>
    /// Loads a note with a reminder, and the SMV/POT Contact/Company Data,
    /// for view in a listview.
    /// </summary>
    public class NoteWithReminderAndContactData : NoteWithReminder
    {
        public string ContactFirstname;
        public string ContactLastname;
        public string CompanyName;
        public string ContactPhone1;
        public string ContactPhone2;
        public string ContactEmail;

        public NoteWithReminderAndContactData()
        {

        }

        public NoteWithReminderAndContactData(ref SqlDataReader dr, bool isSingle)
            : base(ref dr, isSingle)
        {
            ContactFirstname = TypeCast.ToString(dr["Fornavn"]);
            ContactLastname = TypeCast.ToString(dr["Efternavn"]);
            CompanyName = TypeCast.ToString(dr["Firmanavn"]);
            ContactPhone1 = TypeCast.ToString(dr["Telefon1"]);
            ContactPhone2 = TypeCast.ToString(dr["Telefon2"]);
            ContactEmail = TypeCast.ToString(dr["Email"]);
        }
    }

}
