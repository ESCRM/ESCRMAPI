using System;
using System.Collections.Generic;
using System.Linq;

namespace IDS.EBSTCRM.Base {
    /// <summary>
    /// Dynamic tables are created/altered from this class
    /// Each Organisation Has it own set of tables for:
    ///  - SMV Companies
    ///  - SMV Contacts
    ///  
    /// They inherit fields from parent organisation tables
    /// </summary>
    public static class SQLTablesManager {

        #region AVN Note fields

        public static List<AVNField> GetAVNTableFields(ref SQLDB sql, int AvnId, out bool TableExists) {
            TableExists = false;

            List<AVNField> fields = new List<AVNField>();

            foreach (TableColumn t in sql.dynamicFields_getTableColumns("z_avn_" + AvnId)) {
                TableExists = true;

                if (t.Name.IndexOf("_") > -1) {
                    AVNField f = new AVNField();
                    f.Id = TypeCast.ToInt(t.Name.Substring(0, t.Name.IndexOf("_")));
                    f.DatabaseColumn = t.Name.Substring(t.Name.IndexOf("_") + 1);
                    f.AvnId = AvnId;

                    fields.Add(f);
                }
            }

            return fields;
        }

        public static bool RefactorAVNTable(ref User U, ref SQLDB sql, int AvnId, List<AVNField> NewFields) {
            bool tableExists = false;
            List<AVNField> tableFields = GetAVNTableFields(ref sql, AvnId, out tableExists);
            string tblSql = "";

            List<string> FieldsToUpdate = new List<string>();

            //All new table
            if (!tableExists) {
                #region Create all new table

                tblSql = "CREATE TABLE [dbo].[z_avn_" + AvnId + "](\n" +
                                "[Id] [int] IDENTITY(1,1) NOT NULL,\n" +
                                "[OrganisationId] [int] NOT NULL,\n" +
                                "[Name] [nvarchar](255) NOT NULL,\n" +
                                "[CreatedBy] [uniqueidentifier] NOT NULL,\n" +
                                "[Created] [datetime] NOT NULL,\n" +
                                "[LastUpdatedBy] [uniqueidentifier] NULL,\n" +
                                "[LastUpdated] [datetime] NULL,\n" +

                                "[DeletedBy] [uniqueidentifier] NULL,\n" +
                                "[Deleted] [datetime] NULL,\n" +

                                "[SMVContactId] [int] NULL,\n" +
                                "[SMVCompanyId] [int] NULL,\n" +

                                "[SharedWith] [int] NULL\n" +
                                "\n\n";



                foreach (AVNField f in NewFields) {
                    if (IsAVNDatabaseField(f)) {
                        tblSql += ", [" + f.Id + "_" + f.DatabaseColumn.Replace("'", "''").Replace(",", "_") + "] " + GetAVNFieldDataType(f) + " NULL\n";
                    }
                }

                tblSql += ")";

                sql.dynamicFields_refactorTable(tblSql);

                return true;

                #endregion
            } else //Table needs update
              {

                //Deleted fields
                foreach (AVNField f in tableFields) {
                    if (IsAVNFieldDeleted(f, NewFields))
                        FieldsToUpdate.Add("exec DynamicFields_DropColumnCascading 'z_avn_" + f.AvnId + "','" + f.Id + "_" + f.DatabaseColumn.Replace("'", "''").Replace(",", "_") + "'");
                }

                //Renamed and created fields
                foreach (AVNField f in NewFields) {
                    if (IsAVNDatabaseField(f)) {
                        if (IsAVNFieldNew(f, tableFields))
                            FieldsToUpdate.Add("alter table [z_avn_" + f.AvnId + "] add [" + f.Id + "_" + f.DatabaseColumn.Replace("'", "''").Replace(",", "_") + "] " + GetAVNFieldDataType(f));
                        else {
                            string oldName = IsAVNFieldRenamed(f, tableFields);
                            if (oldName != null)
                                FieldsToUpdate.Add("exec [sp_rename] 'z_avn_" + f.AvnId + ".[" + f.Id + "_" + oldName + "]', '" + f.Id + "_" + f.DatabaseColumn.Replace("'", "''").Replace(",", "_") + "', 'COLUMN' ");
                        }
                    }
                }

                //Execute SQL to perform table updates
                if (FieldsToUpdate.Count > 0) {
                    //string sqlUpdateStatement = "";
                    foreach (string s in FieldsToUpdate) {
                        try {
                            sql.dynamicFields_refactorTable(s);
                        } catch (Exception exp) {
                            ExceptionMail.SendException(exp, U);
                        }

                    }
                }

                //Return if any changes has been made
                return FieldsToUpdate.Count > 0;
            }

        }

