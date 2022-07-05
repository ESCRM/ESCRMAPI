using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;


namespace IDS.EBSTCRM.Base
{
    /// <summary>
    /// Early Warning Volunteers Sector Code
    /// Used to define the Volunteers Competences
    /// </summary>
    public class EarlyWarningSectorCode
    {
        public int SectorCodeId;
        public string Name;
        public bool Checked;

        public EarlyWarningSectorCode()
        {

        }

        public EarlyWarningSectorCode(ref SqlDataReader dr)
        {
            SectorCodeId = TypeCast.ToInt(dr["SectorCodeId"]);
            Name = TypeCast.ToString(dr["Name"]);
            Checked = TypeCast.ToInt(dr["Checked"])>0;
        }
    }
}
