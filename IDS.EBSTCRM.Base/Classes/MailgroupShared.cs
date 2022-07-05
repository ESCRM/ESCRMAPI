using System;
using System.Collections.Generic;
using System.Text;

namespace IDS.EBSTCRM.Base
{
    [Serializable()]
    public class MailgroupShared
    {
        public int Id { get; set; }
        public int MailgroupId { get; set; }

        public int SharedWithOrganisationId { get; set; }
        public bool Writeable { get; set; }
        public int? SharedOrganisationFolderId { get; set; }

        public string SharedWithOrganisationName { get; set; }

        public MailgroupShared()
        {

        }

        public MailgroupShared(ref System.Data.SqlClient.SqlDataReader dr)
        {
            Id = TypeCast.ToInt(dr["Id"]);
            MailgroupId = TypeCast.ToInt(dr["MailgroupId"]);
            SharedWithOrganisationId = TypeCast.ToInt(dr["SharedWithOrganisationId"]);
            Writeable = TypeCast.ToBool(dr["Writeable"]);
            SharedOrganisationFolderId = TypeCast.ToIntLoose(dr["SharedOrganisationFolderId"]);

            SharedWithOrganisationName = TypeCast.ToString(dr["SharedWithOrganisationName"]);
        }

    }
}
