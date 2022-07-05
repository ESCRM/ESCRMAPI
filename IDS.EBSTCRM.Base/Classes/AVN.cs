using System;
using System.Collections.Generic;
using System.Text;

namespace IDS.EBSTCRM.Base {
    /// <summary>
    /// AVN Field Design
    /// This class is the blueprints for an AVN, containing default sharing and who designed the AVN
    /// </summary>
    public class AVN {
        public int Id { get; set; }
        public int TypeId { get; set; } //Not in DB
        public int OrganisationId { get; set; }
        public string Name { get; set; }
        public string CreatedBy { get; set; }
        public DateTime Created { get; set; }
        public string LastUpdatedBy { get; set; }
        public DateTime? LastUpdated { get; set; }
        public string DeletedBy { get; set; }
        public DateTime? Deleted { get; set; }
        public int? SMVContactId { get; set; }
        public int? SMVCompanyId { get; set; }
        public int SharedWith { get; set; }
        //public bool SaveToCompany { get; set; }

        public List<AVNFieldWithValue> Fields { get; set; }
        public List<AVNEntityShared> SharedWithList { get; set; }
        public List<DateTime> Reminders { get; set; }

        public AVN() {
            this.Fields = new List<AVNFieldWithValue>();

        }

        public AVN(ref System.Data.SqlClient.SqlDataReader dr, List<AVNField> fields) {
            Id = TypeCast.ToInt(dr["Id"]);
            OrganisationId = TypeCast.ToInt(dr["OrganisationId"]);
            Name = TypeCast.ToString(dr["Name"]);
            CreatedBy = TypeCast.ToString(dr["CreatedBy"]);
            Created = TypeCast.ToDateTime(dr["Created"]);
            LastUpdatedBy = TypeCast.ToString(dr["LastUpdatedBy"]);
            LastUpdated = TypeCast.ToDateTime(dr["LastUpdated"]);
            DeletedBy = TypeCast.ToString(dr["DeletedBy"]);
            Deleted = TypeCast.ToDateTime(dr["Deleted"]);
            SMVContactId = TypeCast.ToInt(dr["SMVContactId"]);
            SMVCompanyId = TypeCast.ToInt(dr["SMVCompanyId"]);
            SharedWith = TypeCast.ToInt(dr["SharedWith"]);
            //SaveToCompany = TypeCast.ToBool(dr["SaveToCompany"]);

            this.Fields = new List<AVNFieldWithValue>();
            foreach (AVNField f in fields) {
                this.Fields.Add(new AVNFieldWithValue(f, null));
            }

            for (int i = 0; i < dr.FieldCount; i++) {
                string name = dr.GetName(i);
                string datatype = dr.GetDataTypeName(i);
                if (name.IndexOf("_") > -1) {
                    int fieldId = TypeCast.ToInt(name.Split('_')[0]);
                    for (int ii = 0; ii < Fields.Count; ii++) {
                        if (Fields[ii].Id == fieldId) {
                            Fields[ii].Value = dr[i];
                            Fields[ii].DataType = datatype;
                            break;
                        }
                    }
                }
            }
        }

        private AVNFieldWithValue getField(List<AVNFieldWithValue> fields, int Id) {
            foreach (AVNFieldWithValue f in fields) {
                if (f.Id == Id)
                    return f;
            }
            return null;
        }
    }

    /// <summary>
    /// AVN Entity design
    /// Contains an entity of a Design with its Read/Write sharing and where its saved (Contact or Company)
    /// </summary>
    public class AVNEntity {
        public int Id { get; set; }
        public int AvnId { get; set; }
        public int EntityId { get; set; }
        public int TypeId { get; set; } //Not in DB
        public int OrganisationId { get; set; }
        public string Name { get; set; }
        public string CreatedBy { get; set; }
        public DateTime Created { get; set; }
        public string LastUpdatedBy { get; set; }
        public DateTime? LastUpdated { get; set; }
        public string DeletedBy { get; set; }
        public DateTime? Deleted { get; set; }
        public int? SMVContactId { get; set; }
        public int? SMVCompanyId { get; set; }
        public int SharedWith { get; set; }
        public bool SaveToCompany { get; set; }

        public string Description { get; set; }
        public string BackgroundColor { get; set; }
        public string AVNName { get; set; }
        public string Icon { get; set; }
        public bool IsAVNActive { get; set; }

        public string Category { get; set; }
        public string CreatedByUsername { get; set; }
        public string LastUpdatedByUsername { get; set; }
        public string SavedTo { get; set; }

        public List<AVNEntityShared> SharedWithList { get; set; }

        public AVNEntity(ref System.Data.SqlClient.SqlDataReader dr) {
            Id = TypeCast.ToInt(dr["Id"]);
            AvnId = TypeCast.ToInt(dr["AvnId"]);
            EntityId = TypeCast.ToInt(dr["EntityId"]);
            OrganisationId = TypeCast.ToInt(dr["OrganisationId"]);
            Name = TypeCast.ToString(dr["Name"]);
            CreatedBy = TypeCast.ToString(dr["CreatedBy"]);
            Created = TypeCast.ToDateTime(dr["Created"]);
            LastUpdatedBy = TypeCast.ToString(dr["LastUpdatedBy"]);
            //LastUpdated = TypeCast.ToDateTime2(dr["LastUpdated"]);

            //** Kalder en ny metode for denne opgave, ESCRM-196/195
            LastUpdated = TypeCast.ToDateTime2(dr["LastUpdated"]);
            
            DeletedBy = TypeCast.ToString(dr["DeletedBy"]);
            Deleted = TypeCast.ToDateTime(dr["Deleted"]);
            SMVContactId = TypeCast.ToInt(dr["SMVContactId"]);
            SMVCompanyId = TypeCast.ToInt(dr["SMVCompanyId"]);
            SharedWith = TypeCast.ToInt(dr["SharedWith"]);
            SaveToCompany = TypeCast.ToBool(dr["SaveToCompany"]);

            Description = TypeCast.ToString(dr["Description"]);
            BackgroundColor = TypeCast.ToString(dr["BackgroundColor"]);
            AVNName = TypeCast.ToString(dr["AVNName"]);
            Icon = TypeCast.ToString(dr["Icon"]);
            IsAVNActive = TypeCast.ToBool(dr["IsAVNActive"]);

            Category = TypeCast.ToString(dr["Category"]);
            CreatedByUsername = TypeCast.ToString(dr["CreatedByUsername"]);
            LastUpdatedByUsername = TypeCast.ToString(dr["LastUpdatedByUsername"]);
            SavedTo = TypeCast.ToString(dr["SavedTo"]);
        }
    }
}
