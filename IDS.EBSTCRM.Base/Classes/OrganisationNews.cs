using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;

namespace IDS.EBSTCRM.Base
{
    /// <summary>
    /// Contains news for the organisation
    /// </summary>
    public class OrganisationNews
    {
        #region Properties

        public int Id { get; set; }
        public int OrganisationId { get; set; }

        public string PrivateNews { get; set; }
        public string SharedNews { get; set; }

        public string PrivateNewsUpdatedBy { get; set; }
        public object PrivateNewsUpdated { get; set; }

        public string SharedNewsUpdatedBy { get; set; }
        public object SharedNewsUpdated { get; set; }


        public string PrivateNewsUpdatedByName { get; set; }
        public string SharedNewsUpdatedByName { get; set; }

        #endregion

        #region Constructors

        public OrganisationNews()
        {

        }

        public OrganisationNews(ref SqlDataReader dr)
        {
            Id = TypeCast.ToInt(dr["Id"]);
            OrganisationId = TypeCast.ToInt(dr["OrganisationId"]);

            PrivateNews = TypeCast.ToString(dr["PrivateNews"]);
            SharedNews = TypeCast.ToString(dr["SharedNews"]);

            PrivateNewsUpdatedBy = TypeCast.ToString(dr["PrivateNewsUpdatedBy"]);
            PrivateNewsUpdated = TypeCast.ToDateTimeOrNull(dr["PrivateNewsUpdated"]);

            SharedNewsUpdatedBy = TypeCast.ToString(dr["SharedNewsUpdatedBy"]);
            SharedNewsUpdated = TypeCast.ToDateTimeOrNull(dr["SharedNewsUpdated"]);


            PrivateNewsUpdatedByName = TypeCast.ToString(dr["PrivateNewsUpdatedByName"]);
            SharedNewsUpdatedByName = TypeCast.ToString(dr["SharedNewsUpdatedByName"]);
        }

        #endregion
    }
}
