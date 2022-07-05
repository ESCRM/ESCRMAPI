using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Collections;



namespace IDS.EBSTCRM.Base
{
    /// <summary>
    /// Deleted SMV/POT Contact
    /// Inherits Contact class
    /// </summary>
    public class ContactDeleted
    {
        public int ContactId;
        public int CompanyId;
        public string DeletedByUser;
        public DateTime ContactDeletedDate;
        public int ContactType;

        public List<TableColumnWithValue> Columns;

        public ContactDeleted()
        {

        }

        public ContactDeleted(ref SqlDataReader dr)
        {
            ContactId = TypeCast.ToInt(dr["ContactId"]);
            CompanyId = TypeCast.ToInt(dr["CompanyId"]);
            DeletedByUser = TypeCast.ToString(dr["DeletedByUser"]);
            ContactDeletedDate = TypeCast.ToDateTime(dr["ContactDeletedDate"]);
            ContactType = TypeCast.ToInt(dr["ContactType"]);

            this.Columns = new List<TableColumnWithValue>();

            for (int i = 0; i < dr.FieldCount; i++)
            {
                if (dr.GetName(i).Substring(0, 2) == "z_")
                {
                    //this.dynamicTblColumns.Add(dr.GetName(i), new TableColumnWithValue(dr.GetName(i), dr.GetDataTypeName(i), 0, dr[i]));
                    this.Columns.Add(new TableColumnWithValue(dr.GetName(i), dr.GetDataTypeName(i), 0, dr[i]));

                }
            }
        }
    }

    /// <summary>
    /// SMV/POT Contact
    /// Base contact class, containing default contact data and Dynamic Data from
    /// an array of DynamicField classes
    /// </summary>
    public class Contact : EventLogBase
    {

        #region Declarations

        private int id;
        private int companyId;
        public int OrganisationId;
        private string createdById;
        private object dateStamp;
        //private TableColumnWithValueCollection dynamicTblColumns;
        private List<TableColumnWithValue> dynamicTblColumns;
        private int type;

        public object DeletedDate;
        public string DeletedBy;

        public string LastUpdatedBy;
        public object LastUpdated;

        public string AbandonedBy;
        public object AbandonedDate;

        #endregion

        #region Properties

        public int Type
        {
            get { return type; }
            set { type = value; }
        }

        public int Id
        {
            get { return id; }
            set { id = value; }
        }

        public int CompanyId
        {
            get { return companyId; }
            set { companyId = value; }
        }

        public string CreatedById
        {
            get { return createdById; }
            set { createdById = value; }
        }

        public object DateStamp
        {
            get { return dateStamp; }
            set { dateStamp = value; }
        }


        public List<TableColumnWithValue> DynamicTblColumns
        {
            get { return dynamicTblColumns; }
            set { dynamicTblColumns = value; }
        }
        /*public TableColumnWithValueCollection DynamicTblColumns
        {
            get { return dynamicTblColumns; }
            set { dynamicTblColumns = value; }
        }*/

        
        #endregion

        #region Constructors

        /// <summary>
        /// Create an empty SMV/POT Contact
        /// </summary>
        public Contact()
        {
            this.dynamicTblColumns = new List<TableColumnWithValue>();
        }

        /// <summary>
        /// Constructs an existing SMV/POT Contact from the database.
        /// </summary>
        /// <param name="dr">Datareader containing Contact Data</param>
        public Contact(ref SqlDataReader dr)
        {
            this.id = TypeCast.ToInt(dr["ContactId"]);
            this.companyId = TypeCast.ToInt(dr["CompanyOwnerId"]);
            this.createdById = TypeCast.ToString(dr["ContactCreatedById"]);
            this.dateStamp = TypeCast.makeDateOrNothing(dr["ContactDateStamp"]);
            this.type = TypeCast.ToInt(dr["ContactType"]);
            this.OrganisationId = TypeCast.ToInt(dr["ContactOrganisationId"]);
            this.DeletedBy = TypeCast.ToString(dr["contactDeletedBy"]);
            this.DeletedDate = TypeCast.ToDateTimeOrNull(dr["contactDeletedDate"]);

            this.LastUpdated = TypeCast.ToDateTimeOrNull(dr["contactLastUpdated"]);
            this.LastUpdatedBy = TypeCast.ToString(dr["contactLastUpdatedBy"]);

            this.AbandonedBy = TypeCast.ToString(dr["contactAbandonedBy"]);
            this.AbandonedDate = TypeCast.ToDateTimeOrNull(dr["contactAbandonedDate"]);

            this.dynamicTblColumns = new List<TableColumnWithValue>();
            
            //Loop thru all fields returned from the datareader
            for (int i = 0; i < dr.FieldCount; i++)
            {
                //Fieldnames starting with "z_" indicates a dynamic field
                if(dr.GetName(i).Substring(0,2)=="z_")
                    this.dynamicTblColumns.Add(new TableColumnWithValue(dr.GetName(i), dr.GetDataTypeName(i), 0, dr[i])); 
            }
        }
        

