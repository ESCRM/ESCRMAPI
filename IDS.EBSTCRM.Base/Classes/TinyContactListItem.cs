using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;

namespace IDS.EBSTCRM.Base
{
    /// <summary>
    /// Tiny Contact, is a small SMV/POT contact
    /// for fast access of simple items, such as firstname, lastname etc.
    /// </summary>
    public class TinyContactListItem
    {
        public int ContactId;
        public string CompanyName;
        public string Firstname;
        public string Lastname;
        public int Type;

        public TinyContactListItem()
        {

        }

        public TinyContactListItem(ref SqlDataReader dr)
        {
            ContactId = TypeCast.ToInt(dr[0]);
            CompanyName = TypeCast.ToString(dr[2]);
            Firstname = TypeCast.ToString(dr[3]);
            Lastname = TypeCast.ToString(dr[4]);
            Type = TypeCast.ToInt(dr[1]);
        }
    }
}
