using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IDS.EBSTCRM.Base {
    /// <summary>
    /// Dynamic functions are created with this class, such as
    ///  - SMV/POT Companies
    ///  - SMV/POT Contacts
    ///  -   -"-   Deleted items etc.
    /// </summary>
    public static class SQLFunctionsManager {
        public static void RefactorFunctions(User U, ref SQLDB sql, Organisation O) {
            //REFACTOR FUNCTIONS HERE
            rf_ContactsAllOrganisations(ref sql, O);
            rf_ContactList(ref sql, O);

            ////foreach(Organisation O2 in sql.Organisations_getOrganisations(O.Id))
            ////{
            ////    RefactorFunctions(U, ref sql, O2);
            ////}
        }

        private static void rf_ContactsAllOrganisations(ref SQLDB sql, Organisation O) {
            var contactColumnsWithComma = string.Join(", \n", sql.ContactFields(O.Id, true).Select(s => s.ColumnName).ToList());
            string ContactSQL = "" +
                        "CREATE FUNCTION [dbo].[ContactsAllOrganisations_" + O.Id + "]()\n" +
                        "returns table\n" +
                        "as\n" +
                        "return\n" +
                        "\n" +
                        "     select\n" +
                        "         " + contactColumnsWithComma + "\n" +
                        "     from\n" +
                        "         Contact\n" +
                        "    where\n" +
                        "        ContactInStagingBy is null and \n" +
                        "        contactDeletedDate is null\n";

            //(RE)CREATE THE FUNCTION!!!
            try {
                sql.dynamicFields_dropFunction("ContactsAllOrganisations_" + O.Id);
            } catch { }
            sql.dynamicFields_updateStoredProcedure(ContactSQL);
        }

        //REFACTOR CONTACTS LIST THRUOUT THE SYSTEM
        private static void rf_ContactList(ref SQLDB sql, Organisation O) {

            /*Tuple<string[],List<string>> contactJoins = rf_contactJoins(ref sql, O);
            string[] tables = contactJoins.Item1;
            List<string> contacttables = contactJoins.Item2;*/

            var contactColumnsWithComma = string.Join(", \n", sql.ContactFields(O.Id).Select(s => s.ColumnName).ToList());
            var companyColumnsWithComma = string.Join(", \n", sql.CompanyFields(O.Id).Select(s => s.ColumnName).ToList());

            //contactColumnsWithComma = contactColumnsWithComma.Replace("[SparseColumns]", "[Contact].[SparseColumns] as ContactSparseColumns");
            //companyColumnsWithComma = companyColumnsWithComma.Replace("[SparseColumns]", "[Company].[SparseColumns] as SparseColumnsSparseColumns");
            contactColumnsWithComma = contactColumnsWithComma.Replace("[SparseColumns],", "");
            companyColumnsWithComma = companyColumnsWithComma.Replace("[SparseColumns],", "");

            string ContactListSQL = "" +
                        "CREATE FUNCTION [dbo].[ContactsAndCompaniesList_" + O.Id + "]()\n" +
                        "returns table\n" +
                        "as\n" +
                        "return\n" +
                        "\n" +
                        "    Select\n" +
                        "         " + contactColumnsWithComma + "\n" +
                        "         , " + companyColumnsWithComma + "\n" +
                        "    from\n" +
                        "        Contact,\n" +
                        "        Company\n" +
                        "    Where\n" +
                        "        companyDeletedDate is null and contactDeletedDate is null and\n" +
                        "        Contact.CompanyOwnerId=Company.CompanyId and \n" +
                        "        Contact.ContactInStagingBy is null and \n" +
                        "        Company.CompanyInStagingBy is null\n";

            string ContactSQL = "" +
                        "CREATE FUNCTION [dbo].[Contacts_" + O.Id + "]()\n" +
                        "returns table\n" +
                        "as\n" +
                        "return\n" +
                        "\n" +
                        "    Select\n" +
                        "         " + contactColumnsWithComma + "\n" +
                        "    from\n" +
                        "         Contact\n" +
                        "    Where\n" +
                        "        ContactInStagingBy is null and \n" +
                        "        contactDeletedDate is null\n";


            string CompanySQL = "" +
                        "CREATE FUNCTION [dbo].[Companies_" + O.Id + "]()\n" +
                        "returns table\n" +
                        "as\n" +
                        "return\n" +
                        "\n" +
                        "    Select\n" +
                        "         " + companyColumnsWithComma + "\n" +
                        "    from\n" +
                        "         Company\n" +
                        "    Where\n" +
                        "        CompanyInStagingBy is null and \n" +
                        "        companyDeletedDate is null\n";


            //STAGING ITEMS
            string ContactStagingSQL = "" +
                        "CREATE FUNCTION [dbo].[ContactsStaging_" + O.Id + "]()\n" +
                        "returns table\n" +
                        "as\n" +
                        "return\n" +
                        "\n" +
                        "    Select\n" +
                        "         " + contactColumnsWithComma + "\n" +
                        "    from\n" +
                        "         Contact\n" +
                        "    Where\n" +
                        "        ContactInStagingBy is not null and \n" +
                        "        contactDeletedDate is null\n";


            string CompanyStagingSQL = "" +
                        "CREATE FUNCTION [dbo].[CompaniesStaging_" + O.Id + "]()\n" +
                        "returns table\n" +
                        "as\n" +
                        "return\n" +
                        "\n" +
                        "    Select\n" +
                        "         " + companyColumnsWithComma + "\n" +
                        "    from\n" +
                        "         Company\n" +
                        "    Where\n" +
                        "        CompanyInStagingBy is not null and \n" +
                        "        companyDeletedDate is null\n";

            //DELETED ITEMS 
            string CompanyDeletedSQL = "" +
                        "CREATE FUNCTION [dbo].[CompaniesDeleted_" + O.Id + "]()\n" +
                        "returns table\n" +
                        "as\n" +
                        "return\n" +
                        "\n" +
                        "    Select\n" +
                        "         " + companyColumnsWithComma + "\n" +
                        "    from\n" +
                        "         Company\n" +
                        "    Where\n" +
                        "        --companyOrganisationId=" + O.Id + " and \n" +
                        "        CompanyInStagingBy is null and \n" +
                        "        companyDeletedDate is not null\n";

            string ContactListDeletedSQL = "" +
                        "CREATE FUNCTION [dbo].[ContactsAndCompaniesDeletedList_" + O.Id + "]()\n" +
                        "returns table\n" +
                        "as\n" +
                        "return\n" +
                        "\n" +
                        "    Select\n" +
                        "         " + contactColumnsWithComma + "\n" +
                        "         , " + companyColumnsWithComma + "\n" +
                        "    from\n" +
                        "        Contact,\n" +
                        "        Company\n" +
                        "    where\n" +
                        "        --contactOrganisationId = " + O.Id + " and \n" +
                        "        ContactDeletedDate is not null and\n" +
                        "        Contact.ContactInStagingBy is null and \n" +
                        "        Company.CompanyInStagingBy is null and \n" +
                        "        Contact.CompanyOwnerId=Company.CompanyId\n";


            string ContactListAnySQL = "" +
                        "CREATE FUNCTION [dbo].[ContactsAndCompaniesAnyList_" + O.Id + "]()\n" +
                        "returns table\n" +
                        "as\n" +
                        "return\n" +
                        "\n" +
                        "    Select\n" +
                        "         " + contactColumnsWithComma + "\n" +
                        "         , " + companyColumnsWithComma + "\n" +
                        "    from\n" +
                        "        Contact,\n" +
                        "        Company\n" +
                        "    Where\n" +
                        "        Contact.ContactInStagingBy is null and \n" +
                        "        Contact.CompanyOwnerId=Company.CompanyId\n";

            //EXECUTE SQL!!!
            //(RE)CREATE THE FUNCTION!!!
            sql.dynamicFields_dropFunction("ContactsAndCompaniesList_" + O.Id);
            sql.dynamicFields_updateStoredProcedure(ContactListSQL);

            //(RE)CREATE THE FUNCTION!!!
            sql.dynamicFields_dropFunction("Contacts_" + O.Id);
            sql.dynamicFields_updateStoredProcedure(ContactSQL);

            //(RE)CREATE THE FUNCTION!!!
            sql.dynamicFields_dropFunction("Companies_" + O.Id);
            sql.dynamicFields_updateStoredProcedure(CompanySQL);

            //STAGING
            //(RE)CREATE THE FUNCTION!!!
            sql.dynamicFields_dropFunction("ContactsStaging_" + O.Id);
            sql.dynamicFields_updateStoredProcedure(ContactStagingSQL);

            //(RE)CREATE THE FUNCTION!!!
            sql.dynamicFields_dropFunction("CompaniesStaging_" + O.Id);
            sql.dynamicFields_updateStoredProcedure(CompanyStagingSQL);


            // DELETED ITEMS
            //(RE)CREATE THE FUNCTION!!!
            sql.dynamicFields_dropFunction("CompaniesDeleted_" + O.Id);
            sql.dynamicFields_updateStoredProcedure(CompanyDeletedSQL);

            //(RE)CREATE THE FUNCTION!!!
            sql.dynamicFields_dropFunction("ContactsAndCompaniesDeletedList_" + O.Id);
            sql.dynamicFields_updateStoredProcedure(ContactListDeletedSQL);

            //(RE)CREATE THE FUNCTION!!!
            sql.dynamicFields_dropFunction("ContactsAndCompaniesAnyList_" + O.Id);
            sql.dynamicFields_updateStoredProcedure(ContactListAnySQL);
        }

        /*private static Tuple<string[], List<string>> rf_contactJoins(ref SQLDB sql,  Organisation O)
        {

            string[] tables = new string[] {"",""};
            var contactTables = new List<string>();

            do
            {
                if (O != null)
                {
                    tables[0] = "        LEFT OUTER JOIN z_contacts_" + O.Id +
                                        " on z_contacts" + (TypeCast.ToInt(O.ParentId) > 0 ? "_" + TypeCast.ToInt(O.ParentId) : "") +
                                        ".contactId" + (TypeCast.ToInt(O.ParentId) > 0 ? "_" + TypeCast.ToInt(O.ParentId) : "") +
                                        "=z_contacts_" + O.Id + ".ContactParentId_" + O.Id +
                                        (TypeCast.ToInt(O.ParentId) > 0 ? "\n" : "\n") + 
                                        
                                        tables[0];


                    tables[1] = "        LEFT OUTER JOIN z_companies_" + O.Id +
                                        " on z_companies" + (TypeCast.ToInt(O.ParentId) > 0 ? "_" + TypeCast.ToInt(O.ParentId) : "") +
                                        ".companyId" + (TypeCast.ToInt(O.ParentId) > 0 ? "_" + TypeCast.ToInt(O.ParentId) : "") +
                                        "=z_companies_" + O.Id + ".CompanyParentId_" + O.Id +
                                        (TypeCast.ToInt(O.ParentId) > 0 ? "\n" : "\n") + 
                                        
                                        tables[1];


                    contactTables.Add("[z_contacts_" + O.Id + "].*");
                    O = sql.Organisations_getOrganisation(TypeCast.ToInt(O.ParentId));
                }
                else
                    break;

            } while (O!=null);

            return new Tuple<string[], List<string>>(tables, contactTables);
        }*/
    }
}