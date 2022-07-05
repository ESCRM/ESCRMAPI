using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Collections;

namespace IDS.EBSTCRM.Base {
    /// <summary>
    /// Deleted SMV/POT Contact
    /// Inherits Company
    /// </summary>
    public class CompanyDeleted {
        public int CompanyId;
        public string DeletedByUser;
        public DateTime CompanyDeletedDate;
        public int CompanyType;
        public List<TableColumnWithValue> Columns;

        public CompanyDeleted() {

        }

        public CompanyDeleted(ref SqlDataReader dr) {
            CompanyId = TypeCast.ToInt(dr["CompanyId"]);
            DeletedByUser = TypeCast.ToString(dr["DeletedByUser"]);
            CompanyDeletedDate = TypeCast.ToDateTime(dr["CompanyDeletedDate"]);
            CompanyType = TypeCast.ToInt(dr["CompanyType"]);

            this.Columns = new List<TableColumnWithValue>();

            for (int i = 0; i < dr.FieldCount; i++) {
                if (dr.GetName(i).Substring(0, 2) == "z_") {
                    //this.dynamicTblColumns.Add(dr.GetName(i), new TableColumnWithValue(dr.GetName(i), dr.GetDataTypeName(i), 0, dr[i]));
                    this.Columns.Add(new TableColumnWithValue(dr.GetName(i), dr.GetDataTypeName(i), 0, dr[i]));

                }
            }
        }
    }

    /// <summary>
    /// SMV/POT Company
    /// Base company class, containing default company data and Dynamic Data from
    /// an array of DynamicField classes
    /// </summary>
    public class Company : EventLogBase {
        #region Declarations

        private int id;
        private int organisationId;
        private string createdById;
        private object dateStamp;
        private int type;
        //private TableColumnWithValueCollection dynamicTblColumns;
        private List<TableColumnWithValue> dynamicTblColumns;

        public object DeletedDate;
        public string DeletedBy;

        public string LastUpdatedBy;
        public object LastUpdated;

        public string AbandonedBy;
        public object AbandonedDate;

        public int NNEId { get; set; }

        #endregion

        #region Properties

        public int Id {
            get { return id; }
            set { id = value; }
        }

        public int OrganisationId {
            get { return organisationId; }
            set { organisationId = value; }
        }

        public string CreatedById {
            get { return createdById; }
            set { createdById = value; }
        }

        public object DateStamp {
            get { return dateStamp; }
            set { dateStamp = value; }
        }

        public int Type {
            get { return type; }
            set { type = value; }
        }

        public List<TableColumnWithValue> DynamicTblColumns {
            get { return dynamicTblColumns; }
            set { dynamicTblColumns = value; }
        }

        //public TableColumnWithValueCollection DynamicTblColumns
        //{
        //    get { return dynamicTblColumns; }
        //    set { dynamicTblColumns = value; }
        //}

        #endregion

        #region Constructors

        public Company() {
            this.dynamicTblColumns = new List<TableColumnWithValue>();
        }

        /// <summary>
        /// Constructs an existing SMV/POT Contact from the database.
        /// </summary>
        /// <param name="dr">Datareader containing Contact Data</param>
        public Company(ref SqlDataReader dr) {
            this.id = TypeCast.ToInt(dr["CompanyId"]);
            this.organisationId = TypeCast.ToInt(dr["CompanyOrganisationId"]);
            this.createdById = TypeCast.ToString(dr["CompanyCreatedById"]);
            this.dateStamp = TypeCast.makeDateOrNothing(dr["CompanyDateStamp"]);
            this.type = TypeCast.ToInt(dr["CompanyType"]);
            this.DeletedBy = TypeCast.ToString(dr["companyDeletedBy"]);
            this.DeletedDate = TypeCast.ToDateTimeOrNull(dr["companyDeletedDate"]);

            this.LastUpdated = TypeCast.ToDateTimeOrNull(dr["companyLastUpdated"]);
            this.LastUpdatedBy = TypeCast.ToString(dr["companyLastUpdatedBy"]);

            this.AbandonedBy = TypeCast.ToString(dr["companyAbandonedBy"]);
            this.AbandonedDate = TypeCast.ToDateTimeOrNull(dr["companyAbandonedDate"]);

            this.NNEId = TypeCast.ToInt(dr["CompanyNNEId"]);

            this.dynamicTblColumns = new List<TableColumnWithValue>();

            //Loop thru all fields returned from the datareader
            for (int i = 0; i < dr.FieldCount; i++) {

                //Fieldnames starting with "z_" indicates a dynamic field
                if (dr.GetName(i).Substring(0, 2) == "z_") {
                    this.dynamicTblColumns.Add(new TableColumnWithValue(dr.GetName(i), dr.GetDataTypeName(i), 0, dr[i]));
                }
            }
        }

        #endregion



        #region EventLog

        /// <summary>
        /// OBSOLETE
        /// </summary>
        /// <param name="Event"></param>
        /// <returns></returns>
        public override VisualItems GetVisualItemsForEventLog(string Event) {
            VisualItems retval = new VisualItems();



            return retval;
        }

        #endregion

    }
}
