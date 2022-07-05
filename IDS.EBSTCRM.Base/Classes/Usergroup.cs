using System;
using System.Collections.Generic;
using System.Text;

namespace IDS.EBSTCRM.Base
{
    /// <summary>
    /// User Groups
    /// </summary>
    public class Usergroup
    {
        public int Id { get; set; }
        public int OrganisationId { get; set; }
        public string OrganisationName { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
        
        public DateTime Created { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedByUserName { get; set; }

        public DateTime? LastUpdated { get; set; }
        public string LastUpdatedBy { get; set; }
        public string LastUpdatedByUserName { get; set; }

        public int SharedWith { get; set; }
        public string SharedWithText { get; set; }
        public List<int> SharedWithOrganisations { get; set; }

        public string SharedWithIds { get; set; }

        public bool Internal { get; set; }

        /// <summary>
        /// Create a new Empty Usergroup
        /// </summary>
        public Usergroup()
        {
            SharedWithOrganisations = new List<int>();
        }


        /// <summary>
        /// Create an exising UserGroup from a SQL Data Reader
        /// </summary>
        /// <param name="dr">SQL Data Reader</param>
        public Usergroup(ref System.Data.SqlClient.SqlDataReader dr)
        {
            Id = TypeCast.ToInt(dr["Id"]);
            OrganisationId = TypeCast.ToInt(dr["OrganisationId"]);
            OrganisationName = TypeCast.ToString(dr["OrganisationName"]);

            Name = TypeCast.ToString(dr["Name"]);
            Description = TypeCast.ToString(dr["Description"]);
            Icon = TypeCast.ToString(dr["Icon"]);

            Created = TypeCast.ToDateTime(dr["DateCreated"]);
            CreatedBy = TypeCast.ToString(dr["CreatedBy"]);
            CreatedByUserName = TypeCast.ToString(dr["CreatedByUserName"]);


            object od = TypeCast.ToDateTimeOrNull(dr["LastUpdated"]);
            if (od != null)
                LastUpdated = (DateTime)od;

            LastUpdatedBy = TypeCast.ToString(dr["LastUpdatedBy"]);
            LastUpdatedByUserName = TypeCast.ToString(dr["LastUpdatedByUserName"]);

            SharedWith = TypeCast.ToInt(dr["SharedWith"]);
            SharedWithText = TypeCast.ToString(dr["Shared"]);

            SharedWithOrganisations = new List<int>();
            SharedWithIds = TypeCast.ToString(dr["SharedIds"]);

            string[] sh = SharedWithIds.Split(',');
            SharedWithIds = "";

            foreach(string s in sh)
            {
                int o = TypeCast.ToInt(s.Trim());
                if (o > 0)
                {
                    SharedWithOrganisations.Add(o);

                    SharedWithIds += (SharedWithIds == "" ? "" : ",") + o;
                }
            }

            Internal = TypeCast.ToBool(dr["Internal"]);
        }
    }

    /// <summary>
    /// Usergroup with bound user and editing rights
    /// </summary>
    public class UserGroupBoundToUser : Usergroup
    {
        public bool IsUserMember { get; set; }
        public bool Editable { get; set; }

        public UserGroupBoundToUser()
            : base()
        {

        }

        public UserGroupBoundToUser(ref System.Data.SqlClient.SqlDataReader dr)
            : base(ref dr)
        {
            IsUserMember = TypeCast.ToBool(dr["IsUserMember"]);
            Editable = TypeCast.ToBool(dr["Editable"]);
        }
    }
}

