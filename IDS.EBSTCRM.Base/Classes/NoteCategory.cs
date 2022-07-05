using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;

namespace IDS.EBSTCRM.Base
{
    /// <summary>
    /// Note category, used to define a category for each note saved on a SMV/POT Contact person
    /// </summary>
    [Serializable()]
    public class NoteCategory : EventLogBase
    {
        public enum CategoryLevel
        {
            Primary = 0,
            Secondary = 1
        }

        public int Id;
        public int OrganisationId;
        public string CategoryName;
        public string CreatedBy;
        public string CreatedByName;
        public DateTime DateCreated;

        public CategoryLevel Level;
        public bool IsDefault;

        public NoteCategory()
        {

        }

        public NoteCategory(ref SqlDataReader dr)
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

            IsDefault = TypeCast.ToBool(dr["IsDefault"]);
            Level = (CategoryLevel)TypeCast.ToInt(dr["Level"]);
        }
    }
}
