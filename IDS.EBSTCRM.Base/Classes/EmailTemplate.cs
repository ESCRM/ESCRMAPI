using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Collections;


namespace IDS.EBSTCRM.Base
{
    /// <summary>
    /// Email Template
    /// OBSOLETE FROM CRM1.0
    /// </summary>
    public class EmailTemplate
    {
        public int Id;
        public int OrganisationId;
        public string Header;
        public string Icon;
        public string Description;

        public EmailTemplate(ref SqlDataReader dr)
        {
            this.Id = TypeCast.ToInt(dr["Id"]);
            this.OrganisationId = TypeCast.ToInt(dr["OrganisationId"]);
            this.Icon = TypeCast.ToString(dr["Icon"]);
            this.Header = TypeCast.ToString(dr["Header"]);
            this.Description = TypeCast.ToString(dr["Description"]);
        }
        
    }

}
