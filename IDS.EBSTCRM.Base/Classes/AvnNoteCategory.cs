using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;


namespace IDS.EBSTCRM.Base
{
    /// <summary>
    /// Note category, used to define a category for each avn note
    /// </summary>
    [Serializable()]
    public class AvnNoteCategory : EventLogBase
    {
        public int Id;
        public int OrganisationId;
        public string CategoryName;
        public string CreatedBy;
        public string CreatedByName;
        public DateTime DateCreated;

        public AvnNoteCategory()
        {

        }

        public AvnNoteCategory(ref SqlDataReader dr)
        {
            Populate(ref dr);
        }

        private void Populate(ref SqlDataReader dr)
        {
            Id = TypeCast.ToInt(dr["Id"]);
            OrganisationId = TypeCast.ToInt(dr["OrganisationId"]);
            CategoryName = TypeCast.ToString(dr["CategoryName"]);
            CreatedBy = TypeCast.ToString(dr["CreatedBy"]);
            CreatedByName = TypeCast.ToString(dr["CreatedByName"]);
            DateCreated = TypeCast.ToDateTime(dr["DateCreated"]);
        }
    }
}