        private static bool IsAVNDatabaseField(AVNField f) {
            return f.FieldType != "hr" &&
                    f.FieldType != "title" &&
                    f.FieldType != "vr" &&
                    f.FieldType != "label" &&
                    f.FieldType != "button" &&
                    f.FieldType != "linkbutton" &&
                    f.FieldType != "sqllabel" &&
                    f.FieldType != "map";
        }

        private static string GetAVNFieldDataType(AVNField f) {
            switch (f.FieldType.ToLower()) {
                case "dropdown":
                case "emailaddress":
                case "textbox":
                    return "[VARCHAR](8000)";

                case "absinteger":
                case "integer":
                case "absnumeric":
                case "numeric":
                    return "[BIGINT]";

                case "float":
                case "absfloat":
                    return "[FLOAT]";

                case "checkbox":
                    return "[BIT]";

                case "listview":
                case "memo":
                    return "[NVARCHAR](max)";

                case "datetime":
                    return "[DATETIME]";
            }
            return null;
        }

        private static bool IsAVNFieldDeleted(AVNField f, List<AVNField> newFields) {
            foreach (AVNField ff in newFields) {
                if (ff.Id == f.Id) return false;
            }

            return true;
        }

        private static bool IsAVNFieldNew(AVNField f, List<AVNField> orgFields) {
            foreach (AVNField ff in orgFields) {
                if (ff.Id == f.Id) return false;
            }

            return true;
        }

        private static string IsAVNFieldRenamed(AVNField f, List<AVNField> orgFields) {
            foreach (AVNField ff in orgFields) {
                if (ff.Id == f.Id && ff.DatabaseColumn != f.DatabaseColumn)
                    return ff.DatabaseColumn;
            }

            return null;
        }

        #endregion

        #region Dynamic Fields

