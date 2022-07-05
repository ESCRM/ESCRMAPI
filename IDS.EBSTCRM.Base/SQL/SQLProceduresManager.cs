using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace IDS.EBSTCRM.Base {
    /// <summary>
    /// Table Column Item for dynamic field (for generating Stored Procedures)
    /// </summary>
    public class SQLColumnItem {
        public string Column = "";
        public int ListIndex = 0;
        public string SortingName = "";
        public SQLColumnItem() {

        }

        public SQLColumnItem(string column, int listIndex) {
            Column = ",\n        [" + column + "]";
            SortingName = column;
            ListIndex = listIndex;
        }
    }

    /// <summary>
    /// All AVN Procedures are generated from this class
    /// </summary>
    public static class AVNProceduresManager {
        /// <summary>
        /// Refactor all stored procedures for AVN
        /// </summary>
        /// <param name="U"></param>
        /// <param name="sql"></param>
        /// <param name="AVNId"></param>
        public static void RefactorStoredProcedures(User U, ref SQLDB sql, int AvnId) {
            List<TableColumn> tableFields = sql.dynamicFields_getTableColumns("z_avn_" + AvnId);

            AdminAVN avn = sql.AdminAVN_GetAVN(U, AvnId);

            rf_CreateUpdateAVN(U, ref sql, AvnId, tableFields);
            rf_GetAVN(U, ref sql, AvnId, tableFields);

            rf_GetAVNsFromReportGenerator(U, ref sql, AvnId, tableFields);
            rf_CreateReportGeneratorStatisticsForAllAVNS(U.OrganisationId, ref sql, true);
            rf_CreateReportGeneratorStatisticsForAllAVNS(U.OrganisationId, ref sql, false);
            rf_CreateStatisticsAVN(U, ref sql, AvnId, tableFields, avn.SaveToCompany);
        }

        public static void RefactorReportGeneratorStatistics(int OrganisaiontId, ref SQLDB sql) {
            rf_CreateReportGeneratorStatisticsForAllAVNS(OrganisaiontId, ref sql, true);
            rf_CreateReportGeneratorStatisticsForAllAVNS(OrganisaiontId, ref sql, false);

        }

        private static void rf_CreateStatisticsAVN(User U, ref SQLDB sql, int AvnId, List<TableColumn> tableFields, bool CompanyAVN) {
            List<AVNField> afs = sql.AVN_GetFields(AvnId);
            List<TableColumn> fields = new List<TableColumn>();

            string sqlFields = "CREATE PROCEDURE z_avn_GetStatistics_" + AvnId + "\r\n" +
                                "\t@UserId uniqueidentifier,\r\n" +
                                "\t@OrganisationId int\r\n" +
                                "AS\r\n" +
                                "SELECT\r\n";

            if (CompanyAVN)
                sqlFields += "\tdbo.[GetCompanyCVR](AVNEntities.SMVCompanyId) as [CVR-nummer],\r\n" +
                            "\tdbo.[GetCompanyName](AVNEntities.SMVCompanyId) as [Virksomhedsnavn]\r\n";
            else {
                sqlFields += "\t	dbo.[GetCompanyCVR]((select companyOwnerId from contact where contactId = AVNEntities.SMVContactId)) as [CVR-nummer],\r\n" +
                "\tdbo.[GetCompanyName]((select companyOwnerId from contact where contactId = AVNEntities.SMVContactId)) as [Virksomhedsnavn],\r\n" +
                "\tdbo.[GetContactName](AVNEntities.SMVContactId) as [Kontaktperson]\r\n";
            }
            afs = afs.OrderBy(o => o.ListIndex).ThenBy(o => o.DatabaseColumn).ToList();

            foreach (AVNField af in afs) {
                if (af.Statistics != "" && af.Statistics != null) {
                    foreach (TableColumn tc in tableFields) {
                        if (tc.Name.StartsWith(af.Id + "_")) {
                            sqlFields += ",\r\n\t[" + tc.Name + "] as [" + af.DatabaseColumn + "]";
                        }
                    }
                }
            }

            sqlFields += "\t,[dbo].[getExtendedUserInfo](AVNEntities.CreatedBy) as [Oprettet af]\r\n";
            sqlFields += "\t,AVNEntities.Created as [Oprettet d.]\r\n";
            sqlFields += "\t,[dbo].[getExtendedUserInfo](AVNEntities.LastUpdatedBy) as [Sidst opdateret af]\r\n";
            sqlFields += "\t,AVNEntities.LastUpdated as [Sidst opdateret d.]\r\n";
            sqlFields += "\t,AVNEntities.Name as [Navn]\r\n";

            sqlFields += "\t,AVNEntities.AvnId\r\n";
            sqlFields += "\t,AVNEntities.EntityId\r\n";

            if (CompanyAVN)
                sqlFields += "\t,AVNEntities.SMVCompanyId\r\n";
            else
                sqlFields += "\t,AVNEntities.SMVContactId\r\n";

            sqlFields += "\r\nFROM\r\n" +
                        "\tz_avn_" + AvnId + ",\r\n" +
                        "\tAVNEntities\r\n" +
                    "WHERE\r\n" +
                    "\tAVNEntities.AvnId=" + AvnId + " and\r\n" +
                    "\tAVNEntities.EntityId = z_avn_" + AvnId + ".Id and\r\n" +
                    "\tAVNEntities.Deleted is null and\r\n" +

                    "\t    (\r\n" +
                    "\t		(\r\n" +
                    "\t			select \r\n" +
                    "\t				COUNT(*) \r\n" +
                    "\t			from \r\n" +
                    "\t				AVNEntityShared \r\n" +
                    "\t			where \r\n" +
                    "\t				AVNEntityShared.AvnId = AVNEntities.AvnId and \r\n" +
                    "\t				AVNEntityShared.EntityId = AVNEntities.EntityId and \r\n" +
                    "\t				(\r\n" +
                    "\t                    \r\n" +
                    "\t					AVNEntityShared.OrganisationId = @organisationId\r\n" +
                    "\t					OR\r\n" +
                    "\t					@UserId In (select UserId from UserGroupUsers where UserGroupId = AVNEntityShared.UsergroupId)\r\n" +
                    "\t				)\r\n" +
                    "\t		) > 0 \r\n" +
                    "\t		or\r\n" +
                    "\t			AVNEntities.OrganisationId = @organisationId\r\n" +
                    "\t	)";


            sql.dynamicFields_dropStoredProcedure("z_avn_GetStatistics_" + AvnId);
            sql.dynamicFields_updateStoredProcedure(sqlFields);

        }

        private static void rf_CreateUpdateAVN(User U, ref SQLDB sql, int AvnId, List<TableColumn> tableFields) {
            string txtSQL = "" +
                            "CREATE PROCEDURE [dbo].[z_avn_UpdateAVN_" + AvnId + "]\n" +
                            "\t@Id int = null,\n" +
                            "\t@OrganisationId int,\n" +
                            "\t@Name nvarchar(255),\n" +
                            "\t@UserId uniqueidentifier,\n" +
                            "\t@SMVContactId int = null,\n" +
                            "\t@SMVCompanyId int = null,\n" +
                            "\t@SharedWith int = 0";


            string update = "";
            string insert = "";
            string args = "";

            foreach (TableColumn t in tableFields) {
                if (t.Name.IndexOf("_") > -1) {
                    string pName = t.Name.Replace(" ", "_").Replace("[", "_").Replace("]", "_").Replace("(", "_").Replace(")", "_").Replace("-", "_").Replace(".", "_").Replace(":", "_").Replace(";", "_").Replace("/", "_").Replace("\\", "_").Replace(",", "_");
                    if (t.DataType == "varchar" || t.DataType == "nvarchar")
                        args += ",\n\t@" + pName + " " + t.DataType + "(" + (t.Length < 0 ? "MAX" : t.Length.ToString()) + ") = null";
                    else
                        args += ",\n\t@" + pName + " " + t.DataType + " = null";

                    update += ",\n\t\t[" + t.Name + "] = @" + pName;
                    insert += ",\n\t\t\t\t@" + pName;
                }
            }

            txtSQL += args + "\n" +
                "AS\n" +
                "\n" +
                "\tUPDATE z_avn_" + AvnId + " SET \n" +
                "\t\t[LastUpdatedBy] = @UserId,\n" +
                "\t\t[Name] = @Name,\n" +
                "\t\t[LastUpdated] = GetDate(),\n" +
                "\t\t[SMVContactId] = @SMVContactId,\n" +
                "\t\t[SMVCompanyId] = @SMVCompanyId,\n" +
                "\t\t[SharedWith] = @SharedWith" +
                update + "\n" +
                "\tWHERE\n" +
                "\t\tId=@Id\n" +
                "\n" +
                "\tIF @@ROWCOUNT = 0 BEGIN\n" +
                "\t\tINSERT INTO z_avn_" + AvnId + "\n" +
                "\t\t\tSELECT \n" +
                "\t\t\t\t@OrganisationId,\n" +
                "\t\t\t\t@Name,\n" +
                "\t\t\t\t@UserId,\n" +
                "\t\t\t\tGetDate(),\n" +

                "\t\t\t\tnull,\n" +
                "\t\t\t\tnull,\n" +
                "\t\t\t\tnull,\n" +
                "\t\t\t\tnull,\n" +

                "\t\t\t\t@SMVContactId,\n" +
                "\t\t\t\t@SMVCompanyId,\n" +

                "\t\t\t\t@SharedWith" +
                insert + "\n" +
                "\n" +
                "\t\t\t\tSET @ID = @@IDENTITY\n" +
                "\tEND\n" +
                "\n" +

                "\tUPDATE AVNEntities SET \n" +
                "\t\t[LastUpdatedBy] = @UserId,\n" +
                "\t\t[Name] = @Name,\n" +
                "\t\t[LastUpdated] = GetDate(),\n" +
                "\t\t[SMVContactId] = @SMVContactId,\n" +
                "\t\t[SMVCompanyId] = @SMVCompanyId,\n" +
                "\t\t[SharedWith] = @SharedWith\n" +
                "\tWHERE\n" +
                "\t\tEntityId=@Id and AvnId=" + AvnId + "\n" +
                "\n" +
                "\tIF @@ROWCOUNT = 0 BEGIN\n" +
                "\t\tINSERT INTO AVNEntities\n" +
                "\t\t\tSELECT \n" +
                "\t\t\t\t" + AvnId + ",\n" +
                "\t\t\t\t@Id,\n" +
                "\t\t\t\t@OrganisationId,\n" +
                "\t\t\t\t@Name,\n" +
                "\t\t\t\t@UserId,\n" +
                "\t\t\t\tGetDate(),\n" +
                "\t\t\t\tnull,\n" +
                "\t\t\t\tnull,\n" +
                "\t\t\t\tnull,\n" +
                "\t\t\t\tnull,\n" +
                "\t\t\t\t@SMVContactId,\n" +
                "\t\t\t\t@SMVCompanyId,\n" +
                "\t\t\t\t@SharedWith\n" +
                "\tEND\n" +

                "\n" +
                "DELETE FROM AVNEntityShared WHERE EntityId=@Id and avnId=" + AvnId + "\n" +
                "\n" +

                "\n" +
                "DELETE FROM AVNEntityReminders WHERE AVNEntityId=@Id and avnTypeId=" + AvnId + " AND userId=@UserId\n" +
                "\n" +

                "SELECT @Id as ID\n";


            //EXECUTE SQL!!!
            if (txtSQL != "" && txtSQL != null) {
                //(RE)CREATE THE PROCEDURE!!!
                sql.dynamicFields_dropStoredProcedure("z_avn_UpdateAVN_" + AvnId);
                sql.dynamicFields_updateStoredProcedure(txtSQL);
            }
        }

        private static void rf_GetAVN(User U, ref SQLDB sql, int AvnId, List<TableColumn> tableFields) {
            string txtSQL = "" +
                            "CREATE PROCEDURE [dbo].[z_avn_GetAVN_" + AvnId + "]\n" +
                            "\t@Id int\n" +
                            "AS\n" +
                            "SELECT * FROM z_avn_" + AvnId + " WHERE id=@id";

            //EXECUTE SQL!!!
            if (txtSQL != "" && txtSQL != null) {
                //(RE)CREATE THE PROCEDURE!!!
                sql.dynamicFields_dropStoredProcedure("z_avn_GetAVN_" + AvnId);
                sql.dynamicFields_updateStoredProcedure(txtSQL);
            }
        }

        private static void rf_DeleteAVN(User U, ref SQLDB sql, int AvnId, List<TableColumn> tableFields) {

        }

        private static void rf_GetAVNsFromContact(User U, ref SQLDB sql, int AvnId, List<TableColumn> tableFields) {

        }

        private static void rf_GetAVNsFromCompany(User U, ref SQLDB sql, int AvnId, List<TableColumn> tableFields) {

        }

        private static void rf_GetAVNsFromContactOrCompany(User U, ref SQLDB sql, int AvnId, List<TableColumn> tableFields) {

        }

        private static void rf_GetAVNsFromReportGenerator(User U, ref SQLDB sql, int AvnId, List<TableColumn> tableFields) {
            string txtSQL = "" +
                            "CREATE FUNCTION [dbo].[AvnGetAVNs_" + AvnId + "](@organisationId int, @userId uniqueidentifier)\n" +
                            "returns table\n" +
                            "AS\n" +
                            "\t\n" +
                            "\t\treturn\n" +
                            "\t\t\t		select \n" +
                            "\t\t\t			'" + AvnId + ".' + convert(varchar,AVNEntities.EntityId) as GlobalId,\n" +
                            "\t\t\t			z_avn_" + AvnId + ".*,\n" +
                            "\t\t\t			AVN.Description as AVN_Description,\n" +
                            "\t\t\t			AVN.Status as AVN_Status,\n" +
                            "\t\t\t			AVN.Category as AVN_Category,\n" +
                            "\t\t\t			AVN.Name as AVN_Name,\n" +
                            "\t\t\t			AVN.DateCreated as AVN_DateCreated,\n" +
                            "\t\t\t			AVN.LastUpdated as AVN_LastUpdated,\n" +
                            "\t\t\t			AVN.CreatedBy as AVN_CreatedBy,\n" +
                            "\t\t\t			AVN.LastUpdatedBy as AVN_LastUpdatedBy,\n" +
                            "\t\t\t			(case when (select count(*)		from flag where flag.objecttype=30 and flag.[type]=2 and flag.objectidint = [AVNEntities].Id and flag.AVNId = [AVNEntities].AvnId) > 0 then convert(bit,1) else convert(bit,0) end) AS NoExpiry,\n" +
                            "\t\t\t			(select top 1 isnull([date],'') from flag where flag.objecttype=30 and flag.[type]=3 and flag.objectidint = [AVNEntities].Id and flag.AVNId = [AVNEntities].AvnId) AS Anonymize,\n" +
                            "\t\t\t			(select top 1 isnull([date],'') from flag where flag.objecttype=30 and flag.[type]=6 and flag.objectidint = [AVNEntities].Id and flag.AVNId = [AVNEntities].AvnId) AS DeleteOn\n" +
                            "\t\t\t		from \n" +
                            "\t\t\t			z_avn_" + AvnId + ",\n" +
                            "\t\t\t			AVN,\n" +
                            "\t\t\t			AVNEntities\n" +
                            "\t\t\t		where\n" +
                            "\t\t\t			AVNEntities.AVNId = AVN.Id and\n" +
                            "\t\t\t			AVNEntities.EntityId=z_avn_" + AvnId + ".Id and\n" +
                            "\t\t\t			AVN.Id=" + AvnId + " and\n" +
                            "\t\t\t			AVNEntities.deleted is null and\n" +
                            "\t\t\t			(\n" +
                            "\t\t\t				(select COUNT(*) from AVNEntityShared where AVNEntityShared.AvnId = " + AvnId + " and AVNEntityShared.EntityId = z_avn_" + AvnId + ".Id and AVNEntityShared.OrganisationId = @organisationId) > 0 or\n" +
                            "\t\t\t				(select COUNT(*) from AVNEntityShared where AVNEntityShared.AvnId = " + AvnId + " and AVNEntityShared.EntityId = z_avn_" + AvnId + ".Id and AVNEntityShared.userGroupId in (select UserGroupId from userGroupUsers where UserId=@UserId) ) > 0 or\n" +
                            "\t\t\t				z_avn_" + AvnId + ".OrganisationId = @organisationId\n" +
                            "\t\t\t			)\n";


            //EXECUTE SQL!!!
            if (txtSQL != "" && txtSQL != null) {
                //(RE)CREATE THE PROCEDURE!!!
                sql.dynamicFields_dropFunction("AvnGetAVNs_" + AvnId);
                sql.dynamicFields_updateStoredProcedure(txtSQL);
            }
        }

        private static void rf_CreateReportGeneratorStatisticsForAllAVNS(int OrganisationId, ref SQLDB sql, bool ForCompany) {
            string s = "";

            foreach (AdminAVN a in sql.AdminAVN_GetAVNs(OrganisationId)) {
                s += (s == "" ? "" : "\n\nUNION ALL\n\n") + "select\n" +
                        "\tId, \n" +
                        "\tOrganisationId,\n" +
                        "\tName,\n" +
                        "\tCreatedBy,\n" +
                        "\tCreated,\n" +
                        "\tLastUpdatedBy,\n" +
                        "\tLastUpdated,\n" +
                        "\tSMVCompanyId,\n" +
                        "\tSMVContactId,\n" +
                        "\tSharedWith,\n" +
                        "\tAVN_Description,\n" +
                        "\tAVN_Category,\n" +
                        "\tAVN_Name,\n" +
                        "\tAVN_Status,\n" +
                        "\tAVN_DateCreated,\n" +
                        "\tAVN_LastUpdated,\n" +
                        "\tAVN_CreatedBy,\n" +
                        "\tAVN_LastUpdatedBy\n" +
                    "from\n" +
                    "\tdbo.AvnGetAVNs_" + a.Id + "(@organisationId, @UserId)\n" +
                    "where\n" +
                    "\t" + (ForCompany ? "SMVCompanyId" : "SMVContactId") + " is not null\n\n";
            }

            if (s != "") {
                s = "CREATE FUNCTION AvnGetAVNsStatisticalData" + (ForCompany ? "Companies" : "Contacts") + "_" + OrganisationId + "(@organisationId int, @UserId uniqueidentifier)\n" +
                    "returns table\n" +
                    "AS\n" +
                    "return\n" +
                    "(\n" + s + "\n)";

                sql.dynamicFields_dropFunction("AvnGetAVNsStatisticalData" + (ForCompany ? "Companies" : "Contacts") + "_" + OrganisationId);
                sql.dynamicFields_updateStoredProcedure(s);
            }
        }
    }

    /// <summary>
    /// All Dynamic Stored Procedures are generated from this class
    /// </summary>
    public static class SQLProceduresManager {
        //static List<dynamicFieldOrganisationContainer> oFields = null;

        public static void RefactorStoredProcedures(User U, ref SQLDB sql, Organisation O) //,  List<dynamicFieldOrganisationContainer> OrgFields)
        {
            if (O.Id == 178) {
                System.Diagnostics.Debug.Write("Something is wrong here...");
            }

            //'
            rf_ContactsGetContactsFromCompanyAllOrganisationFields(ref sql, O);
            rf_ContactsGetContactAllOrganisationFields(ref sql, O);
            rf_GetContactsWithSameEmail(ref sql, O);
            rf_GetContactsWithSameCPR(ref sql, O);
            rf_GetCompaniesWithSameCVR(ref sql, O);
            rf_GetCompaniesWithSamePNR(ref sql, O);
            rf_ContactsTransferredOut(ref sql, O);
            rf_ContactsTransferred(ref sql, O);
            rf_ContactsRecent(ref sql, O);
            rf_CompanyGetCVR(ref sql, O);
            rf_CompanyGetNameFromEmail(ref sql, O);
            rf_ContactGetName(ref sql, O);

            //Userevaluation stuff
            rf_Contacts_Get_Contacts_SentToEvaluation_AllColumns_EntireYear(ref sql, O);
            rf_Contacts_Get_Contacts_SentToLocalEvaluation_AllColumns_EntireYear(ref sql, O);

            rf_Contacts_ExtractToUserFinalcialPoolEvaluation(ref sql, O);

            //REFACTOR STORED PROCEDURES HERE
            rf_ContactsQuickSearch2(ref sql, O); // Replaced with new Search routine
            rf_ContactsQuickSearch(ref sql, O);

            rf_ContactsGetContact(ref sql, O);
            rf_ContactsGetContactFromStaging(ref sql, O);

            rf_CompanyGetCompany(ref sql, O);
            rf_CompanyGetCompanyFromStaging(ref sql, O);

            rf_ContactsGetIncompleteContacts(ref sql, O);

            rf_ContactsGetRecentContacts(ref sql, O);

            rf_CompaniesLocateDoubles(ref sql, O);

            rf_ContactsGetContacts_FromCompany(ref sql, O);
            rf_ContactsGetContacts_FromPartner_AllColumns(ref sql, O);

            rf_ContactsGetContacts_FromMailGroup(ref sql, O);

            rf_MailGroups_ExportContactEmails(ref sql, O);

            rf_Filearchive_getFilesByFolderId(ref sql, O);

            rf_Filearchive_getFilesByOrgIdAndFolderType(ref sql, O);

            rf_Filearchive_getFilesByUserAndFolderType(ref sql, O);

            rf_Filearchive_Quicksearch(ref sql, O);

            rf_ContactsGetContacts_FromPartner(ref sql, O);

            rf_ContactsGetContact_ForPartner(ref sql, O);

            rf_GetSingleFromContactsAndCompanies(ref sql, O);

            rf_Exchange_findContacts(ref sql, O);

            rf_Exchange_getContactEmailFromId(ref sql, O);

            rf_Contacts_isSentToEvaluation(ref sql, O);
            rf_Contacts_isSentToLocalEvaluation(ref sql, O);

            rf_Contacts_ExtractToUserEvaluation(ref sql, O);
            rf_Contacts_ExtractToUserLocalEvaluation(ref sql, O);

            rf_CompanyOrContactExists(ref sql, O);

            rf_CompanyGetName(ref sql, O);

            rf_z_Exchange_GetMailRules(ref sql, O);
            rf_z_Exchange_GetMailRule(ref sql, O);

            //DELETED ITEMS
            rf_Companies_GetDeletedCompanies(ref sql, O);
            rf_Contacts_GetDeletedContacts(ref sql, O);
            rf_Contacts_GetDeletedNotes(ref sql, O);


            ////foreach (Organisation O2 in sql.Organisations_getOrganisations(O.Id))
            ////{
            ////    RefactorStoredProcedures(U, ref sql, O2); //, oFields);
            ////}
        }

        //Private generic List To String
        public static string SQLColumnItemsToString(List<SQLColumnItem> List) {
            string items = "";
            foreach (SQLColumnItem s in List) {
                if (s.SortingName != "" && s.SortingName != null)
                    items += s.Column;
            }

            return items;
        }

        //Get correct fields
        public static List<DynamicField> dynamicFields_getFieldsForOrganisation(Organisation O, ref SQLDB sql, bool ForListview) {
            return dynamicFields_getFieldsForOrganisation(O, "", ref sql, ForListview);
        }
        public static List<DynamicField> dynamicFields_getFieldsForOrganisation(Organisation O, string Type, ref SQLDB sql, bool ForListview) {
            if (ForListview) {
                List<DynamicField> fields = sql.dynamicFields_getFieldsForOrganisationFromSQL(O, Type);
                for (int i = fields.Count - 1; i >= 0; i--) {
                    if (fields[i].NoInherit(O.Id) && fields[i].BaseOrganisationId != fields[i].OrganisationId)
                        fields.RemoveAt(i);
                }
                return fields;
            } else
                return sql.dynamicFields_getFieldsForOrganisationFromSQL(O, Type);
        }

        //REFACTOR THE CONTACTS TRANSFERRED
        public static void rf_ContactsTransferred(ref SQLDB sql, Organisation O) {

            List<DynamicField> Fields = dynamicFields_getFieldsForOrganisation(O, "contacts", ref sql, true);
            List<SQLColumnItem> colItems = new List<SQLColumnItem>();

            foreach (DynamicField f in Fields) {
                if (TypeCast.ToInt(f.ShowInListview) > 0 && f.OrganisationId == O.Id) {
                    string col = "";
                    if (f.DataSource.IndexOf('_') > 0) {
                        col = f.DataSource + "_" + f.DataSource.Split('_')[2];
                    } else {
                        col = f.DatabaseTable + "_" + f.DatabaseColumn + "_" + f.BaseOrganisationId;
                    }

                    //Add the columns desired in a sorted manor!
                    bool inserted = false;
                    int index = f.ListviewIndex;

                    if (colItems.Count == 0) {
                        colItems.Add(new SQLColumnItem(col, f.ListviewIndex));
                    } else {
                        for (int ii = 0; ii < colItems.Count; ii++) {
                            int cpIndex = colItems[ii].ListIndex;
                            if (inserted == false && index < cpIndex) {
                                inserted = true;
                                colItems.Insert(ii > colItems.Count ? colItems.Count : ii, new SQLColumnItem(col, f.ListviewIndex));
                            }
                        }

                        if (inserted == false)
                            colItems.Add(new SQLColumnItem(col, f.ListviewIndex));
                    }

                }
            }

            string txtSQL = "" +
                            "CREATE PROCEDURE [dbo].[z_Contacts_Transferred_" + O.Id + "]\n" +
                            "   @organisationId int,\n" +
                            "   @userId uniqueidentifier\n" +
                            "AS\n" +
                            "    SET ARITHABORT ON\n\n" +
                            "   SELECT top 100 \n" +
                            "        Id,\n" +
                            "        cts.ContactId,\n" +
                            "        CompanyOwnerId,\n" +
                            "        ContactOrganisationId,\n" +
                            "        ContactCreatedById,\n" +
                            "        ContactDateStamp,\n" +
                            "        ContactType,\n" +
                            "        CompanyDeletedDate,\n" +
                            "        CompanyDeletedBy,\n" +
                            "        ContactDeletedDate,\n" +
                            "        ContactDeletedBy,\n" +
                            "        CompanyLastUpdated,\n" +
                            "        CompanyLastUpdatedBy, CompanyAbandonedDate, CompanyAbandonedBy,\n" +
                            "        ContactLastUpdated,\n" +
                            "        ContactLastUpdatedBy, ContactAbandonedDate, ContactAbandonedBy,\n" +
                            "\n" +
                            "       SenderUserId,\n" +
                            "       [dbo].[getUserInfo](SenderUserId) as SenderUserName,\n" +
                            "       SenderOrganisationId,\n" +
                            "       [dbo].[getOrganisationInfo](SenderOrganisationId) as SenderOrganisationName,\n" +

                            "       ReceiverUserId,\n" +
                            "       [dbo].[getUserInfo](ReceiverUserId) as ReceiverUserName,\n" +
                            "       ReceiverOrganisationId,\n" +
                            "       [dbo].[getOrganisationInfo](ReceiverOrganisationId) as ReceiverOrganisationName,\n" +

                            "       AcceptedByUserId,\n" +
                            "       [dbo].[getUserInfo](AcceptedByUserId) as AcceptedByUserName,\n" +
                            "       AcceptedDate,\n" +
                            "       AcceptedReason,\n" +

                            "       RejectedByUserId,\n" +
                            "       [dbo].[getUserInfo](RejectedByUserId) as RejectedByUserName,\n" +
                            "       RejectedDate,\n" +
                            "       RejectedReason,\n" +

                            "       TransferDate,\n" +
                            "       TransferReason,\n" +
                            "       ForwardedToId\n" +
                            "\n" +
                            SQLColumnItemsToString(colItems) +
                            "   FROM\n" +
                            "       [dbo].[ContactsAndCompaniesList_" + O.Id + "]() as cts,\n" +
                            "       ContactTransfers\n" +
                            "   WHERE\n" +
                            "ContactTransfers.ContactId = cts.ContactId and receiverOrganisationId = @organisationId and (receiverUserId=@UserId or receiverUserId is null) AND\n" +
                            "(acceptedByUserId is null or datediff(d, acceptedDate, getdate())<=30) AND\n" +
                            "ContactTransfers.ForwardedToId is null AND\n" +
                            "ContactTransfers.RejectedDate is null\n" +
                            "order by \n" +
                            "TransferDate";


            //EXECUTE SQL!!!
            if (txtSQL != "" && txtSQL != null) {
                //(RE)CREATE THE PROCEDURE!!!
                sql.dynamicFields_dropStoredProcedure("z_Contacts_Transferred_" + O.Id);
                sql.dynamicFields_updateStoredProcedure(txtSQL);
            }
        }


        //REFACTOR THE CONTACTS TRANSFERRED OUT
        public static void rf_ContactsTransferredOut(ref SQLDB sql, Organisation O) {

            List<DynamicField> Fields = dynamicFields_getFieldsForOrganisation(O, "contacts", ref sql, true);
            List<SQLColumnItem> colItems = new List<SQLColumnItem>();

            foreach (DynamicField f in Fields) {
                if (TypeCast.ToInt(f.ShowInListview) > 0 && f.OrganisationId == O.Id) {
                    string col = "";
                    if (f.DataSource.IndexOf('_') > 0) {
                        col = f.DataSource + "_" + f.DataSource.Split('_')[2];
                    } else {
                        col = f.DatabaseTable + "_" + f.DatabaseColumn + "_" + f.BaseOrganisationId;
                    }

                    //Add the columns desired in a sorted manor!
                    bool inserted = false;
                    int index = f.ListviewIndex;

                    if (colItems.Count == 0) {
                        colItems.Add(new SQLColumnItem(col, f.ListviewIndex));
                    } else {
                        for (int ii = 0; ii < colItems.Count; ii++) {
                            int cpIndex = colItems[ii].ListIndex;
                            if (inserted == false && index < cpIndex) {
                                inserted = true;
                                colItems.Insert(ii > colItems.Count ? colItems.Count : ii, new SQLColumnItem(col, f.ListviewIndex));
                            }
                        }

                        if (inserted == false)
                            colItems.Add(new SQLColumnItem(col, f.ListviewIndex));
                    }

                }
            }

            string txtSQL = "" +
                            "CREATE PROCEDURE [dbo].[z_Contacts_TransferredOut_" + O.Id + "]\n" +
                            "   @organisationId int,\n" +
                            "   @userId uniqueidentifier\n" +
                            "AS\n" +
                            "    SET ARITHABORT ON\n\n" +
                            "   SELECT top 100 \n" +
                            "        Id,\n" +
                            "        cts.ContactId,\n" +
                            "        CompanyOwnerId,\n" +
                            "        ContactOrganisationId,\n" +
                            "        ContactCreatedById,\n" +
                            "        ContactDateStamp,\n" +
                            "        ContactType,\n" +
                            "        CompanyDeletedDate,\n" +
                            "        CompanyDeletedBy,\n" +
                            "        ContactDeletedDate,\n" +
                            "        ContactDeletedBy,\n" +
                            "        CompanyLastUpdated,CompanyAbandonedDate, CompanyAbandonedBy,\n" +
                            "        CompanyLastUpdatedBy,\n" +
                            "        ContactLastUpdated,\n" +
                            "        ContactLastUpdatedBy, ContactAbandonedDate, ContactAbandonedBy,\n" +
                            "\n" +
                            "       SenderUserId,\n" +
                            "       [dbo].[getUserInfo](SenderUserId) as SenderUserName,\n" +
                            "       SenderOrganisationId,\n" +
                            "       [dbo].[getOrganisationInfo](SenderOrganisationId) as SenderOrganisationName,\n" +

                            "       ReceiverUserId,\n" +
                            "       [dbo].[getUserInfo](ReceiverUserId) as ReceiverUserName,\n" +
                            "       ReceiverOrganisationId,\n" +
                            "       [dbo].[getOrganisationInfo](ReceiverOrganisationId) as ReceiverOrganisationName,\n" +

                            "       AcceptedByUserId,\n" +
                            "       [dbo].[getUserInfo](AcceptedByUserId) as AcceptedByUserName,\n" +
                            "       AcceptedDate,\n" +
                            "       AcceptedReason,\n" +

                            "       RejectedByUserId,\n" +
                            "       [dbo].[getUserInfo](RejectedByUserId) as RejectedByUserName,\n" +
                            "       RejectedDate,\n" +
                            "       RejectedReason,\n" +

                            "       TransferDate,\n" +
                            "       TransferReason,\n" +
                            "       ForwardedToId\n" +


                            "\n" +
                            "\n" +
                            SQLColumnItemsToString(colItems) +
                            "   FROM\n" +
                            "       [dbo].[ContactsAndCompaniesList_" + O.Id + "]() as cts, ContactTransfers\n" +
                            "   WHERE\n" +
                            "ContactTransfers.ContactId = cts.ContactId and " +
                            "senderOrganisationId = @organisationId and " +
                            "senderUserId=@UserId and " +
                            "(acceptedByUserId is null or datediff(d, acceptedDate, getdate())<=30) AND\n" +
                            "(RejectedByUserId is null or datediff(d, RejectedDate, getdate())<=30) AND\n" +
                            "ContactTransfers.ForwardedToId is null\n" +
                            "order by \n" +
                            "TransferDate";


            //EXECUTE SQL!!!
            if (txtSQL != "" && txtSQL != null) {
                //(RE)CREATE THE PROCEDURE!!!
                sql.dynamicFields_dropStoredProcedure("z_Contacts_TransferredOut_" + O.Id);
                sql.dynamicFields_updateStoredProcedure(txtSQL);
            }
        }


        //REFACTOR THE CONTACTS RECENT
        public static void rf_ContactsRecent(ref SQLDB sql, Organisation O) {

            List<DynamicField> Fields = dynamicFields_getFieldsForOrganisation(O, "contacts", ref sql, true);
            List<SQLColumnItem> colItems = new List<SQLColumnItem>();

            foreach (DynamicField f in Fields) {
                if (TypeCast.ToInt(f.ShowInListview) > 0 && f.OrganisationId == O.Id) {
                    string col = "";
                    if (f.DataSource.IndexOf('_') > 0) {
                        col = f.DataSource + "_" + f.DataSource.Split('_')[2];
                    } else {
                        col = f.DatabaseTable + "_" + f.DatabaseColumn + "_" + f.BaseOrganisationId;
                    }

                    //Add the columns desired in a sorted manor!
                    bool inserted = false;
                    int index = f.ListviewIndex;

                    if (colItems.Count == 0) {
                        colItems.Add(new SQLColumnItem(col, f.ListviewIndex));
                    } else {
                        for (int ii = 0; ii < colItems.Count; ii++) {
                            int cpIndex = colItems[ii].ListIndex;
                            if (inserted == false && index < cpIndex) {
                                inserted = true;
                                colItems.Insert(ii > colItems.Count ? colItems.Count : ii, new SQLColumnItem(col, f.ListviewIndex));
                            }
                        }

                        if (inserted == false)
                            colItems.Add(new SQLColumnItem(col, f.ListviewIndex));
                    }

                }
            }

            string txtSQL = "" +
                            "CREATE PROCEDURE [dbo].[z_Contacts_Recent_" + O.Id + "]\n" +
                            "   @userId uniqueidentifier\n" +
                            "AS\n" +
                            "    SET ARITHABORT ON\n\n" +
                            "declare @table table(DateStamp datetime, ContactId int, LastChanged varchar(255))\n" +
                            "insert into @table select * from dbo.[getContactsLastUpdatedRange](@userId)\n" +
                            "\n" +
                            "   SELECT top 100 \n" +
                            "        ContactId,\n" +
                            "        CompanyOwnerId,\n" +
                            "        ContactOrganisationId,\n" +
                            "        ContactCreatedById,\n" +
                            "        ContactDateStamp,\n" +
                            "        ContactType,\n" +
                            "        CompanyDeletedDate,\n" +
                            "        CompanyDeletedBy,\n" +
                            "        ContactDeletedDate,\n" +
                            "        ContactDeletedBy,\n" +
                            "        CompanyLastUpdated,CompanyAbandonedDate, CompanyAbandonedBy,\n" +
                            "        CompanyLastUpdatedBy,\n" +
                            "        --ContactLastUpdated,\n" +
                            "        ContactLastUpdatedBy, ContactAbandonedDate, ContactAbandonedBy,\n" +
                            "\n" +
                            "        isnull((select top 1 datestamp from @table dtRange where dtRange.ContactId=Contacts.ContactId order by DateStamp desc), isnull(contacts.ContactLastUpdated, contacts.ContactDateStamp)) as ContactLastUpdated,\n" +
                            "        (select top 1 LastChanged from @table dtRange where dtRange.ContactId=Contacts.ContactId order by DateStamp desc) as ContactLastUpdatedReason\n" +
                            "\n" +
                            SQLColumnItemsToString(colItems) +
                            "   FROM\n" +
                            "       [dbo].[ContactsAndCompaniesList_" + O.Id + "]() as Contacts\n" +
                            "   WHERE\n" +
                            "contactLastUpdatedBy=@userId or contactCreatedById=@userId or\n" +
                            "Contacts.ContactId in (select ContactId from @table)\n" +
                            "order by \n" +
                            "   (select top 1 datestamp from @table dtRange where dtRange.ContactId=Contacts.ContactId order by DateStamp desc) desc, (isnull(contacts.ContactLastUpdated, contacts.ContactDateStamp)) desc";


            //EXECUTE SQL!!!
            if (txtSQL != "" && txtSQL != null) {
                //(RE)CREATE THE PROCEDURE!!!
                sql.dynamicFields_dropStoredProcedure("z_Contacts_Recent_" + O.Id);
                sql.dynamicFields_updateStoredProcedure(txtSQL);
            }
        }

        private static int getDataLinkWeight(ref SQLDB sql, DynamicField f, Organisation O) {
            switch (f.DataLink) {
                case "NNECompanyName":
                    return 4;

                case "NNECompanyCVR":
                    return 3;

                case "NNECompanyPNR":
                    return 3;

                case "DGSContactFirstname":
                    return 2;

                case "DGSCOntactLastname":
                    return 2;


            }

            //Data source (external data link)
            if (f.DataSource != "" && f.DataSource != null && f.DataSource != "null") {
                List<DynamicField> Fields = dynamicFields_getFieldsForOrganisation(O, "", ref sql, true);

                foreach (DynamicField ff in Fields) {
                    if (f.DataSource == ff.DatabaseTable + "_" + ff.DatabaseColumn) {
                        return getDataLinkWeight(ref sql, ff, O);
                    }
                }
            }

            return 1;
        }

        //REFACTOR THE CONTACTS QUICKSEARCH2
        public static void rf_ContactsQuickSearch2(ref SQLDB sql, Organisation O) {

            List<DynamicField> Fields = dynamicFields_getFieldsForOrganisation(O, "contacts", ref sql, false);

            string query = "";
            List<SQLColumnItem> colItems = new List<SQLColumnItem>();

            foreach (DynamicField f in Fields) {
                if (f.DatabaseColumn.IndexOf("navne") > -1) {
                    System.Diagnostics.Debug.Write(f.DatabaseColumn);
                }

                if (!f.NoInherit(O.Id) || f.BaseOrganisationId == f.OrganisationId) {
                    if (TypeCast.ToInt(f.ShowInListview) > 0 && f.OrganisationId == O.Id) {
                        string col = "";
                        if (f.DataSource.IndexOf('_') > 0) {
                            col = f.DataSource + "_" + f.DataSource.Split('_')[2];
                        } else {
                            col = f.DatabaseTable + "_" + f.DatabaseColumn + "_" + f.BaseOrganisationId;
                        }

                        query += "convert(varchar(8000),[" + col + "]) like '%' + tbl.query + '%' or\n";

                        //query += (query == "" ? "" : " union all \n") +
                        //    "select " + weight + " as ct, ContactId from [dbo].[ContactsAndCompaniesList_1](), @qs where convert(varchar(8000),[" + col + "]) like " + (weight == 4 ? "'%'" : "@freesearchBefore") + " + string + '%'\n";

                        //Add the columns desired in a sorted manor!
                        bool inserted = false;
                        int index = f.ListviewIndex;

                        if (colItems.Count == 0) {
                            colItems.Add(new SQLColumnItem(col, f.ListviewIndex));
                        } else {
                            for (int ii = 0; ii < colItems.Count; ii++) {
                                int cpIndex = colItems[ii].ListIndex;
                                if (inserted == false && index < cpIndex) {
                                    inserted = true;
                                    colItems.Insert(ii > colItems.Count ? colItems.Count : ii, new SQLColumnItem(col, f.ListviewIndex));
                                }
                            }

                            if (inserted == false)
                                colItems.Add(new SQLColumnItem(col, f.ListviewIndex));
                        }


                    }
                }
            }

            string qf = "";

            if (query.Length > 3)
                qf = "AND (select count(query) from @table tbl where \n\t(" + query.Substring(0, query.Length - 3) + ")) = @matchThis \n";



            string txtSQL = "" +
                            "CREATE PROCEDURE [dbo].[z_Contacts_QuickSearch2_" + O.Id + "]\n" +
                            "   @organisationId int,\n" +
                            "   @userId uniqueidentifier,\n" +
                            "   @q1 varchar(500)=null,\n" +
                            "   @q2 varchar(500)=null,\n" +
                            "   @q3 varchar(500)=null,\n" +
                            "   @q4 varchar(500)=null,\n" +
                            "   @q5 varchar(500)=null,\n" +
                            "   @q6 varchar(500)=null,\n" +
                            "   @q7 varchar(500)=null,\n" +
                            "   @q8 varchar(500)=null,\n" +
                            "   @q9 varchar(500)=null,\n" +
                            "   @q10 varchar(500)=null,\n" +
                            "   @type int=null,\n" +
                            "   @searchIn int=-1,\n" +
                            "   @SortOrder varchar(50)='" + colItems[0].SortingName + "',\n" +
                            "   @sortAsc varchar(4)='asc'\n" +

                            "AS\n" +

"   \n" +
"   SET NOCOUNT ON\n" +
"   SET ARITHABORT ON\n" +
"   \n" +
"   declare @table table(query varchar(500))\n" +
"   declare @matchThis int\n" +
"   \n" +
"   insert into @table select @q1 where @q1 is not null and @q1 not in (select query from @table)\n" +
"   insert into @table select @q2 where @q2 is not null and @q2 not in (select query from @table)\n" +
"   insert into @table select @q3 where @q3 is not null and @q3 not in (select query from @table)\n" +
"   insert into @table select @q4 where @q4 is not null and @q4 not in (select query from @table)\n" +
"   insert into @table select @q5 where @q5 is not null and @q5 not in (select query from @table)\n" +
"   insert into @table select @q6 where @q6 is not null and @q6 not in (select query from @table)\n" +
"   insert into @table select @q7 where @q7 is not null and @q7 not in (select query from @table)\n" +
"   insert into @table select @q8 where @q8 is not null and @q8 not in (select query from @table)\n" +
"   insert into @table select @q9 where @q9 is not null and @q9 not in (select query from @table)\n" +
"   insert into @table select @q10 where @q10 is not null and @q10 not in (select query from @table)\n" +
"   \n" +
"   set @matchThis = (select count(*) from @table)\n" +
"   \n" +

                            "   SELECT\n" +
                            "        ContactId,\n" +
                            "        CompanyOwnerId,\n" +
                            "        ContactOrganisationId,\n" +
                            "        ContactCreatedById,\n" +
                            "        ContactDateStamp,\n" +
                            "        ContactType,\n" +
                            "        CompanyDeletedDate,\n" +
                            "        CompanyDeletedBy,\n" +
                            "        ContactDeletedDate,\n" +
                            "        ContactDeletedBy,\n" +
                            "        CompanyLastUpdated,CompanyAbandonedDate, CompanyAbandonedBy,\n" +
                            "        CompanyLastUpdatedBy,\n" +
                            "        ContactLastUpdated,\n" +
                            "        ContactLastUpdatedBy, ContactAbandonedDate, ContactAbandonedBy\n" +
                            "\n" +
                            SQLColumnItemsToString(colItems) +
                            "   FROM\n" +
                            "       [dbo].[ContactsAndCompaniesList_" + O.Id + "]()\n" +
                            "   WHERE\n" +
                            "   -- WHICH ORGANISATION(S)/USER TO SEARCH\n" +
                            "   (\n" +
                            "       (@searchIn=0 and (ContactCreatedById=@userId OR ContactLastUpdatedBy=@userId)) OR\n" +
                            "       (@searchIn>0 and ContactOrganisationId=@searchIn) OR\n" +
                            "       (@searchIn<0)\n" +
                            "   )\n" +
                            "   -- SPECIFIC CONTACT TYPE? POT/SMV/Dont care\n" +
                            "   AND\n" +
                            "   (\n" +
                            "       (@type is not null and @type=ContactType) OR\n" +
                            "       (@type is null)\n" +
                            ")\n" +
                            qf +
                            "\n" +
                            "\n";


            //EXECUTE SQL!!!
            if (txtSQL != "" && txtSQL != null) {
                //(RE)CREATE THE PROCEDURE!!!
                sql.dynamicFields_dropStoredProcedure("z_Contacts_QuickSearch2_" + O.Id);
                sql.dynamicFields_updateStoredProcedure(txtSQL);
            }

            //sql.dynamicFields_dropStoredProcedure("z_Contacts_QuickSearch2_" + O.Id);
        }

        public static void rf_ContactsQuickSearch2_OLD(ref SQLDB sql, Organisation O) {

            List<DynamicField> Fields = dynamicFields_getFieldsForOrganisation(O, "contacts", ref sql, true);

            string query = "";
            List<SQLColumnItem> colItems = new List<SQLColumnItem>();

            foreach (DynamicField f in Fields) {
                if (!f.NoInherit(O.Id) || f.BaseOrganisationId == f.OrganisationId) {
                    if (TypeCast.ToInt(f.ShowInListview) > 0 && f.OrganisationId == O.Id) {
                        string col = "";
                        if (f.DataSource.IndexOf('_') > 0) {
                            col = f.DataSource + "_" + f.DataSource.Split('_')[2];
                        } else {
                            col = f.DatabaseTable + "_" + f.DatabaseColumn + "_" + f.BaseOrganisationId;
                        }

                        query += "convert(varchar(8000),[" + col + "]) like '%' + @q# + '%' or\n";

                        //query += (query == "" ? "" : " union all \n") +
                        //    "select " + weight + " as ct, ContactId from [dbo].[ContactsAndCompaniesList_1](), @qs where convert(varchar(8000),[" + col + "]) like " + (weight == 4 ? "'%'" : "@freesearchBefore") + " + string + '%'\n";

                        //Add the columns desired in a sorted manor!
                        bool inserted = false;
                        int index = f.ListviewIndex;

                        if (colItems.Count == 0) {
                            colItems.Add(new SQLColumnItem(col, f.ListviewIndex));
                        } else {
                            for (int ii = 0; ii < colItems.Count; ii++) {
                                int cpIndex = colItems[ii].ListIndex;
                                if (inserted == false && index < cpIndex) {
                                    inserted = true;
                                    colItems.Insert(ii > colItems.Count ? colItems.Count : ii, new SQLColumnItem(col, f.ListviewIndex));
                                }
                            }

                            if (inserted == false)
                                colItems.Add(new SQLColumnItem(col, f.ListviewIndex));
                        }


                    }
                }
            }

            //rebuild query to 10 sets
            string qf = "";

            for (int i = 1; i < 11; i++) {
                qf += "AND\n\t(" + query.Replace("@q#", "@q" + i) + "\n" +
                    "[dbo].[getUserInfo](ContactCreatedById) like '%' + @q" + i + " + '%' or \n" +
                    "[dbo].[getUserInfo](CompanyCreatedById) like '%' + @q" + i + " + '%' or \n" +
                    "[dbo].[getUserInfo](ContactLastUpdatedBy) like '%' + @q" + i + " + '%' or \n" +
                    "[dbo].[getUserInfo](CompanyLastUpdatedBy) like '%' + @q" + i + " + '%' or \n" +
                    "@q" + i + " is null)\n\n";
            }

            string txtSQL = "" +
                            "CREATE PROCEDURE [dbo].[z_Contacts_QuickSearch2_" + O.Id + "]\n" +
                            "   @organisationId int,\n" +
                            "   @userId uniqueidentifier,\n" +
                            "   @q1 varchar(500)=null,\n" +
                            "   @q2 varchar(500)=null,\n" +
                            "   @q3 varchar(500)=null,\n" +
                            "   @q4 varchar(500)=null,\n" +
                            "   @q5 varchar(500)=null,\n" +
                            "   @q6 varchar(500)=null,\n" +
                            "   @q7 varchar(500)=null,\n" +
                            "   @q8 varchar(500)=null,\n" +
                            "   @q9 varchar(500)=null,\n" +
                            "   @q10 varchar(500)=null,\n" +
                            "   @type int=null,\n" +
                            "   @searchIn int=-1,\n" +
                            "   @SortOrder varchar(50)='" + colItems[0].SortingName + "',\n" +
                            "   @sortAsc varchar(4)='asc'\n" +

                            "AS\n" +

                            "   SELECT\n" +
                            "        ContactId,\n" +
                            "        CompanyOwnerId,\n" +
                            "        ContactOrganisationId,\n" +
                            "        ContactCreatedById,\n" +
                            "        ContactDateStamp,\n" +
                            "        ContactType,\n" +
                            "        CompanyDeletedDate,\n" +
                            "        CompanyDeletedBy,\n" +
                            "        ContactDeletedDate,\n" +
                            "        ContactDeletedBy,\n" +
                            "        CompanyLastUpdated,CompanyAbandonedDate, CompanyAbandonedBy,\n" +
                            "        CompanyLastUpdatedBy,\n" +
                            "        ContactLastUpdated,\n" +
                            "        ContactLastUpdatedBy, ContactAbandonedDate, ContactAbandonedBy\n" +
                            "\n" +
                            SQLColumnItemsToString(colItems) +
                            "   FROM\n" +
                            "       [dbo].[ContactsAndCompaniesList_" + O.Id + "]()\n" +
                            "   WHERE\n" +
                            "   -- WHICH ORGANISATION(S)/USER TO SEARCH\n" +
                            "   (\n" +
                            "       (@searchIn=0 and (ContactCreatedById=@userId OR ContactLastUpdatedBy=@userId)) OR\n" +
                            "       (@searchIn>0 and ContactOrganisationId=@searchIn) OR\n" +
                            "       (@searchIn<0)\n" +
                            "   )\n" +
                            "   -- SPECIFIC CONTACT TYPE? POT/SMV/Dont care\n" +
                            "   AND\n" +
                            "   (\n" +
                            "       (@type is not null and @type=ContactType) OR\n" +
                            "       (@type is null)\n" +
                            ")\n" +
                            qf +
                            "\n" +
                            "\n";


            //EXECUTE SQL!!!
            if (txtSQL != "" && txtSQL != null) {
                //(RE)CREATE THE PROCEDURE!!!
                sql.dynamicFields_dropStoredProcedure("z_Contacts_QuickSearch2_" + O.Id);
                sql.dynamicFields_updateStoredProcedure(txtSQL);
            }
        }

        //REFACTOR THE CONTACTS QUICKSEARCH
        public static void rf_ContactsQuickSearch(ref SQLDB sql, Organisation O) {

            List<DynamicField> Fields = dynamicFields_getFieldsForOrganisation(O, "contacts", ref sql, true);

            string query = "";
            List<SQLColumnItem> colItems = new List<SQLColumnItem>();

            foreach (DynamicField f in Fields) {
                if (TypeCast.ToInt(f.ShowInListview) > 0 && f.OrganisationId == O.Id) {
                    string col = "";
                    if (f.DataSource.IndexOf('_') > 0) {
                        col = f.DataSource + "_" + f.DataSource.Split('_')[2];
                    } else {
                        col = f.DatabaseTable + "_" + f.DatabaseColumn + "_" + f.BaseOrganisationId;
                    }

                    int weight = getDataLinkWeight(ref sql, f, O);
                    query += (query == "" ? "" : " union all \n") +
                        "select " + weight + " as ct, ContactId from [dbo].[ContactsAndCompaniesList_1](), @qs where convert(varchar(8000),[" + col + "]) like " + (weight == 4 ? "'%'" : "@freesearchBefore") + " + string + '%'\n";


                    //Add the columns desired in a sorted manor!
                    bool inserted = false;
                    int index = f.ListviewIndex;

                    if (colItems.Count == 0) {
                        colItems.Add(new SQLColumnItem(col, f.ListviewIndex));
                    } else {
                        for (int ii = 0; ii < colItems.Count; ii++) {
                            int cpIndex = colItems[ii].ListIndex;
                            if (inserted == false && index < cpIndex) {
                                inserted = true;
                                colItems.Insert(ii > colItems.Count ? colItems.Count : ii, new SQLColumnItem(col, f.ListviewIndex));
                            }
                        }

                        if (inserted == false)
                            colItems.Add(new SQLColumnItem(col, f.ListviewIndex));
                    }


                }
            }

            string txtSQL = "" +
                            "CREATE PROCEDURE [dbo].[z_Contacts_QuickSearch_" + O.Id + "]\n" +
                            "   @organisationId int,\n" +
                            "   @userId uniqueidentifier,\n" +
                            "   @query varchar(500)='',\n" +
                            "   @type int=null,\n" +
                            "   @searchIn int=null,\n" +
                            "   @SortOrder varchar(50)='" + colItems[0].SortingName + "',\n" +
                            "   @sortAsc varchar(4)='asc',\n" +
                            "   @freesearchBefore varchar(1)='',\n" +
                            "   @laxSearchCount int=0\n" +
                            "AS\n" +
                            "if @query='' set @query='%'\n" +
                            "declare @qs table(string varchar(40), startIndex int)\n" +
                            "insert into @qs select @query, 0\n" +
                            "insert into @qs select * from [dbo].StringSplit(replace(replace(replace(@query,'*','%'),'?','_'),' ','|'),'|')\n" +
                            "delete from @qs where string='' or string is null\n" +
                            "update @qs set string = replace(string, '+',' ')\n" +
                            "\n" +
                            "declare @counter int\n" +
                            "set @counter = (select count(*) from @qs)-@laxSearchCount\n" +
                            "\n" +

                            "   SELECT\n" +
                            "        ContactId,\n" +
                            "        CompanyOwnerId,\n" +
                            "        ContactOrganisationId,\n" +
                            "        ContactCreatedById,\n" +
                            "        ContactDateStamp,\n" +
                            "        ContactType,\n" +
                            "        CompanyDeletedDate,\n" +
                            "        CompanyDeletedBy,\n" +
                            "        ContactDeletedDate,\n" +
                            "        ContactDeletedBy,\n" +
                            "        CompanyLastUpdated,CompanyAbandonedDate, CompanyAbandonedBy,\n" +
                            "        CompanyLastUpdatedBy,\n" +
                            "        ContactLastUpdated,\n" +
                            "        ContactLastUpdatedBy, ContactAbandonedDate, ContactAbandonedBy\n" +
                            "\n" +
                            SQLColumnItemsToString(colItems) +
                            "   FROM\n" +
                            "       [dbo].[ContactsAndCompaniesList_" + O.Id + "]()\n" +
                            "   WHERE\n" +
                            "   -- WHICH ORGANISATION(S)/USER TO SEARCH\n" +
                            "   (\n" +
                            "       (@searchIn=0 and (ContactCreatedById=@userId OR ContactLastUpdatedBy=@userId)) OR\n" +
                            "       (@searchIn>0 and ContactOrganisationId=@searchIn) OR\n" +
                            "       (@searchIn<0)\n" +
                            "   )\n" +
                            "   -- SPECIFIC CONTACT TYPE? POT/SMV/Dont care\n" +
                            "   AND\n" +
                            "   (\n" +
                            "       (@type is not null and @type=ContactType) OR\n" +
                            "       (@type is null)\n" +
                            "   ) AND CONTACTID IN ( select distinct ContactId from (\n" +
                            query +
                            ") as qry group by qry.ContactId	having sum(ct)>@counter )\n" +
                            "\n" +
                            "\n";


            //EXECUTE SQL!!!
            if (txtSQL != "" && txtSQL != null) {
                //(RE)CREATE THE PROCEDURE!!!
                sql.dynamicFields_dropStoredProcedure("z_Contacts_QuickSearch_" + O.Id);
                sql.dynamicFields_updateStoredProcedure(txtSQL);
            }
        }


        //REFACTOR THE CONTACTS QUICKSEARCH
        public static void rf_ContactsQuickSearch_old(ref SQLDB sql, Organisation O) {

            List<DynamicField> Fields = dynamicFields_getFieldsForOrganisation(O, "contacts", ref sql, true);

            string query = "";
            List<SQLColumnItem> colItems = new List<SQLColumnItem>();
            string sortOrder = "";

            foreach (DynamicField f in Fields) {
                if (TypeCast.ToInt(f.ShowInListview) > 0 && f.OrganisationId == O.Id) {
                    string col = "";
                    if (f.DataSource.IndexOf('_') > 0) {
                        col = f.DataSource + "_" + f.DataSource.Split('_')[2];
                    } else {
                        col = f.DatabaseTable + "_" + f.DatabaseColumn + "_" + f.BaseOrganisationId;
                    }
                    query += "" + (query == "" ? "" : "    OR ") +
                                "       (convert(varchar(8000),[" + col + "]) LIKE '%' + @query + '%')\n";


                    query += "    OR " +
                                "       ((@query LIKE '%' + convert(varchar(8000),[" + col + "]) + '%') AND convert(varchar(8000),isnull([" + col + "],''))<>'')\n";

                    //Add the columns desired in a sorted manor!
                    bool inserted = false;
                    int index = f.ListviewIndex;

                    if (colItems.Count == 0) {
                        colItems.Add(new SQLColumnItem(col, f.ListviewIndex));
                    } else {
                        for (int ii = 0; ii < colItems.Count; ii++) {
                            int cpIndex = colItems[ii].ListIndex;
                            if (inserted == false && index < cpIndex) {
                                inserted = true;
                                colItems.Insert(ii > colItems.Count ? colItems.Count : ii, new SQLColumnItem(col, f.ListviewIndex));
                            }
                        }

                        if (inserted == false)
                            colItems.Add(new SQLColumnItem(col, f.ListviewIndex));
                    }


                    sortOrder += (sortOrder == "" ? "    ORDER BY\n" : "    ,\n") +
                                 "    CASE\n" +
                                 "        WHEN @sortOrder='" + col + "' AND @sortAsc='desc' THEN " + GetSortCast(f.FieldType, col) + "\n" +
                                 "    END DESC,\n" +
                                 "    CASE\n" +
                                 "        WHEN @sortOrder='" + col + "' AND @sortAsc='asc' THEN " + GetSortCast(f.FieldType, col) + "\n" +
                                 "    END ASC\n";

                }
            }
            if (query != "") {
                query = "AND (\n" + query + "\n)";
            }

            string txtSQL = "" +
                            "CREATE PROCEDURE [dbo].[z_Contacts_QuickSearch_" + O.Id + "]\n" +
                            "   @organisationId int,\n" +
                            "   @userId uniqueidentifier,\n" +
                            "   @query varchar(500)='',\n" +
                            "   @type int=null,\n" +
                            "   @searchIn int=null,\n" +
                            "   @SortOrder varchar(50)='" + colItems[0].SortingName + "',\n" +
                            "   @sortAsc varchar(4)='asc'\n" +
                            "AS\n" +
                            "   SELECT\n" +
                            "        ContactId,\n" +
                            "        CompanyOwnerId,\n" +
                            "        ContactOrganisationId,\n" +
                            "        ContactCreatedById,\n" +
                            "        ContactDateStamp,\n" +
                            "        ContactType,\n" +
                            "        CompanyDeletedDate,\n" +
                            "        CompanyDeletedBy,\n" +
                            "        ContactDeletedDate,\n" +
                            "        ContactDeletedBy,\n" +
                            "        CompanyLastUpdated,CompanyAbandonedDate, CompanyAbandonedBy,\n" +
                            "        CompanyLastUpdatedBy,\n" +
                            "        ContactLastUpdated,\n" +
                            "        ContactLastUpdatedBy, ContactAbandonedDate, ContactAbandonedBy\n" +
                            "\n" +
                            SQLColumnItemsToString(colItems) +
                            "   FROM\n" +
                            "       [dbo].[ContactsAndCompaniesList_" + O.Id + "]()\n" +
                            "   WHERE\n" +
                            "   -- WHICH ORGANISATION(S)/USER TO SEARCH\n" +
                            "   (\n" +
                            "       (@searchIn=0 and (ContactCreatedById=@userId OR ContactLastUpdatedBy=@userId)) OR\n" +
                            "       (@searchIn>0 and ContactOrganisationId=@searchIn) OR\n" +
                            "       (@searchIn<0)\n" +
                            "   )\n" +
                            "   -- SPECIFIC CONTACT TYPE? POT/SMV/Dont care\n" +
                            "   AND\n" +
                            "   (\n" +
                            "       (@type is not null and @type=ContactType) OR\n" +
                            "       (@type is null)\n" +
                            "   )\n" +
                            query +
                            "\n" +
                            "\n" +
                            "\n" +
                            sortOrder;


            //EXECUTE SQL!!!
            if (txtSQL != "" && txtSQL != null) {
                //(RE)CREATE THE PROCEDURE!!!
                sql.dynamicFields_dropStoredProcedure("z_Contacts_QuickSearch_" + O.Id);
                sql.dynamicFields_updateStoredProcedure(txtSQL);
            }
        }


        //REFACTOR THE CONTACTS GETSINGLEFROMCONTACTSANDCOMPANIES
        public static void rf_GetSingleFromContactsAndCompanies(ref SQLDB sql, Organisation O) {
            List<DynamicField> Fields = dynamicFields_getFieldsForOrganisation(O, ref sql, true);

            List<SQLColumnItem> colItems = new List<SQLColumnItem>();

            foreach (DynamicField f in Fields) {    //** Ekstra check Favrskov
                if (((f.DataLink == "NNECompanyName" && f.BaseOrganisationId == 1) || f.DataLink == "DGSContactFirstname" || f.DataLink == "DGSCOntactLastname") && f.OrganisationId == O.Id) {
                    string col = "";
                    if (f.DataSource.IndexOf('_') > 0) {
                        col = f.DataSource + "_" + f.DataSource.Split('_')[2];
                    } else {
                        col = f.DatabaseTable + "_" + f.DatabaseColumn + "_" + f.BaseOrganisationId;
                    }

                    //Add the columns desired in a sorted manor!
                    bool inserted = false;
                    int index = f.ListviewIndex;

                    if (colItems.Count == 0) {
                        colItems.Add(new SQLColumnItem(col, f.ListviewIndex));
                    } else {
                        for (int ii = 0; ii < colItems.Count; ii++) {
                            int cpIndex = colItems[ii].ListIndex;
                            if (inserted == false && index < cpIndex) {
                                inserted = true;
                                colItems.Insert(ii > colItems.Count ? colItems.Count : ii, new SQLColumnItem(col, f.ListviewIndex));
                            }
                        }

                        if (inserted == false)
                            colItems.Add(new SQLColumnItem(col, f.ListviewIndex));
                    }
                }
            }

            string txtSQL = "" +
                            "CREATE PROCEDURE [dbo].[z_Contacts_GetSingleFromContactsAndCompanies_" + O.Id + "]\n" +
                            "   @ContactId int\n" +
                            "AS\n" +
                            "   SELECT\n" +
                            "        ContactId,\n" +
                            "        CompanyOwnerId,\n" +
                            "        ContactOrganisationId,\n" +
                            "        ContactCreatedById,\n" +
                            "        ContactDateStamp,\n" +
                            "        ContactType,\n" +
                            "        CompanyDeletedDate,\n" +
                            "        CompanyDeletedBy,\n" +
                            "        ContactDeletedDate,\n" +
                            "        ContactDeletedBy,\n" +
                            "        CompanyLastUpdated,CompanyAbandonedDate, CompanyAbandonedBy,\n" +
                            "        CompanyLastUpdatedBy,\n" +
                            "        ContactLastUpdated,\n" +
                            "        ContactLastUpdatedBy, ContactAbandonedDate, ContactAbandonedBy\n" +
                            "\n" +
                            SQLColumnItemsToString(colItems) +
                            "   FROM\n" +
                            "       [dbo].[ContactsAndCompaniesList_" + O.Id + "]()\n" +
                            "   WHERE \n" +
                            "       ContactId = @ContactId\n" +
                            "\n" +
                            "\n" +
                            "\n" +
                            "\n" +
                            "\n" +
                            "\n";

            //EXECUTE SQL!!!
            if (txtSQL != "" && txtSQL != null) {
                //(RE)CREATE THE PROCEDURE!!!
                sql.dynamicFields_dropStoredProcedure("z_Contacts_GetSingleFromContactsAndCompanies_" + O.Id);
                sql.dynamicFields_updateStoredProcedure(txtSQL);
            }
        }

        //REFACTOR THE COMPANIES LOCATE DOUBLES
        public static void rf_CompanyOrContactExists(ref SQLDB sql, Organisation O) {

            List<DynamicField> Fields = dynamicFields_getFieldsForOrganisation(O, "", ref sql, false);

            string query = "";
            List<SQLColumnItem> colItems = new List<SQLColumnItem>();

            foreach (DynamicField f in Fields) {
                if ((f.DataLink == "NNECompanyCVR" || f.DataLink == "CompanyCPR") && f.OrganisationId == O.Id) {
                    string col = "";
                    if (f.DataSource.IndexOf('_') > 0) {
                        col = f.DataSource + "_" + f.DataSource.Split('_')[2];
                    } else {
                        col = f.DatabaseTable + "_" + f.DatabaseColumn + "_" + f.BaseOrganisationId;
                    }
                    query += "" + (query == "" ? "" : "    OR ") +
                        "       convert(varchar(8000),[" + col + "]) = " + (f.DataLink == "NNECompanyCVR" ? "@cvr" : "@cpr") + "\n";


                    //Add the columns desired in a sorted manor!
                    bool inserted = false;
                    int index = f.ListviewIndex;

                    if (colItems.Count == 0) {
                        colItems.Add(new SQLColumnItem(col, f.ListviewIndex));
                    } else {
                        for (int ii = 0; ii < colItems.Count; ii++) {
                            int cpIndex = colItems[ii].ListIndex;
                            if (inserted == false && index < cpIndex) {
                                inserted = true;
                                colItems.Insert(ii > colItems.Count ? colItems.Count : ii, new SQLColumnItem(col, f.ListviewIndex));
                            }
                        }

                        if (inserted == false)
                            colItems.Add(new SQLColumnItem(col, f.ListviewIndex));
                    }

                }
            }
            if (query != "") {
                query = "AND (\n" + query + "\n)";
            }

            string txtSQL = "" +
                            "CREATE PROCEDURE [dbo].[z_CompanyOrContactExists_" + O.Id + "]\n" +
                            "   @cvr varchar(500)=null,\n" +
                            "   @cpr varchar(500)=null,\n" +
                            "   @notThisCompanyId int=null,\n" +
                            "   @notThisContactId int=null\n" +
                            "AS\n" +
                            "   SELECT DISTINCT\n" +
                            "       ContactId,\n" +
                            "       CompanyOwnerId\n" +
                            "\n" +
                            SQLColumnItemsToString(colItems) +
                            "   FROM\n" +
                            "       [dbo].[ContactsAndCompaniesList_" + O.Id + "]()\n" +
                            "   WHERE\n" +
                            "       (CompanyOwnerId<>@notThisCompanyId OR @notThisCompanyId is null) AND\n" +
                            "       (ContactId<>@notThisContactId OR @notThisContactId is null) \n" +
                            "   -- SPECIFIC CONTACT TYPE? POT/SMV/Dont care\n" +
                            query +
                            "\n" +
                            "\n" +
                            "\n";


            //EXECUTE SQL!!!
            if (txtSQL != "" && txtSQL != null) {
                //(RE)CREATE THE PROCEDURE!!!
                sql.dynamicFields_dropStoredProcedure("z_CompanyOrContactExists_" + O.Id);
                sql.dynamicFields_updateStoredProcedure(txtSQL);
            }
        }

        //REFACTOR THE COMPANIES LOCATE DOUBLES
        public static void rf_CompaniesLocateDoubles(ref SQLDB sql, Organisation O) {

            List<DynamicField> Fields = dynamicFields_getFieldsForOrganisation(O, "", ref sql, false);

            string query = "";
            List<SQLColumnItem> colItems = new List<SQLColumnItem>();

            foreach (DynamicField f in Fields) {
                if (f.DataLink != "" && f.DataLink != "null" && f.OrganisationId == O.Id) {
                    string col = "";
                    if (f.DataSource.IndexOf('_') > 0) {
                        col = f.DataSource + "_" + f.DataSource.Split('_')[2];
                    } else {
                        col = f.DatabaseTable + "_" + f.DatabaseColumn + "_" + f.BaseOrganisationId;
                    }
                    query += "" + (query == "" ? "" : "    OR ") +
                                "       (convert(varchar(8000),[" + col + "]) LIKE '%' + @query + '%')\n";


                    //query += "    OR " +
                    //            "       ((@query LIKE '%' + convert(varchar(8000),[" + col + "]) + '%') AND convert(varchar(8000),isnull([" + col + "],''))<>'')\n";

                    //Add the columns desired in a sorted manor!
                    bool inserted = false;
                    int index = f.ListviewIndex;

                    if (colItems.Count == 0) {
                        colItems.Add(new SQLColumnItem(col, f.ListviewIndex));
                    } else {
                        for (int ii = 0; ii < colItems.Count; ii++) {
                            int cpIndex = colItems[ii].ListIndex;
                            if (inserted == false && index < cpIndex) {
                                inserted = true;
                                colItems.Insert(ii > colItems.Count ? colItems.Count : ii, new SQLColumnItem(col, f.ListviewIndex));
                            }
                        }

                        if (inserted == false)
                            colItems.Add(new SQLColumnItem(col, f.ListviewIndex));
                    }

                }
            }
            if (query != "") {
                query = "AND (\n" + query + "\n)";
            }

            string txtSQL = "" +
                            "CREATE PROCEDURE [dbo].[z_Companies_LocateDoubles_" + O.Id + "]\n" +
                            "   @organisationId int,\n" +
                            "   @userId uniqueidentifier,\n" +
                            "   @query varchar(500)='',\n" +
                            "   @type int=0,\n" +
                            "   @notThisId int=null\n" +
                            "AS\n" +
                            "   SELECT DISTINCT\n" +
                            "       CompanyId,\n" +
                            "       CompanyOrganisationId,\n" +
                            "       CompanyCreatedById,\n" +
                            "       CompanyDateStamp,\n" +
                            "       CompanyType," +
                            "        CompanyDeletedDate,\n" +
                            "        CompanyDeletedBy,\n" +
                            "        CompanyLastUpdated,CompanyAbandonedDate, CompanyAbandonedBy,\n" +
                            "        CompanyLastUpdatedBy,\n" +
                            "       CompanyNNEId\n" +
                            "\n" +
                            SQLColumnItemsToString(colItems) +
                            "   FROM\n" +
                            "       [dbo].[ContactsAndCompaniesList_" + O.Id + "]()\n" +
                            "   WHERE\n" +
                            "       companyType=@type and\n" +
                            "       (companyId<>@notThisId OR @notThisId is null) and\n" +
                            "       contactType=@type\n" +
                            "   -- SPECIFIC CONTACT TYPE? POT/SMV/Dont care\n" +
                            query +
                            "\n" +
                            "\n" +
                            "\n";


            //EXECUTE SQL!!!
            if (txtSQL != "" && txtSQL != null) {
                //(RE)CREATE THE PROCEDURE!!!
                sql.dynamicFields_dropStoredProcedure("z_Companies_LocateDoubles_" + O.Id);
                sql.dynamicFields_updateStoredProcedure(txtSQL);
            }
        }

        public static void rf_ContactsGetArgumentsAndColumns(ref string[] args, ref SQLDB sql, Organisation O) {

            string update = "";
            string insert = "";

            string CP = (TypeCast.ToInt(O.ParentId) > 0 ? "_" + TypeCast.ToInt(O.ParentId) : "");
            List<TableColumn> TableColumns = sql.dynamicFields_getTableColumns("z_contacts_" + O.Id);


            //UPDATE (STARTING)
            update += "" +
                        "   DECLARE @ContactId_" + O.Id + " INT\n" +
                        "   SET @ContactId_" + O.Id + " = (SELECT TOP 1 ContactId_" + O.Id + " FROM z_contacts_" + O.Id + " WHERE z_contacts_" + O.Id + ".ContactParentId_" + O.Id + "=@ContactId" + CP + ")\n" +
                        "\n" +
                        "   UPDATE z_contacts_" + O.Id + " SET\n" +
                        "       ContactParentId_" + O.Id + "=@ContactId" + CP;
            ////

            //INSERT (STARTING)
            insert += "" +
                        "   IF(@@ROWCOUNT=0) BEGIN\n" +
                        "       INSERT INTO z_contacts_" + O.Id + "\n" +
                        "           SELECT\n" +
                        "               @ContactId" + CP;


            for (int i = 2; i < TableColumns.Count; i++) {
                TableColumn T = TableColumns[i];

                //ARGUMENTS FOR THE SPROC
                args[0] += ",\n   @" + TypeCast.PrepareArgument(T.Name) +
                            " " + T.DataType +
                            (T.DataType == "varchar" || T.DataType == "nvarchar" ? "(" + (T.Length < 1 ? "MAX" : T.Length.ToString()) + ")" : "") + " = null";

                //UPDATE
                update += "" +
                            ",\n       [" + T.Name + "]=@" + TypeCast.PrepareArgument(T.Name);


                //INSERT
                insert += "" +
                            ",\n               @" + TypeCast.PrepareArgument(T.Name);
            }

            //UPDATE (ENDING)
            update += "" +
                        "\n   WHERE\n" +
                        "       ContactId_" + O.Id + "=@ContactId_" + O.Id + "\n";
            ////


            //INSERT (ENDING)
            insert += "" +
                        "\n" +
                        "       SET @ContactId_" + O.Id + "=@@IDENTITY\n" +
                        "   END\n";



            args[1] = update + "\n\n" + insert + "\n\n\n\n" + args[1];
            args[2] = "";


            if (TypeCast.ToInt(O.ParentId) > 0) {
                O = sql.Organisations_getOrganisation(TypeCast.ToInt(O.ParentId));
                if (O != null)
                    rf_ContactsGetArgumentsAndColumns(ref args, ref sql, O);
            }


        }

        public static void rf_CompaniesGetArgumentsAndColumns(ref string[] args, ref SQLDB sql, Organisation O) {

            string update = "";
            string insert = "";

            string CP = (TypeCast.ToInt(O.ParentId) > 0 ? "_" + TypeCast.ToInt(O.ParentId) : "");
            List<TableColumn> TableColumns = sql.dynamicFields_getTableColumns("z_companies_" + O.Id);


            //UPDATE (STARTING)
            update += "" +
                        "   DECLARE @CompanyId_" + O.Id + " INT\n" +
                        "   SET @CompanyId_" + O.Id + " = (SELECT TOP 1 CompanyId_" + O.Id + " FROM z_companies_" + O.Id + " WHERE z_companies_" + O.Id + ".CompanyParentId_" + O.Id + "=@CompanyId" + CP + ")\n" +
                        "\n" +
                        "   UPDATE z_companies_" + O.Id + " SET\n" +
                        "       CompanyParentId_" + O.Id + "=@CompanyId" + CP;
            ////

            //INSERT (STARTING)
            insert += "" +
                        "   IF(@@ROWCOUNT=0) BEGIN\n" +
                        "       INSERT INTO z_companies_" + O.Id + "\n" +
                        "           SELECT\n" +
                        "               @CompanyId" + CP;


            for (int i = 2; i < TableColumns.Count; i++) {
                TableColumn T = TableColumns[i];

                //ARGUMENTS FOR THE SPROC
                args[0] += ",\n   @" + TypeCast.PrepareArgument(T.Name) +
                            " " + T.DataType +
                            (T.DataType == "varchar" || T.DataType == "nvarchar" ? "(" + (T.Length < 1 ? "MAX" : T.Length.ToString()) + ")" : "") + " = null";

                //UPDATE
                update += "" +
                            ",\n       [" + T.Name + "]=@" + TypeCast.PrepareArgument(T.Name);


                //INSERT
                insert += "" +
                            ",\n               @" + TypeCast.PrepareArgument(T.Name);
            }

            //UPDATE (ENDING)
            update += "" +
                        "\n   WHERE\n" +
                        "       CompanyId_" + O.Id + "=@CompanyId_" + O.Id + "\n";
            ////


            //INSERT (ENDING)
            insert += "" +
                        "\n" +
                        "       SET @CompanyId_" + O.Id + "=@@IDENTITY\n" +
                        "   END\n";



            args[1] = update + "\n\n" + insert + "\n\n\n\n" + args[1];
            args[2] = "";


            if (TypeCast.ToInt(O.ParentId) > 0) {
                O = sql.Organisations_getOrganisation(TypeCast.ToInt(O.ParentId));
                if (O != null)
                    rf_CompaniesGetArgumentsAndColumns(ref args, ref sql, O);
            }


        }

        //REFACTOR GET CONTACTS FROM COMPANY STATEMENT
        public static void rf_ContactsGetContacts_FromCompany(ref SQLDB sql, Organisation O) {

            List<DynamicField> Fields = dynamicFields_getFieldsForOrganisation(O, "contacts", ref sql, true);

            List<SQLColumnItem> colItems = new List<SQLColumnItem>();

            foreach (DynamicField f in Fields) {
                if (TypeCast.ToInt(f.ShowInListview) > 0 && f.OrganisationId == O.Id) {
                    string col = "";
                    if (f.DataSource.IndexOf('_') > 0) {
                        col = f.DataSource + "_" + f.DataSource.Split('_')[2];
                    } else {
                        col = f.DatabaseTable + "_" + f.DatabaseColumn + "_" + f.BaseOrganisationId;
                    }

                    //Add the columns desired in a sorted manor!
                    bool inserted = false;
                    int index = f.ListviewIndex;

                    if (colItems.Count == 0) {
                        colItems.Add(new SQLColumnItem(col, f.ListviewIndex));
                    } else {
                        for (int ii = 0; ii < colItems.Count; ii++) {
                            int cpIndex = colItems[ii].ListIndex;
                            if (inserted == false && index < cpIndex) {
                                inserted = true;
                                colItems.Insert(ii > colItems.Count ? colItems.Count : ii, new SQLColumnItem(col, f.ListviewIndex));
                            }
                        }

                        if (inserted == false)
                            colItems.Add(new SQLColumnItem(col, f.ListviewIndex));
                    }
                }
            }

            string txtSQL = "" +
                            "CREATE PROCEDURE [dbo].[z_Contacts_Get_Contacts_FromCompany_" + O.Id + "]\n" +
                            "   @companyId int\n" +
                            "AS\n" +
                            "   SELECT\n" +
                            "        ContactId,\n" +
                            "        CompanyOwnerId,\n" +
                            "        ContactOrganisationId,\n" +
                            "        ContactCreatedById,\n" +
                            "        ContactDateStamp,\n" +
                            "        ContactType,\n" +
                            "        CompanyDeletedDate,\n" +
                            "        CompanyDeletedBy,\n" +
                            "        ContactDeletedDate,\n" +
                            "        ContactDeletedBy,\n" +
                            "        CompanyLastUpdated,\n" +
                            "        CompanyLastUpdatedBy, CompanyAbandonedDate, CompanyAbandonedBy,\n" +
                            "        ContactLastUpdated,\n" +
                            "        ContactLastUpdatedBy, ContactAbandonedDate, ContactAbandonedBy\n" +
                            "\n" +
                            SQLColumnItemsToString(colItems) +
                            "   FROM\n" +
                            "       [dbo].[ContactsAndCompaniesList_" + O.Id + "]()\n" +
                            "   WHERE \n" +
                            "       CompanyOwnerId=@companyId\n" +
                            "\n" +
                            "\n" +
                            "\n" +
                            "\n" +
                            "\n" +
                            "\n";

            //EXECUTE SQL!!!
            if (txtSQL != "" && txtSQL != null) {
                //(RE)CREATE THE PROCEDURE!!!
                sql.dynamicFields_dropStoredProcedure("z_Contacts_Get_Contacts_FromCompany_" + O.Id);
                sql.dynamicFields_updateStoredProcedure(txtSQL);
            }
        }

        //REFACTOR GET CONTACTS FROM MAILGROUP
        public static void rf_ContactsGetContacts_FromMailGroup(ref SQLDB sql, Organisation O) {

            List<DynamicField> Fields = dynamicFields_getFieldsForOrganisation(O, "contacts", ref sql, true);

            List<SQLColumnItem> colItems = new List<SQLColumnItem>();

            foreach (DynamicField f in Fields) {
                if (TypeCast.ToInt(f.ShowInListview) > 0 && f.OrganisationId == O.Id) {
                    string col = "";
                    if (f.DataSource.IndexOf('_') > 0) {
                        col = f.DataSource + "_" + f.DataSource.Split('_')[2];
                    } else {
                        col = f.DatabaseTable + "_" + f.DatabaseColumn + "_" + f.BaseOrganisationId;
                    }

                    //Add the columns desired in a sorted manor!
                    bool inserted = false;
                    int index = f.ListviewIndex;

                    if (colItems.Count == 0) {
                        colItems.Add(new SQLColumnItem(col, f.ListviewIndex));
                    } else {
                        for (int ii = 0; ii < colItems.Count; ii++) {
                            int cpIndex = colItems[ii].ListIndex;
                            if (inserted == false && index < cpIndex) {
                                inserted = true;
                                colItems.Insert(ii > colItems.Count ? colItems.Count : ii, new SQLColumnItem(col, f.ListviewIndex));
                            }
                        }

                        if (inserted == false)
                            colItems.Add(new SQLColumnItem(col, f.ListviewIndex));
                    }
                }
            }

            string txtSQL = "" +
                            "CREATE PROCEDURE [dbo].[z_Contacts_Get_Contacts_FromMailGroup_" + O.Id + "]\n" +
                            "   @mailgroupId int\n" +
                            "AS\n" +
                            "   SELECT\n" +
                            "        ContactId,\n" +
                            "        CompanyOwnerId,\n" +
                            "        ContactOrganisationId,\n" +
                            "        ContactCreatedById,\n" +
                            "        ContactDateStamp,\n" +
                            "        ContactType,\n" +
                            "        CompanyDeletedDate,\n" +
                            "        CompanyDeletedBy,\n" +
                            "        ContactDeletedDate,\n" +
                            "        ContactDeletedBy,\n" +
                            "        CompanyLastUpdated, CompanyAbandonedDate, CompanyAbandonedBy,\n" +
                            "        CompanyLastUpdatedBy,\n" +
                            "        ContactLastUpdated,\n" +
                            "        ContactLastUpdatedBy, ContactAbandonedDate, ContactAbandonedBy\n" +
                            "\n" +
                            SQLColumnItemsToString(colItems) +
                            "   FROM\n" +
                            "       [dbo].[ContactsAndCompaniesList_" + O.Id + "]()\n" +
                            "   WHERE \n" +
                            "       ContactId IN (SELECT CONTACTID FROM Mailgroup_Contacts WHERE MailgroupId=@mailgroupId)\n" +
                            "\n" +
                            "\n" +
                            "\n" +
                            "\n" +
                            "\n" +
                            "\n";

            //EXECUTE SQL!!!
            if (txtSQL != "" && txtSQL != null) {
                //(RE)CREATE THE PROCEDURE!!!
                sql.dynamicFields_dropStoredProcedure("z_Contacts_Get_Contacts_FromMailGroup_" + O.Id);
                sql.dynamicFields_updateStoredProcedure(txtSQL);
            }
        }

        //REFACTOR GET CONTACTS FROM PARTNER
        public static void rf_ContactsGetContact_ForPartner(ref SQLDB sql, Organisation O) {
            List<DynamicField> Fields = dynamicFields_getFieldsForOrganisation(O, ref sql, true);

            List<SQLColumnItem> colItems = new List<SQLColumnItem>();

            colItems.Add(new SQLColumnItem("", 0));
            colItems.Add(new SQLColumnItem("", 0));
            colItems.Add(new SQLColumnItem("", 0));

            foreach (DynamicField f in Fields) {
                if (
                       (
                        f.DataLink == "DGSContactFirstname" ||
                        f.DataLink == "DGSCOntactLastname" ||
                        (f.DataLink == "NNECompanyName" && f.BaseOrganisationId == 1) //** Ekstra check Favrskov
                       ) && f.OrganisationId == O.Id) {
                    string col = "";
                    if (f.DataSource.IndexOf('_') > 0) {
                        col = f.DataSource + "_" + f.DataSource.Split('_')[2];
                    } else {
                        col = f.DatabaseTable + "_" + f.DatabaseColumn + "_" + f.BaseOrganisationId;
                    }
                    //Add the columns desired in a sorted manor!
                    switch (f.DataLink) {
                        case "DGSContactFirstname":
                            colItems[1] = new SQLColumnItem(col, f.ListviewIndex);
                            break;

                        case "DGSCOntactLastname":
                            colItems[2] = new SQLColumnItem(col, f.ListviewIndex);
                            break;

                        case "NNECompanyName":
                            colItems[0] = new SQLColumnItem(col, f.ListviewIndex);
                            break;


                    }
                }
            }

            string txtSQL = "" +
                            "CREATE PROCEDURE [dbo].[z_Contacts_GetContact_ForPartner_" + O.Id + "]\n" +
                            "   @ContactId int\n" +
                            "AS\n" +
                            "   SELECT\n" +
                            "        ContactId,\n" +
                            "        CompanyOwnerId,\n" +
                            "        ContactOrganisationId,\n" +
                            "        ContactCreatedById,\n" +
                            "        ContactDateStamp,\n" +
                            "        ContactType,\n" +
                            "        CompanyDeletedDate,\n" +
                            "        CompanyDeletedBy,\n" +
                            "        ContactDeletedDate,\n" +
                            "        ContactDeletedBy,\n" +
                            "        CompanyLastUpdated,\n" +
                            "        CompanyLastUpdatedBy, CompanyAbandonedDate, CompanyAbandonedBy,\n" +
                            "        ContactLastUpdated,\n" +
                            "        ContactLastUpdatedBy, ContactAbandonedDate, ContactAbandonedBy\n" +
                            "\n" +
                            SQLColumnItemsToString(colItems) +
                            "   FROM\n" +
                            "       [dbo].[ContactsAndCompaniesList_" + O.Id + "]()\n" +
                            "   WHERE \n" +
                            "       ContactId = @ContactId\n" +
                            "\n" +
                            "\n" +
                            "\n" +
                            "\n" +
                            "\n" +
                            "\n";

            //EXECUTE SQL!!!
            if (txtSQL != "" && txtSQL != null) {
                //(RE)CREATE THE PROCEDURE!!!
                sql.dynamicFields_dropStoredProcedure("z_Contacts_GetContact_ForPartner_" + O.Id);
                sql.dynamicFields_updateStoredProcedure(txtSQL);
            }
        }

        //REFACTOR GET CONTACTS FROM PARTNER
        public static void rf_ContactsGetContacts_FromPartner_AllColumns(ref SQLDB sql, Organisation O) {
            List<DynamicField> Fields = dynamicFields_getFieldsForOrganisation(O, "contacts", ref sql, true);

            List<SQLColumnItem> colItems = new List<SQLColumnItem>();

            colItems.Add(new SQLColumnItem("", 0));
            colItems.Add(new SQLColumnItem("", 0));
            colItems.Add(new SQLColumnItem("", 0));

            foreach (DynamicField f in Fields) {
                if (TypeCast.ToInt(f.ShowInListview) > 0 && f.OrganisationId == O.Id) {
                    string col = "";
                    if (f.DataSource.IndexOf('_') > 0) {
                        col = f.DataSource + "_" + f.DataSource.Split('_')[2];
                    } else {
                        col = f.DatabaseTable + "_" + f.DatabaseColumn + "_" + f.BaseOrganisationId;
                    }

                    //Add the columns desired in a sorted manor!
                    bool inserted = false;
                    int index = f.ListviewIndex;

                    if (colItems.Count == 0) {
                        colItems.Add(new SQLColumnItem(col, f.ListviewIndex));
                    } else {
                        for (int ii = 0; ii < colItems.Count; ii++) {
                            int cpIndex = colItems[ii].ListIndex;
                            if (inserted == false && index < cpIndex) {
                                inserted = true;
                                colItems.Insert(ii > colItems.Count ? colItems.Count : ii, new SQLColumnItem(col, f.ListviewIndex));
                            }
                        }

                        if (inserted == false)
                            colItems.Add(new SQLColumnItem(col, f.ListviewIndex));
                    }


                }
            }


            string txtSQL = "" +
                            "CREATE PROCEDURE [dbo].[z_Contacts_Get_Contacts_FromPartner_AllColumns_" + O.Id + "]\n" +
                            "   @PartnerId int\n" +
                            "AS\n" +
                            "   SELECT\n" +
                            "        cts.ContactId,\n" +
                            "        cts.CompanyOwnerId,\n" +
                            "        cts.ContactOrganisationId,\n" +
                            "        cts.ContactCreatedById,\n" +
                            "        cts.ContactDateStamp,\n" +
                            "        cts.ContactType,\n" +
                            "        cts.CompanyDeletedDate,\n" +
                            "        cts.CompanyDeletedBy,\n" +
                            "        cts.ContactDeletedDate,\n" +
                            "        cts.ContactDeletedBy,\n" +
                            "        cts.CompanyLastUpdated,\n" +
                            "        cts.CompanyLastUpdatedBy, CompanyAbandonedDate, CompanyAbandonedBy,\n" +
                            "        cts.ContactLastUpdated,\n" +
                            "        cts.ContactLastUpdatedBy, ContactAbandonedDate, ContactAbandonedBy,\n" +
                            "\n" +
                            "       Contact_Partners.DateStamp as Relation_DateStamp,\n" +
                            "       Contact_Partners.IsMentor as Relation_IsMentor,\n" +
                            "       Contact_Partners.IsRedirect as Relation_IsRedirect,\n" +
                            "       Contact_Partners.IsCooporative as Relation_IsCooporative,\n" +
                            "       Contact_Partners.CreatedBy as Relation_CreatedBy,\n" +
                            "       Contact_Partners.FinalcialPoolId as Relation_FinalcialPoolId,\n" +
                            "       Contact_Partners.FinalcialPool as Relation_FinalcialPool,\n" +
                            "       Contact_Partners.FinancialPoolAmount as Relation_FinalcialPoolAmount,\n" +
                            "       [dbo].[getUsername](users.id) as Relation_username,\n" +
                            "       Contact_Partners.Id as ContactPartnerRelationId\n" +
                            "\n" +
                            SQLColumnItemsToString(colItems) +
                            "\n" +
                            "   FROM\n" +
                            "       [dbo].[ContactsAndCompaniesList_" + O.Id + "]() as cts,\n" +
                            "       Contact_Partners\n" +
                            "       left join users on Contact_Partners.createdBy = users.id\n" +
                            "   WHERE \n" +
                            "       cts.ContactId = Contact_Partners.ContactId and\n" +
                            "       Contact_Partners.PartnerId=@PartnerId and @partnerId>0 \n" +
                            "\n" +
                            "\n" +
                            "\n" +
                            "\n" +
                            "\n";

            //EXECUTE SQL!!!
            if (txtSQL != "" && txtSQL != null) {
                //(RE)CREATE THE PROCEDURE!!!
                sql.dynamicFields_dropStoredProcedure("z_Contacts_Get_Contacts_FromPartner_AllColumns_" + O.Id);
                sql.dynamicFields_updateStoredProcedure(txtSQL);
            }
        }

        //REFACTOR GET CONTACTS SENT TO EVALUATION
        public static void rf_Contacts_Get_Contacts_SentToEvaluation_AllColumns_EntireYear(ref SQLDB sql, Organisation O) {
            List<DynamicField> Fields = dynamicFields_getFieldsForOrganisation(O, "contacts", ref sql, true);

            List<SQLColumnItem> colItems = new List<SQLColumnItem>();

            colItems.Add(new SQLColumnItem("", 0));
            colItems.Add(new SQLColumnItem("", 0));
            colItems.Add(new SQLColumnItem("", 0));

            foreach (DynamicField f in Fields) {
                if (TypeCast.ToInt(f.ShowInListview) > 0 && f.OrganisationId == O.Id) {
                    string col = "";
                    if (f.DataSource.IndexOf('_') > 0) {
                        col = f.DataSource + "_" + f.DataSource.Split('_')[2];
                    } else {
                        col = f.DatabaseTable + "_" + f.DatabaseColumn + "_" + f.BaseOrganisationId;
                    }

                    //Add the columns desired in a sorted manor!
                    bool inserted = false;
                    int index = f.ListviewIndex;

                    if (colItems.Count == 0) {
                        colItems.Add(new SQLColumnItem(col, f.ListviewIndex));
                    } else {
                        for (int ii = 0; ii < colItems.Count; ii++) {
                            int cpIndex = colItems[ii].ListIndex;
                            if (inserted == false && index < cpIndex) {
                                inserted = true;
                                colItems.Insert(ii > colItems.Count ? colItems.Count : ii, new SQLColumnItem(col, f.ListviewIndex));
                            }
                        }

                        if (inserted == false)
                            colItems.Add(new SQLColumnItem(col, f.ListviewIndex));
                    }


                }
            }


            string txtSQL = "" +
                            "CREATE PROCEDURE [dbo].[z_Contacts_Get_Contacts_SentToEvaluation_AllColumns_EntireYear_" + O.Id + "]\n" +
                            "   @year int,\n" +
                            "   @organisationId int,\n" +
                            "   @userId uniqueidentifier = null\n" +
                            "AS\n" +
                            "   SELECT\n" +
                            "       contactsToEvaluation.Datestamp as evaluationDate,\n" +
                            "       contactsToEvaluation.exported as exportedDate,\n" +
                            "       contactsToEvaluation.userId as evaluatedByUserId,\n" +
                            "       users.username as evaluatedByUsername,\n" +
                            "       contactsToEvaluation.HasRedirect  as HasRedirect,\n" +
                            "       contactsToEvaluation.HasValidRedirect   as HasValidRedirect,\n" +
                            "\n" +
                            "        cta.ContactId,\n" +
                            "        cta.CompanyOwnerId,\n" +
                            "        cta.ContactOrganisationId,\n" +
                            "        cta.ContactCreatedById,\n" +
                            "        cta.ContactDateStamp,\n" +
                            "        cta.ContactType,\n" +
                            "        cta.CompanyDeletedDate,\n" +
                            "        cta.CompanyDeletedBy,\n" +
                            "        cta.ContactDeletedDate,\n" +
                            "        cta.ContactDeletedBy,\n" +
                            "        cta.CompanyLastUpdated,\n" +
                            "        cta.CompanyLastUpdatedBy, CompanyAbandonedDate, CompanyAbandonedBy,\n" +
                            "        cta.ContactLastUpdated,\n" +
                            "        cta.ContactLastUpdatedBy, ContactAbandonedDate, ContactAbandonedBy\n" +
                            "\n" +
                            SQLColumnItemsToString(colItems) +
                            "\n" +
                            "   FROM\n" +
                            "       dbo.getContactsToEvaluation() as contactsToEvaluation,\n" +
                            "       ContactsAndCompaniesAnyList_" + O.Id + "() cta,\n" +
                            "       users\n" +
                            "   WHERE \n" +
                            "       (contactsToEvaluation.userId=@userId or @userId is null) and\n" +
                            "       contactsToEvaluation.organisationId=@organisationId and\n" +

                            "       --datestamp>=@ds and datestamp<@de and \n" +
                            "       year(datestamp)=@year and \n" +

                            "       cta.contactId=contactsToEvaluation.contactId and\n" +
                            "       --cta.companyId=contactsToEvaluation.companyId and\n" +
                            "       users.id = contactsToEvaluation.userId\n" +
                            "   order by\n" +
                            "       users.username\n" +
                            "\n";

            //EXECUTE SQL!!!
            if (txtSQL != "" && txtSQL != null) {
                //(RE)CREATE THE PROCEDURE!!!
                sql.dynamicFields_dropStoredProcedure("z_Contacts_Get_Contacts_SentToEvaluation_AllColumns_EntireYear_" + O.Id);
                sql.dynamicFields_updateStoredProcedure(txtSQL);
            }
        }
        public static void rf_Contacts_Get_Contacts_SentToLocalEvaluation_AllColumns_EntireYear(ref SQLDB sql, Organisation O) {
            List<DynamicField> Fields = dynamicFields_getFieldsForOrganisation(O, "contacts", ref sql, true);

            List<SQLColumnItem> colItems = new List<SQLColumnItem>();

            colItems.Add(new SQLColumnItem("", 0));
            colItems.Add(new SQLColumnItem("", 0));
            colItems.Add(new SQLColumnItem("", 0));

            foreach (DynamicField f in Fields) {
                if (TypeCast.ToInt(f.ShowInListview) > 0 && f.OrganisationId == O.Id) {
                    string col = "";
                    if (f.DataSource.IndexOf('_') > 0) {
                        col = f.DataSource + "_" + f.DataSource.Split('_')[2];
                    } else {
                        col = f.DatabaseTable + "_" + f.DatabaseColumn + "_" + f.BaseOrganisationId;
                    }

                    //Add the columns desired in a sorted manor!
                    bool inserted = false;
                    int index = f.ListviewIndex;

                    if (colItems.Count == 0) {
                        colItems.Add(new SQLColumnItem(col, f.ListviewIndex));
                    } else {
                        for (int ii = 0; ii < colItems.Count; ii++) {
                            int cpIndex = colItems[ii].ListIndex;
                            if (inserted == false && index < cpIndex) {
                                inserted = true;
                                colItems.Insert(ii > colItems.Count ? colItems.Count : ii, new SQLColumnItem(col, f.ListviewIndex));
                            }
                        }

                        if (inserted == false)
                            colItems.Add(new SQLColumnItem(col, f.ListviewIndex));
                    }


                }
            }


            string txtSQL = "" +
                            "CREATE PROCEDURE [dbo].[z_Contacts_Get_Contacts_SentToLocalEvaluation_AllColumns_EntireYear_" + O.Id + "]\n" +
                            "   @year int,\n" +
                            "   @organisationId int,\n" +
                            "   @userId uniqueidentifier = null\n" +
                            "AS\n" +
                            "   SELECT\n" +
                            "       contactsToLocalEvaluation.Datestamp as evaluationDate,\n" +
                            "       contactsToLocalEvaluation.exported as exportedDate,\n" +
                            "       contactsToLocalEvaluation.userId as evaluatedByUserId,\n" +
                            "       users.username as evaluatedByUsername,\n" +
                            "       contactsToLocalEvaluation.HasRedirect  as HasRedirect,\n" +
                            "       contactsToLocalEvaluation.HasValidRedirect   as HasValidRedirect,\n" +
                            "\n" +
                            "        cta.ContactId,\n" +
                            "        cta.CompanyOwnerId,\n" +
                            "        cta.ContactOrganisationId,\n" +
                            "        cta.ContactCreatedById,\n" +
                            "        cta.ContactDateStamp,\n" +
                            "        cta.ContactType,\n" +
                            "        cta.CompanyDeletedDate,\n" +
                            "        cta.CompanyDeletedBy,\n" +
                            "        cta.ContactDeletedDate,\n" +
                            "        cta.ContactDeletedBy,\n" +
                            "        cta.CompanyLastUpdated,\n" +
                            "        cta.CompanyLastUpdatedBy, CompanyAbandonedDate, CompanyAbandonedBy,\n" +
                            "        cta.ContactLastUpdated,\n" +
                            "        cta.ContactLastUpdatedBy, ContactAbandonedDate, ContactAbandonedBy\n" +
                            "\n" +
                            SQLColumnItemsToString(colItems) +
                            "\n" +
                            "   FROM\n" +
                            "       dbo.getContactsToLocalEvaluation() as contactsToLocalEvaluation,\n" +
                            "       ContactsAndCompaniesAnyList_" + O.Id + "() cta,\n" +
                            "       users\n" +
                            "   WHERE \n" +
                            "       (contactsToLocalEvaluation.userId=@userId or @userId is null) and\n" +
                            "       contactsToLocalEvaluation.organisationId=@organisationId and\n" +

                            "       --datestamp>=@ds and datestamp<@de and \n" +
                            "       year(datestamp)=@year and \n" +

                            "       cta.contactId=contactsToLocalEvaluation.contactId and\n" +
                            "       --cta.companyId=contactsToLocalEvaluation.companyId and\n" +
                            "       users.id = contactsToLocalEvaluation.userId\n" +
                            "   order by\n" +
                            "       users.username\n" +
                            "\n";

            //EXECUTE SQL!!!
            if (txtSQL != "" && txtSQL != null) {
                //(RE)CREATE THE PROCEDURE!!!
                sql.dynamicFields_dropStoredProcedure("z_Contacts_Get_Contacts_SentToLocalEvaluation_AllColumns_EntireYear_" + O.Id);
                sql.dynamicFields_updateStoredProcedure(txtSQL);
            }
        }
        public static void rf_ContactsGetContacts_FromPartner(ref SQLDB sql, Organisation O) {
            List<DynamicField> Fields = dynamicFields_getFieldsForOrganisation(O, ref sql, true);

            List<SQLColumnItem> colItems = new List<SQLColumnItem>();

            colItems.Add(new SQLColumnItem("", 0));
            colItems.Add(new SQLColumnItem("", 0));
            colItems.Add(new SQLColumnItem("", 0));

            foreach (DynamicField f in Fields) {
                if (
                       (
                        f.DataLink == "DGSContactFirstname" ||
                        f.DataLink == "DGSCOntactLastname" ||
                        (f.DataLink == "NNECompanyName" && f.BaseOrganisationId == 1) //** Ekstra check Favrskov
                       ) && f.OrganisationId == O.Id) {
                    string col = "";
                    if (f.DataSource.IndexOf('_') > 0) {
                        col = f.DataSource + "_" + f.DataSource.Split('_')[2];
                    } else {
                        col = f.DatabaseTable + "_" + f.DatabaseColumn + "_" + f.BaseOrganisationId;
                    }
                    //Add the columns desired in a sorted manor!
                    switch (f.DataLink) {
                        case "DGSContactFirstname":
                            colItems[1] = new SQLColumnItem(col, f.ListviewIndex);
                            break;

                        case "DGSCOntactLastname":
                            colItems[2] = new SQLColumnItem(col, f.ListviewIndex);
                            break;

                        case "NNECompanyName":
                            colItems[0] = new SQLColumnItem(col, f.ListviewIndex);
                            break;


                    }
                }
            }

            string txtSQL = "" +
                            "CREATE PROCEDURE [dbo].[z_Contacts_Get_Contacts_FromPartner_" + O.Id + "]\n" +
                            "   @PartnerId int\n" +
                            "AS\n" +
                            "   SELECT\n" +
                            "        cts.ContactId,\n" +
                            "        cts.CompanyOwnerId,\n" +
                            "        cts.ContactOrganisationId,\n" +
                            "        cts.ContactCreatedById,\n" +
                            "        cts.ContactDateStamp,\n" +
                            "        cts.ContactType,\n" +
                            "        cts.CompanyDeletedDate,\n" +
                            "        cts.CompanyDeletedBy,\n" +
                            "        cts.ContactDeletedDate,\n" +
                            "        cts.ContactDeletedBy,\n" +
                            "        cts.CompanyLastUpdated,\n" +
                            "        cts.CompanyLastUpdatedBy, CompanyAbandonedDate, CompanyAbandonedBy,\n" +
                            "        cts.ContactLastUpdated,\n" +
                            "        cts.ContactLastUpdatedBy, ContactAbandonedDate, ContactAbandonedBy,\n" +
                            "\n" +
                            "       Contact_Partners.DateStamp as Relation_DateStamp,\n" +
                            "       Contact_Partners.IsMentor as Relation_IsMentor,\n" +
                            "       Contact_Partners.IsRedirect as Relation_IsRedirect,\n" +
                            "       Contact_Partners.IsCooporative as Relation_IsCooporative,\n" +
                            "       Contact_Partners.CreatedBy as Relation_CreatedBy,\n" +
                            "       Contact_Partners.CreatedBy as Relation_FinalcialPool,\n" +
                            "       Contact_Partners.FinancialPoolAmount as Relation_FinalcialPoolAmount,\n" +
                            "       [dbo].[getUsername](users.id) as Relation_username,\n" +
                            "       Contact_Partners.Id as ContactPartnerRelationId\n" +
                            "\n" +
                            SQLColumnItemsToString(colItems) +
                            "\n" +
                            "   FROM\n" +
                            "       [dbo].[ContactsAndCompaniesList_" + O.Id + "]() as cts,\n" +
                            "       Contact_Partners\n" +
                            "       left join users on Contact_Partners.createdBy = users.id\n" +
                            "   WHERE \n" +
                            "       cts.ContactId = Contact_Partners.ContactId and\n" +
                            "       Contact_Partners.PartnerId=@PartnerId and @partnerId>0 \n" +
                            "\n" +
                            "\n" +
                            "\n" +
                            "\n" +
                            "\n";

            //EXECUTE SQL!!!
            if (txtSQL != "" && txtSQL != null) {
                //(RE)CREATE THE PROCEDURE!!!
                sql.dynamicFields_dropStoredProcedure("z_Contacts_Get_Contacts_FromPartner_" + O.Id);
                sql.dynamicFields_updateStoredProcedure(txtSQL);
            }
        }

        //REFACTOR GET CONTACT STATEMENT
        public static void rf_ContactsGetContact(ref SQLDB sql, Organisation O) {
            string txtSQL = "" +
                            "CREATE PROCEDURE [dbo].[z_Contacts_Get_" + O.Id + "]\n" +
                            "   @contactId int\n" +
                            "AS\n" +
                            "   SELECT\n" +
                            "       *\n" +
                            "   FROM\n" +
                            "       [dbo].[Contacts_" + O.Id + "]() as Contacts\n" +
                            "   WHERE \n" +
                            "       Contacts.ContactId=@contactId\n" +
                            "\n" +
                            "\n" +
                            "\n" +
                            "\n" +
                            "\n" +
                            "\n";

            //EXECUTE SQL!!!
            if (txtSQL != "" && txtSQL != null) {
                //(RE)CREATE THE PROCEDURE!!!
                sql.dynamicFields_dropStoredProcedure("z_Contacts_Get_" + O.Id);
                sql.dynamicFields_updateStoredProcedure(txtSQL);
            }
        }

        //REFACTOR GET CONTACT STATEMENT
        public static void rf_ContactsGetContactsFromCompanyAllOrganisationFields(ref SQLDB sql, Organisation O) {
            string txtSQL = "" +
                            "CREATE PROCEDURE [dbo].[z_Contacts_GetContactsFromCompanyAllOrganisationFields_" + O.Id + "]\n" +
                            "   @companyId int\n" +
                            "AS\n" +
                            "   SELECT\n" +
                            "       *\n" +
                            "   FROM\n" +
                            "       [dbo].[ContactsAllOrganisations_" + O.Id + "]() as Contacts\n" +
                            "   WHERE \n" +
                            "       Contacts.CompanyOwnerId=@companyId\n" +
                            "\n" +
                            "\n" +
                            "\n" +
                            "\n" +
                            "\n" +
                            "\n";

            //EXECUTE SQL!!!
            if (txtSQL != "" && txtSQL != null) {
                //(RE)CREATE THE PROCEDURE!!!
                sql.dynamicFields_dropStoredProcedure("z_Contacts_GetContactsFromCompanyAllOrganisationFields_" + O.Id);
                sql.dynamicFields_updateStoredProcedure(txtSQL);
            }
        }

        //REFACTOR GET CONTACT STATEMENT
        public static void rf_ContactsGetContactAllOrganisationFields(ref SQLDB sql, Organisation O) {
            string txtSQL = "" +
                            "CREATE PROCEDURE [dbo].[z_Contacts_GetAllOrganisationFields_" + O.Id + "]\n" +
                            "   @contactId int\n" +
                            "AS\n" +
                            "   SELECT\n" +
                            "       *\n" +
                            "   FROM\n" +
                            "       [dbo].[ContactsAllOrganisations_" + O.Id + "]() as Contacts\n" +
                            "   WHERE \n" +
                            "       Contacts.ContactId=@contactId\n" +
                            "\n" +
                            "\n" +
                            "\n" +
                            "\n" +
                            "\n" +
                            "\n";

            //EXECUTE SQL!!!
            if (txtSQL != "" && txtSQL != null) {
                //(RE)CREATE THE PROCEDURE!!!
                sql.dynamicFields_dropStoredProcedure("z_Contacts_GetAllOrganisationFields_" + O.Id);
                sql.dynamicFields_updateStoredProcedure(txtSQL);
            }
        }

        //REFACTOR GET CONTACT FROM STAGING STATEMENT
        public static void rf_ContactsGetContactFromStaging(ref SQLDB sql, Organisation O) {
            string txtSQL = "" +
                            "CREATE PROCEDURE [dbo].[z_Contacts_Get_FromStaging_" + O.Id + "]\n" +
                            "   @contactId int\n" +
                            "AS\n" +
                            "   SELECT\n" +
                            "       *\n" +
                            "   FROM\n" +
                            "       [dbo].[ContactsStaging_" + O.Id + "]() as Contacts\n" +
                            "   WHERE \n" +
                            "       Contacts.ContactId=@contactId\n" +
                            "\n" +
                            "\n" +
                            "\n" +
                            "\n" +
                            "\n" +
                            "\n";

            //EXECUTE SQL!!!
            if (txtSQL != "" && txtSQL != null) {
                //(RE)CREATE THE PROCEDURE!!!
                sql.dynamicFields_dropStoredProcedure("z_Contacts_Get_FromStaging_" + O.Id);
                sql.dynamicFields_updateStoredProcedure(txtSQL);
            }
        }

        //REFACTOR GET COMPANY STATEMENT
        public static void rf_CompanyGetCompany(ref SQLDB sql, Organisation O) {
            string txtSQL = "" +
                            "CREATE PROCEDURE [dbo].[z_Companies_Get_" + O.Id + "]\n" +
                            "   @companyId int\n" +
                            //"   , @Log       xml=null\n" +
                            "AS\n" +
                            "   SELECT\n" +
                            "       *\n" +
                            "   FROM\n" +
                            "       [dbo].[Companies_" + O.Id + "]() as Companies\n" +
                            "   WHERE \n" +
                            "       Companies.CompanyId=@companyId\n" +
                            "\n" +
                            "\n" +
                            "\n" +
                            "\n" +
                            "\n" +
                            "\n";
            //"-- Log information - 20170904\n" +
            //" EXEC [LogEntry_Add] @Log=@Log   ";

            //EXECUTE SQL!!!
            if (txtSQL != "" && txtSQL != null) {
                //(RE)CREATE THE PROCEDURE!!!
                sql.dynamicFields_dropStoredProcedure("z_Companies_Get_" + O.Id);
                sql.dynamicFields_updateStoredProcedure(txtSQL);
            }
        }

        //REFACTOR GET COMPANY STATEMENT FROM STAGING
        public static void rf_CompanyGetCompanyFromStaging(ref SQLDB sql, Organisation O) {
            string txtSQL = "" +
                            "CREATE PROCEDURE [dbo].[z_Companies_Get_FromStaging_" + O.Id + "]\n" +
                            "   @companyId int\n" +
                            "AS\n" +
                            "   SELECT\n" +
                            "       *\n" +
                            "   FROM\n" +
                            "       [dbo].[CompaniesStaging_" + O.Id + "]() as Companies\n" +
                            "   WHERE \n" +
                            "       Companies.CompanyId=@companyId\n" +
                            "\n" +
                            "\n" +
                            "\n" +
                            "\n" +
                            "\n" +
                            "\n";

            //EXECUTE SQL!!!
            if (txtSQL != "" && txtSQL != null) {
                //(RE)CREATE THE PROCEDURE!!!
                sql.dynamicFields_dropStoredProcedure("z_Companies_Get_FromStaging_" + O.Id);
                sql.dynamicFields_updateStoredProcedure(txtSQL);
            }
        }


        public static void rf_GetContactsWithSameEmail(ref SQLDB sql, Organisation O) {
            List<DynamicField> Fields = null; // dynamicFields_getFieldsForOrganisation(O);
            List<SQLColumnItem> colItems = new List<SQLColumnItem>();

            //Add empty items
            colItems.Add(new SQLColumnItem());
            colItems.Add(new SQLColumnItem());
            colItems.Add(new SQLColumnItem());

            Fields = dynamicFields_getFieldsForOrganisation(O, "", ref sql, false);

            string colEmail = "";


            foreach (DynamicField f in Fields) {
                if (
                       (
                        f.DataLink == "DGSEmail"
                       ) && f.OrganisationId == O.Id) {
                    if (f.DataSource.IndexOf('_') > 0) {
                        colEmail = f.DataSource + "_" + f.DataSource.Split('_')[2];
                    } else {
                        colEmail = f.DatabaseTable + "_" + f.DatabaseColumn + "_" + f.BaseOrganisationId;
                    }
                } else if (
                         (
                          f.DataLink == "DGSContactFirstname" ||
                          f.DataLink == "DGSCOntactLastname" ||
                          (f.DataLink == "NNECompanyName" && f.BaseOrganisationId == 1) //** Ekstra check Favrskov
                         ) && f.OrganisationId == O.Id) {
                    string col = "";
                    if (f.DataSource.IndexOf('_') > 0) {
                        col = f.DataSource + "_" + f.DataSource.Split('_')[2];
                    } else {
                        col = f.DatabaseTable + "_" + f.DatabaseColumn + "_" + f.BaseOrganisationId;
                    }
                    //Add the columns desired in a sorted manor!

                    switch (f.DataLink) {
                        case "DGSContactFirstname":
                            colItems[1] = new SQLColumnItem(col, f.ListviewIndex);
                            break;

                        case "DGSCOntactLastname":
                            colItems[2] = new SQLColumnItem(col, f.ListviewIndex);
                            break;

                        case "NNECompanyName":
                            colItems[0] = new SQLColumnItem(col, f.ListviewIndex);
                            break;


                    }
                }
            }

            if (colEmail != "") {

                string txtSQL = "" +
                                "CREATE PROCEDURE [dbo].[z_GetContactsWithSameEmail_" + O.Id + "]\n" +
                                "   @contactId int=0,\n" +
                                "   @email varchar(250)\n" +
                                "AS\n" +
                                "   IF(@contactId is null) set @contactId=0\n\n" +
                                "   SELECT\n" +
                                "       ContactId,\n" +
                                "       ContactType\n" +
                                SQLColumnItemsToString(colItems) +
                                "   FROM\n" +
                                "       [dbo].[ContactsAndCompaniesList_" + O.Id + "]()\n" +
                                "   WHERE \n" +
                                "       ContactId<>@ContactId and\n" +
                                "       [" + colEmail + "] = @email and\n" +
                                "       @email <> '' and @email is not null\n" +
                                "\n" +
                                "\n" +
                                "\n" +
                                "\n";

                //EXECUTE SQL!!!
                if (txtSQL != "" && txtSQL != null) {
                    //(RE)CREATE THE PROCEDURE!!!
                    sql.dynamicFields_dropStoredProcedure("z_GetContactsWithSameEmail_" + O.Id);
                    sql.dynamicFields_updateStoredProcedure(txtSQL);
                }
            }
        }


        //REFACTOR GET CONTACTS WITH SAME CPR
        public static void rf_GetContactsWithSameCPR(ref SQLDB sql, Organisation O) {
            List<DynamicField> Fields = null; // dynamicFields_getFieldsForOrganisation(O);

            Fields = dynamicFields_getFieldsForOrganisation(O, "", ref sql, false);

            string col = "";
            foreach (DynamicField f in Fields) {
                if (
                       (
                        f.DataLink == "CompanyCPR"
                       ) && f.OrganisationId == O.Id) {

                    if (f.DataSource.IndexOf('_') > 0) {
                        col = f.DataSource + "_" + f.DataSource.Split('_')[2];
                    } else {
                        col = f.DatabaseTable + "_" + f.DatabaseColumn + "_" + f.BaseOrganisationId;
                    }
                    break;
                }
            }

            if (col != "") {

                string txtSQL = "" +
                                "CREATE PROCEDURE [dbo].[z_GetContactsWithSameCPR_" + O.Id + "]\n" +
                                "   @contactId int=0,\n" +
                                "   @cpr varchar(50)\n" +
                                "AS\n" +
                                "   SELECT\n" +
                                "       ContactId\n" +
                                "   FROM\n" +
                                "       [dbo].[Contacts_" + O.Id + "]()\n" +
                                "   WHERE \n" +
                                "       ContactId<>@ContactId and\n" +
                                "       [" + col + "] = @cpr and\n" +
                                "       @cpr <> '' and @cpr is not null\n" +
                                "\n" +
                                "\n" +
                                "\n" +
                                "\n";

                //EXECUTE SQL!!!
                if (txtSQL != "" && txtSQL != null) {
                    //(RE)CREATE THE PROCEDURE!!!
                    sql.dynamicFields_dropStoredProcedure("z_GetContactsWithSameCPR_" + O.Id);
                    sql.dynamicFields_updateStoredProcedure(txtSQL);
                }
            }
        }

        //REFACTOR GET COMPANY WITH SAME PNUMBER
        public static void rf_GetCompaniesWithSamePNR(ref SQLDB sql, Organisation O) {
            List<DynamicField> Fields = null; // dynamicFields_getFieldsForOrganisation(O);

            Fields = dynamicFields_getFieldsForOrganisation(O, "", ref sql, false);

            string col = "";
            foreach (DynamicField f in Fields) {
                if (
                       (
                        f.DataLink == "NNECompanyPNR"
                       ) && f.OrganisationId == O.Id) {

                    if (f.DataSource.IndexOf('_') > 0) {
                        col = f.DataSource + "_" + f.DataSource.Split('_')[2];
                    } else {
                        col = f.DatabaseTable + "_" + f.DatabaseColumn + "_" + f.BaseOrganisationId;
                    }
                    break;
                }
            }

            if (col != "") {

                string txtSQL = "" +
                                "CREATE PROCEDURE [dbo].[z_GetCompaniesWithSamePNR_" + O.Id + "]\n" +
                                "   @companyId int=0,\n" +
                                "   @pnr varchar(50)\n" +
                                "AS\n" +
                                "   SELECT\n" +
                                "       CompanyId\n" +
                                "   FROM\n" +
                                "       [dbo].[Companies_" + O.Id + "]()\n" +
                                "   WHERE \n" +
                                "       companyType=0 and\n" +
                                "       CompanyId<>@companyId and\n" +
                                "       [" + col + "] = @pnr and\n" +
                                "       @pnr <> '' and @pnr is not null\n" +
                                "\n" +
                                "\n" +
                                "\n" +
                                "\n";

                //EXECUTE SQL!!!
                if (txtSQL != "" && txtSQL != null) {
                    //(RE)CREATE THE PROCEDURE!!!
                    sql.dynamicFields_dropStoredProcedure("z_GetCompaniesWithSamePNR_" + O.Id);
                    sql.dynamicFields_updateStoredProcedure(txtSQL);
                }
            }
        }

        //REFACTOR GET COMPANY NAME
        public static void rf_GetCompaniesWithSameCVR(ref SQLDB sql, Organisation O) {
            List<DynamicField> Fields = null; // dynamicFields_getFieldsForOrganisation(O);

            Fields = dynamicFields_getFieldsForOrganisation(O, "", ref sql, false);

            string col = "";
            foreach (DynamicField f in Fields) {
                if (
                       (
                        f.DataLink == "NNECompanyCVR"
                       ) && f.OrganisationId == O.Id) {

                    if (f.DataSource.IndexOf('_') > 0) {
                        col = f.DataSource + "_" + f.DataSource.Split('_')[2];
                    } else {
                        col = f.DatabaseTable + "_" + f.DatabaseColumn + "_" + f.BaseOrganisationId;
                    }
                    break;
                }
            }

            if (col != "") {

                string txtSQL = "" +
                                "CREATE PROCEDURE [dbo].[z_GetCompaniesWithSameCVR_" + O.Id + "]\n" +
                                "   @companyId int\n" +
                                "AS\n" +
                                "   SELECT\n" +
                                "       b2.CompanyId\n" +
                                "   FROM\n" +
                                "       [dbo].[Companies_" + O.Id + "]() as b1,\n" +
                                "       [dbo].[Companies_" + O.Id + "]() as b2\n" +
                                "   WHERE \n" +
                                "       b2.CompanyId<>b1.CompanyId and\n" +
                                "       b1.companyType=0 and\n" +
                                "       b1.CompanyId=@companyId and\n" +
                                "       b2.CompanyId<>@companyId and\n" +
                                "       b2.[" + col + "] = b1.[" + col + "] and\n" +
                                "       b1.[" + col + "] <>'' and\n" +
                                "       b1.[" + col + "] is not null\n" +
                                "\n" +
                                "\n" +
                                "\n" +
                                "\n" +
                                "\n";

                //EXECUTE SQL!!!
                if (txtSQL != "" && txtSQL != null) {
                    //(RE)CREATE THE PROCEDURE!!!
                    sql.dynamicFields_dropStoredProcedure("z_GetCompaniesWithSameCVR_" + O.Id);
                    sql.dynamicFields_updateStoredProcedure(txtSQL);
                }
            }
        }


        //REFACTOR GET COMPANY NAME
        public static void rf_CompanyGetCVR(ref SQLDB sql, Organisation O) {
            List<DynamicField> Fields = null; // dynamicFields_getFieldsForOrganisation(O);
            List<SQLColumnItem> colItems = new List<SQLColumnItem>();

            colItems.Add(new SQLColumnItem("", 0));
            Fields = dynamicFields_getFieldsForOrganisation(O, "", ref sql, false);

            foreach (DynamicField f in Fields) {
                if (
                       (
                        f.DataLink == "NNECompanyCVR"
                       ) && f.OrganisationId == O.Id) {
                    string col = "";
                    if (f.DataSource.IndexOf('_') > 0) {
                        col = f.DataSource + "_" + f.DataSource.Split('_')[2];
                    } else {
                        col = f.DatabaseTable + "_" + f.DatabaseColumn + "_" + f.BaseOrganisationId;
                    }

                    colItems[0] = new SQLColumnItem(col, f.ListviewIndex);

                }
            }

            string txtSQL = "" +
                            "CREATE PROCEDURE [dbo].[z_Companies_GetCVR_" + O.Id + "]\n" +
                            "   @companyId int\n" +
                            "AS\n" +
                            "   SELECT\n" +
                            "       CompanyId\n" +
                            SQLColumnItemsToString(colItems) +
                            "   FROM\n" +
                            "       [dbo].[Companies_" + O.Id + "]() as Companies\n" +
                            "   WHERE \n" +
                            "       Companies.CompanyId=@companyId\n" +
                            "\n" +
                            "\n" +
                            "\n" +
                            "\n" +
                            "\n" +
                            "\n";

            //EXECUTE SQL!!!
            if (txtSQL != "" && txtSQL != null) {
                //(RE)CREATE THE PROCEDURE!!!
                sql.dynamicFields_dropStoredProcedure("z_Companies_GetCVR_" + O.Id);
                sql.dynamicFields_updateStoredProcedure(txtSQL);
            }
        }

        //REFACTOR GET COMPANY NAME
        public static void rf_CompanyGetNameFromEmail(ref SQLDB sql, Organisation O) {
            List<DynamicField> Fields = null; // dynamicFields_getFieldsForOrganisation(O);
            List<SQLColumnItem> colItems = new List<SQLColumnItem>();

            colItems.Add(new SQLColumnItem("", 0));
            Fields = dynamicFields_getFieldsForOrganisation(O, "", ref sql, false);
            string conditions = "";

            foreach (DynamicField f in Fields) {
                if (
                       (
                        (f.DataLink == "NNECompanyName" && f.BaseOrganisationId == 1) //** Ekstra check Favrskov
                       ) && f.OrganisationId == O.Id) {
                    string col = "";
                    if (f.DataSource.IndexOf('_') > 0) {
                        col = f.DataSource + "_" + f.DataSource.Split('_')[2];
                    } else {
                        col = f.DatabaseTable + "_" + f.DatabaseColumn + "_" + f.BaseOrganisationId;
                    }

                    colItems[0] = new SQLColumnItem(col, f.ListviewIndex);

                }

                if (f.FieldType == "emailaddress") {
                    string col = "";
                    if (f.DataSource.IndexOf('_') > 0) {
                        col = f.DataSource + "_" + f.DataSource.Split('_')[2];
                    } else {
                        col = f.DatabaseTable + "_" + f.DatabaseColumn + "_" + f.BaseOrganisationId;
                    }

                    conditions += (conditions == "" ? "" : "\t\tOR ") + "[" + col + "]=@email\n";
                }
            }

            if (conditions.Trim() == "")
                conditions = "1=0";

            string txtSQL = "" +
                            "CREATE PROCEDURE [dbo].[z_Companies_GetNameFromEmail_" + O.Id + "]\n" +
                            "   @email nvarchar(max)\n" +
                            "AS\n" +
                            "   SELECT\n" +
                            "       CompanyId\n" +
                            SQLColumnItemsToString(colItems) +
                            "   FROM\n" +
                            "       [dbo].[ContactsAndCompaniesList_" + O.Id + "]() as Companies\n" +
                            "   WHERE \n" +
                            "\n" +
                            "\n" + conditions +
                            "\n" +
                            "\n" +
                            "\n" +
                            "\n";

            //EXECUTE SQL!!!
            if (txtSQL != "" && txtSQL != null) {
                //(RE)CREATE THE PROCEDURE!!!
                sql.dynamicFields_dropStoredProcedure("z_Companies_GetNameFromEmail_" + O.Id);
                sql.dynamicFields_updateStoredProcedure(txtSQL);
            }
        }

        //REFACTOR GET COMPANY NAME
        public static void rf_ContactGetName(ref SQLDB sql, Organisation O) {
            List<DynamicField> Fields = null; // dynamicFields_getFieldsForOrganisation(O);
            List<SQLColumnItem> colItems = new List<SQLColumnItem>();

            colItems.Add(new SQLColumnItem("", 0));
            colItems.Add(new SQLColumnItem("", 0));
            colItems.Add(new SQLColumnItem("", 0));
            Fields = dynamicFields_getFieldsForOrganisation(O, "", ref sql, false);

            foreach (DynamicField f in Fields) {
                if (
                       (
                        f.DataLink == "DGSContactFirstname"
                       ) && f.OrganisationId == O.Id) {
                    string col = "";
                    if (f.DataSource.IndexOf('_') > 0) {
                        col = f.DataSource + "_" + f.DataSource.Split('_')[2];
                    } else {
                        col = f.DatabaseTable + "_" + f.DatabaseColumn + "_" + f.BaseOrganisationId;
                    }

                    colItems[0] = new SQLColumnItem(col, 0);

                } else if (
                         (
                          f.DataLink == "DGSCOntactLastname"
                         ) && f.OrganisationId == O.Id) {
                    string col = "";
                    if (f.DataSource.IndexOf('_') > 0) {
                        col = f.DataSource + "_" + f.DataSource.Split('_')[2];
                    } else {
                        col = f.DatabaseTable + "_" + f.DatabaseColumn + "_" + f.BaseOrganisationId;
                    }

                    colItems[1] = new SQLColumnItem(col, 1);

                } else if (
                    (
                      (f.DataLink == "NNECompanyName" && f.BaseOrganisationId == 1) //** Ekstra check Favrskov
                    )) {
                    string col = "";
                    if (f.DataSource.IndexOf('_') > 0) {
                        col = f.DataSource + "_" + f.DataSource.Split('_')[2];
                    } else {
                        col = f.DatabaseTable + "_" + f.DatabaseColumn + "_" + f.BaseOrganisationId;
                    }

                    colItems[2] = new SQLColumnItem(col, 2);

                }
            }

            for (int i = colItems.Count - 1; i >= 0; i--) {
                if (colItems[i] == null)
                    colItems.RemoveAt(i);
            }

            string txtSQL = "" +
                            "CREATE PROCEDURE [dbo].[z_Contacts_GetName_" + O.Id + "]\n" +
                            "   @ContactId int\n" +
                            "AS\n" +
                            "   SELECT\n" +
                            "       ContactId\n" +
                            SQLColumnItemsToString(colItems) +
                            "   FROM\n" +
                            "       [dbo].[ContactsAndCompaniesAnyList_" + O.Id + "]() as Contacts\n" +
                            "   WHERE \n" +
                            "       Contacts.ContactId=@ContactId\n" +
                            "\n" +
                            "\n" +
                            "\n" +
                            "\n" +
                            "\n" +
                            "\n";

            //EXECUTE SQL!!!
            if (txtSQL != "" && txtSQL != null) {
                //(RE)CREATE THE PROCEDURE!!!
                sql.dynamicFields_dropStoredProcedure("z_Contacts_GetName_" + O.Id);
                sql.dynamicFields_updateStoredProcedure(txtSQL);
            }
        }

        //REFACTOR GET COMPANY NAME
        public static void rf_CompanyGetName(ref SQLDB sql, Organisation O) {
            List<DynamicField> Fields = null; // dynamicFields_getFieldsForOrganisation(O);
            List<SQLColumnItem> colItems = new List<SQLColumnItem>();

            colItems.Add(new SQLColumnItem("", 0));
            Fields = dynamicFields_getFieldsForOrganisation(O, "", ref sql, false);

            foreach (DynamicField f in Fields) {
                if (
                       (
                        (f.DataLink == "NNECompanyName" && f.BaseOrganisationId == 1) //** Ekstra check Favrskov
                       ) && f.OrganisationId == O.Id) {
                    string col = "";
                    if (f.DataSource.IndexOf('_') > 0) {
                        col = f.DataSource + "_" + f.DataSource.Split('_')[2];
                    } else {
                        col = f.DatabaseTable + "_" + f.DatabaseColumn + "_" + f.BaseOrganisationId;
                    }

                    colItems[0] = new SQLColumnItem(col, f.ListviewIndex);

                }
            }

            string txtSQL = "" +
                            "CREATE PROCEDURE [dbo].[z_Companies_GetName_" + O.Id + "]\n" +
                            "   @companyId int\n" +
                            "AS\n" +
                            "   SELECT\n" +
                            "       CompanyId\n" +
                            SQLColumnItemsToString(colItems) +
                            "   FROM\n" +
                            "       [dbo].[Companies_" + O.Id + "]() as Companies\n" +
                            "   WHERE \n" +
                            "       Companies.CompanyId=@companyId\n" +
                            "\n" +
                            "\n" +
                            "\n" +
                            "\n" +
                            "\n" +
                            "\n";

            //EXECUTE SQL!!!
            if (txtSQL != "" && txtSQL != null) {
                //(RE)CREATE THE PROCEDURE!!!
                sql.dynamicFields_dropStoredProcedure("z_Companies_GetName_" + O.Id);
                sql.dynamicFields_updateStoredProcedure(txtSQL);
            }
        }

        //REFACTOR INCOMPLETE CONTACTS
        public static void rf_ContactsGetIncompleteContacts(ref SQLDB sql, Organisation O) {

            List<DynamicField> Fields = dynamicFields_getFieldsForOrganisation(O, ref sql, false);

            string rqFields = "";
            string unknownContact = "";
            List<SQLColumnItem> colItems = new List<SQLColumnItem>();

            foreach (DynamicField f in Fields) {
                //if ((f.RequiredState > 0 || (f.DataLink == "Ukendt kontaktperson" && f.ViewState == "SMV")) && f.FieldType != "externaldatalink" && f.FieldType != "title" && f.FieldType != "label" && f.FieldType != "vr" && f.FieldType != "hr" && f.FieldType != "map")
                if (f.FieldType != "externaldatalink" && f.FieldType != "title" && f.FieldType != "label" && f.FieldType != "vr" && f.FieldType != "hr" && f.FieldType != "map") {
                    if (f.DataLink == "Ukendt kontaktperson" && f.ViewState == "SMV") {
                        unknownContact += "\t\t\tAND ([" + f.DatabaseTable + "_" + f.DatabaseColumn + "_" + f.BaseOrganisationId + "] is null or [" + f.DatabaseTable + "_" + f.DatabaseColumn + "_" + f.BaseOrganisationId + "] = 0)\n";
                    }

                    bool noInherit = f.NoInherit(O.Id);
                    if (f.RequiredState > 0 && !noInherit) {
                        rqFields += (rqFields == "" ? "" : "\tOR\n") +
                            "\t\t(\n\t\tconvert(varchar(8000),isnull([" + f.DatabaseTable + "_" + f.DatabaseColumn + "_" + f.BaseOrganisationId + "],'')) = ''\n";


                        if (f.DataLink == "NNECompanyCVR" || f.DataLink == "NNECompanyPNR" || f.DataLink == "Counties") {
                            rqFields += "\t\t\tAND ([z_companies_1_Land_1] = 'DENMARK' OR [z_companies_1_Land_1] = 'DANMARK')\n";
                        }



                        if (f.ViewState == "SMV") {
                            if (f.DatabaseTable.Split('_')[1].ToLower() == "contacts")
                                rqFields += "\t\t\tAND ContactType=0)\n";
                            else
                                rqFields += "\t\t\tAND CompanyType=0)\n";
                        } else if (f.ViewState == "POT") {
                            if (f.DatabaseTable.Split('_')[1].ToLower() == "contacts")
                                rqFields += "\t\t\tAND ContactType=1)\n";
                            else
                                rqFields += "\t\t\tAND CompanyType=1)\n";
                        } else {
                            rqFields += "\t\t)\n";
                        }
                    }
                }
            }
            if (rqFields != "") {
                rqFields = "AND (\n" + rqFields + "\n)";
            }

            Fields = dynamicFields_getFieldsForOrganisation(O, "contacts", ref sql, true);

            foreach (DynamicField f in Fields) {
                if (TypeCast.ToInt(f.ShowInListview) > 0 && f.OrganisationId == O.Id) {
                    string col = "";
                    if (f.DataSource.IndexOf('_') > 0) {
                        col = f.DataSource + "_" + f.DataSource.Split('_')[2];
                    } else {
                        col = f.DatabaseTable + "_" + f.DatabaseColumn + "_" + f.BaseOrganisationId;
                    }
                    //Add the columns desired in a sorted manor!
                    bool inserted = false;
                    int index = f.ListviewIndex;

                    if (colItems.Count == 0) {
                        colItems.Add(new SQLColumnItem(col, f.ListviewIndex));
                    } else {
                        for (int ii = 0; ii < colItems.Count; ii++) {
                            int cpIndex = colItems[ii].ListIndex;
                            if (inserted == false && index < cpIndex) {
                                inserted = true;
                                colItems.Insert(ii > colItems.Count ? colItems.Count : ii, new SQLColumnItem(col, f.ListviewIndex));
                            }
                        }

                        if (inserted == false)
                            colItems.Add(new SQLColumnItem(col, f.ListviewIndex));
                    }
                }
            }


            string txtSQL = "" +
                            "CREATE PROCEDURE [dbo].[z_Contacts_GetIncompleteContacts_" + O.Id + "]\n" +
                            "   @organisationId int,\n" +
                            "   @userId uniqueidentifier\n" +
                            "AS\n" +
                            "   SELECT\n" +
                            "        ContactId,\n" +
                            "        CompanyOwnerId,\n" +
                            "        ContactOrganisationId,\n" +
                            "        ContactCreatedById,\n" +
                            "        ContactDateStamp,\n" +
                            "        ContactType,\n" +
                            "        CompanyDeletedDate,\n" +
                            "        CompanyDeletedBy,\n" +
                            "        ContactDeletedDate,\n" +
                            "        ContactDeletedBy,\n" +
                            "        CompanyLastUpdated,CompanyAbandonedDate, CompanyAbandonedBy,\n" +
                            "        CompanyLastUpdatedBy,\n" +
                            "        ContactLastUpdated,\n" +
                            "        ContactLastUpdatedBy, ContactAbandonedDate, ContactAbandonedBy\n" +
                            "\n" +
                            SQLColumnItemsToString(colItems) +
                            "   FROM\n" +
                            "       [dbo].[ContactsAndCompaniesList_" + O.Id + "]() as Contacts\n" +
                            "   WHERE \n" +
                            "       (ContactCreatedById=@userId or ContactLastUpdatedBy=@userId or CompanyCreatedById=@userId or CompanyLastUpdatedBy=@userId)\n" +
                            rqFields +
                            unknownContact +
                            "\n" +
                            "\n" +
                            "\n" +
                            "\n";

            //EXECUTE SQL!!!
            if (txtSQL != "" && txtSQL != null) {
                //(RE)CREATE THE PROCEDURE!!!
                sql.dynamicFields_dropStoredProcedure("z_Contacts_GetIncompleteContacts_" + O.Id);
                sql.dynamicFields_updateStoredProcedure(txtSQL);
            }
        }

        //REFACTOR RECENT CONTACTS
        public static void rf_ContactsGetRecentContacts(ref SQLDB sql, Organisation O) {

            List<DynamicField> Fields = null; // dynamicFields_getFieldsForOrganisation(O,"");
            List<SQLColumnItem> colItems = new List<SQLColumnItem>();


            Fields = dynamicFields_getFieldsForOrganisation(O, "contacts", ref sql, true);
            foreach (DynamicField f in Fields) {
                if (TypeCast.ToInt(f.ShowInListview) > 0 && f.OrganisationId == O.Id) {
                    string col = "";
                    if (f.DataSource.IndexOf('_') > 0) {
                        col = f.DataSource + "_" + f.DataSource.Split('_')[2];
                    } else {
                        col = f.DatabaseTable + "_" + f.DatabaseColumn + "_" + f.BaseOrganisationId;
                    }
                    //Add the columns desired in a sorted manor!
                    bool inserted = false;
                    int index = f.ListviewIndex;

                    if (colItems.Count == 0) {
                        colItems.Add(new SQLColumnItem(col, f.ListviewIndex));
                    } else {
                        for (int ii = 0; ii < colItems.Count; ii++) {
                            int cpIndex = colItems[ii].ListIndex;
                            if (inserted == false && index < cpIndex) {
                                inserted = true;
                                colItems.Insert(ii > colItems.Count ? colItems.Count : ii, new SQLColumnItem(col, f.ListviewIndex));
                            }
                        }

                        if (inserted == false)
                            colItems.Add(new SQLColumnItem(col, f.ListviewIndex));
                    }
                }
            }


            string txtSQL = "" +
                            "CREATE PROCEDURE [dbo].[z_Contacts_GetRecentContacts_" + O.Id + "]\n" +
                            "   @organisationId int,\n" +
                            "   @userId uniqueidentifier\n" +
                            "AS\n" +
                            "\n" +
                            "declare @table table(DateStamp datetime, ContactId int, LastChanged varchar(255))\n" +
                            "insert into @table select * from dbo.[getContactsLastUpdatedRange](@userId)\n" +
                            "\n" +
                            "   SELECT TOP 5\n" +
                            "        ContactId,\n" +
                            "        CompanyOwnerId,\n" +
                            "        ContactOrganisationId,\n" +
                            "        ContactCreatedById,\n" +
                            "        ContactDateStamp,\n" +
                            "        ContactType,\n" +
                            "        CompanyDeletedDate,\n" +
                            "        CompanyDeletedBy,\n" +
                            "        ContactDeletedDate,\n" +
                            "        ContactDeletedBy,\n" +
                            "        CompanyLastUpdated,CompanyAbandonedDate, CompanyAbandonedBy,\n" +
                            "        CompanyLastUpdatedBy,\n" +
                            "        --ContactLastUpdated,\n" +
                            "        ContactLastUpdatedBy, ContactAbandonedDate, ContactAbandonedBy,\n" +
                            "\n" +
                            "       isnull((select top 1 datestamp from @table dtRange where dtRange.ContactId=Contacts.ContactId order by DateStamp desc), isnull(contacts.ContactLastUpdated, contacts.ContactDateStamp)) as ContactLastUpdated,\n" +
                            "       (select top 1 LastChanged from @table dtRange where dtRange.ContactId=Contacts.ContactId order by DateStamp desc) as ContactLastUpdatedReason\n" +
                            "\n" +

                            SQLColumnItemsToString(colItems) +
                            "   FROM\n" +
                            "       [dbo].[ContactsAndCompaniesList_" + O.Id + "]() as Contacts\n" +
                            "   WHERE \n" +
                            "       contactLastUpdatedBy=@userId or contactCreatedById=@userId or\n" +
                            "       Contacts.ContactId in (select ContactId from @table)\n" +
                            "\n" +
                            "\n" +
                            "\n" +
                            "   ORDER BY\n" +
                            "       (select top 1 datestamp from @table dtRange where dtRange.ContactId=Contacts.ContactId order by DateStamp desc) desc, (isnull(contacts.ContactLastUpdated, contacts.ContactDateStamp)) desc\n" +
                            "\n" +
                            "\n";

            //EXECUTE SQL!!!
            if (txtSQL != "" && txtSQL != null) {
                //(RE)CREATE THE PROCEDURE!!!
                sql.dynamicFields_dropStoredProcedure("z_Contacts_GetRecentContacts_" + O.Id);
                sql.dynamicFields_updateStoredProcedure(txtSQL);
            }
        }

        //GET CONTACT FROM COMPANY, WHO HAVE BEEN SENT TO EVALUATION WITHIN THE LAST YEAR
        public static void rf_Contacts_isSentToEvaluation(ref SQLDB sql, Organisation O) {

            List<DynamicField> Fields = null; // dynamicFields_getFieldsForOrganisation(O);
            List<SQLColumnItem> colItems = new List<SQLColumnItem>();

            Fields = dynamicFields_getFieldsForOrganisation(O, "contacts", ref sql, true);

            //bool firstnameOK = false;
            bool lastnameOK = false;

            foreach (DynamicField f in Fields) {
                if ((f.DataLink == "DGSContactFirstname" || f.DataLink == "DGSCOntactLastname") && f.OrganisationId == O.Id) {
                    string col = "";
                    if (f.DataSource.IndexOf('_') > 0) {
                        col = f.DataSource + "_" + f.DataSource.Split('_')[2];
                    } else {
                        col = f.DatabaseTable + "_" + f.DatabaseColumn + "_" + f.BaseOrganisationId;
                    }

                    if (f.DataLink == "DGSContactFirstname") {
                        if (lastnameOK == true)
                            colItems.Insert(0, new SQLColumnItem(col, f.ListviewIndex));
                        else
                            colItems.Add(new SQLColumnItem(col, f.ListviewIndex));

                        //firstnameOK = true;
                    } else {
                        colItems.Add(new SQLColumnItem(col, f.ListviewIndex));
                    }


                }
            }


            string txtSQL = "" +
                            "CREATE PROCEDURE [dbo].[z_Contacts_isSentToEvaluation_" + O.Id + "]\n" +
                            "   @organisationId int,\n" +
                            "   @companyId int\n" +
                            "AS\n" +
                            "   SELECT TOP 1\n" +
                            "        Contacts.ContactId,\n" +
                            "        Contacts.CompanyOwnerId,\n" +
                            "        Contacts.ContactOrganisationId,\n" +
                            "        Contacts.ContactCreatedById,\n" +
                            "        Contacts.ContactDateStamp,\n" +
                            "        Contacts.ContactType,\n" +
                            "        ContactDeletedDate,\n" +
                            "        ContactDeletedBy,\n" +
                            "        ContactLastUpdated,\n" +
                            "        ContactLastUpdatedBy, ContactAbandonedDate, ContactAbandonedBy\n" +
                            "\n" +
                            SQLColumnItemsToString(colItems) +
                            "   FROM\n" +
                            "       [dbo].[Contacts_" + O.Id + "]() as Contacts,\n" +
                            "       contactsToEvaluation\n" +
                            "\n" +
                            "   WHERE \n" +
                            "           Contacts.ContactId=contactsToEvaluation.contactId AND\n" +
                            "           Contacts.CompanyOwnerId=@companyId AND\n" +
                            "           --contactsToEvaluation.companyId=@companyId AND\n" +
                            "           --contactsToEvaluation.organisationId=@organisationId AND\n" +
                            "           datediff(yyyy,contactsToEvaluation.dateStamp, getDate()) = 0\n" +
                            "   ORDER BY\n" +
                            "           contactsToEvaluation.dateStamp desc\n" +
                            "\n" +
                            "\n";

            //EXECUTE SQL!!!
            if (txtSQL != "" && txtSQL != null) {
                //(RE)CREATE THE PROCEDURE!!!
                sql.dynamicFields_dropStoredProcedure("z_Contacts_isSentToEvaluation_" + O.Id);
                sql.dynamicFields_updateStoredProcedure(txtSQL);
            }
        }

        public static void rf_Contacts_isSentToLocalEvaluation(ref SQLDB sql, Organisation O) {

            List<DynamicField> Fields = null; // dynamicFields_getFieldsForOrganisation(O);
            List<SQLColumnItem> colItems = new List<SQLColumnItem>();

            Fields = dynamicFields_getFieldsForOrganisation(O, "contacts", ref sql, true);

            //bool firstnameOK = false;
            bool lastnameOK = false;

            foreach (DynamicField f in Fields) {
                if ((f.DataLink == "DGSContactFirstname" || f.DataLink == "DGSCOntactLastname") && f.OrganisationId == O.Id) {
                    string col = "";
                    if (f.DataSource.IndexOf('_') > 0) {
                        col = f.DataSource + "_" + f.DataSource.Split('_')[2];
                    } else {
                        col = f.DatabaseTable + "_" + f.DatabaseColumn + "_" + f.BaseOrganisationId;
                    }

                    if (f.DataLink == "DGSContactFirstname") {
                        if (lastnameOK == true)
                            colItems.Insert(0, new SQLColumnItem(col, f.ListviewIndex));
                        else
                            colItems.Add(new SQLColumnItem(col, f.ListviewIndex));

                        //firstnameOK = true;
                    } else {
                        colItems.Add(new SQLColumnItem(col, f.ListviewIndex));
                    }


                }
            }


            string txtSQL = "" +
                            "CREATE PROCEDURE [dbo].[z_Contacts_isSentToLocalEvaluation_" + O.Id + "]\n" +
                            "   @organisationId int,\n" +
                            "   @companyId int\n" +
                            "AS\n" +
                            "   SELECT TOP 1\n" +
                            "        Contacts.ContactId,\n" +
                            "        Contacts.CompanyOwnerId,\n" +
                            "        Contacts.ContactOrganisationId,\n" +
                            "        Contacts.ContactCreatedById,\n" +
                            "        Contacts.ContactDateStamp,\n" +
                            "        Contacts.ContactType,\n" +
                            "        ContactDeletedDate,\n" +
                            "        ContactDeletedBy,\n" +
                            "        ContactLastUpdated,\n" +
                            "        ContactLastUpdatedBy, ContactAbandonedDate, ContactAbandonedBy\n" +
                            "\n" +
                            SQLColumnItemsToString(colItems) +
                            "   FROM\n" +
                            "       [dbo].[Contacts_" + O.Id + "]() as Contacts,\n" +
                            "       contactsToLocalEvaluation\n" +
                            "\n" +
                            "   WHERE \n" +
                            "           Contacts.ContactId=contactsToLocalEvaluation.contactId AND\n" +
                            "           Contacts.CompanyOwnerId=@companyId AND\n" +
                            "           --contactsToLocalEvaluation.companyId=@companyId AND\n" +
                            "           --contactsToLocalEvaluation.organisationId=@organisationId AND\n" +
                            "           datediff(yyyy,contactsToLocalEvaluation.dateStamp, getDate()) = 0\n" +
                            "   ORDER BY\n" +
                            "           contactsToLocalEvaluation.dateStamp desc\n" +
                            "\n" +
                            "\n";

            //EXECUTE SQL!!!
            if (txtSQL != "" && txtSQL != null) {
                //(RE)CREATE THE PROCEDURE!!!
                sql.dynamicFields_dropStoredProcedure("z_Contacts_isSentToLocalEvaluation_" + O.Id);
                sql.dynamicFields_updateStoredProcedure(txtSQL);
            }
        }

        //GET CONTACT EMAIL FROM ID
        public static void rf_Exchange_getContactEmailFromId(ref SQLDB sql, Organisation O) {

            List<DynamicField> Fields = null; // dynamicFields_getFieldsForOrganisation(O);
            List<SQLColumnItem> colItems = new List<SQLColumnItem>();

            Fields = dynamicFields_getFieldsForOrganisation(O, "contacts", ref sql, true);
            foreach (DynamicField f in Fields) {
                if (f.FieldType == "emailaddress" && f.OrganisationId == O.Id) {
                    string col = "";
                    if (f.DataSource.IndexOf('_') > 0) {
                        col = f.DataSource + "_" + f.DataSource.Split('_')[2];
                    } else {
                        col = f.DatabaseTable + "_" + f.DatabaseColumn + "_" + f.BaseOrganisationId;
                    }


                    //Add the columns desired in a sorted manor!
                    bool inserted = false;
                    int index = f.ListviewIndex;

                    if (colItems.Count == 0) {
                        colItems.Add(new SQLColumnItem(col, f.ListviewIndex));
                    } else {
                        for (int ii = 0; ii < colItems.Count; ii++) {
                            int cpIndex = colItems[ii].ListIndex;
                            if (inserted == false && index < cpIndex) {
                                inserted = true;
                                colItems.Insert(ii > colItems.Count ? colItems.Count : ii, new SQLColumnItem(col, f.ListviewIndex));
                            }
                        }

                        if (inserted == false)
                            colItems.Add(new SQLColumnItem(col, f.ListviewIndex));
                    }
                }
            }


            string txtSQL = "" +
                            "CREATE PROCEDURE [dbo].[z_Exchange_getContactEmailFromId_" + O.Id + "]\n" +
                            "   @organisationId int,\n" +
                            "   @contactId int\n" +
                            "AS\n" +
                            "   SELECT \n" +
                            "        Contacts.ContactId,\n" +
                            "        Contacts.CompanyOwnerId,\n" +
                            "        Contacts.ContactOrganisationId,\n" +
                            "        Contacts.ContactCreatedById,\n" +
                            "        Contacts.ContactDateStamp,\n" +
                            "        Contacts.ContactType,\n" +
                            "        ContactDeletedDate,\n" +
                            "        ContactDeletedBy,\n" +
                            "        ContactLastUpdated,\n" +
                            "        ContactLastUpdatedBy, ContactAbandonedDate, ContactAbandonedBy\n" +
                            "\n" +
                            SQLColumnItemsToString(colItems) +
                            "   FROM\n" +
                            "       [dbo].[Contacts_" + O.Id + "]() as Contacts\n" +
                            "   WHERE \n" +
                            "           Contacts.ContactId=@contactId\n" +
                            "\n" +
                            "\n";

            //EXECUTE SQL!!!
            if (txtSQL != "" && txtSQL != null) {
                //(RE)CREATE THE PROCEDURE!!!
                sql.dynamicFields_dropStoredProcedure("z_Exchange_getContactEmailFromId_" + O.Id);
                sql.dynamicFields_updateStoredProcedure(txtSQL);
            }
        }

        //SEARCH FOR CONTACTS TO BE ADDED TO A MEETING IN OUTLOOK
        public static void rf_Exchange_findContacts(ref SQLDB sql, Organisation O) {

            List<DynamicField> Fields = null; // dynamicFields_getFieldsForOrganisation(O);
            List<SQLColumnItem> colItems = new List<SQLColumnItem>();

            string query = "";

            Fields = dynamicFields_getFieldsForOrganisation(O, "", ref sql, false);
            foreach (DynamicField f in Fields) {                                                        //** Ekstra check Favrskov
                if (((f.FieldType == "emailaddress" && f.DatabaseTable.IndexOf("z_contacts") > -1) || (f.DataLink == "NNECompanyName" && f.BaseOrganisationId == 1) || f.DataLink == "DGSContactFirstname" || f.DataLink == "DGSCOntactLastname") && f.OrganisationId == O.Id) {
                    string col = "";
                    if (f.DataSource.IndexOf('_') > 0) {
                        col = f.DataSource + "_" + f.DataSource.Split('_')[2];
                    } else {
                        col = f.DatabaseTable + "_" + f.DatabaseColumn + "_" + f.BaseOrganisationId;
                    }

                    query += "" + (query == "" ? "" : "    OR ") +
                                "       (convert(varchar(8000),[" + col + "]) LIKE '%' + @query + '%')\n";

                    //Add the columns desired in a sorted manor!
                    bool inserted = false;
                    int index = f.ListviewIndex;

                    if (colItems.Count == 0) {
                        colItems.Add(new SQLColumnItem(col, f.ListviewIndex));
                    } else {
                        for (int ii = 0; ii < colItems.Count; ii++) {
                            int cpIndex = colItems[ii].ListIndex;
                            if (inserted == false && index < cpIndex) {
                                inserted = true;
                                colItems.Insert(ii > colItems.Count ? colItems.Count : ii, new SQLColumnItem(col, f.ListviewIndex));
                            }
                        }

                        if (inserted == false)
                            colItems.Add(new SQLColumnItem(col, f.ListviewIndex));
                    }
                }
            }

            if (query != "" && query != null)
                query = "(" + query + ")";

            string txtSQL = "" +
                            "CREATE PROCEDURE [dbo].[z_Exchange_findContacts_" + O.Id + "]\n" +
                            "   @organisationId int,\n" +
                            "   @query varchar(8000)\n" +
                            "AS\n" +
                            "   SELECT \n" +
                            "        Contacts.ContactId,\n" +
                            "        Contacts.CompanyOwnerId,\n" +
                            "        Contacts.ContactOrganisationId,\n" +
                            "        Contacts.ContactCreatedById,\n" +
                            "        Contacts.ContactDateStamp,\n" +
                            "        Contacts.ContactType,\n" +
                            "        CompanyDeletedDate,\n" +
                            "        CompanyDeletedBy,\n" +
                            "        ContactDeletedDate,\n" +
                            "        ContactDeletedBy,\n" +
                            "        CompanyLastUpdated,CompanyAbandonedDate, CompanyAbandonedBy,\n" +
                            "        CompanyLastUpdatedBy,\n" +
                            "        ContactLastUpdated,\n" +
                            "        ContactLastUpdatedBy, ContactAbandonedDate, ContactAbandonedBy\n" +
                            "\n" +
                            SQLColumnItemsToString(colItems) +
                            "   FROM\n" +
                            "       [dbo].[ContactsAndCompaniesList_" + O.Id + "]() as Contacts\n" +
                            "   WHERE \n" +
                            "       CompanyAbandonedDate is null and \n" +
                            "       ContactAbandonedDate is null  and \n" +
                            "       CompanyAbandonedBy is null and  \n" +
                            "       ContactAbandonedBy is null \n" +
                            "       " + (query != "" ? " and \n" : "\n") +
                            query +
                            "\n" +
                            "\n";

            //EXECUTE SQL!!!
            if (txtSQL != "" && txtSQL != null) {
                //(RE)CREATE THE PROCEDURE!!!
                sql.dynamicFields_dropStoredProcedure("z_Exchange_findContacts_" + O.Id);
                sql.dynamicFields_updateStoredProcedure(txtSQL);
            }
        }

        //REFACTOR EXPORT EMAILS FROM MAILGROUP
        public static void rf_MailGroups_ExportContactEmails(ref SQLDB sql, Organisation O) {

            List<DynamicField> Fields = null; // dynamicFields_getFieldsForOrganisation(O);
            List<SQLColumnItem> colItems = new List<SQLColumnItem>();


            Fields = dynamicFields_getFieldsForOrganisation(O, "contacts", ref sql, true);
            foreach (DynamicField f in Fields) {
                if (f.FieldType == "emailaddress" && f.OrganisationId == O.Id) {
                    string col = "";
                    if (f.DataSource.IndexOf('_') > 0) {
                        col = f.DataSource + "_" + f.DataSource.Split('_')[2];
                    } else {
                        col = f.DatabaseTable + "_" + f.DatabaseColumn + "_" + f.BaseOrganisationId;
                    }
                    //Add the columns desired in a sorted manor!
                    bool inserted = false;
                    int index = f.ListviewIndex;

                    if (colItems.Count == 0) {
                        colItems.Add(new SQLColumnItem(col, f.ListviewIndex));
                    } else {
                        for (int ii = 0; ii < colItems.Count; ii++) {
                            int cpIndex = colItems[ii].ListIndex;
                            if (inserted == false && index < cpIndex) {
                                inserted = true;
                                colItems.Insert(ii > colItems.Count ? colItems.Count : ii, new SQLColumnItem(col, f.ListviewIndex));
                            }
                        }

                        if (inserted == false)
                            colItems.Add(new SQLColumnItem(col, f.ListviewIndex));
                    }
                }
            }


            string txtSQL = "" +
                            "CREATE PROCEDURE [dbo].[z_MailGroups_ExportContactEmails_" + O.Id + "]\n" +
                            "   @organisationId int,\n" +
                            "   @mailGroupId int\n" +
                            "AS\n" +
                            "   SELECT \n" +
                            "        Contacts.ContactId,\n" +
                            "        Contacts.CompanyOwnerId,\n" +
                            "        Contacts.ContactOrganisationId,\n" +
                            "        Contacts.ContactCreatedById,\n" +
                            "        Contacts.ContactDateStamp,\n" +
                            "        Contacts.ContactType,\n" +
                            "        CompanyDeletedDate,\n" +
                            "        CompanyDeletedBy,\n" +
                            "        ContactDeletedDate,\n" +
                            "        ContactDeletedBy,\n" +
                            "        CompanyLastUpdated,CompanyAbandonedDate, CompanyAbandonedBy,\n" +
                            "        CompanyLastUpdatedBy,\n" +
                            "        ContactLastUpdated,\n" +
                            "        ContactLastUpdatedBy, ContactAbandonedDate, ContactAbandonedBy\n" +
                            "\n" +
                            SQLColumnItemsToString(colItems) +
                            "   FROM\n" +
                            "       [dbo].[ContactsAndCompaniesList_" + O.Id + "]() as Contacts,\n" +
                            "       [mailGroup_Contacts],\n" +
                            "       [mailGroups]\n" +
                            "   WHERE \n" +
                            "       mailgroupId = @mailGroupId AND\n" +
                            "       mailGroup_Contacts.contactId=Contacts.contactId AND\n" +
                            "       mailGroups.Id=mailGroup_Contacts.MailGroupId AND\n" +
                            "       mailGroups.organisationId=@organisationId\n" +
                            "\n" +
                            "\n";

            //EXECUTE SQL!!!
            if (txtSQL != "" && txtSQL != null) {
                //(RE)CREATE THE PROCEDURE!!!
                sql.dynamicFields_dropStoredProcedure("z_MailGroups_ExportContactEmails_" + O.Id);
                sql.dynamicFields_updateStoredProcedure(txtSQL);
            }
        }

        //REFACTOR FILEARCHIVE GET FILE BY FOLDER ID
        public static void rf_z_Exchange_GetMailRules(ref SQLDB sql, Organisation O) {
            List<DynamicField> Fields = null; // dynamicFields_getFieldsForOrganisation(O);
            List<SQLColumnItem> colItems = new List<SQLColumnItem>();
            string orderBy = "";


            Fields = dynamicFields_getFieldsForOrganisation(O, ref sql, true);
            foreach (DynamicField f in Fields) {    //** Ekstra check Favrskov
                if (((f.DataLink == "NNECompanyName" && f.BaseOrganisationId == 1) || f.DataLink == "DGSContactFirstname" || f.DataLink == "DGSCOntactLastname")
                        && f.OrganisationId == O.Id) {
                    string col = "";
                    if (f.DataSource.IndexOf('_') > 0) {
                        col = f.DataSource + "_" + f.DataSource.Split('_')[2];
                    } else {
                        col = f.DatabaseTable + "_" + f.DatabaseColumn + "_" + f.BaseOrganisationId;
                    }

                    //dynamically added columns
                    if (orderBy == "") {
                        orderBy = "[" + col + "]";
                    }

                    //Add the columns desired in a sorted manor!
                    bool inserted = false;
                    int index = f.ListviewIndex;

                    if (colItems.Count == 0) {
                        colItems.Add(new SQLColumnItem(col, f.ListviewIndex));
                    } else {
                        for (int ii = 0; ii < colItems.Count; ii++) {
                            int cpIndex = colItems[ii].ListIndex;
                            if (inserted == false && index < cpIndex) {
                                inserted = true;
                                colItems.Insert(ii > colItems.Count ? colItems.Count : ii, new SQLColumnItem(col, f.ListviewIndex));
                            }
                        }

                        if (inserted == false)
                            colItems.Add(new SQLColumnItem(col, f.ListviewIndex));
                    }
                }
            }
            string txtSQL = "" +
                           "CREATE PROCEDURE [dbo].[z_Exchange_GetMailRules_" + O.Id + "]\n" +
                           "    @UserId uniqueidentifier\n" +
                           "AS\n" +
                           "   SELECT \n" +
                            "       [ExchangeMailRules].Id,\n" +
                            "       [ExchangeMailRules].[userId],\n" +
                            "       [ExchangeMailRules].[OrganisationId],\n" +
                            "       [ExchangeMailRules].senderEmail,\n" +
                            "       [ExchangeMailRules].ContactId,\n" +
                            "       [ExchangeMailRules].VisibleTo\n" +

                            SQLColumnItemsToString(colItems) +

                           "   FROM\n" +
                           "       [ExchangeMailRules]\n" +
                           "        LEFT JOIN [dbo].[ContactsAndCompaniesList_" + O.Id + "]() Contacts on [ExchangeMailRules].ContactId = Contacts.ContactId \n" +
                           "   WHERE \n" +
                           "       ExchangeMailRules.UserId = @UserId \n" +
                           "   ORDER BY ExchangeMailRules.senderEmail    ";

            //EXECUTE SQL!!!
            if (txtSQL != "" && txtSQL != null) {
                //(RE)CREATE THE PROCEDURE!!!
                sql.dynamicFields_dropStoredProcedure("z_Exchange_GetMailRules_" + O.Id);
                sql.dynamicFields_updateStoredProcedure(txtSQL);
            }
        }

        //REFACTOR FILEARCHIVE GET FILE BY FOLDER ID
        public static void rf_z_Exchange_GetMailRule(ref SQLDB sql, Organisation O) {
            List<DynamicField> Fields = null; // dynamicFields_getFieldsForOrganisation(O);
            List<SQLColumnItem> colItems = new List<SQLColumnItem>();
            string orderBy = "";


            Fields = dynamicFields_getFieldsForOrganisation(O, ref sql, true);
            foreach (DynamicField f in Fields) {    //** Ekstra check Favrskov
                if (((f.DataLink == "NNECompanyName" && f.BaseOrganisationId == 1) || f.DataLink == "DGSContactFirstname" || f.DataLink == "DGSCOntactLastname")
                        && f.OrganisationId == O.Id) {
                    string col = "";
                    if (f.DataSource.IndexOf('_') > 0) {
                        col = f.DataSource + "_" + f.DataSource.Split('_')[2];
                    } else {
                        col = f.DatabaseTable + "_" + f.DatabaseColumn + "_" + f.BaseOrganisationId;
                    }

                    //dynamically added columns
                    if (orderBy == "") {
                        orderBy = "[" + col + "]";
                    }

                    //Add the columns desired in a sorted manor!
                    bool inserted = false;
                    int index = f.ListviewIndex;

                    if (colItems.Count == 0) {
                        colItems.Add(new SQLColumnItem(col, f.ListviewIndex));
                    } else {
                        for (int ii = 0; ii < colItems.Count; ii++) {
                            int cpIndex = colItems[ii].ListIndex;
                            if (inserted == false && index < cpIndex) {
                                inserted = true;
                                colItems.Insert(ii > colItems.Count ? colItems.Count : ii, new SQLColumnItem(col, f.ListviewIndex));
                            }
                        }

                        if (inserted == false)
                            colItems.Add(new SQLColumnItem(col, f.ListviewIndex));
                    }
                }
            }
            string txtSQL = "" +
                           "CREATE PROCEDURE [dbo].[z_Exchange_GetMailRule_" + O.Id + "]\n" +
                           "    @UserId uniqueidentifier,\n" +
                           "    @Id int\n" +
                           "AS\n" +
                           "   SELECT \n" +
                            "       [ExchangeMailRules].Id,\n" +
                            "       [ExchangeMailRules].[userId],\n" +
                            "       [ExchangeMailRules].[OrganisationId],\n" +
                            "       [ExchangeMailRules].senderEmail,\n" +
                            "       [ExchangeMailRules].ContactId,\n" +
                            "       [ExchangeMailRules].VisibleTo\n" +

                            SQLColumnItemsToString(colItems) +

                           "   FROM\n" +
                           "       [ExchangeMailRules]\n" +
                           "        LEFT JOIN [dbo].[ContactsAndCompaniesList_" + O.Id + "]() Contacts on [ExchangeMailRules].ContactId = Contacts.ContactId \n" +
                           "   WHERE \n" +
                           "       ExchangeMailRules.UserId = @UserId AND\n" +
                           "       ExchangeMailRules.Id = @Id \n";

            //EXECUTE SQL!!!
            if (txtSQL != "" && txtSQL != null) {
                //(RE)CREATE THE PROCEDURE!!!
                sql.dynamicFields_dropStoredProcedure("z_Exchange_GetMailRule_" + O.Id);
                sql.dynamicFields_updateStoredProcedure(txtSQL);
            }
        }

        //REFACTOR EXPORT EMAILS FROM MAILGROUP
        public static void rf_Contacts_ExtractToUserFinalcialPoolEvaluation(ref SQLDB sql, Organisation O) {

            List<DynamicField> Fields = null; // dynamicFields_getFieldsForOrganisation(O);
            List<SQLColumnItem> colItems = new List<SQLColumnItem>();

            colItems.Add(new SQLColumnItem("", 0));
            colItems.Add(new SQLColumnItem("", 0));
            colItems.Add(new SQLColumnItem("", 0));
            colItems.Add(new SQLColumnItem("", 0));
            colItems.Add(new SQLColumnItem("", 0));
            colItems.Add(new SQLColumnItem("", 0));

            colItems.Add(new SQLColumnItem("", 0));
            colItems.Add(new SQLColumnItem("", 0));
            colItems.Add(new SQLColumnItem("", 0));

            Fields = dynamicFields_getFieldsForOrganisation(O, "", ref sql, false);
            foreach (DynamicField f in Fields) {
                if (
                       (
                        f.DataLink == "DGSContactFirstname" ||
                        f.DataLink == "DGSCOntactLastname" ||
                        f.DataLink == "DGSEmail" ||
                        f.DataLink == "DGSCOntactPhone" ||
                        (f.DataLink == "NNECompanyName" && f.BaseOrganisationId == 1) || //** Ekstra check Favrskov
                        f.DataLink == "NNECompanyCVR" ||

                        f.DataLink == "NNECompanyAddress1" ||
                        f.DataLink == "DGSCOntactAddress" ||
                        f.DataLink == "NNECompanyZipcode" ||
                        f.DataLink == "DGSCOntactZipcode" ||
                        f.DataLink == "NNECompanyCity" ||
                        f.DataLink == "DGSCOntactCity"
                       ) && f.OrganisationId == O.Id) {
                    string col = "";
                    if (f.DataSource.IndexOf('_') > 0) {
                        col = f.DataSource + "_" + f.DataSource.Split('_')[2];
                    } else {
                        col = f.DatabaseTable + "_" + f.DatabaseColumn + "_" + f.BaseOrganisationId;
                    }
                    //Add the columns desired in a sorted manor!
                    switch (f.DataLink) {
                        case "DGSContactFirstname":
                            colItems[0] = new SQLColumnItem(col, f.ListviewIndex);
                            break;

                        case "DGSCOntactLastname":
                            colItems[1] = new SQLColumnItem(col, f.ListviewIndex);
                            break;

                        case "DGSEmail":
                            colItems[2] = new SQLColumnItem(col, f.ListviewIndex);
                            break;

                        case "DGSCOntactPhone":
                            colItems[3] = new SQLColumnItem(col, f.ListviewIndex);
                            break;

                        case "NNECompanyName":
                            colItems[4] = new SQLColumnItem(col, f.ListviewIndex);
                            break;

                        case "NNECompanyCVR":
                            colItems[5] = new SQLColumnItem(col, f.ListviewIndex);
                            break;



                        case "NNECompanyAddress1":
                        case "DGSCOntactAddress":
                            if (colItems[6].SortingName == "") colItems[6] = new SQLColumnItem(col, f.ListviewIndex);
                            break;

                        case "NNECompanyZipcode":
                        case "DGSCOntactZipcode":
                            if (colItems[7].SortingName == "") colItems[7] = new SQLColumnItem(col, f.ListviewIndex);
                            break;

                        case "NNECompanyCity":
                        case "DGSCOntactCity":
                            if (colItems[8].SortingName == "") colItems[8] = new SQLColumnItem(col, f.ListviewIndex);
                            break;

                            //case "CompanyCPR":
                            //    colItems[6] = new SQLColumnItem(col, f.ListviewIndex);
                            //    break;

                    }
                }
            }


            string txtSQL = "" +
                            "CREATE PROCEDURE [dbo].[z_Contacts_ExtractToUserFinalcialPoolEvaluation_" + O.Id + "]\n" +
                            " @inDebug bit = 0,\n" +
                            " @extractAll bit = 0,\n" +
                            " @dateStart datetime = null,\n" +
                            " @dateEnd datetime = null\n" +
                            "AS\n" +
                            "   SELECT DISTINCT\n" +
                                "PartnerContacts.ContactId,\n" +
                                "Organisations.name as OrganisationName,\n" +
                                "Organisations.Id as OrganisationId,\n" +
                                "Users.firstname + ' ' + Users.lastName as Username,\n" +
                                "Users.Id as UserId,\n" +
                                "Users.email as UserEmail,\n" +
                                "Contacts.ContactType,\n" +
                                "Contacts.ContactId as SMVPOTConcatId,\n" +
                                "Contacts.CompanyOwnerId as SMVPOTCompanyId,\n" +
                                "\n" +
                                "Users.RecieveEmailOnExportToUserEvaluation,\n" +
                                "\n" +
                                "partnersToEvaluation.DateStamp as DateForEvaluation,\n" +
                                "partnersToEvaluation.Id as EvaluationId,\n" +
                                "\n" +
                                "PartnerContacts.Firstname as SAMFirstname,\n" +
                                "PartnerContacts.Lastname as SAMLastname,\n" +
                                "PartnerContacts.Email as SAMEmail,\n" +
                                "PartnerContacts.Phone1 as SAMPhone1,\n" +
                                "PartnerCompanies.CompanyName as SAMCompanyName,\n" +
                                "PartnerCompanies.CVR as SAMCVR,\n" +
                                "PartnerCompanies.Address as SAMAddress,\n" +
                                "PartnerCompanies.Zipcode as SAMZipcode,\n" +
                                "PartnerCompanies.City as SAMCity\n" +
                            "\n" +
                            SQLColumnItemsToString(colItems) +
                            "   FROM\n" +
                            "       [dbo].[ContactsAndCompaniesList_" + O.Id + "]() as Contacts,\n" +
                            "       contact_partners,\n" +
                            "       partnersToEvaluation,\n" +
                            "       PartnerContacts,\n" +
                            "       PartnerCompanies,\n" +
                            "       users,\n" +
                            "       organisations\n" +
                            "   WHERE \n" +
                                "partnersToEvaluation.ContactPartnerRelationId = contact_partners.Id and\n" +
                                "partnersToEvaluation.contactId = partnerContacts.contactId and\n" +
                                "partnerContacts.companyId = partnerCompanies.companyId and\n" +
                                "\n" +
                                "users.id = partnersToEvaluation.userId and\n" +
                                "organisations.Id = users.organisationId and\n" +
                                "organisations.id<>1 and\n" +
                                "\n" +
                                "Contacts.ContactId=partnersToEvaluation.SMVPOTContactId AND\n" +
                                "Contacts.CompanyOwnerId=partnersToEvaluation.SMVPOTCompanyId AND\n" +
                                "Contacts.ContactType = 0 and\n" +
                                "\n" +
                                "(partnersToEvaluation.exported is null or @extractAll=1) and\n" +
                                "(@dateStart <= partnersToEvaluation.DateStamp or @dateStart is null) and\n" +
                                "(@dateEnd > partnersToEvaluation.DateStamp or @dateEnd is null)\n" +

                                    "\n" +
                                    "	--update\n" +
                                    "   if (@inDebug=0) begin\n" +
                                    "       UPDATE partnersToEvaluation set Exported=getDate() where Exported is null AND\n" +
                                    "           (@dateStart <= partnersToEvaluation.DateStamp or @dateStart is null) and\n" +
                                    "           (@dateEnd > partnersToEvaluation.DateStamp or @dateEnd is null)\n" +
                                    "           update contact_partners set FinalcialPoolExported = getDate()\n" +
                                    "               where id in (select ContactPartnerRelationId from partnersToEvaluation where exported is not null) and FinalcialPoolExported is null\n" +
                                    "   end\n" +

                            "\n";

            //EXECUTE SQL!!!
            if (txtSQL != "" && txtSQL != null) {
                //(RE)CREATE THE PROCEDURE!!!
                sql.dynamicFields_dropStoredProcedure("z_Contacts_ExtractToUserFinalcialPoolEvaluation_" + O.Id);
                sql.dynamicFields_updateStoredProcedure(txtSQL);
            }
        }

        public static void rf_Contacts_ExtractToUserLocalEvaluation(ref SQLDB sql, Organisation O) {

            List<DynamicField> Fields = null; // dynamicFields_getFieldsForOrganisation(O);
            List<SQLColumnItem> colItems = new List<SQLColumnItem>();

            colItems.Add(new SQLColumnItem("", 0));
            colItems.Add(new SQLColumnItem("", 0));
            colItems.Add(new SQLColumnItem("", 0));
            colItems.Add(new SQLColumnItem("", 0));
            colItems.Add(new SQLColumnItem("", 0));
            colItems.Add(new SQLColumnItem("", 0));

            colItems.Add(new SQLColumnItem("", 0));
            colItems.Add(new SQLColumnItem("", 0));
            colItems.Add(new SQLColumnItem("", 0));

            Fields = dynamicFields_getFieldsForOrganisation(O, "", ref sql, false);
            foreach (DynamicField f in Fields) {
                if (
                       (
                        f.DataLink == "DGSContactFirstname" ||
                        f.DataLink == "DGSCOntactLastname" ||
                        f.DataLink == "DGSEmail" ||
                        f.DataLink == "DGSCOntactPhone" ||
                        (f.DataLink == "NNECompanyName" && f.BaseOrganisationId == 1) ||    //** Ekstra check Favrskov
                        f.DataLink == "NNECompanyCVR" ||

                        f.DataLink == "NNECompanyAddress1" ||
                        f.DataLink == "DGSCOntactAddress" ||
                        f.DataLink == "NNECompanyZipcode" ||
                        f.DataLink == "DGSCOntactZipcode" ||
                        f.DataLink == "NNECompanyCity" ||
                        f.DataLink == "DGSCOntactCity"
                       ) && f.OrganisationId == O.Id) {
                    string col = "";
                    if (f.DataSource.IndexOf('_') > 0) {
                        col = f.DataSource + "_" + f.DataSource.Split('_')[2];
                    } else {
                        col = f.DatabaseTable + "_" + f.DatabaseColumn + "_" + f.BaseOrganisationId;
                    }
                    //Add the columns desired in a sorted manor!
                    switch (f.DataLink) {
                        case "DGSContactFirstname":
                            colItems[0] = new SQLColumnItem(col, f.ListviewIndex);
                            break;

                        case "DGSCOntactLastname":
                            colItems[1] = new SQLColumnItem(col, f.ListviewIndex);
                            break;

                        case "DGSEmail":
                            colItems[2] = new SQLColumnItem(col, f.ListviewIndex);
                            break;

                        case "DGSCOntactPhone":
                            colItems[3] = new SQLColumnItem(col, f.ListviewIndex);
                            break;

                        case "NNECompanyName":
                            colItems[4] = new SQLColumnItem(col, f.ListviewIndex);
                            break;

                        case "NNECompanyCVR":
                            colItems[5] = new SQLColumnItem(col, f.ListviewIndex);
                            break;



                        case "NNECompanyAddress1":
                        case "DGSCOntactAddress":
                            if (colItems[6].SortingName == "") colItems[6] = new SQLColumnItem(col, f.ListviewIndex);
                            break;

                        case "NNECompanyZipcode":
                        case "DGSCOntactZipcode":
                            if (colItems[7].SortingName == "") colItems[7] = new SQLColumnItem(col, f.ListviewIndex);
                            break;

                        case "NNECompanyCity":
                        case "DGSCOntactCity":
                            if (colItems[8].SortingName == "") colItems[8] = new SQLColumnItem(col, f.ListviewIndex);
                            break;

                            //case "CompanyCPR":
                            //    colItems[6] = new SQLColumnItem(col, f.ListviewIndex);
                            //    break;

                    }
                }
            }


            string txtSQL = "" +
                            "CREATE PROCEDURE [dbo].[z_Contacts_ExtractToUserLocalEvaluation_" + O.Id + "]\n" +
                            " @inDebug bit = 0,\n" +
                            " @extractAll bit = 0,\n" +
                            " @dateStart datetime = null,\n" +
                            " @dateEnd datetime = null\n" +
                            "AS\n" +
                            "   SELECT DISTINCT\n" +
                            "       Contacts.ContactId,\n" +
                            "       Organisations.name as OrganisationName,\n" +
                            "       Organisations.Id as OrganisationId,\n" +
                            "       Users.firstname + ' ' + Users.lastName as Username,\n" +
                            "       Users.email,\n" +
                            "       Users.RecieveEmailOnExportToUserEvaluation,\n" +
                            "       Contacts.ContactType,\n" +
                            "       contactsToLocalEvaluation.DateStamp as DateForEvaluation\n" +
                            "\n" +
                            "\n" +
                            "\n" +
                            "\n" +
                            "\n" +
                            SQLColumnItemsToString(colItems) +
                            "   FROM\n" +
                            "       [dbo].[ContactsAndCompaniesAnyList_" + O.Id + "]() as Contacts,\n" +
                            "       Users,\n" +
                            "       Organisations,\n" +
                            "       contactsToLocalEvaluation\n" +
                            "   WHERE \n" +
                            "       (contactsToLocalEvaluation.exported is null or @extractAll=1) AND\n" +
                            "       contactsToLocalEvaluation.organisationId = Organisations.Id AND\n" +
                            "       contactsToLocalEvaluation.userId=users.Id AND\n" +
                            "       Users.organisationId=organisations.Id AND\n" +
                            "       Contacts.ContactId=contactsToLocalEvaluation.ContactId AND\n" +
                            "       --Contacts.CompanyOwnerId=contactsToLocalEvaluation.CompanyId AND\n" +

                            "       (@dateStart <= contactsToLocalEvaluation.DateStamp or @dateStart is null) AND\n" +
                            "       (@dateEnd > contactsToLocalEvaluation.DateStamp or @dateEnd is null) \n" +

                            "\n" +
                            "   if (@inDebug=0) begin\n" +
                            "       UPDATE contactsToLocalEvaluation set Exported=getDate() where Exported is null AND\n" +
                            "           (@dateStart <= contactsToLocalEvaluation.DateStamp or @dateStart is null) and\n" +
                            "           (@dateEnd > contactsToLocalEvaluation.DateStamp or @dateEnd is null)\n" +
                            "   end\n" +
                            "\n" +
                            "\n";

            //EXECUTE SQL!!!
            if (txtSQL != "" && txtSQL != null) {
                //(RE)CREATE THE PROCEDURE!!!
                sql.dynamicFields_dropStoredProcedure("z_Contacts_ExtractToUserLocalEvaluation_" + O.Id);
                sql.dynamicFields_updateStoredProcedure(txtSQL);
            }
        }

        public static void rf_Contacts_ExtractToUserEvaluation(ref SQLDB sql, Organisation O) {

            List<DynamicField> Fields = null; // dynamicFields_getFieldsForOrganisation(O);
            List<SQLColumnItem> colItems = new List<SQLColumnItem>();

            colItems.Add(new SQLColumnItem("", 0));
            colItems.Add(new SQLColumnItem("", 0));
            colItems.Add(new SQLColumnItem("", 0));
            colItems.Add(new SQLColumnItem("", 0));
            colItems.Add(new SQLColumnItem("", 0));
            colItems.Add(new SQLColumnItem("", 0));

            colItems.Add(new SQLColumnItem("", 0));
            colItems.Add(new SQLColumnItem("", 0));
            colItems.Add(new SQLColumnItem("", 0));

            Fields = dynamicFields_getFieldsForOrganisation(O, "", ref sql, false);
            foreach (DynamicField f in Fields) {
                if (
                       (
                        f.DataLink == "DGSContactFirstname" ||
                        f.DataLink == "DGSCOntactLastname" ||
                        f.DataLink == "DGSEmail" ||
                        f.DataLink == "DGSCOntactPhone" ||
                        (f.DataLink == "NNECompanyName" && f.BaseOrganisationId == 1) ||    //** Ekstra check Favrskov
                        f.DataLink == "NNECompanyCVR" ||

                        f.DataLink == "NNECompanyAddress1" ||
                        f.DataLink == "DGSCOntactAddress" ||
                        f.DataLink == "NNECompanyZipcode" ||
                        f.DataLink == "DGSCOntactZipcode" ||
                        f.DataLink == "NNECompanyCity" ||
                        f.DataLink == "DGSCOntactCity"
                       ) && f.OrganisationId == O.Id) {
                    string col = "";
                    if (f.DataSource.IndexOf('_') > 0) {
                        col = f.DataSource + "_" + f.DataSource.Split('_')[2];
                    } else {
                        col = f.DatabaseTable + "_" + f.DatabaseColumn + "_" + f.BaseOrganisationId;
                    }
                    //Add the columns desired in a sorted manor!
                    switch (f.DataLink) {
                        case "DGSContactFirstname":
                            colItems[0] = new SQLColumnItem(col, f.ListviewIndex);
                            break;

                        case "DGSCOntactLastname":
                            colItems[1] = new SQLColumnItem(col, f.ListviewIndex);
                            break;

                        case "DGSEmail":
                            colItems[2] = new SQLColumnItem(col, f.ListviewIndex);
                            break;

                        case "DGSCOntactPhone":
                            colItems[3] = new SQLColumnItem(col, f.ListviewIndex);
                            break;

                        case "NNECompanyName":
                            colItems[4] = new SQLColumnItem(col, f.ListviewIndex);
                            break;

                        case "NNECompanyCVR":
                            colItems[5] = new SQLColumnItem(col, f.ListviewIndex);
                            break;



                        case "NNECompanyAddress1":
                        case "DGSCOntactAddress":
                            if (colItems[6].SortingName == "") colItems[6] = new SQLColumnItem(col, f.ListviewIndex);
                            break;

                        case "NNECompanyZipcode":
                        case "DGSCOntactZipcode":
                            if (colItems[7].SortingName == "") colItems[7] = new SQLColumnItem(col, f.ListviewIndex);
                            break;

                        case "NNECompanyCity":
                        case "DGSCOntactCity":
                            if (colItems[8].SortingName == "") colItems[8] = new SQLColumnItem(col, f.ListviewIndex);
                            break;

                            //case "CompanyCPR":
                            //    colItems[6] = new SQLColumnItem(col, f.ListviewIndex);
                            //    break;

                    }
                }
            }


            string txtSQL = "" +
                            "CREATE PROCEDURE [dbo].[z_Contacts_ExtractToUserEvaluation_" + O.Id + "]\n" +
                            "   @inDebug    bit = 0,            \n" +
                            "   @extractAll bit = 0,            \n" +
                            "   @dateStart  datetime = null,    \n" +
                            "   @dateEnd    datetime = null,    \n" +
                            "   @type       varchar(25) = null  \n" +
                            "AS\n" +
                            "   SELECT DISTINCT\n" +
                            "       Contacts.ContactId,\n" +
                            "       Organisations.name as OrganisationName,\n" +
                            "       Organisations.Id as OrganisationId,\n" +
                            "       Users.firstname + ' ' + Users.lastName as Username,\n" +
                            "       Users.email,\n" +
                            "       Users.RecieveEmailOnExportToUserEvaluation,\n" +
                            "       Contacts.ContactType,\n" +
                            "       contactsToEvaluation.DateStamp as DateForEvaluation,\n" +
                            "       contactsToEvaluation.Type as EvaluationType\n" +
                            "\n" +
                            "\n" +
                            "\n" +
                            "\n" +
                            "\n" +
                            SQLColumnItemsToString(colItems) +
                            "   FROM\n" +
                            "       [dbo].[ContactsAndCompaniesAnyList_" + O.Id + "]() as Contacts,\n" +
                            "       Users,\n" +
                            "       Organisations,\n" +
                            "       contactsToEvaluation\n" +
                            "   WHERE \n" +
                            "       (contactsToEvaluation.exported is null or @extractAll=1) AND\n" +
                            "       contactsToEvaluation.organisationId = Organisations.Id AND\n" +
                            "       contactsToEvaluation.userId=users.Id AND\n" +
                            "       Users.organisationId=organisations.Id AND\n" +
                            "       Contacts.ContactId=contactsToEvaluation.ContactId AND\n" +
                            "       --Contacts.CompanyOwnerId=contactsToEvaluation.CompanyId AND\n" +

                            "       (@dateStart <= contactsToEvaluation.DateStamp or @dateStart is null) AND\n" +
                            "       (@dateEnd > contactsToEvaluation.DateStamp or @dateEnd is null)      AND\n" +
                            "       (contactsToEvaluation.[type]=@type or @type is null) \n" +

                            "\n" +
                            "   if (@inDebug=0) begin\n" +
                            "       UPDATE contactsToEvaluation set Exported=getDate() where Exported is null AND\n" +
                            "           (@dateStart <= contactsToEvaluation.DateStamp or @dateStart is null) and\n" +
                            "           (@dateEnd > contactsToEvaluation.DateStamp or @dateEnd is null)\n" +
                            "   end\n" +
                            "\n" +
                            "\n";

            //EXECUTE SQL!!!
            if (txtSQL != "" && txtSQL != null) {
                //(RE)CREATE THE PROCEDURE!!!
                sql.dynamicFields_dropStoredProcedure("z_Contacts_ExtractToUserEvaluation_" + O.Id);
                sql.dynamicFields_updateStoredProcedure(txtSQL);
            }
        }

        //REFACTOR GET DELETED COMPANIES
        public static void rf_Companies_GetDeletedCompanies(ref SQLDB sql, Organisation O) {

            List<DynamicField> Fields = null; // dynamicFields_getFieldsForOrganisation(O);
            List<SQLColumnItem> colItems = new List<SQLColumnItem>();

            colItems.Add(new SQLColumnItem("", 0));
            colItems.Add(new SQLColumnItem("", 0));

            Fields = dynamicFields_getFieldsForOrganisation(O, "", ref sql, false);
            foreach (DynamicField f in Fields) {
                if (
                       (
                        (f.DataLink == "NNECompanyName" && f.BaseOrganisationId == 1) ||    //** Ekstra check Favrskov
                        f.DataLink == "NNECompanyCVR"
                       ) && f.OrganisationId == O.Id) {
                    string col = "";
                    if (f.DataSource.IndexOf('_') > 0) {
                        col = f.DataSource + "_" + f.DataSource.Split('_')[2];
                    } else {
                        col = f.DatabaseTable + "_" + f.DatabaseColumn + "_" + f.BaseOrganisationId;
                    }
                    //Add the columns desired in a sorted manor!
                    switch (f.DataLink) {
                        case "NNECompanyName":
                            colItems[0] = new SQLColumnItem(col, f.ListviewIndex);
                            break;

                        case "NNECompanyCVR":
                            colItems[1] = new SQLColumnItem(col, f.ListviewIndex);
                            break;

                    }
                }
            }


            string txtSQL = "" +
                            "CREATE PROCEDURE [dbo].[z_Companies_GetDeletedCompanies_" + O.Id + "]\n" +
                            "AS\n" +
                            "   SELECT DISTINCT\n" +
                            "       Companies.CompanyId,\n" +
                            "       Users.firstname + ' ' + Users.lastName as deletedByUser,\n" +
                            "       Companies.CompanyDeletedDate,\n" +
                            "       Companies.CompanyType,\n" +
                            "       Companies.CompanyNNEId\n" +
                            "\n" +
                            "\n" +
                            "\n" +
                            "\n" +
                            "\n" +
                            SQLColumnItemsToString(colItems) +
                            "   FROM\n" +
                            "       [dbo].[CompaniesDeleted_" + O.Id + "]() as Companies,\n" +
                            "       Users\n" +
                            "   WHERE \n" +
                            "       (Companies.CompanyOrganisationId=" + O.Id + " or users.OrganisationId=" + O.Id + ") AND\n" +
                            "       Companies.CompanyDeletedBy=Users.Id\n" +
                            "\n";

            //EXECUTE SQL!!!
            if (txtSQL != "" && txtSQL != null) {
                //(RE)CREATE THE PROCEDURE!!!
                sql.dynamicFields_dropStoredProcedure("z_Companies_GetDeletedCompanies_" + O.Id);
                sql.dynamicFields_updateStoredProcedure(txtSQL);
            }
        }

        //REFACTOR GET DELETED CONTACTS
        public static void rf_Contacts_GetDeletedContacts(ref SQLDB sql, Organisation O) {

            List<DynamicField> Fields = null; // dynamicFields_getFieldsForOrganisation(O);
            List<SQLColumnItem> colItems = new List<SQLColumnItem>();

            colItems.Add(new SQLColumnItem("", 0));
            colItems.Add(new SQLColumnItem("", 0));
            colItems.Add(new SQLColumnItem("", 0));
            colItems.Add(new SQLColumnItem("", 0));
            colItems.Add(new SQLColumnItem("", 0));
            Fields = dynamicFields_getFieldsForOrganisation(O, "", ref sql, false);
            foreach (DynamicField f in Fields) {
                if (
                       (
                        f.DataLink == "CompanyCPR" ||
                        (f.DataLink == "NNECompanyName" && f.BaseOrganisationId == 1) ||    //** Ekstra check Favrskov
                        f.DataLink == "NNECompanyCVR" ||
                        f.DataLink == "DGSContactFirstname" ||
                        f.DataLink == "DGSCOntactLastname"
                       ) && f.OrganisationId == O.Id) {
                    string col = "";
                    if (f.DataSource.IndexOf('_') > 0) {
                        col = f.DataSource + "_" + f.DataSource.Split('_')[2];
                    } else {
                        col = f.DatabaseTable + "_" + f.DatabaseColumn + "_" + f.BaseOrganisationId;
                    }
                    //Add the columns desired in a sorted manor!
                    switch (f.DataLink) {
                        case "NNECompanyName":
                            colItems[0] = new SQLColumnItem(col, f.ListviewIndex);
                            break;

                        case "NNECompanyCVR":
                            colItems[1] = new SQLColumnItem(col, f.ListviewIndex);
                            break;

                        case "CompanyCPR":
                            colItems[2] = new SQLColumnItem(col, f.ListviewIndex);
                            break;

                        case "DGSContactFirstname":
                            colItems[3] = new SQLColumnItem(col, f.ListviewIndex);
                            break;

                        case "DGSCOntactLastname":
                            colItems[4] = new SQLColumnItem(col, f.ListviewIndex);
                            break;

                    }
                }
            }


            string txtSQL = "" +
                            "CREATE PROCEDURE [dbo].[z_Contacts_GetDeletedContacts_" + O.Id + "]\n" +
                            "AS\n" +
                            "   SELECT DISTINCT\n" +
                            "       Contacts.ContactId,\n" +
                            "       Contacts.CompanyId,\n" +
                            "       Users.firstname + ' ' + Users.lastName as deletedByUser,\n" +
                            "       Contacts.ContactDeletedDate,\n" +
                            "       Contacts.ContactType\n" +
                            "\n" +
                            "\n" +
                            "\n" +
                            "\n" +
                            "\n" +
                            SQLColumnItemsToString(colItems) +
                            "   FROM\n" +
                            "       [dbo].[ContactsAndCompaniesDeletedList_" + O.Id + "]() as Contacts,\n" +
                            "       Users\n" +
                            "   WHERE \n" +
                            "       (Contacts.ContactOrganisationId=" + O.Id + " or Users.Organisationid=" + O.Id + ")  AND\n" +
                            "       Contacts.ContactDeletedBy=Users.Id\n" +
                            "\n";

            //EXECUTE SQL!!!
            if (txtSQL != "" && txtSQL != null) {
                //(RE)CREATE THE PROCEDURE!!!
                sql.dynamicFields_dropStoredProcedure("z_Contacts_GetDeletedContacts_" + O.Id);
                sql.dynamicFields_updateStoredProcedure(txtSQL);
            }
        }

        //REFACTOR GET DELETED NOTES
        public static void rf_Contacts_GetDeletedNotes(ref SQLDB sql, Organisation O) {

            List<DynamicField> Fields = null; // dynamicFields_getFieldsForOrganisation(O);
            List<SQLColumnItem> colItems = new List<SQLColumnItem>();

            colItems.Add(new SQLColumnItem("", 0));
            colItems.Add(new SQLColumnItem("", 0));
            colItems.Add(new SQLColumnItem("", 0));
            colItems.Add(new SQLColumnItem("", 0));
            colItems.Add(new SQLColumnItem("", 0));
            Fields = dynamicFields_getFieldsForOrganisation(O, "", ref sql, false);
            foreach (DynamicField f in Fields) {
                if (
                       (
                        f.DataLink == "CompanyCPR" ||
                        (f.DataLink == "NNECompanyName" && f.BaseOrganisationId == 1) ||    //** Ekstra check Favrskov
                        f.DataLink == "NNECompanyCVR" ||
                        f.DataLink == "DGSContactFirstname" ||
                        f.DataLink == "DGSCOntactLastname"
                       ) && f.OrganisationId == O.Id) {
                    string col = "";
                    if (f.DataSource.IndexOf('_') > 0) {
                        col = f.DataSource + "_" + f.DataSource.Split('_')[2];
                    } else {
                        col = f.DatabaseTable + "_" + f.DatabaseColumn + "_" + f.BaseOrganisationId;
                    }
                    //Add the columns desired in a sorted manor!
                    switch (f.DataLink) {
                        case "NNECompanyName":
                            colItems[0] = new SQLColumnItem(col, f.ListviewIndex);
                            break;

                        case "NNECompanyCVR":
                            colItems[1] = new SQLColumnItem(col, f.ListviewIndex);
                            break;

                        case "CompanyCPR":
                            colItems[2] = new SQLColumnItem(col, f.ListviewIndex);
                            break;

                        case "DGSContactFirstname":
                            colItems[3] = new SQLColumnItem(col, f.ListviewIndex);
                            break;

                        case "DGSCOntactLastname":
                            colItems[4] = new SQLColumnItem(col, f.ListviewIndex);
                            break;

                    }
                }
            }


            string txtSQL = "" +
                            "CREATE PROCEDURE [dbo].[z_Contacts_GetDeletedNotes_" + O.Id + "]\n" +
                            "AS\n" +
                            "   SELECT DISTINCT\n" +
                            "       Contacts.ContactId,\n" +
                            "       Contacts.CompanyId,\n" +
                            "       Users.firstname + ' ' + Users.lastName as deletedByUser,\n" +
                            "       Contacts.ContactDeletedDate,\n" +
                            "       Contacts.ContactType,\n" +
                            "       Notes.Id as NoteId,\n" +
                            "       Notes.Name as NoteName,\n" +
                            "       Notes.DateCreated as NoteDate,\n" +
                            "       Notes.dateDeleted as DeletedDate,\n" +
                            "       Notes.ContactId as noteContactId,\n" +
                            "       Notes.CompanyId as noteCompanyId,\n" +
                            "       Notes.IsHighPriority as NoteIsHighPriority\n" +
                            "\n" +
                            SQLColumnItemsToString(colItems) +
                            "   FROM\n" +
                            "       [dbo].[ContactsAndCompaniesAnyList_" + O.Id + "]() as Contacts,\n" +
                            "       notes,\n" +
                            "       Users\n" +
                            "   WHERE \n" +
                            "       (notes.contactId = Contacts.contactId or notes.CompanyId = Contacts.CompanyId) AND\n" +
                            "       notes.dateDeleted is not null AND\n" +
                            "       notes.OrganisationId=" + O.Id + " AND\n" +
                            "       notes.DeletedBy=Users.Id\n" +
                            "\n";

            //EXECUTE SQL!!!
            if (txtSQL != "" && txtSQL != null) {
                //(RE)CREATE THE PROCEDURE!!!
                sql.dynamicFields_dropStoredProcedure("z_Contacts_GetDeletedNotes_" + O.Id);
                sql.dynamicFields_updateStoredProcedure(txtSQL);
            }
        }

        //REFACTOR FILEARCHIVE GET FILE BY FOLDER ID
        public static void rf_Filearchive_getFilesByFolderId(ref SQLDB sql, Organisation O) {
            List<DynamicField> Fields = null; // dynamicFields_getFieldsForOrganisation(O);
            List<SQLColumnItem> colItems = new List<SQLColumnItem>();
            string orderBy = "";


            Fields = dynamicFields_getFieldsForOrganisation(O, ref sql, true);
            foreach (DynamicField f in Fields) {    //** Ekstra check Favrskov
                if (((f.DataLink == "NNECompanyName" && f.BaseOrganisationId == 1) || f.DataLink == "DGSContactFirstname" || f.DataLink == "DGSCOntactLastname")
                        && f.OrganisationId == O.Id) {
                    string col = "";
                    if (f.DataSource.IndexOf('_') > 0) {
                        col = f.DataSource + "_" + f.DataSource.Split('_')[2];
                    } else {
                        col = f.DatabaseTable + "_" + f.DatabaseColumn + "_" + f.BaseOrganisationId;
                    }

                    //dynamically added columns
                    if (orderBy == "") {
                        orderBy = "[" + col + "]";
                    }

                    //Add the columns desired in a sorted manor!
                    bool inserted = false;
                    int index = f.ListviewIndex;

                    if (colItems.Count == 0) {
                        colItems.Add(new SQLColumnItem(col, f.ListviewIndex));
                    } else {
                        for (int ii = 0; ii < colItems.Count; ii++) {
                            int cpIndex = colItems[ii].ListIndex;
                            if (inserted == false && index < cpIndex) {
                                inserted = true;
                                colItems.Insert(ii > colItems.Count ? colItems.Count : ii, new SQLColumnItem(col, f.ListviewIndex));
                            }
                        }

                        if (inserted == false)
                            colItems.Add(new SQLColumnItem(col, f.ListviewIndex));
                    }
                }
            }
            string txtSQL = "" +
                           "CREATE PROCEDURE [dbo].[z_Filearchive_getFilesByFolderId_" + O.Id + "]\n" +
                           "    @folderId int,\n" +
                           "    @sortOrder varchar(50)='filename',\n" +
                           "    @sortAsc varchar(4)='asc'\n" +
                           "AS\n" +
                           "   SELECT \n" +
                            "       [FILE].Id,\n" +
                            "       [FILE].[filename],\n" +
                            "       dbo.getFiledescription([File].contenttype) as contenttype,\n" +
                            "       [FILE].[binary],\n" +
                            "       isnull([FILE].description,'Ingen') AS description,\n" +
                            "       [FILE].userId,\n" +
                            "       [FILE].FileFolder,\n" +
                            "       [FILE].contentgroup,\n" +
                            "       [FILE].contentlength,\n" +
                            "       [FILE].deletedDate,\n" +
                            "       [FILE].deletedBy,\n" +
                            "       [FILE].folderType,\n" +
                            "       [FILE].organisationId,\n" +
                            "       dbo.getDynamicTexts(Id) AS DynamicText,\n" +
                            "       dbo.getUsername(userId) AS Username,\n" +
                            "       dateCreated,\n" +
                            "       [FILE].contactId\n" +
                            SQLColumnItemsToString(colItems) +
                           "   FROM\n" +
                           "       [File]\n" +
                           "        LEFT JOIN [dbo].[ContactsAndCompaniesList_" + O.Id + "]() Contacts on [FILE].ContactId = Contacts.ContactId \n" +
                           "   WHERE \n" +
                           "       FileFolder = @folderId AND\n" +
                           "       deletedDate is NULL\n" +
                           "   ORDER BY\n" +
                           "        CASE\n" +
                           "        WHEN @sortOrder='filename' AND @sortAsc='asc' THEN [filename]\n" +
                           "        END ASC,\n" +
                           "        CASE\n" +
                           "        WHEN @sortOrder='filename' AND @sortAsc='desc' THEN [filename]\n" +
                           "        END DESC,\n" +
                           "        CASE\n" +
                           "        WHEN @sortOrder='description' AND @sortAsc='desc' THEN description\n" +
                           "        END DESC,\n" +
                           "        CASE\n" +
                           "        WHEN @sortOrder='description' AND @sortAsc='asc' THEN description\n" +
                           "        END ASC,\n" +
                           "        CASE\n" +
                           "        WHEN @sortOrder='contenttype' AND @sortAsc='asc' THEN dbo.getFiledescription(contenttype)\n" +
                           "        END ASC,\n" +
                           "        CASE\n" +
                           "        WHEN @sortOrder='contenttype' AND @sortAsc='desc' THEN dbo.getFiledescription(contenttype)\n" +
                           "        END DESC,\n" +
                           "        CASE\n" +
                           "        WHEN @sortOrder='contentlength' AND @sortAsc='asc' THEN contentlength\n" +
                           "        END ASC,\n" +
                           "        CASE\n" +
                           "        WHEN @sortOrder='contentlength' AND @sortAsc='desc' THEN contentlength\n" +
                           "        END DESC,\n" +
                           "        CASE\n" +
                           "        WHEN @sortOrder='dynamictext' AND @sortAsc='asc' THEN dbo.getDynamicTexts(Id)\n" +
                           "        END ASC,\n" +
                           "        CASE\n" +
                           "        WHEN @sortOrder='dynamictext' AND @sortAsc='desc' THEN dbo.getDynamicTexts(Id)\n" +
                           "        END DESC,\n" +
                           "        CASE\n" +
                           "        WHEN @sortOrder='username' AND @sortAsc='asc' THEN dbo.getusername(userId)\n" +
                           "        END ASC,\n" +
                           "        CASE\n" +
                           "        WHEN @sortOrder='username' AND @sortAsc='desc' THEN dbo.getusername(userId)\n" +
                           "        END DESC,\n" +
                           "        CASE\n" +
                           "        WHEN @sortOrder='datecreated' AND @sortAsc='asc' THEN Convert(char(12), dateCreated, 104)\n" +
                           "        END ASC,\n" +
                           "        CASE\n" +
                           "        WHEN @sortOrder='datecreated' AND @sortAsc='desc' THEN Convert(char(12), dateCreated, 104)\n" +
                           "        END DESC,\n" +
                           "        CASE\n" +
                           "        WHEN @sortOrder='contactperson' AND @sortAsc='asc' THEN " + orderBy + " \n " +
                           "        END ASC,\n" +
                           "        CASE\n" +
                           "        WHEN @sortOrder='contactperson' AND @sortAsc='desc' THEN " + orderBy + " \n " +
                           "        END DESC\n" +
                           "\n" +
                           "\n";

            //EXECUTE SQL!!!
            if (txtSQL != "" && txtSQL != null) {
                //(RE)CREATE THE PROCEDURE!!!
                sql.dynamicFields_dropStoredProcedure("z_Filearchive_getFilesByFolderId_" + O.Id);
                sql.dynamicFields_updateStoredProcedure(txtSQL);
            }
        }

        //REFACTOR FILEARCHIVE GET FILE BY ORG ID AND FOLDERTYPE
        public static void rf_Filearchive_getFilesByOrgIdAndFolderType(ref SQLDB sql, Organisation O) {
            List<DynamicField> Fields = null; // dynamicFields_getFieldsForOrganisation(O);
            List<SQLColumnItem> colItems = new List<SQLColumnItem>();
            string orderBy = "";


            Fields = dynamicFields_getFieldsForOrganisation(O, ref sql, true);
            foreach (DynamicField f in Fields) {    //** Ekstra check Favrskov
                if (((f.DataLink == "NNECompanyName" && f.BaseOrganisationId == 1) || f.DataLink == "DGSContactFirstname" || f.DataLink == "DGSCOntactLastname")
                        && f.OrganisationId == O.Id) {
                    string col = "";
                    if (f.DataSource.IndexOf('_') > 0) {
                        col = f.DataSource + "_" + f.DataSource.Split('_')[2];
                    } else {
                        col = f.DatabaseTable + "_" + f.DatabaseColumn + "_" + f.BaseOrganisationId;
                    }

                    //dynamically added columns
                    if (orderBy == "") {
                        orderBy = "[" + col + "]";
                    }

                    //Add the columns desired in a sorted manor!
                    bool inserted = false;
                    int index = f.ListviewIndex;

                    if (colItems.Count == 0) {
                        colItems.Add(new SQLColumnItem(col, f.ListviewIndex));
                    } else {
                        for (int ii = 0; ii < colItems.Count; ii++) {
                            int cpIndex = colItems[ii].ListIndex;
                            if (inserted == false && index < cpIndex) {
                                inserted = true;
                                colItems.Insert(ii > colItems.Count ? colItems.Count : ii, new SQLColumnItem(col, f.ListviewIndex));
                            }
                        }

                        if (inserted == false)
                            colItems.Add(new SQLColumnItem(col, f.ListviewIndex));
                    }
                }
            }
            string txtSQL = "" +
                           "CREATE PROCEDURE [dbo].[z_Filearchive_getFilesByOrgIdAndFolderType_" + O.Id + "]\n" +
                           "    @folderType int,\n" +
                           "    @OrgId int = null,\n" +
                           "    @sortOrder varchar(50)='filename',\n" +
                           "    @sortAsc varchar(4)='asc'\n" +
                           "AS\n" +
                           "   SELECT \n" +
                            "       [FILE].Id,\n" +
                            "       [FILE].[filename],\n" +
                            "       dbo.getFiledescription([File].contenttype) as contenttype,\n" +
                            "       [FILE].[binary],\n" +
                            "       isnull([FILE].description,'Ingen') AS description,\n" +
                            "       [FILE].userId,\n" +
                            "       [FILE].FileFolder,\n" +
                            "       [FILE].contentgroup,\n" +
                            "       [FILE].contentlength,\n" +
                            "       [FILE].deletedDate,\n" +
                            "       [FILE].deletedBy,\n" +
                            "       [FILE].folderType,\n" +
                            "       [FILE].organisationId,\n" +
                            "       dbo.getDynamicTexts(Id) AS DynamicText,\n" +
                            "       dbo.getUsername(userId) AS Username,\n" +
                            "       dateCreated,\n" +
                            "       [FILE].contactId\n" +
                           SQLColumnItemsToString(colItems) +
                           "   FROM\n" +
                           "       [File]\n" +
                           "        LEFT JOIN [dbo].[ContactsAndCompaniesList_" + O.Id + "]() Contacts on [FILE].ContactId = Contacts.ContactId \n" +
                           "       WHERE \n" +
                           "        (@folderType = 10 AND folderType = @folderType  AND deletedDate IS NULL AND FileFolder IS null) OR\n" +
                           "        (@folderType = 0 AND folderType = @folderType  AND deletedDate IS NULL AND FileFolder IS null) OR\n" +
                           "        (@folderType = 1 AND (folderType = @folderType AND organisationId = @OrgId AND deletedDate IS NULL AND FileFolder IS null))OR\n" +
                           "        (@folderType = 2 AND (folderType = @folderType AND organisationId = @OrgId AND deletedDate IS NULL AND FileFolder IS null))\n" +
                           "   ORDER BY\n" +
                           "        CASE\n" +
                           "        WHEN @sortOrder='filename' AND @sortAsc='asc' THEN [filename]\n" +
                           "        END ASC,\n" +
                           "        CASE\n" +
                           "        WHEN @sortOrder='filename' AND @sortAsc='desc' THEN [filename]\n" +
                           "        END DESC,\n" +
                           "        CASE\n" +
                           "        WHEN @sortOrder='description' AND @sortAsc='desc' THEN description\n" +
                           "        END DESC,\n" +
                           "        CASE\n" +
                           "        WHEN @sortOrder='description' AND @sortAsc='asc' THEN description\n" +
                           "        END ASC,\n" +
                           "        CASE\n" +
                           "        WHEN @sortOrder='contenttype' AND @sortAsc='asc' THEN dbo.getFiledescription(contenttype)\n" +
                           "        END ASC,\n" +
                           "        CASE\n" +
                           "        WHEN @sortOrder='contenttype' AND @sortAsc='desc' THEN dbo.getFiledescription(contenttype)\n" +
                           "        END DESC,\n" +
                           "        CASE\n" +
                           "        WHEN @sortOrder='contentlength' AND @sortAsc='asc' THEN contentlength\n" +
                           "        END ASC,\n" +
                           "        CASE\n" +
                           "        WHEN @sortOrder='contentlength' AND @sortAsc='desc' THEN contentlength\n" +
                           "        END DESC,\n" +
                           "        CASE\n" +
                           "        WHEN @sortOrder='dynamictext' AND @sortAsc='asc' THEN dbo.getDynamicTexts(Id)\n" +
                           "        END ASC,\n" +
                           "        CASE\n" +
                           "        WHEN @sortOrder='dynamictext' AND @sortAsc='desc' THEN dbo.getDynamicTexts(Id)\n" +
                           "        END DESC,\n" +
                           "        CASE\n" +
                           "        WHEN @sortOrder='username' AND @sortAsc='asc' THEN dbo.getusername(userId)\n" +
                           "        END ASC,\n" +
                           "        CASE\n" +
                           "        WHEN @sortOrder='username' AND @sortAsc='desc' THEN dbo.getusername(userId)\n" +
                           "        END DESC,\n" +
                           "        CASE\n" +
                           "        WHEN @sortOrder='datecreated' AND @sortAsc='asc' THEN Convert(char(12), dateCreated, 104)\n" +
                           "        END ASC,\n" +
                           "        CASE\n" +
                           "        WHEN @sortOrder='datecreated' AND @sortAsc='desc' THEN Convert(char(12), dateCreated, 104)\n" +
                           "        END DESC,\n" +
                           "        CASE\n" +
                           "        WHEN @sortOrder='contactperson' AND @sortAsc='asc' THEN " + orderBy + " \n " +
                           "        END ASC,\n" +
                           "        CASE\n" +
                           "        WHEN @sortOrder='contactperson' AND @sortAsc='desc' THEN " + orderBy + " \n " +
                           "        END DESC\n" +
                           "\n" +
                           "\n";

            //EXECUTE SQL!!!
            if (txtSQL != "" && txtSQL != null) {
                //(RE)CREATE THE PROCEDURE!!!
                sql.dynamicFields_dropStoredProcedure("z_Filearchive_getFilesByOrgIdAndFolderType_" + O.Id);
                sql.dynamicFields_updateStoredProcedure(txtSQL);
            }
        }

        //REFACTOR FILEARCHIVE GET FILE BY USER AND FOLDERTYPE
        public static void rf_Filearchive_getFilesByUserAndFolderType(ref SQLDB sql, Organisation O) {
            List<DynamicField> Fields = null; // dynamicFields_getFieldsForOrganisation(O);
            List<SQLColumnItem> colItems = new List<SQLColumnItem>();
            string orderBy = "";


            Fields = dynamicFields_getFieldsForOrganisation(O, ref sql, true);
            foreach (DynamicField f in Fields) {    //** Ekstra check Favrskov
                if (((f.DataLink == "NNECompanyName" && f.BaseOrganisationId == 1) || f.DataLink == "DGSContactFirstname" || f.DataLink == "DGSCOntactLastname")
                        && f.OrganisationId == O.Id) {
                    string col = "";
                    if (f.DataSource.IndexOf('_') > 0) {
                        col = f.DataSource + "_" + f.DataSource.Split('_')[2];
                    } else {
                        col = f.DatabaseTable + "_" + f.DatabaseColumn + "_" + f.BaseOrganisationId;
                    }

                    //dynamically added columns
                    if (orderBy == "") {
                        orderBy = "[" + col + "]";
                    }

                    //Add the columns desired in a sorted manor!
                    bool inserted = false;
                    int index = f.ListviewIndex;

                    if (colItems.Count == 0) {
                        colItems.Add(new SQLColumnItem(col, f.ListviewIndex));
                    } else {
                        for (int ii = 0; ii < colItems.Count; ii++) {
                            int cpIndex = colItems[ii].ListIndex;
                            if (inserted == false && index < cpIndex) {
                                inserted = true;
                                colItems.Insert(ii > colItems.Count ? colItems.Count : ii, new SQLColumnItem(col, f.ListviewIndex));
                            }
                        }

                        if (inserted == false)
                            colItems.Add(new SQLColumnItem(col, f.ListviewIndex));
                    }
                }
            }
            string txtSQL = "" +
                           "CREATE PROCEDURE [dbo].[z_Filearchive_getFilesByUserAndFolderType_" + O.Id + "]\n" +
                           "    @folderType int,\n" +
                           "    @userId UNIQUEIDENTIFIER = null,\n" +
                           "    @sortOrder varchar(50)='filename',\n" +
                           "    @sortAsc varchar(4)='asc'\n" +
                           "AS\n" +
                           "   SELECT \n" +
                            "       [FILE].Id,\n" +
                            "       [FILE].[filename],\n" +
                            "       dbo.getFiledescription([File].contenttype) as contenttype,\n" +
                            "       [FILE].[binary],\n" +
                            "       isnull([FILE].description,'Ingen') AS description,\n" +
                            "       [FILE].userId,\n" +
                            "       [FILE].FileFolder,\n" +
                            "       [FILE].contentgroup,\n" +
                            "       [FILE].contentlength,\n" +
                            "       [FILE].deletedDate,\n" +
                            "       [FILE].deletedBy,\n" +
                            "       [FILE].folderType,\n" +
                            "       [FILE].organisationId,\n" +
                            "       dbo.getDynamicTexts(Id) AS DynamicText,\n" +
                            "       dbo.getUsername(userId) AS Username,\n" +
                            "       dateCreated,\n" +
                            "       [FILE].contactId\n" +
                            SQLColumnItemsToString(colItems) +
                           "   FROM\n" +
                           "       [File]\n" +
                           "        LEFT JOIN [dbo].[ContactsAndCompaniesList_" + O.Id + "]() Contacts on [FILE].ContactId = Contacts.ContactId \n" +
                           "       WHERE \n" +
                           "        FolderType = @FolderType AND\n" +
                           "        userId = @userId AND\n" +
                           "        FileFolder IS NULL AND\n" +
                           "        deletedDate IS NULL\n" +
                           "   ORDER BY\n" +
                           "        CASE\n" +
                           "        WHEN @sortOrder='filename' AND @sortAsc='asc' THEN [filename]\n" +
                           "        END ASC,\n" +
                           "        CASE\n" +
                           "        WHEN @sortOrder='filename' AND @sortAsc='desc' THEN [filename]\n" +
                           "        END DESC,\n" +
                           "        CASE\n" +
                           "        WHEN @sortOrder='description' AND @sortAsc='desc' THEN description\n" +
                           "        END DESC,\n" +
                           "        CASE\n" +
                           "        WHEN @sortOrder='description' AND @sortAsc='asc' THEN description\n" +
                           "        END ASC,\n" +
                           "        CASE\n" +
                           "        WHEN @sortOrder='contenttype' AND @sortAsc='asc' THEN dbo.getFiledescription(contenttype)\n" +
                           "        END ASC,\n" +
                           "        CASE\n" +
                           "        WHEN @sortOrder='contenttype' AND @sortAsc='desc' THEN dbo.getFiledescription(contenttype)\n" +
                           "        END DESC,\n" +
                           "        CASE\n" +
                           "        WHEN @sortOrder='contentlength' AND @sortAsc='asc' THEN contentlength\n" +
                           "        END ASC,\n" +
                           "        CASE\n" +
                           "        WHEN @sortOrder='contentlength' AND @sortAsc='desc' THEN contentlength\n" +
                           "        END DESC,\n" +
                           "        CASE\n" +
                           "        WHEN @sortOrder='dynamictext' AND @sortAsc='asc' THEN dbo.getDynamicTexts(Id)\n" +
                           "        END ASC,\n" +
                           "        CASE\n" +
                           "        WHEN @sortOrder='dynamictext' AND @sortAsc='desc' THEN dbo.getDynamicTexts(Id)\n" +
                           "        END DESC,\n" +
                           "        CASE\n" +
                           "        WHEN @sortOrder='username' AND @sortAsc='asc' THEN dbo.getusername(userId)\n" +
                           "        END ASC,\n" +
                           "        CASE\n" +
                           "        WHEN @sortOrder='username' AND @sortAsc='desc' THEN dbo.getusername(userId)\n" +
                           "        END DESC,\n" +
                           "        CASE\n" +
                           "        WHEN @sortOrder='datecreated' AND @sortAsc='asc' THEN Convert(char(12), dateCreated, 104)\n" +
                           "        END ASC,\n" +
                           "        CASE\n" +
                           "        WHEN @sortOrder='datecreated' AND @sortAsc='desc' THEN Convert(char(12), dateCreated, 104)\n" +
                           "        END DESC,\n" +
                           "        CASE\n" +
                           "        WHEN @sortOrder='contactperson' AND @sortAsc='asc' THEN " + orderBy + " \n " +
                           "        END ASC,\n" +
                           "        CASE\n" +
                           "        WHEN @sortOrder='contactperson' AND @sortAsc='desc' THEN " + orderBy + " \n " +
                           "        END DESC\n" +
                           "\n" +
                           "\n";

            //EXECUTE SQL!!!
            if (txtSQL != "" && txtSQL != null) {
                //(RE)CREATE THE PROCEDURE!!!
                sql.dynamicFields_dropStoredProcedure("z_Filearchive_getFilesByUserAndFolderType_" + O.Id);
                sql.dynamicFields_updateStoredProcedure(txtSQL);
            }
        }

        //REFACTOR FILEARCHIVE QUICKSEARCH
        public static void rf_Filearchive_Quicksearch(ref SQLDB sql, Organisation O) {
            List<DynamicField> Fields = null; // dynamicFields_getFieldsForOrganisation(O);
            List<SQLColumnItem> colItems = new List<SQLColumnItem>();
            string query = "";
            string columnName = "";
            string orderBy = "";


            Fields = dynamicFields_getFieldsForOrganisation(O, ref sql, true);
            foreach (DynamicField f in Fields) {    //** Ekstra check Favrskov
                if (((f.DataLink == "NNECompanyName" && f.BaseOrganisationId == 1) || f.DataLink == "DGSContactFirstname" || f.DataLink == "DGSCOntactLastname")
                        && f.OrganisationId == O.Id) {
                    string col = "";
                    if (f.DataSource.IndexOf('_') > 0) {
                        col = f.DataSource + "_" + f.DataSource.Split('_')[2];
                    } else {
                        col = f.DatabaseTable + "_" + f.DatabaseColumn + "_" + f.BaseOrganisationId;
                    }

                    //column names added dynamically
                    query += "" + (query == "" ? "" : "    OR ") + "\t(convert(varchar(max),[" + col + "]) LIKE '%' + @query + '%')\n";
                    if (columnName == "") {
                        columnName = "AND ([" + col + "] IS NOT NULL)";
                    }
                    if (orderBy == "") {
                        orderBy = "[" + col + "]";
                    }

                    //Add the columns desired in a sorted manor!
                    bool inserted = false;
                    int index = f.ListviewIndex;

                    if (colItems.Count == 0) {
                        colItems.Add(new SQLColumnItem(col, f.ListviewIndex));
                    } else {
                        for (int ii = 0; ii < colItems.Count; ii++) {
                            int cpIndex = colItems[ii].ListIndex;
                            if (inserted == false && index < cpIndex) {
                                inserted = true;
                                colItems.Insert(ii > colItems.Count ? colItems.Count : ii, new SQLColumnItem(col, f.ListviewIndex));
                            }
                        }

                        if (inserted == false)
                            colItems.Add(new SQLColumnItem(col, f.ListviewIndex));
                    }
                }
            }
            string txtSQL = "" +
                           "CREATE PROCEDURE [dbo].[z_Filearchive_Quicksearch_" + O.Id + "]\n" +
                           "    @organisationId int,\n" +
                           "    @userId UNIQUEIDENTIFIER,\n" +
                           "    @query varchar(500)='',\n" +
                           "    --doc,xls,pdf osv.\n" +
                           "    @type varchar(50)='filename',\n" +
                           "    --alle=0, org=1, mine dok=2,\n" +
                           "    @searchIn int=null,\n" +
                           "    @sortOrder varchar(50)='filename',\n" +
                           "    @sortAsc varchar(4)='asc'\n" +
                           "AS\n" +
                           "   SELECT \n" +
                            "       [File].Id,\n" +
                            "       [File].[filename],\n" +
                            "       dbo.getFiledescription([File].contenttype) as contenttype,\n" +
                            "       [File].[binary],\n" +
                            "       isnull(description,'Ingen') AS description,\n" +
                            "       [FILE].userId,\n" +
                            "       [FILE].FileFolder,\n" +
                            "       [FILE].contentgroup,\n" +
                            "       [FILE].contentlength,\n" +
                            "       [FILE].deletedDate,\n" +
                            "       [FILE].deletedBy,\n" +
                            "       [FILE].folderType,\n" +
                            "       [FILE].organisationId,\n" +
                            "       dbo.getDynamicTexts(Id) AS DynamicText,\n" +
                            "       dbo.getUsername(userId) AS Username,\n" +
                            "       dateCreated,\n" +
                            "       [FILE].contactId,\n" +
                            "       dbo.getPath(FileFolder,@searchIn,organisationId, id) AS [path]\n" +
                            SQLColumnItemsToString(colItems) +
                           "   FROM\n" +
                           "       [File]\n" +
                           "        LEFT JOIN [dbo].[ContactsAndCompaniesList_" + O.Id + "]() Contacts on [FILE].ContactId = Contacts.ContactId \n" +
                           "       WHERE \n" +
                           "       (\n" +
                           "            \t(@searchIn=10 AND deletedBy IS NULL AND folderType=@searchIn) OR\n" +
                           "            \t(@searchIn=0 AND deletedBy IS NULL AND folderType=@searchIn)\n" +
                           "            \tOR\n" +
                           "            \t(@searchIn=1 AND deletedBy IS NULL AND folderType=@searchIN AND organisationId = @organisationId)\n" +
                           "            \tOR\n" +
                           "            \t(@searchIn=2 AND userId=@userId AND deletedBy IS NULL AND folderType=@searchIn AND organisationId = @organisationId)\n" +
                           "            \tOR\n" +
                           "            \t--Administratorer kan søge i mine dokumenter folderne for alle konsulenterne i organisationen\n" +
                           "            \t(@searchIn=3 AND deletedBy IS NULL AND folderType=2 AND organisationId = @organisationId)\n" +
                           "       )\n" +
                           "        AND\n" +
                           "        (\n" +
                           "                \t(@type is not NULL AND @type !='office' AND @type=contenttype)\n" +
                           "                \tOR\n" +
                           "                \t(@type IS null)\n" +
                           "                \tOR\n" +
                           "                \t(@type = 'office' AND (contenttype = 'doc' or contenttype='xls' or contenttype='ppt'or contenttype='mdb' or contenttype='pub'))\n" +
                           "                \t--søg emneord\n" +
                           "                \tOR\n " +
                           "                \t(@type = 'emneord' AND (dbo.getDynamicTexts(Id) <>''))\n" +
                           "                \tOR\n " +
                           "                \t(@type = 'kontaktperson' " + columnName + ")\n" +
                           "        )\n" +
                           "        AND\n" +
                           "        (\n" +
                           "            \t(convert(varchar(max),[description]) LIKE '%'+@query+'%')\n" +
                           "            \tOR\n" +
                           "            \t((convert(varchar(50),[filename]) LIKE '%'+@query+'%') AND convert(varchar(50),isnull([filename],''))<>'')\n" +
                           "            \t--søg emneord \n" +
                           "            \tOR\n" +
                           "            \t(convert(varchar(max),dbo.getDynamicTexts(Id)) LIKE '%'+@query+'%')\n" +
                           "            \t--søg i kontaktperson \n" +
                           "            \tOR\n" +
                           "            \t" + query + "\n" +
                           "        )\n" +
                           "   ORDER BY\n" +
                           "        CASE\n" +
                           "        WHEN @sortOrder='filename' AND @sortAsc='asc' THEN [filename]\n" +
                           "        END ASC,\n" +
                           "        CASE\n" +
                           "        WHEN @sortOrder='filename' AND @sortAsc='desc' THEN [filename]\n" +
                           "        END DESC,\n" +
                           "        CASE\n" +
                           "        WHEN @sortOrder='description' AND @sortAsc='desc' THEN description\n" +
                           "        END DESC,\n" +
                           "        CASE\n" +
                           "        WHEN @sortOrder='description' AND @sortAsc='asc' THEN description\n" +
                           "        END ASC,\n" +
                           "        CASE\n" +
                           "        WHEN @sortOrder='contenttype' AND @sortAsc='asc' THEN dbo.getFiledescription(contenttype)\n" +
                           "        END ASC,\n" +
                           "        CASE\n" +
                           "        WHEN @sortOrder='contenttype' AND @sortAsc='desc' THEN dbo.getFiledescription(contenttype)\n" +
                           "        END DESC,\n" +
                           "        CASE\n" +
                           "        WHEN @sortOrder='contentlength' AND @sortAsc='asc' THEN contentlength\n" +
                           "        END ASC,\n" +
                           "        CASE\n" +
                           "        WHEN @sortOrder='contentlength' AND @sortAsc='desc' THEN contentlength\n" +
                           "        END DESC,\n" +
                           "        CASE\n" +
                           "        WHEN @sortOrder='path' AND @sortAsc='desc' THEN dbo.getPath(FileFolder,@searchIn,organisationId, id)\n" +
                           "        END DESC,\n" +
                           "        CASE\n" +
                           "        WHEN @sortOrder='path' AND @sortAsc='asc' THEN dbo.getPath(FileFolder,@searchIn,organisationId, id)\n" +
                           "        END ASC,\n" +
                           "        CASE\n" +
                           "        WHEN @sortOrder='dynamictext' AND @sortAsc='asc' THEN dbo.getDynamicTexts(Id)\n" +
                           "        END ASC,\n" +
                           "        CASE\n" +
                           "        WHEN @sortOrder='dynamictext' AND @sortAsc='desc' THEN dbo.getDynamicTexts(Id)\n" +
                           "        END DESC,\n" +
                           "        CASE\n" +
                           "        WHEN @sortOrder='username' AND @sortAsc='asc' THEN dbo.getusername(userId)\n" +
                           "        END ASC,\n" +
                           "        CASE\n" +
                           "        WHEN @sortOrder='username' AND @sortAsc='desc' THEN dbo.getusername(userId)\n" +
                           "        END DESC,\n" +
                           "        CASE\n" +
                           "        WHEN @sortOrder='datecreated' AND @sortAsc='asc' THEN Convert(char(12), dateCreated, 104)\n" +
                           "        END ASC,\n" +
                           "        CASE\n" +
                           "        WHEN @sortOrder='datecreated' AND @sortAsc='desc' THEN Convert(char(12), dateCreated, 104)\n" +
                           "        END DESC,\n" +
                           "        CASE\n" +
                           "        WHEN @sortOrder='contactperson' AND @sortAsc='asc' THEN " + orderBy + " \n " +
                           "        END ASC,\n" +
                           "        CASE\n" +
                           "        WHEN @sortOrder='contactperson' AND @sortAsc='desc' THEN " + orderBy + " \n " +
                           "        END DESC\n" +
                           "\n" +
                           "\n";

            //EXECUTE SQL!!!
            if (txtSQL != "" && txtSQL != null) {
                //(RE)CREATE THE PROCEDURE!!!
                sql.dynamicFields_dropStoredProcedure("z_Filearchive_Quicksearch_" + O.Id);
                sql.dynamicFields_updateStoredProcedure(txtSQL);
            }

        }

        public static string GetSortCast(string fieldType, string col) {
            if (fieldType.ToLower() == "memo" || fieldType.ToLower() == "listview" || fieldType.ToLower() == "dropdown" || fieldType.ToLower() == "externaldatalink") {
                return "convert(varchar(8000),[" + col + "])";
            } else
                return "[" + col + "]";

        }
    }
}
