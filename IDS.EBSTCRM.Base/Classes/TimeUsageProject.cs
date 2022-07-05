using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Collections;
using System.Data;
using IDS.EBSTCRM.Base.Helper;

namespace IDS.EBSTCRM.Base {
    /// <summary>
    /// Each Hour Registration made on a given Case Number
    /// Is stored in a TimeUsage class
    /// </summary>
    public class TimeUsageProject : EventLogBase {
        public int id;
        public int timeUsageId;
        public string userId;
        public int organisationId;
        public int dayOfWeek;
        public decimal timeSpent;
        public int PrimaryProjectTypeId;
        public string PrimaryProjectTypeName;
        public int SecondaryProjectTypeId;
        public string SecondaryProjectTypeName;
        public int SecondaryProjectTypeSerialNo;
        public int CaseNumberId;
        public int CaseNumberRelationId;
        public int CaseNumber;
        public string CaseNumberName;
        public string Description;
        public int RowIndex;

        public int NavisionIndex;

        public string CounterPostedBy;
        public object CounterPosted;


        public int Status;
        public string RejectedDescription;

        public TimeUsageProject() {

        }

        public TimeUsageProject(ref SqlDataReader dr) {
            id = TypeCast.ToInt(dr["id"]);
            timeUsageId = TypeCast.ToInt(dr["timeUsageId"]);
            userId = TypeCast.ToString(dr["userId"]);
            organisationId = TypeCast.ToInt(dr["organisationId"]);
            dayOfWeek = TypeCast.ToInt(dr["dayOfWeek"]);
            timeSpent = TypeCast.ToDecimal(dr["timeSpent"]);
            PrimaryProjectTypeId = TypeCast.ToInt(dr["PrimaryProjectTypeId"]);
            PrimaryProjectTypeName = TypeCast.ToString(dr["PrimaryProjectTypeName"]);
            SecondaryProjectTypeId = TypeCast.ToInt(dr["SecondaryProjectTypeId"]);
            SecondaryProjectTypeName = TypeCast.ToString(dr["SecondaryProjectTypeName"]);
            SecondaryProjectTypeSerialNo = TypeCast.ToInt(dr["SecondaryProjectTypeSerialNo"]);
            CaseNumberId = TypeCast.ToInt(dr["CaseNumberId"]);
            CaseNumberRelationId = TypeCast.ToInt(dr["CaseNumberRelationId"]);
            CaseNumber = TypeCast.ToInt(dr["CaseNumber"]);
            CaseNumberName = TypeCast.ToString(dr["CaseNumberName"]);
            Description = TypeCast.ToString(dr["Description"]);
            RowIndex = TypeCast.ToInt(dr["RowIndex"]);

            NavisionIndex = TypeCast.ToInt(dr["NavisionIndex"]);

            CounterPosted = TypeCast.ToDateTimeOrNull(dr["CounterPosted"]);
            CounterPostedBy = TypeCast.ToString(dr["CounterPostedBy"]);

            Status = TypeCast.ToInt(dr["Status"]);
            RejectedDescription = TypeCast.ToString(dr["RejectedDescription"]);
        }
    }
}