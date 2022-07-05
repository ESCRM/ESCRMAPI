using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;

namespace IDS.EBSTCRM.Base
{
    /// <summary>
    /// Address for for driving registrations
    /// Used to store user defined addresses for quick access, when creating or modifying Driving Registrations
    /// </summary>
    public class AddressBookItem
    {
        public int Id;
        public string UserId;
        public int OrganisationId;
        public string Address;
        public string Zipcode;
        public string City;
        public string Country;

        public AddressBookItem()
        {

        }

        /// <summary>
        /// Constructors a new Address using a DataReader
        /// </summary>
        /// <param name="dr"></param>
        public AddressBookItem(ref SqlDataReader dr)
        {
            Id = TypeCast.ToInt(dr["id"]);
            UserId = TypeCast.ToString(dr["UserId"]);
            OrganisationId = TypeCast.ToInt(dr["OrganisationId"]);
            Address = TypeCast.ToString(dr["Address"]);
            Zipcode = TypeCast.ToString(dr["Zipcode"]);
            City = TypeCast.ToString(dr["City"]);
            Country = TypeCast.ToString(dr["Country"]);
        }
    }
}
