using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;


namespace IDS.EBSTCRM.Base
{
    /// <summary>
    /// Reported meeting in CRM - used to register Meetings stored on an Exchange Server
    /// </summary>
    public class MeetingReport
    {
        public int Id;
        public int OrganisationId;
        public string UserId;
        public string MeetingUrl;
        public string Subject;
        public string Location;
        public DateTime DateStart;
        public DateTime DateEnd;
        public string Body;
        public Decimal Timespent;
        public int PrimaryProjectTypeId;
        public string PrimaryProjectTypeName;
        public int SecondaryProjectTypeId;
        public string SecondaryProjectTypeName;
        public int SecondaryProjectTypeSerialNo;
        public int CaseNumberId;
        public int CaseNumberRelationId;
        public int CaseNumber;
        public string CaseNumberName;
        public DateTime DateStamp;
        public int CompanyId;

        public MeetingReport()
        {

        }

        public MeetingReport(ref SqlDataReader dr)
        {
            CompanyId = TypeCast.ToInt(dr["companyId"]);

            Id = TypeCast.ToInt(dr["Id"]);
            OrganisationId = TypeCast.ToInt(dr["OrganisationId"]);
            UserId = TypeCast.ToString(dr["UserId"]);
            MeetingUrl = TypeCast.ToString(dr["MeetingUrl"]);
            Subject = TypeCast.ToString(dr["Subject"]);
            Location = TypeCast.ToString(dr["Location"]);
            DateStart = TypeCast.ToDateTime(dr["DateStart"]);
            DateEnd = TypeCast.ToDateTime(dr["DateEnd"]);
            Body = TypeCast.ToString(dr["Body"]);
            Timespent = TypeCast.ToDecimal(dr["Timespent"]);
            PrimaryProjectTypeId = TypeCast.ToInt(dr["PrimaryProjectTypeId"]);
            PrimaryProjectTypeName = TypeCast.ToString(dr["PrimaryProjectTypeName"]);
            SecondaryProjectTypeId = TypeCast.ToInt(dr["SecondaryProjectTypeId"]);
            SecondaryProjectTypeName = TypeCast.ToString(dr["SecondaryProjectTypeName"]);
            SecondaryProjectTypeSerialNo = TypeCast.ToInt(dr["SecondaryProjectTypeSerialNo"]);
            CaseNumberId = TypeCast.ToInt(dr["CaseNumberId"]);
            CaseNumberRelationId = TypeCast.ToInt(dr["CaseNumberRelationId"]);
            CaseNumber = TypeCast.ToInt(dr["CaseNumber"]);
            CaseNumberName = TypeCast.ToString(dr["CaseNumberName"]);
            DateStamp = TypeCast.ToDateTime(dr["DateStamp"]);

        }
    }
}
