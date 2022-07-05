using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Collections;

namespace IDS.EBSTCRM.Base
{
    /// <summary>
    /// SMV/POT Contact Last Updated by Class
    /// </summary>
    public class lastUpdatedBy
    {
        public string creatorId;
        public string creatorFirstname;
        public string createLastname;
        public string creatorEmail;

        public string updaterId;
        public string updaterFirstname;
        public string updaterLastname;
        public string updaterEmail;

        public DateTime CompanyDateStamp;
        public object CompanylastUpdated;

        public lastUpdatedBy()
        {

        }

        public lastUpdatedBy(ref SqlDataReader dr)
        {
            creatorId = TypeCast.ToString(dr["creatorId"]);
            creatorFirstname = TypeCast.ToString(dr["creatorFirstname"]);
            createLastname = TypeCast.ToString(dr["createLastname"]);
            creatorEmail = TypeCast.ToString(dr["creatorEmail"]);

            updaterId = TypeCast.ToString(dr["updaterId"]);
            updaterFirstname = TypeCast.ToString(dr["updaterFirstname"]);
            updaterLastname = TypeCast.ToString(dr["updaterLastname"]);
            updaterEmail = TypeCast.ToString(dr["updaterEmail"]);

            CompanyDateStamp = TypeCast.ToDateTime(dr["CompanyDateStamp"]);
            CompanylastUpdated = TypeCast.ToDateTimeOrNull(dr["CompanylastUpdated"]);
        }
    }
}
