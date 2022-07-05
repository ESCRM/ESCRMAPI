using System;
using System.Collections.Generic;
using System.Text;

namespace IDS.EBSTCRM.Base
{
    /// <summary>
    /// AVN Entity default sharing class
    /// </summary>
    public class AVNEntityDefaultShared
    {
        public int Id { get; set; }
        public int OwnerOrganisationId { get; set; }
        public int OrganisationId { get; set; }
        public int UsergroupId { get; set; }
        public int ReadWriteState { get; set; }
        public string SharedWithOrganisationName { get; set; }
        public string UserGroupName { get; set; }
        public string UserGroupIcon { get; set; }

        public AVNEntityDefaultShared()
        {

        }

        /// <summary>
        /// Contructs a new Sharing Level
        /// </summary>
        /// <param name="ownerOrganisationId"></param>
        /// <param name="organisationId"></param>
        /// <param name="usergroupId"></param>
        /// <param name="readWriteState"></param>
        public AVNEntityDefaultShared(int ownerOrganisationId, int organisationId, int usergroupId, int readWriteState)
        {
            OwnerOrganisationId = ownerOrganisationId;
            OrganisationId = organisationId;
            UsergroupId = usergroupId;
            ReadWriteState = readWriteState;
        }

        /// <summary>
        /// Contructs a new Sharing Level from the database
        /// </summary>
        /// <param name="dr"></param>
        public AVNEntityDefaultShared(ref System.Data.SqlClient.SqlDataReader dr)
        {
            Id = TypeCast.ToInt(dr["Id"]);
            OwnerOrganisationId = TypeCast.ToInt(dr["OwnerOrganisationId"]);
            OrganisationId = TypeCast.ToInt(dr["OrganisationId"]);
            UsergroupId = TypeCast.ToInt(dr["UsergroupId"]);
            ReadWriteState = TypeCast.ToInt(dr["ReadWriteState"]);
            SharedWithOrganisationName = TypeCast.ToString(dr["SharedWithOrganisationName"]);
            UserGroupName = TypeCast.ToString(dr["UserGroupName"]);
            UserGroupIcon = TypeCast.ToString(dr["UserGroupIcon"]);
        }
    }
}
