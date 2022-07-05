using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Collections;

namespace IDS.EBSTCRM.Base
{
    /// <summary>
    /// Administrative Hour Usage Item
    /// Inherits TimeUsage and extends with Approval states and such
    /// </summary>
    public class TimeUsageAdmin : TimeUsage
    {
        public string Username;
        public string UserFirstname;
        public string UserLastname;

        public TimeUsageAdmin()
            : base()
        {

        }

        public TimeUsageAdmin(ref SqlDataReader dr)
            : base(ref dr)
        {
            Username = TypeCast.ToString(dr["username"]);
            UserFirstname = TypeCast.ToString(dr["UserFirstname"]);
            UserLastname = TypeCast.ToString(dr["UserLastname"]);
        }

    }

    /// <summary>
    /// Hour usage registration for a week
    /// Contains all user registration for the given week
    /// </summary>
    public class TimeUsage : EventLogBase
    {
        public int id;
        public int organisationId;
        public string userId;
        public DateTime weekDate;
        public int weekNumber;
        public decimal year;
        public decimal monday;
        public decimal tuesday;
        public decimal wednesday;
        public decimal thursday;
        public decimal friday;
        public decimal saturday;
        public decimal sunday;
        public decimal total;
        public Object pendingApproval;
        public Object approved;
        public string approvedBy;

        public Object notApproved;
        public string notApprovedBy;

        public string status;

        public string NotApprovedReason = "";

        public string ApprovedByUsername;
        public string ApprovedByFirstname;
        public string ApprovedByLastname;

        public string NotApprovedByUsername;
        public string NotApprovedByFirstname;
        public string NotApprovedByLastname;

        public string UserFirstname;
        public string UserLastname;
        public string Username;

        public TimeUsage()
        {

        }

        public TimeUsage(ref SqlDataReader dr)
        {
            id = TypeCast.ToInt(dr["id"]);
            organisationId = TypeCast.ToInt(dr["organisationId"]);
            userId = TypeCast.ToString(dr["userId"]);
            weekDate = TypeCast.ToDateTime(dr["weekDate"]);
            weekNumber = TypeCast.ToInt(dr["weekNumber"]);
            year = TypeCast.ToDecimal(dr["year"]);
            monday = TypeCast.ToDecimal(dr["monday"]);
            tuesday = TypeCast.ToDecimal(dr["tuesday"]);
            wednesday = TypeCast.ToDecimal(dr["wednesday"]);
            thursday = TypeCast.ToDecimal(dr["thursday"]);
            friday = TypeCast.ToDecimal(dr["friday"]);
            saturday = TypeCast.ToDecimal(dr["saturday"]);
            sunday = TypeCast.ToDecimal(dr["sunday"]);
            total = TypeCast.ToDecimal(dr["total"]);
            pendingApproval = TypeCast.ToDateTimeOrNull(dr["pendingApproval"]);
            approved = TypeCast.ToDateTimeOrNull(dr["approved"]);
            approvedBy = TypeCast.ToString(dr["approvedBy"]);

            notApproved = TypeCast.ToDateTimeOrNull(dr["notApproved"]);
            notApprovedBy = TypeCast.ToString(dr["notApprovedBy"]);

            status = TypeCast.ToString(dr["status"]);
            NotApprovedReason = TypeCast.ToString(dr["NotApprovedReason"]);


            ApprovedByUsername = TypeCast.ToString(dr["ApprovedByUsername"]);
            ApprovedByFirstname = TypeCast.ToString(dr["ApprovedByFirstname"]);
            ApprovedByLastname = TypeCast.ToString(dr["ApprovedByLastname"]);

            NotApprovedByUsername = TypeCast.ToString(dr["NotApprovedByUsername"]);
            NotApprovedByFirstname = TypeCast.ToString(dr["NotApprovedByFirstname"]);
            NotApprovedByLastname = TypeCast.ToString(dr["NotApprovedByLastname"]);

            UserFirstname = TypeCast.ToString(dr["UserFirstname"]);
            UserLastname = TypeCast.ToString(dr["UserLastname"]);
            Username = TypeCast.ToString(dr["Username"]);
        }

        public override VisualItems GetVisualItemsForEventLog(string Event)
        {
            VisualItems retval = new VisualItems();

            retval.Text = eventToText(Event) + " ugeregnskabet for " + weekNumber.ToString("#0") + " / " + year;
            retval.Icon = "images/listviewIcons/timeUsage.png";

            retval.JavaScript = (Event == "DELETE" ?
                                    "alert('Ugeregnskabet er blevet slettet');"
                                    :
                                    "top.frames['root'].frames['frameUser'].timeUsage_editThisWeek(" + id + ");");



            return retval;
        }
    }
}
