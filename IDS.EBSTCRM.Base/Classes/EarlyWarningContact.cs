using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Data.SqlClient;

namespace IDS.EBSTCRM.Base
{
    /// <summary>
    /// Early Warning Contact
    /// Contains all Early Warning Contact Data
    /// And a relation to an Early Warning Company
    /// </summary>
    public class EarlyWarningContact
    {
        public int Id;
        public int OrganisationId;
        public int CompanyId;
        public string Firstname;
        public string Lastname;
        public string Phone;
        public string MobilePhone;
        public string Email;
        public string JobPosition;
        public string EWKnowledge;
        public object DateDeleted;
        public string DeletedBy;
        public DateTime DateCreated;
        public string CreatedBy;
        public object LastUpdated;
        public string LastUpdatedBy;

        public string Sex;

        public EarlyWarningContact()
        {

        }

        public EarlyWarningContact(HttpRequest Request)
        {
            Id = TypeCast.ToInt(Request.Form["Id"]);
            if (Id == 0) Id = TypeCast.ToInt(Request.Form["txtId"]);

            OrganisationId = TypeCast.ToInt(Request.Form["OrganisationId"]);
            
            CompanyId = TypeCast.ToInt(Request.Form["CompanyId"]);
            if (CompanyId == 0) CompanyId = TypeCast.ToInt(Request.Form["txtCompanyId"]);

            Firstname = TypeCast.ToString(Request.Form["Firstname"]);
            Lastname = TypeCast.ToString(Request.Form["Lastname"]);
            Phone = TypeCast.ToString(Request.Form["Phone"]);
            MobilePhone = TypeCast.ToString(Request.Form["MobilePhone"]);
            Email = TypeCast.ToString(Request.Form["Email"]);
            JobPosition = TypeCast.ToString(Request.Form["JobPosition"]);
            EWKnowledge = TypeCast.ToString(Request.Form["EWKnowledge"]);
            DateDeleted = TypeCast.ToDateTimeOrNull(Request.Form["DateDeleted"]);
            DeletedBy = TypeCast.ToString(Request.Form["DeletedBy"]);
            DateCreated = TypeCast.ToDateTime(Request.Form["DateCreated"]);
            CreatedBy = TypeCast.ToString(Request.Form["CreatedBy"]);
            LastUpdated = TypeCast.ToDateTimeOrNull(Request.Form["LastUpdated"]);
            LastUpdatedBy = TypeCast.ToString(Request.Form["LastUpdatedBy"]);

            Sex = TypeCast.ToString(Request.Form["Sex"]);

        }

        public EarlyWarningContact(HttpRequest Request, User U)
        {
            Id = TypeCast.ToInt(Request.Form["txtId"]);
            OrganisationId = U.OrganisationId;

            CompanyId = TypeCast.ToInt(Request.Form["txtCompanyId"]);

            Firstname = TypeCast.ToString(Request.Form["Firstname"]);
            Lastname = TypeCast.ToString(Request.Form["Lastname"]);
            Phone = TypeCast.ToString(Request.Form["Phone"]);
            MobilePhone = TypeCast.ToString(Request.Form["MobilePhone"]);
            Email = TypeCast.ToString(Request.Form["Email"]);
            JobPosition = TypeCast.ToString(Request.Form["JobPosition"]);
            EWKnowledge = TypeCast.ToString(Request.Form["EWKnowledge"]);

            Sex = TypeCast.ToString(Request.Form["Sex"]);

        }

        public EarlyWarningContact(ref SqlDataReader dr)
        {
            Id = TypeCast.ToInt(dr["Id"]);
            OrganisationId = TypeCast.ToInt(dr["OrganisationId"]);
            CompanyId = TypeCast.ToInt(dr["CompanyId"]);
            Firstname = TypeCast.ToString(dr["Firstname"]);
            Lastname = TypeCast.ToString(dr["Lastname"]);
            Phone = TypeCast.ToString(dr["Phone"]);
            MobilePhone = TypeCast.ToString(dr["MobilePhone"]);
            Email = TypeCast.ToString(dr["Email"]);
            JobPosition = TypeCast.ToString(dr["JobPosition"]);
            EWKnowledge = TypeCast.ToString(dr["EWKnowledge"]);
            DateDeleted = TypeCast.ToDateTimeOrNull(dr["DateDeleted"]);
            DeletedBy = TypeCast.ToString(dr["DeletedBy"]);
            DateCreated = TypeCast.ToDateTime(dr["DateCreated"]);
            CreatedBy = TypeCast.ToString(dr["CreatedBy"]);
            LastUpdated = TypeCast.ToDateTimeOrNull(dr["LastUpdated"]);
            LastUpdatedBy = TypeCast.ToString(dr["LastUpdatedBy"]);

            Sex = TypeCast.ToString(dr["Sex"]);

        }
    }
}
