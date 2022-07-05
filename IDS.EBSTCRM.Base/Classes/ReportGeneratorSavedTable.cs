using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;

namespace IDS.EBSTCRM.Base
{
    /// <summary>
    /// ReportGenerator
    /// Saved table within a saved Report
    /// </summary>
    public class ReportGeneratorSavedTable
    {
        public int Id;
        public string TableName;
        public int X;
        public int Y;
        public int Width;
        public int Height;

        public ReportGeneratorSavedTable()
        {

        }

        public ReportGeneratorSavedTable(ref SqlDataReader dr)
        {
            Id = TypeCast.ToInt(dr["Id"]);
            TableName = TypeCast.ToString(dr["TableName"]);
            X = TypeCast.ToInt(dr["X"]);
            Y = TypeCast.ToInt(dr["Y"]);
            Width = TypeCast.ToInt(dr["Width"]);
            Height = TypeCast.ToInt(dr["Height"]);
        }

    }
}
