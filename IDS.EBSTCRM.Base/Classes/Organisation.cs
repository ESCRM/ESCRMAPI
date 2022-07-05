using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Data;

namespace IDS.EBSTCRM.Base {
    /// <summary>
    /// Organisation class
    /// Defines an organisaion where
    ///  - Users
    ///  - SubOrganisations
    ///  - Fielddesigns
    ///  - Secondary Project Types
    ///  - Case numbers
    ///  - etc.
    ///  Are stored
    /// </summary>
    [Serializable()]
    public class Organisation : EventLogBase {
        #region Declarations

        private int id;
        private object parentId;
        private string name;
        private int children;
        private OrganisationType type;

        public object ContractSigned;

        #endregion

        #region Properties


        public bool AllowHourlyTimesheet { get; set; }
        public int Id {
            get { return id; }
            set { id = value; }
        }

        public object ParentId {
            get { return parentId; }
            set { parentId = value; }
        }

        public string Name {
            get { return name; }
            set { name = value; }
        }

        public int Children {
            get { return children; }
            set { children = value; }
        }

        public OrganisationType Type {
            get { return type; }
            set { type = value; }
        }

        public enum OrganisationType {
            EBST = 0,
            ConsultantHouse = 1,
            Sattelite = 2,
            County = 3
        }

        #region New Invoicing Propeties

        public string CVR { get; set; }
        public string Street { get; set; }
        public string ZipCode { get; set; }
        public string City { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Note { get; set; }
        public bool SendInvoice { get; set; }
        public object CRMContractSigned { get; set; }
        public object CRMContractCancelled { get; set; }
        public string Attn { get; set; }
        public string Ref { get; set; }
        public string ContactPerson { get; set; }
        public string ContactPersonEmail { get; set; }
        public string ContactPerson2 { get; set; }
        public string ContactPersonEmail2 { get; set; }
        public string InvoiceMethod { get; set; }
        public string EAN { get; set; }
        public string AltCVR { get; set; }
        public string AltName { get; set; }
        public string AltStreet { get; set; }
        public string AltZipCode { get; set; }
        public string AltCity { get; set; }
        public string AltPhone { get; set; }
        public string AltEmail { get; set; }
        public string AltEAN { get; set; }

        public int PNumber { get; set; }
        public string DPOUserId { get; set; }

        public bool EnableContactAgreement { get; set; } 

        #endregion

        #endregion

        #region Constructors

        public Organisation() {
            this.id = 0;
            this.parentId = null;
            this.name = "";
            this.children = 0;
            this.type = OrganisationType.ConsultantHouse;
        }

        public Organisation(ref SqlDataReader dr) {
            this.id = TypeCast.ToInt(dr["Id"]);
            this.parentId = TypeCast.ToIntOrNull(dr["ParentId"]);
            this.name = TypeCast.ToString(dr["Name"]);
            this.children = TypeCast.ToInt(dr["children"]);
            this.type = (OrganisationType)TypeCast.ToInt(dr["Type"]);
            this.ContractSigned = TypeCast.ToDateTimeOrNull(dr["ContractSigned"]);


            //New Invoice Properties
            this.CVR = TypeCast.ToString(dr["CVR"]);
            this.Street = TypeCast.ToString(dr["Street"]);
            this.ZipCode = TypeCast.ToString(dr["ZipCode"]);
            this.City = TypeCast.ToString(dr["City"]);
            this.Email = TypeCast.ToString(dr["Email"]);
            this.Phone = TypeCast.ToString(dr["Phone"]);
            this.Note = TypeCast.ToString(dr["Note"]);
            this.SendInvoice = TypeCast.ToBool(dr["SendInvoice"]);
            this.CRMContractSigned = TypeCast.ToDateTimeOrNull(dr["CRMContractSigned"]);
            this.CRMContractCancelled = TypeCast.ToDateTimeOrNull(dr["CRMContractCancelled"]);
            this.Attn = TypeCast.ToString(dr["Attn"]);
            this.Ref = TypeCast.ToString(dr["Ref"]);
            this.ContactPerson = TypeCast.ToString(dr["ContactPerson"]);
            this.ContactPersonEmail = TypeCast.ToString(dr["ContactPersonEmail"]);
            this.ContactPerson2 = TypeCast.ToString(dr["ContactPerson2"]);
            this.ContactPersonEmail2 = TypeCast.ToString(dr["ContactPersonEmail2"]);
            this.InvoiceMethod = TypeCast.ToString(dr["InvoiceMethod"]);
            this.EAN = TypeCast.ToString(dr["EAN"]);
            this.AltCVR = TypeCast.ToString(dr["AltCVR"]);
            this.AltName = TypeCast.ToString(dr["AltName"]);
            this.AltStreet = TypeCast.ToString(dr["AltStreet"]);
            this.AltZipCode = TypeCast.ToString(dr["AltZipCode"]);
            this.AltCity = TypeCast.ToString(dr["AltCity"]);
            this.AltPhone = TypeCast.ToString(dr["AltPhone"]);
            this.AltEmail = TypeCast.ToString(dr["AltEmail"]);
            this.AltEAN = TypeCast.ToString(dr["AltEAN"]);

            this.PNumber = TypeCast.ToInt(dr["PNumber"]);
            this.DPOUserId = TypeCast.ToString(dr["DPOUserId"]);

            this.EnableContactAgreement = TypeCast.ToBool(dr["EnableContactAgreement"]);
            this.AllowHourlyTimesheet = TypeCast.ToBool(dr["AllowHourlyTimesheet"]);
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

        public override VisualItems GetVisualItemsForEventLog(string Event) {
            VisualItems retval = new VisualItems();

            retval.Text = eventToText(Event) + " organisationen " + Name;
            retval.Icon = "images/listviewIcons/organisation.png";

            retval.JavaScript = (Event == "DELETE" ?
                                    "alert('Organisationen er blevet slettet');"
                                    :
                                    "top.frames['root'].frames['frameGlobalAdmin'].organisations_CreateOrganisation(" + Id + ");");



            return retval;
        }
        #endregion
    }
}
