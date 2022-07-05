using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Collections;
using System.Data;

namespace IDS.EBSTCRM.Base {
    /// <summary>
    /// AVN item, for administrative usage (design, edit and create)
    /// </summary>
    public class AdminAVN {
        public int Id { get; set; }
        public int OrganisationId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string BackgroundColor { get; set; }
        public string DisabledBackgroundColor { get; set; }
        public string Icon { get; set; }
        public string Category { get; set; }
        public int Status { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? LastUpdated { get; set; }
        public string CreatedBy { get; set; }
        public string LastUpdatedBy { get; set; }

        public string Shared { get; set; }
        public string SharedIds { get; set; }

        public string CreatedByUser { get; set; }
        public string LastUpdatedByUser { get; set; }

        public bool SaveToCompany { get; set; }

        public List<AVNEntityDefaultShared> DefaultSharedWith { get; set; }

        public string OnLoad { get; set; }
        public string OnNewLoad { get; set; }
        public string OnExistingLoad { get; set; }
        public string OnBeforeSave { get; set; }
        public string OnAfterCopy { get; set; }

        public bool VisibleInReportGenerator { get; set; }

        public bool AllowChangingExpiration { get; set; }

        //** Nyt felt geografi, ESCRM-11/183
        public int Geografi { get; set; }

        public AdminAVN() {
            DefaultSharedWith = new List<AVNEntityDefaultShared>();
        }

        /// <summary>
        /// Contructs a new AVN item using an SQL DataReader
        /// </summary>
        /// <param name="dr">Data reader to populate class from</param>
        public AdminAVN(ref SqlDataReader dr) {
            Id = TypeCast.ToInt(dr["Id"]);
            OrganisationId = TypeCast.ToInt(dr["OrganisationId"]);
            Name = TypeCast.ToString(dr["Name"]);
            Description = TypeCast.ToString(dr["Description"]);
            BackgroundColor = TypeCast.ToString(dr["BackgroundColor"]);
            DisabledBackgroundColor = TypeCast.ToString(dr["DisabledBackgroundColor"]);
            Icon = TypeCast.ToString(dr["Icon"]);
            Category = TypeCast.ToString(dr["Category"]);
            Status = TypeCast.ToInt(dr["Status"]);
            DateCreated = TypeCast.ToDateTime(dr["DateCreated"]);
            LastUpdated = TypeCast.ToDateTimeLoose(dr["LastUpdated"]);
            CreatedBy = TypeCast.ToString(dr["CreatedBy"]);
            LastUpdatedBy = TypeCast.ToString(dr["LastUpdatedBy"]);

            Shared = TypeCast.ToString(dr["Shared"]);
            SharedIds = TypeCast.ToString(dr["SharedIds"]);
            CreatedByUser = TypeCast.ToString(dr["CreatedByUser"]);
            LastUpdatedByUser = TypeCast.ToString(dr["LastUpdatedByUser"]);

            SaveToCompany = TypeCast.ToBool(dr["SaveToCompany"]);

            OnLoad = TypeCast.ToString(dr["OnLoad"]);
            OnNewLoad = TypeCast.ToString(dr["OnNewLoad"]);
            OnExistingLoad = TypeCast.ToString(dr["OnExistingLoad"]);
            OnBeforeSave = TypeCast.ToString(dr["OnBeforeSave"]);
            OnAfterCopy = TypeCast.ToString(dr["OnAfterCopy"]);

            VisibleInReportGenerator = TypeCast.ToBool(dr["VisibleInReportGenerator"]);

            if (ColumnExists(dr, "AllowChangingExpiration")) {
                AllowChangingExpiration = TypeCast.ToBool(dr["AllowChangingExpiration"]);
            }

            //** Læs geografi, ESCRM-11/183
            Geografi = TypeCast.ToInt(dr["Geografi"]);

            DefaultSharedWith = new List<AVNEntityDefaultShared>();
        }

        /// <summary>
        /// Checking if column exist in the datareader
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public bool ColumnExists(IDataReader reader, string columnName) {
            for (int i = 0; i < reader.FieldCount; i++) {
                if (reader.GetName(i).Equals(columnName, StringComparison.InvariantCultureIgnoreCase)) {
                    return true;
                }
            }
            return false;
        }
    }
}
