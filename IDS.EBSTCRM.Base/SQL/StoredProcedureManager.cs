using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace IDS.EBSTCRM.Base
{
    /// <summary>
    /// Stored procedure Refactor
    /// OBSOLETE
    /// </summary>
    public class StoredProcedureReplacement
    {
        public string arguments = "";
        public string update_or_insert_statement;
        public string tables = "";
        public string tableConditions = "";
        public string tableColumns = "";
        public string searchArguments = "";
        public string searchConditions = "";

        public string quickSearchArguments = "";
        public string quickSearchConditions = "";

        public StoredProcedureReplacement()
        {}
    }

    public static class StoredProcedureManager
    {
        private static string tabs = "\t\t\t\t";

        //private static void RefactorOrganisation(ref SQLDB sql, int organisationId)
        //{
        //    foreach (string sproc in sql.dynamicFields_getGenericStoredProcedures())
        //    {
        //        RefactorStoredProcedure(ref sql, sproc, organisationId);
        //    }
        //}

        //public static string RefactorStoredProcedure(ref SQLDB sql, string storedProcedure, int organisationId)
        //{
        //    if (storedProcedure == "" || storedProcedure == null || storedProcedure == " ") return "";
        //    string sproc = sql.dynamicFields_getStoredProcedure(storedProcedure);
        //    Organisation O = sql.Organisations_getOrganisation(organisationId);
        //    StoredProcedureReplacement replacements = new StoredProcedureReplacement();
        //    string tableName = getTableName(storedProcedure);
        //    //Recursive refactor the stored procedure
        //    RefactorSegment(ref sql, ref O, ref replacements, tableName);


        //    //static items
        //    replacements.quickSearchArguments = ",\r\n@query varchar(8000)";

        //    //Do replacements!
        //    sproc = sproc.Replace("--%tables%--", "\r\n" + tabs + "/* GENERATED TABLES */" + replacements.tables + "\r\n" + tabs + "/* END OF GENERATED TABLES */");
        //    sproc = sproc.Replace("--%tableconditions%--", "\r\n" + tabs + "/* GENERATED TABLE CONDITIONS */" + replacements.tableConditions + "\r\n" + tabs + "/* END OF GENERATED TABLE CONDITIONS */");
        //    sproc = sproc.Replace("--%tablecolumns%--", "\r\n" + tabs + "/* GENERATED TABLE COLUMNS */" + replacements.tableColumns + "\r\n" + tabs + "/* END OF GENERATED TABLE COLUMNS */");
        //    sproc = sproc.Replace("--%update_or_insert_statement%--", "\r\n" + tabs + "/* GENERATED UPDATE/INSERT STATEMENTS */" + replacements.update_or_insert_statement + "\r\n" + tabs + "/* END OF GENERATED UPDATE/INSERT STATEMENTS */");
        //    sproc = sproc.Replace("--%arguments%--", "\r\n" + tabs + "/* GENERATED ARGUMENTS */" + replacements.arguments + "\r\n" + tabs + "/* END OF GENERATED ARGUMENTS */");
        //    sproc = sproc.Replace("--%searcharguments%--", "\r\n" + tabs + "/* GENERATED SEARCH ARGUMENTS */" + replacements.searchArguments + "\r\n" + tabs + "/* END OF GENERATED SEARCH ARGUMENTS */");
        //    sproc = sproc.Replace("--%searchconditions%--", "\r\n" + tabs + "/* GENERATED SEARCH CONDITIONS */" + replacements.searchConditions + "\r\n" + tabs + "/* END OF GENERATED SEARCH CONDITIONS */");
        //    sproc = sproc.Replace("--%quicksearcharguments%--", "\r\n" + tabs + "/* GENERATED QUICKSEARCH ARGUMENTS */" + replacements.quickSearchArguments + "\r\n" + tabs + "/* END OF GENERATED QUICKSEARCH ARGUMENTS */");
        //    sproc = sproc.Replace("--%quicksearchconditions%--", "\r\n" + tabs + "/* GENERATED QUICKSEARCH CONDITIONS */ AND (" + replacements.quickSearchConditions + ")\r\n" + tabs + "/* END OF GENERATED QUICKSEARCH CONDITIONS */");


        //    //Set Name and CREATE or UPDATE
        //    string createOrUpdate=sql.dynamicFields_existsStoredProcedure(storedProcedure + "_" + organisationId) ? "ALTER" : "CREATE";
        //    System.Text.RegularExpressions.Regex regEx = new System.Text.RegularExpressions.Regex("(CREATE|create) (PROCEDURE|procedure) (.*?)\n");
        //    foreach(System.Text.RegularExpressions.Match m in regEx.Matches(sproc))
        //    {
        //        sproc = sproc.Replace(m.Value, createOrUpdate + " PROCEDURE [" + storedProcedure + "_" + organisationId + "]\r\n");
        //    }

        //    //Update the Stored Procedure
        //    try
        //    {
        //        sql.dynamicFields_updateStoredProcedure(sproc);
        //    }
        //    catch (Exception ex)
        //    {
        //        sproc = "Fejl under refactoring: " + ex + "\r\n" + sproc;
        //    }
            
        //    return sproc;
            
        //}
        
        private static void RefactorSegment(ref SQLDB sql, ref Organisation O, ref StoredProcedureReplacement replacements, string tableName)
        {
            //Make sure the organisation and SQL object is valid
            if(sql==null || O==null) return;

            //Local variables
            string rootTable = tableName;
            string currentTableName = tableName +"_"+ O.Id;
            int ParentId = TypeCast.ToInt(O.ParentId);

            //Manage tables
            replacements.tables = ",\r\n" + tabs + currentTableName + replacements.tables;      

            
            //Manage tableColumns
            List<TableColumn> TableColumns = sql.dynamicFields_getTableColumns(currentTableName);

            for (int i = 2; i < TableColumns.Count; i++)
            {
                replacements.tableColumns = ",\r\n" + tabs + currentTableName + ".[" + TableColumns[i].Name +"]"+ replacements.tableColumns;    
            }
 
            //Manage procedure arguments
            for (int i = 2; i < TableColumns.Count; i++)
            {
                replacements.arguments = ",\r\n" + tabs + "@" + TypeCast.PrepareArgument(TableColumns[i].Name) + " " + TableColumns[i].DataType + (TableColumns[i].DataType == "varchar" || TableColumns[i].DataType == "nvarchar" ? "(" + TableColumns[i].Length + ")" : "") + " = null " + replacements.arguments;
                replacements.searchArguments = ",\r\n" + tabs + "@" + TypeCast.PrepareArgument(TableColumns[i].Name) + " " + TableColumns[i].DataType + (TableColumns[i].DataType == "varchar" || TableColumns[i].DataType == "nvarchar" ? "(" + TableColumns[i].Length + ")" : "")+" = null "+ replacements.searchArguments; 
            }

            string insertUpdate = "\r\n" + tabs + "declare @id_" + O.Id + " int";
            insertUpdate += "\r\n" + tabs + "--TEST\nset @id_" + O.Id + " = (select top 1 id_" + O.Id + " FROM " + currentTableName + " WHERE " + currentTableName + ".parentId_" + O.Id + "=@id" + (ParentId > 0 ? "_" + ParentId : "") + ")";
    
            //Manage updates or inserts for current organisation
            //if (TableColumns.Count > 2)
            //{
                insertUpdate += "\r\n" + tabs + "UPDATE " + currentTableName + " SET";
                insertUpdate += "\r\n" + tabs + "\t\t\t\tparentId_"+ O.Id + "=@id" + (ParentId > 0 ? "_" + ParentId : "") ;
                for (int i = 2; i < TableColumns.Count; i++)
                {
                    if (i > 1) insertUpdate += ",";
                    insertUpdate += "\r\n" + tabs + "\t[" + TableColumns[i].Name + "] = @" + TypeCast.PrepareArgument(TableColumns[i].Name);
                }
                insertUpdate += "\r\n" + tabs + "WHERE " + currentTableName + ".Id_" + O.Id + " = @id_" + O.Id + "\r\n";

                //Insert if not exists
                insertUpdate += "\r\n" + tabs + "IF (@@rowcount=0) BEGIN\r\n" +
                                                    tabs + "\tINSERT INTO " + currentTableName + "\r\n\t\t" + tabs + "SELECT";

                insertUpdate += "\r\n" + tabs + "\t\t\t\t@id" + (ParentId > 0 ? "_" + ParentId : "");
                for (int i = 2; i < TableColumns.Count; i++)
                {
                    insertUpdate += ",";
                    insertUpdate += "\r\n" + tabs + "\t\t\t\t@" + TypeCast.PrepareArgument(TableColumns[i].Name);
                }

                insertUpdate += "\r\n\t\t" + tabs + "SET @id_" + O.Id + " = @@identity";
                insertUpdate += "\r\n" + tabs + "END\r\n";
            //}

            //append to main update_or_insert_statement
            replacements.update_or_insert_statement = insertUpdate + replacements.update_or_insert_statement;

            //Manage searchconditions
            for (int i = 2; i < TableColumns.Count; i++)
            {
                if (TableColumns[i].DataType == "varchar" || TableColumns[i].DataType == "nvarchar")
                {
                    replacements.searchConditions = " AND \r\n" + tabs + "(" + currentTableName + ".[" + TableColumns[i].Name + "] LIKE '%' +replace(@" + TypeCast.PrepareArgument(TableColumns[i].Name) + ", ' ', '%') + '%' OR @" + TypeCast.PrepareArgument(TableColumns[i].Name) + " is null)" + replacements.searchConditions;
                    replacements.quickSearchConditions = (ParentId == 0 && i == TableColumns.Count - 1 ? "" : " OR ") + "\r\n" + tabs + "(" + currentTableName + ".[" + TableColumns[i].Name + "] LIKE '%' +replace(@query, ' ', '%') + '%' OR @query is null)" + replacements.quickSearchConditions;
                }
                else
                {
                    replacements.searchConditions = " AND \r\n" + tabs + "(convert(varchar(8000)," + currentTableName + ".[" + TableColumns[i].Name + "]) = @" + TypeCast.PrepareArgument(TableColumns[i].Name) + " OR @" + TypeCast.PrepareArgument(TableColumns[i].Name) + " is null)" + replacements.quickSearchConditions;
                    replacements.quickSearchConditions = (ParentId == 0 && i == TableColumns.Count - 1 ? "" : " OR ") + "\r\n" + tabs + "(convert(varchar(8000)," + currentTableName + ".[" + TableColumns[i].Name + "]) = @query OR @query is null)" + replacements.quickSearchConditions;
                }
            }

            //Go up one Level in the organisation tree until we hit the top organisation
            if (ParentId > 0)
            {
                //Manage tableconditions with parentTable
                replacements.tableConditions = " AND\r\n" + tabs + currentTableName + ".parentId_" + O.Id + "="+rootTable+"_" + ParentId + ".id_" + ParentId + replacements.tableConditions;

                //Grab previous organisation
                O = sql.Organisations_getOrganisation(ParentId);
                RefactorSegment(ref sql, ref O, ref replacements, tableName);
            }
            else
            {
                //Manage tableconditions at top level
                replacements.tableConditions = " AND\r\n" + tabs + currentTableName + ".parentId_" + O.Id + "="+rootTable+".id" + replacements.tableConditions;
            }

        }



        #region helper methods

        public static string getTableName(string sproc)
        {
            string tableName = "";
            int i = sproc.IndexOf('_', 2);
            tableName = sproc.Substring(0, i);

            return tableName;
        }





        #endregion      
    }


}
