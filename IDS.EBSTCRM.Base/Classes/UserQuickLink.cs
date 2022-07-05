using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;

namespace IDS.EBSTCRM.Base
{
    /// <summary>
    /// GUI Quicklinks stored on a user
    /// </summary>
    [Serializable()]
    public class UserQuickLink
    {
        public int Id;
		public int OrganisationId;
		public string UserId;
		public string Name;
		public string Icon16;
		public string Icon32;
		public string URL;
        public int SortOrderInt;

        public UserQuickLink()
        {
        }

        public UserQuickLink(ref SqlDataReader dr)
        {
            Id = TypeCast.ToInt(dr["id"]);
            OrganisationId = TypeCast.ToInt(dr["OrganisationId"]);
            UserId = TypeCast.ToString(dr["UserId"]);
            Name = TypeCast.ToString(dr["Name"]);
            Icon16 = TypeCast.ToString(dr["Icon16"]);
            Icon32 = TypeCast.ToString(dr["Icon32"]);
            URL = TypeCast.ToString(dr["URL"]);
            SortOrderInt = TypeCast.ToInt(dr["SortOrderInt"]);
        }
    }
}
