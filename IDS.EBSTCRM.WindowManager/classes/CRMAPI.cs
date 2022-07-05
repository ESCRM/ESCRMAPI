using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using IDS.EBSTCRM.Base;
using System.Collections.Generic;

namespace IDS.EBSTCRM.WindowManager {
    public static class CRMAPI {

        public static List<int> PopulateSMVFields(IDS.EBSTCRM.Base.User U, int SMVCompanyId, int SMVContactId, ref IDS.EBSTCRM.Base.SQLDB sql, HtmlGenericControl Owner) {
            List<int> r = new List<int>();

            IDS.EBSTCRM.Base.Contact c = sql.Contact_Get(U.OrganisationId, SMVContactId, U);
            if (c != null) {
                foreach (TableColumnWithValue t in c.DynamicTblColumns) {

                    Owner.Controls.Add(CreateControl(t.Name, t.ValueFormatted));
                    int orgId = TypeCast.ToInt(t.Name.Substring(t.Name.LastIndexOf("_") + 1));
                    if (!r.Contains(orgId))
                        r.Add(orgId);
                }
            }

            if (SMVCompanyId <= 0 && c != null)
                SMVCompanyId = c.CompanyId;

            IDS.EBSTCRM.Base.Company cp = sql.Company_Get(U.OrganisationId, SMVCompanyId, U);
            if (cp != null) {
                foreach (TableColumnWithValue t in cp.DynamicTblColumns) {
                    Owner.Controls.Add(CreateControl(t.Name, t.ValueFormatted));
                    int orgId = TypeCast.ToInt(t.Name.Substring(t.Name.LastIndexOf("_") + 1));
                    if (!r.Contains(orgId))
                        r.Add(orgId);
                }
            }
            return r;
        }

        private static HtmlInputHidden CreateControl(string Name, string Value) {
            HtmlInputHidden c = new HtmlInputHidden();

            c.Attributes["name"] = Name;
            c.Attributes["id"] = Name;
            c.Value = Value;

            return c;
        }
    }
}