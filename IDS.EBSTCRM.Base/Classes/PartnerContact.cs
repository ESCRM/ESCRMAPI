using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;

namespace IDS.EBSTCRM.Base
{
    /// <summary>
    /// Deleted SAM Contact
    /// </summary>
    public class PartnerContactDeleted : PartnerCompanyDeleted
    {

        public string Firstname;
        public string Lastname;
        public int ContactId;

        public PartnerContactDeleted()
        {

        }

        public PartnerContactDeleted(ref SqlDataReader dr)
            : base(ref dr)
        {
            ContactId = TypeCast.ToInt(dr["ContactId"]);
            Firstname = TypeCast.ToString(dr["Firstname"]);
            Lastname = TypeCast.ToString(dr["Lastname"]);
        }
    }

    /// <summary>
    /// SAM Contact
    /// </summary>
    [Serializable()]
    public class PartnerContact : EventLogBase
    {
        public int ContactId;
        public int OrganisationId;
        public int ContactType;
        public string Firstname;
        public string Lastname;
        public string Email;
        public string Phone1;
        public string Phone2;
        public int CompanyId;
        public string CreatedBy;
        public DateTime DateCreated;
        public string LastUpdatedBy;
        public DateTime LastUpdated;
        public DateTime DeletedDate;
        public string DeletedBy;
        public string Nace;
        public string OwnDescription;

        public bool IsMentor;
        public bool IsCooporative;
        public bool IsRedirect;

        

        public PartnerContact()
        { 

        }


        public PartnerContact(ref SqlDataReader dr)
        {
            ContactId = TypeCast.ToInt(dr["ContactId"]);
            OrganisationId = TypeCast.ToInt(dr["OrganisationId"]);
            ContactType = TypeCast.ToInt(dr["ContactType"]);
            Firstname = TypeCast.ToString(dr["Firstname"]);
            Lastname = TypeCast.ToString(dr["Lastname"]);
            Email = TypeCast.ToString(dr["Email"]);
            Phone1 = TypeCast.ToString(dr["Phone1"]);
            Phone2 = TypeCast.ToString(dr["Phone2"]);
            Nace = TypeCast.ToString(dr["Nace"]);
            OwnDescription = TypeCast.ToString(dr["OwnDescription"]);

            CompanyId = TypeCast.ToInt(dr["CompanyId"]);
            CreatedBy = TypeCast.ToString(dr["CreatedBy"]);
            DateCreated = TypeCast.ToDateTime(dr["DateCreated"]);
            LastUpdatedBy = TypeCast.ToString(dr["LastUpdatedBy"]);
            LastUpdated = TypeCast.ToDateTime(dr["LastUpdated"]);
            DeletedDate = TypeCast.ToDateTime(dr["DeletedDate"]);
            DeletedBy = TypeCast.ToString(dr["DeletedBy"]);

            IsMentor = TypeCast.ToBool(dr["IsMentor"]);
            IsCooporative = TypeCast.ToBool(dr["IsCooporative"]);
            IsRedirect = TypeCast.ToBool(dr["IsRedirect"]);
        }


    }

    /// <summary>
    /// SAM Contact for listview item
    /// </summary>
    public class PartnerContactForList : PartnerContact
    {
        public string Companyname;
        public string CVR;
        public string Address;
        public string Zipcode;
        public string City;
        public string County;

		public bool Relation_IsMentor;
		public bool Relation_IsRedirect;
		public bool Relation_IsCooporative;
		public object Relation_DateStamp;
		public string Relation_CreatedBy;
		public string Relation_username;

        public int Relation_FinalcialPoolId;
        public string Relation_FinalcialPool;
        public decimal Relation_FinalcialPoolAmount;

        public int ContactPartnerRelationId;

        public PartnerContactForList()
        {

        }

        public PartnerContactForList(ref SqlDataReader dr)
            : base(ref dr)
        {

            Companyname = TypeCast.ToString(dr["Companyname"]);
            CVR = TypeCast.ToString(dr["CVR"]);
            Address = TypeCast.ToString(dr["Address"]);
            Zipcode = TypeCast.ToString(dr["Zipcode"]);
            City = TypeCast.ToString(dr["City"]);
            County = TypeCast.ToString(dr["County"]);

            Relation_IsMentor = TypeCast.ToBool(dr["Relation_IsMentor"]);
            Relation_IsRedirect = TypeCast.ToBool(dr["Relation_IsRedirect"]);
            Relation_IsCooporative = TypeCast.ToBool(dr["Relation_IsCooporative"]);
            Relation_DateStamp = TypeCast.ToDateTimeOrNull(dr["Relation_DateStamp"]);
            Relation_CreatedBy = TypeCast.ToString(dr["Relation_CreatedBy"]);
            Relation_username = TypeCast.ToString(dr["Relation_username"]);
            Relation_FinalcialPoolId = TypeCast.ToInt(dr["Relation_FinalcialPoolId"]);
            Relation_FinalcialPool = TypeCast.ToString(dr["Relation_FinalcialPool"]);
            Relation_FinalcialPoolAmount = TypeCast.ToDecimal(dr["Relation_FinalcialPoolAmount"]);

            ContactPartnerRelationId = TypeCast.ToInt(dr["ContactPartnerRelationId"]);

        }
    }
}
