using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;

namespace IDS.EBSTCRM.Base
{
    /// <summary>
    /// OBSOLOTETE FROM CRMV1.0
    /// </summary>
    public class BugTracker
    {
        public int Id;
        public int ResponseToId;
        public string Title;
        public DateTime Date;
        public string Comment;
        public int Type;
        public int Status;
        public DateTime DateClosed;
        public int MinimumAccessLevel;
        public string CreatedBy;

        public BugTracker()
        {

        }

        public BugTracker(ref SqlDataReader dr)
        {
            Id = TypeCast.ToInt(dr["Id"]);
            ResponseToId = TypeCast.ToInt(dr["ResponseToId"]);
            Title = TypeCast.ToString(dr["Title"]);
            Date = TypeCast.ToDateTime(dr["Date"]);
            Comment = TypeCast.ToString(dr["Comment"]);
            Type = TypeCast.ToInt(dr["Type"]);
            Status = TypeCast.ToInt(dr["Status"]);
            DateClosed = TypeCast.ToDateTime(dr["DateClosed"]);
            MinimumAccessLevel = TypeCast.ToInt(dr["MinimumAccessLevel"]);
            CreatedBy = TypeCast.ToString(dr["CreatedBy"]);

        }
    }
}
