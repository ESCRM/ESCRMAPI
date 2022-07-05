using System;
using System.Collections.Generic;
using System.Text;

namespace IDS.EBSTCRM.Base
{
    /// <summary>
    /// AVN Entity sharing class
    /// </summary>
    public class AVNEntityShared
    {
        public int Id { get; set; }
        public int EntityId { get; set; }
        public int OrganisationId { get; set; }
        public int UsergroupId { get; set; }
        public int ReadWriteState { get; set; }
        public string SharedWithOrganisationName { get; set; }
        public string UserGroupName { get; set; }
        public string UserGroupIcon { get; set; }

        public AVNEntityShared()
        {

        }

        /// <summary>
        /// Contructs a new Sharing Level
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="organisationId"></param>
        /// <param name="usergroupId"></param>
        /// <param name="readWriteState"></param>
        public AVNEntityShared(int entityId, int organisationId, int usergroupId, int readWriteState)
        {
            EntityId = entityId;
            OrganisationId = organisationId;
            UsergroupId = usergroupId;
            ReadWriteState = readWriteState;
        }

        /// <summary>
        /// Contructs a new Sharing Level from the database
        /// </summary>
        /// <param name="dr"></param>
        public AVNEntityShared(ref System.Data.SqlClient.SqlDataReader dr)
        {
            Id = TypeCast.ToInt(dr["Id"]);
            EntityId = TypeCast.ToInt(dr["EntityId"]);
            OrganisationId = TypeCast.ToInt(dr["OrganisationId"]);
            UsergroupId = TypeCast.ToInt(dr["UsergroupId"]);
            ReadWriteState = TypeCast.ToInt(dr["ReadWriteState"]);
            SharedWithOrganisationName = TypeCast.ToString(dr["SharedWithOrganisationName"]);
            UserGroupName = TypeCast.ToString(dr["UserGroupName"]);
            UserGroupIcon = TypeCast.ToString(dr["UserGroupIcon"]);
        }
    }
}

