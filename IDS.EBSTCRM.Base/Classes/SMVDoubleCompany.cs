using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Collections;

namespace IDS.EBSTCRM.Base
{
    /// <summary>
    /// Listview item displaying a doublet company
    /// </summary>
    public class SMVDoubleCompany
    {
        public int CompanyId;
        public string CVR;
        public string CompanyName;
        public DateTime DateStamp;
        public string UserId;
        public string UserInfo;
        public int OrganisationId;
        public string OrganisationName;
        public int DoubleCount;
        public string PNr;

        public SMVDoubleCompany()
        {

        }

        public SMVDoubleCompany(ref SqlDataReader dr)
        {
            CompanyId = TypeCast.ToInt(dr["CompanyId"]);
            CVR = TypeCast.ToString(dr["CVR"]);
            CompanyName = TypeCast.ToString(dr["CompanyName"]);
            DateStamp = TypeCast.ToDateTime(dr["DateStamp"]);
            UserId = TypeCast.ToString(dr["UserId"]);
            UserInfo = TypeCast.ToString(dr["UserInfo"]);
            OrganisationId = TypeCast.ToInt(dr["OrganisationId"]);
            OrganisationName = TypeCast.ToString(dr["OrganisationName"]);
            DoubleCount = TypeCast.ToInt(dr["DoubleCount"]);
            PNr = TypeCast.ToString(dr["PNr"]);

        }

    }
}
