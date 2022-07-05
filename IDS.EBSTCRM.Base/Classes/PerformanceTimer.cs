using System;
using System.Collections.Generic;
using System.Text;

namespace IDS.EBSTCRM.Base
{
    /// <summary>
    /// Performance timer, times Database speed
    /// OBSOLOTE: Part of CRM1.0
    /// </summary>
    public class PerformanceTimer
    {
        public enum PerformaceRegion
        {
            UnknownRegion=0,

            SMVandPOTContactSearch=1,
            SMVandPOTContactOpen = 2,

            SAMContactSearch=10,

            HourUsageListItems=20,
            HourUsageOpen=21,

            DrivingRegistrationListItems=30,
            DrivingRegistrationOpen=31,

            FileArchiveSearch=40,
            FileArchiveOpenFolder=41,

            CalendarOpenDay=50,
            CalendarOpenItem=51,

            MailGroupsOpenListGroupContacts = 60,
            MailGroupsOpenGroup = 61,

            UsersListUsers = 70
        }

        public long TimerStart = 0;
        public long TimeUsed = 0;
        public User CurrentUser = null;
        public PerformaceRegion CurrentPerformaceRegion = 0;
        public int RowCount = 0;

        public PerformanceTimer(User U, PerformaceRegion pr)
        {
            TimerStart = System.Environment.TickCount;
            CurrentUser = U;
            CurrentPerformaceRegion = pr;
        }

        public void RecordTimeUsed(ref SQLDB sql, int rowCount)
        {
            TimeUsed = System.Environment.TickCount - TimerStart;

            RowCount = rowCount;

            //sql.PerformanceTimers_AddEntry(this);

        }
    }
}