        public static bool RefactorTable(ref SQLDB sql, string tableName, Organisation O, User U, List<TableColumnModified> modifiedCollection) {

            var FieldsToUpdate = new List<string>();
            sql.Organisations_CreateUpdate(null, O);


            // Company Table Updates
            if (string.Equals(tableName, "Company", StringComparison.CurrentCultureIgnoreCase)) {

                var originalColumns = sql.dynamicFields_getTableColumns(tableName);
                var originalCollection = new List<TableColumn>();
                for (int i = 0; i < originalColumns.Count; i++) {
                    TableColumn t = originalColumns[i];
                    if (t.Name.Contains("z_companies_" + O.Id + "_")) {
                        originalCollection.Add(t);
                    }
                }

                // Find fields to drop
                foreach (TableColumn tc in originalCollection) {
                    if (IsTableColumnDeleted(modifiedCollection, tc)) {
                        FieldsToUpdate.Add("Alter Table [" + tableName + "] Drop Column [" + tc.Name + "] ");
                    }
                }

                // Find new and renamed fields
                foreach (TableColumnModified tcm in modifiedCollection) {
                    if (IsTableColumnRenamed(originalCollection, tcm)) {
                        FieldsToUpdate.Add("Exec [sp_rename] '" + tableName + ".[" + tcm.OldName + "]', '" + tcm.Name + "', 'COLUMN' ");
                    } else if (IsTableColumnNew(originalCollection, tcm)) {
                        if (tcm.DataType.ToLower() == "bit") {
                            FieldsToUpdate.Add("Alter Table [" + tableName + "] Add [" + tcm.Name + "] [bit] SPARSE");
                        } else {
                            string type = "";
                            if (tcm.DataType.ToLower() == "varchar" || tcm.DataType.ToLower() == "nvarchar") { type = "(" + tcm.Length + ")"; }
                            FieldsToUpdate.Add("Alter Table [" + tableName + "] Add [" + tcm.Name + "] " + tcm.DataType + type);
                        }
                    }
                }

            }

            // Company Table Updates
            if (string.Equals(tableName, "Contact", StringComparison.CurrentCultureIgnoreCase)) {

                var originalColumns = sql.dynamicFields_getTableColumns(tableName);
                var originalCollection = new List<TableColumn>();
                for (int i = 0; i < originalColumns.Count; i++) {
                    TableColumn t = originalColumns[i];
                    if (t.Name.Contains("z_contacts_" + O.Id + "_")) {
                        originalCollection.Add(t);
                    }
                }

                // Find fields to drop
                foreach (TableColumn tc in originalCollection) {
                    if (IsTableColumnDeleted(modifiedCollection, tc)) {
                        FieldsToUpdate.Add("Alter Table [" + tableName + "] Drop Column [" + tc.Name + "] ");
                    }
                }

                // Find new and renamed fields
                foreach (TableColumnModified tcm in modifiedCollection) {
                    if (IsTableColumnRenamed(originalCollection, tcm)) {
                        FieldsToUpdate.Add("Exec [sp_rename] '" + tableName + ".[" + tcm.OldName + "]', '" + tcm.Name + "', 'COLUMN' ");
                    } else if (IsTableColumnNew(originalCollection, tcm)) {
                        if (tcm.DataType.ToLower() == "bit") {
                            FieldsToUpdate.Add("Alter Table [" + tableName + "] Add [" + tcm.Name + "] [bit] SPARSE");
                        } else {
                            string type = "";
                            if (tcm.DataType.ToLower() == "varchar" || tcm.DataType.ToLower() == "nvarchar") { type = "(" + tcm.Length + ")"; }
                            FieldsToUpdate.Add("Alter Table [" + tableName + "] Add [" + tcm.Name + "] " + tcm.DataType + type);
                        }
                    }
                }

            }

            // Contact Dynamic Tables            
            /*else {

                tableName += "_" + O.Id;

                List<TableColumn> OriginalColumns = sql.dynamicFields_getTableColumns(tableName);
                List<TableColumn> originalCollection = new List<TableColumn>();
                for (int i = 2; i < OriginalColumns.Count; i++) {
                    TableColumn t = OriginalColumns[i];
                    originalCollection.Add(t);
                }

                // Find fields to drop
                foreach (TableColumn tc in originalCollection) {
                    if (IsTableColumnDeleted(modifiedCollection, tc)) {
                        FieldsToUpdate.Add("exec DynamicFields_DropColumnCascading '" + tableName + "','" + tc.Name.Replace("'", "''").Replace(",", "_") + "'");
                    }
                }

                // Find new and renamed fields
                foreach (TableColumnModified tcm in modifiedCollection) {
                    if (IsTableColumnRenamed(originalCollection, tcm)) {
                        FieldsToUpdate.Add("exec [sp_rename] '" + tableName + ".[" + tcm.OldName + "]', '" + tcm.Name + "', 'COLUMN' ");
                    } else if (IsTableColumnNew(originalCollection, tcm)) {
                        string type = "";
                        if (tcm.DataType.ToLower() == "varchar" || tcm.DataType.ToLower() == "nvarchar") { type = "(" + tcm.Length + ")"; }
                        FieldsToUpdate.Add("alter table [" + tableName + "] add [" + tcm.Name + "] " + tcm.DataType + type);
                    }
                }
            }*/

            //Update in one sql statement
            if (FieldsToUpdate.Count > 0) {
                foreach (string s in FieldsToUpdate) {
                    try {
                        sql.dynamicFields_refactorTable(s);
                    } catch (Exception exp) {
                        ExceptionMail.SendException(exp, U);
                    }

                }
            }

            return FieldsToUpdate.Count > 0;
        }

        private static bool IsTableColumnNew(List<TableColumn> originalCollection, TableColumnModified tcm) {
            foreach (TableColumn tc in originalCollection) {
                if (tc.Name.ToLower() == tcm.Name.ToLower())
                    return false;
            }

            return true;
        }

        private static bool IsTableColumnDeleted(List<TableColumnModified> modifiedCollection, TableColumn tc) {
            foreach (TableColumnModified tcm in modifiedCollection) {
                if (tc.Name.ToLower() == tcm.Name.ToLower() || tc.Name.ToLower() == tcm.OldName.ToLower())
                    return false;
            }

            return true;
        }

        private static bool IsTableColumnRenamed(List<TableColumn> originalCollection, TableColumnModified tcm) {
            foreach (TableColumn tc in originalCollection) {
                if (tc.Name.ToLower() == tcm.OldName.ToLower() && tcm.Name.ToLower() != tcm.OldName.ToLower())
                    return true;
            }

            return false;
        }

        private static bool HasModifications(TableColumn original, TableColumnModified modificaion) {
            return ((original.Length != modificaion.Length && modificaion.Length > 0) || original.DataType != modificaion.DataType);
        }

        private static TableColumnModified GetModificationWihtRename(TableColumn currentOriginal, List<TableColumnModified> modifiedCollection) {
            foreach (TableColumnModified t in modifiedCollection) {
                if (t.OldName == currentOriginal.Name)
                    return t;
            }

            return null;
        }

        #endregion

    }

}
