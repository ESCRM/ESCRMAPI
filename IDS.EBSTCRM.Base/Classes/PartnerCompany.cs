using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;

namespace IDS.EBSTCRM.Base
{
    /// <summary>
    /// Deleted SAM Partner Company
    /// Inherits from PartnerCompany
    /// </summary>
    public class PartnerCompanyDeleted
    {
        public int CompanyId;
        public string CompanyName;
        public string CVR;
        public string DeletedByUser;
        public DateTime DateDeleted;

        public PartnerCompanyDeleted()
        {

        }

        public PartnerCompanyDeleted(ref SqlDataReader dr)
        {
            CompanyId = TypeCast.ToInt(dr["companyId"]);
            CompanyName = TypeCast.ToString(dr["companyName"]);
            CVR = TypeCast.ToString(dr["CVR"]);
            DeletedByUser = TypeCast.ToString(dr["DeletedByUser"]);
            DateDeleted = TypeCast.ToDateTime(dr["DeletedDate"]);
        }
    }

    /// <summary>
    /// SAM Partner Company
    /// </summary>
    [Serializable()]
    public class PartnerCompany : EventLogBase
    {
        public int CompanyId;
        public int OrganisationId;
        public string CompanyName;
        public string CVR;
        public string Address;
        public string Zipcode;
        public string City;
        public string County;
        public string Memo;
        public DateTime DateCreated;
        public string CreatedBy;
        public DateTime LastUpdated;
        public string LastUpdatedBy;
        public DateTime DeletedDate;
        public string DeletedBy;
        public bool IsPublicInstance;
        public string Web;

        public PartnerCompany()
        { 
        }

        public PartnerCompany(ref SqlDataReader dr)
        {
            CompanyId = TypeCast.ToInt(dr["CompanyId"]);
            OrganisationId = TypeCast.ToInt(dr["OrganisationId"]);
            CompanyName = TypeCast.ToString(dr["CompanyName"]);
            CVR = TypeCast.ToString(dr["CVR"]);
            Address = TypeCast.ToString(dr["Address"]);
            Zipcode = TypeCast.ToString(dr["Zipcode"]);
            City = TypeCast.ToString(dr["City"]);
            County = TypeCast.ToString(dr["County"]);
            Memo = TypeCast.ToString(dr["Memo"]);
            IsPublicInstance = TypeCast.ToBool(dr["IsPublicInstance"]);
            DateCreated = TypeCast.ToDateTime(dr["DateCreated"]);
            CreatedBy = TypeCast.ToString(dr["CreatedBy"]);
            LastUpdated = TypeCast.ToDateTime(dr["LastUpdated"]);
            LastUpdatedBy = TypeCast.ToString(dr["LastUpdatedBy"]);
            DeletedDate = TypeCast.ToDateTime(dr["DeletedDate"]);
            DeletedBy = TypeCast.ToString(dr["DeletedBy"]);
            Web = TypeCast.ToString(dr["Web"]);

        }
  
    }
}
