using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;

namespace IDS.EBSTCRM.Base
{
    /// <summary>
    /// Exchange Email Import Rules
    /// OBSOLETE
    /// </summary>
    public class MailRule
    {
        public int Id;
        public string UserId;
        public int OrganisationId;
        public string SenderEmail;
        public int ContactId;
        public int VisibleTo;

        public List<TableColumnWithValue> ContactData = new List<TableColumnWithValue>();

        public MailRule()
        {

        }

        public MailRule(ref SqlDataReader dr)
        {
            Id = TypeCast.ToInt(dr["id"]);
            UserId = TypeCast.ToString(dr["UserId"]);
            OrganisationId = TypeCast.ToInt(dr["OrganisationId"]);
            SenderEmail = TypeCast.ToString(dr["SenderEmail"]);
            ContactId = TypeCast.ToInt(dr["ContactId"]);
            VisibleTo = TypeCast.ToInt(dr["VisibleTo"]);

            for (int i = 4; i < dr.FieldCount; i++)
            {
                if (dr.GetName(i).StartsWith("z_"))
                    ContactData.Add(new TableColumnWithValue(dr.GetName(i), dr.GetDataTypeName(i), 0, dr[i]));

            }
        }

        public string getContactData()
        {
            string ret = "";
            int count = 0;
            foreach (TableColumnWithValue t in ContactData)
            {
                if (t.ValueFormatted != "")
                {
                    ret += (count == 0 ? "" : (count == 1 ? " / " : " ")) + t.ValueFormatted;
                    count++;
                }
            }
            return ret == "" ? "Ingen" : ret;
        }
    }
}
