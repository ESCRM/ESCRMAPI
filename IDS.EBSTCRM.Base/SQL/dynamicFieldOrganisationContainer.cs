using System;
using System.Collections.Generic;
using System.Text;

namespace IDS.EBSTCRM.Base
{
    /// <summary>
    /// Dynamic fields for a specific Organisation
    /// </summary>
    public class dynamicFieldOrganisationContainer
    {
        public List<DynamicField> AllFields; // = new List<DynamicField>();
        public List<DynamicField> ContactFields; // = new List<DynamicField>();
        public List<DynamicField> CompanyFields; // = new List<DynamicField>();

        public Organisation FieldsOrganiation;

        public dynamicFieldOrganisationContainer(Organisation O, ref SQLDB sql)
        {
            FieldsOrganiation = O;

            AllFields = sql.dynamicFields_getFieldsForOrganisationFromSQL(FieldsOrganiation);
            ContactFields = sql.dynamicFields_getFieldsForOrganisationFromSQL(FieldsOrganiation, "contacts");
            CompanyFields = sql.dynamicFields_getFieldsForOrganisationFromSQL(FieldsOrganiation, "companies");
        }

        public void reload(ref SQLDB sql)
        {
            AllFields = sql.dynamicFields_getFieldsForOrganisationFromSQL(FieldsOrganiation);
            ContactFields = sql.dynamicFields_getFieldsForOrganisationFromSQL(FieldsOrganiation, "contacts");
            CompanyFields = sql.dynamicFields_getFieldsForOrganisationFromSQL(FieldsOrganiation, "companies");
        }

        public List<DynamicField> dynamicFields_getFieldsForOrganisation(string Type)
        {
            switch (Type)
            {
                case "":
                    return AllFields;

                case "contacts":
                    return ContactFields;

                case "companies":
                    return CompanyFields;

            }

            return null;
        }
    }
}
