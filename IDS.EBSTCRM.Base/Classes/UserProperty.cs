using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;

namespace IDS.EBSTCRM.Base
{
    /// <summary>
    /// UserProperties
    /// Used for specific GUI Operations to
    /// show special properties/settings for a given user
    /// </summary>
    [Serializable()]
    public class UserProperty
    {
        public string Property;
        public string Value;

        public UserProperty()
        {

        }

        public UserProperty(ref SqlDataReader dr)
        {
            Property = TypeCast.ToString(dr["Property"]).ToLower();
            Value = TypeCast.ToString(dr["Value"]);
        }

        public UserProperty(string property, string value)
        {
            Property = property.ToLower();
            Value = value;
        }
    }
}
