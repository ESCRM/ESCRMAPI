using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;

namespace IDS.EBSTCRM.Base
{
    /// <summary>
    /// ReportGenerator
    /// Saved filters to a specific field on a saved Report
    /// </summary>
    public class ReportGeneratorSavedFilter
    {
        public int ReportId;
        public string FieldId;
        public string FilterCondition;
        public string FilterValue;

        public ReportGeneratorSavedFilter(ref SqlDataReader dr)
        {
            ReportId = TypeCast.ToInt(dr["reportId"]);
            FieldId = TypeCast.ToString(dr["FieldId"]);
            FilterCondition = TypeCast.ToString(dr["FilterCondition"]);
            FilterValue = TypeCast.ToString(dr["FilterValue"]);
        }

        public ReportGeneratorSavedFilter()
        {

        }
    }
}
