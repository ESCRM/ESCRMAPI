using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Collections;

namespace IDS.EBSTCRM.Base {
    /// <summary>
    /// Project type and case number for dropdown menus, when creating and modifying Hour Registrations
    /// </summary>
    public class HourUsageProjectTypeAndCaseNumber {
        public int ProjectTypeId;
        public int CaseNumberId;
        public string ProjectTypeName;
        public string CaseNumberName;
        public int ProjectTypeSerialNo;
        public int CaseNumber;
        public bool ControlledActivityProject;

        public HourUsageProjectTypeAndCaseNumber() { }
        public HourUsageProjectTypeAndCaseNumber(ref SqlDataReader dr) {
            ProjectTypeId = TypeCast.ToInt(dr["ProjectTypeId"]);
            CaseNumberId = TypeCast.ToInt(dr["CaseNumberId"]);
            ProjectTypeName = TypeCast.ToString(dr["ProjectTypeName"]);
            CaseNumberName = TypeCast.ToString(dr["CaseNumberName"]);
            ProjectTypeSerialNo = TypeCast.ToInt(dr["ProjectTypeSerialNo"]);
            CaseNumber = TypeCast.ToInt(dr["CaseNumber"]);
            ControlledActivityProject = (TypeCast.ToInt(dr["ControlledActivityProject"]) > 0);
        }
    }
}