        /// <summary>
        /// OBSOLETE
        /// </summary>
        /// <param name="Event"></param>
        /// <returns></returns>
        public override VisualItems GetVisualItemsForEventLog(string Event)
        {
            VisualItems retval = new VisualItems();

            retval.Text = eventToText(Event) + " kontaktpersonen ";
            retval.Icon = "images/listviewIcons/" + (type == 0 ? "smv" : "pot") + "Contact.png";

            retval.JavaScript = (Event == "DELETE" ?
                                    "alert('Kontaktpersonen er blevet slettet');"
                                    :
                                    "top.frames['root'].frames['frameUser'].editContact(" + Id + "," + companyId + ",'" + (type == 0 ? "SMV" : "POT") + "',null);");



            return retval;
        }

        #endregion
    }

    /// <summary>
    /// Contact for listview in SAM Contact view (contains additional data, for relation type)
    /// </summary>
    public class ContactAndPartnerList : Contact
    {
        #region Declarations / Properties

        public bool Relation_IsMentor;
        public bool Relation_IsRedirect;
        public bool Relation_IsCooporative;
        public object Relation_DateStamp;
        public string Relation_CreatedBy;
        public string Relation_username;
        public string Relation_FinalcialPool;
        public decimal Relation_FinalcialPoolAmount;

        public int ContactPartnerRelationId;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs an empty Contact Item for SAM listview
        /// </summary>
        public ContactAndPartnerList()
            : base()
        {

        }

        /// <summary>
        /// Constructs a new contact listview item for SAM listview
        /// </summary>
        /// <param name="dr">Data Reader containg Contact Data</param>
        public ContactAndPartnerList(ref SqlDataReader dr)
            : base(ref dr)
        {
            try
            {
                Relation_IsMentor = TypeCast.ToBool(dr["Relation_IsMentor"]);
                Relation_IsRedirect = TypeCast.ToBool(dr["Relation_IsRedirect"]);
                Relation_IsCooporative = TypeCast.ToBool(dr["Relation_IsCooporative"]);
                Relation_DateStamp = TypeCast.ToDateTimeOrNull(dr["Relation_DateStamp"]);
                Relation_CreatedBy = TypeCast.ToString(dr["Relation_CreatedBy"]);
                Relation_FinalcialPool = TypeCast.ToString(dr["Relation_FinalcialPool"]);
                Relation_username = TypeCast.ToString(dr["Relation_username"]);
                Relation_FinalcialPoolAmount = TypeCast.ToDecimal(dr["Relation_FinalcialPoolAmount"]);
                ContactPartnerRelationId = TypeCast.ToInt(dr["ContactPartnerRelationId"]);
            }
            catch
            {
            }
        }

        #endregion

    }

    /// <summary>
    /// Contact for Export to Competence House via http://[crm]/Exports/Brugerevaluering.aspx?[args] 
    /// </summary>
    public class ContactSendToEvaluationList : Contact
    {
        #region Declarations / Properties

        public DateTime EvaluationDate;
        public object ExportedDate;
        public string EvaluatedByUserId;
        public string EvaluatedByUsername;
        public bool HasRedirect;
        public bool HasValidRedirect;

        #endregion

        #region Constructors

        public ContactSendToEvaluationList()
            : base()
        {

        }

        public ContactSendToEvaluationList(ref SqlDataReader dr)
            : base(ref dr)
        {
            try
            {
                EvaluationDate = TypeCast.ToDateTime(dr["EvaluationDate"]);
                ExportedDate = TypeCast.ToDateTimeOrNull(dr["ExportedDate"]);
                EvaluatedByUserId = TypeCast.ToString(dr["EvaluatedByUserId"]);
                EvaluatedByUsername = TypeCast.ToString(dr["EvaluatedByUsername"]);
                HasRedirect = TypeCast.ToInt(dr["HasRedirect"])>0;
                HasValidRedirect = TypeCast.ToInt(dr["HasValidRedirect"]) > 0;
            }
            catch
            {
            }
        }

        #endregion

    }

    /// <summary>
    /// Class representing a recent contact
    /// </summary>
    public class ContactRecent : Contact
    {
        public string ContactLastUpdatedReason { get; set; }

        public ContactRecent()
            :base()
        {

        }

        public ContactRecent(ref SqlDataReader dr) : base(ref dr)
        {
            ContactLastUpdatedReason = TypeCast.ToString(dr["ContactLastUpdatedReason"]);
        }


    }

}
