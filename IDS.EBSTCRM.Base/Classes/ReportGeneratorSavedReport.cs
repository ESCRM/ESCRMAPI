using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;

namespace IDS.EBSTCRM.Base
{
    /// <summary>
    /// ReportGenerator
    /// Saved Report information
    /// </summary>
    public class ReportGeneratorSavedReport
    {
        public int Id;
        public string Name;
        public DateTime DateCreated;
        public object LastUpdated;

        public string CreatedBy;
        public string LastUpdatedBy;

        public string User_CreatedBy;
        public string User_LastUpdatedBy;

        public object SharedToUserLevel;

        public int VisibleTo;
        public string VisibleToTexts;

        public string SQL;
        public string IntegrationKey;

        public string User_IntegrationApprovedBy;
        public string IntegrationApprovedBy;
        public DateTime? IntegrationApprovedDate;

        public string IntegrationRejectedBy;
        public string User_IntegrationRejectedBy;
        public DateTime? IntegrationRejectedDate;
        public string IntegrationRejectedReason;
        public string IntegrationReportDescription;

        public List<int> SharedWithOrganisations = new List<int>();

        public ReportGeneratorSavedReport(ref SqlDataReader dr, bool isSingle)
        {
            Id = TypeCast.ToInt(dr["Id"]);
            Name = TypeCast.ToString(dr["Name"]);

            DateCreated = TypeCast.ToDateTime(dr["DateCreated"]);
            LastUpdated = TypeCast.ToDateTimeOrNull(dr["LastUpdated"]);

            SharedToUserLevel = TypeCast.ToIntOrNull(dr["SharedToUserLevel"]);

            User_CreatedBy = TypeCast.ToString(dr["User_CreatedBy"]);
            User_LastUpdatedBy = TypeCast.ToString(dr["User_LastUpdatedBy"]);

            CreatedBy = TypeCast.ToString(dr["CreatedBy"]);
            LastUpdatedBy = TypeCast.ToString(dr["LastUpdatedBy"]);

            VisibleTo = TypeCast.ToInt(dr["VisibleTo"]);
            VisibleToTexts = TypeCast.ToString(dr["VisibleToTexts"]);
            
            SQL = TypeCast.ToString(dr["SQL"]);
            IntegrationKey = TypeCast.ToString(dr["IntegrationKey"]);

            User_IntegrationApprovedBy = TypeCast.ToString(dr["User_IntegrationApprovedBy"]);
            IntegrationApprovedBy = TypeCast.ToString(dr["IntegrationApprovedBy"]);
            IntegrationApprovedDate = TypeCast.ToDateTimeLoose(dr["IntegrationApprovedDate"]);

            IntegrationRejectedBy = TypeCast.ToString(dr["User_IntegrationApprovedBy"]);
            User_IntegrationRejectedBy = TypeCast.ToString(dr["User_IntegrationRejectedBy"]);
            IntegrationRejectedDate = TypeCast.ToDateTimeLoose(dr["IntegrationRejectedDate"]);
            IntegrationRejectedReason = TypeCast.ToString(dr["IntegrationRejectedReason"]);
            IntegrationReportDescription = TypeCast.ToString(dr["IntegrationReportDescription"]);

            if (isSingle)
            {
                dr.NextResult();

                while (dr.Read())
                {
                    SharedWithOrganisations.Add(TypeCast.ToInt(dr["SharedWithOrganisationId"]));
                }
            }
            else
            {
                string sharedWith = TypeCast.ToString(dr["SharedWithOrganisations"]);
                if (sharedWith != "")
                {
                    foreach (string s in sharedWith.Split(','))
                    {
                        int i = TypeCast.ToInt(s);
                        if (i > 0)
                            SharedWithOrganisations.Add(i);
                    }
                }
            }
        }

        public ReportGeneratorSavedReport()
        {

        }

        public string SharedTo
        {
            get
            {
                if(this.SharedToUserLevel == null)
                    return "";

                int level = TypeCast.ToInt(this.SharedToUserLevel);

                switch (level)
                {
                    case 0:
                        return "Konsulenter og opefter";

                    case 1:
                        return "Mellemledere og opefter";

                    case 2:
                        return "Direktører og opefter";

                    case 3:
                        return "Administratorer";
                }

                return "Administratorer";
            }
        }
    }
}
