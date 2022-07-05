using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Xml;
using System.Data.SqlClient;

namespace IDS.EBSTCRM.WindowManager.Integration
{
    /// <summary>
    /// Summary description for ReportGenerator
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class ReportGenerator : System.Web.Services.WebService
    {
        [WebMethod]
        public XmlDocument ExecuteReportAsXML(string Name, string Key)
        {
            IDS.EBSTCRM.Base.SQLBase sql = new IDS.EBSTCRM.Base.SQLBase(System.Configuration.ConfigurationManager.AppSettings["connectionString"]);

            IDS.EBSTCRM.Base.ReportGeneratorSavedReport report = null;
            sql.commandText = "ReportGenerator_getSavedReportFromIntegrationKey";
            sql.parameters.AddWithValue("@IntegrationKey", Key);
            sql.parameters.AddWithValue("@Name", Name);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read())
            {
                report = new Base.ReportGeneratorSavedReport(ref dr, true);
            }
            dr.Close();
            sql.reset();

            if (report == null)
            {
                throw new Exception("Rapporten \"" + Name + "\" og nøgle \"" + Key + "\" mathcer ikke en aktiv SOAP rapport! (0x01)");
            }

            if (report.IntegrationRejectedDate != null)
            {
                throw new Exception("Rapporten \"" + Name + "\" og nøgle \"" + Key + "\" mathcer ikke en aktiv SOAP rapport! (0x02)");
            }

            if (report.IntegrationApprovedDate == null)
            {
                throw new Exception("Rapporten \"" + Name + "\" og nøgle \"" + Key + "\" mathcer ikke en aktiv SOAP rapport! (0x03)");
            }

            sql.commandType = System.Data.CommandType.Text;
            sql.commandText = report.SQL + " for XML AUTO";
            dr = sql.executeReader;

            string result = "";

            while (dr.Read())
            {
                result += IDS.EBSTCRM.Base.TypeCast.ToString(dr[0]);
            }

            if (result != null && result != "")
            {
                result = System.Text.RegularExpressions.Regex.Replace(result, "_x0040__x0040_unify\\:", "_");

                result = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n\r\n<result>" + result + "</result>";

                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(result);
                return xmlDocument;
            }
            else
                return null;
        }

        [WebMethod]
        public XmlDocument ExecuteReportAsXMLStructured(string Name, string Key)
        {
            IDS.EBSTCRM.Base.SQLBase sql = new IDS.EBSTCRM.Base.SQLBase(System.Configuration.ConfigurationManager.AppSettings["ReadOnlyConnectionString"]); //@"Data Source=192.168.200.206\mssql2008;Initial Catalog=EBSTCRM_CRM2_SANDKASSE;Uid=sa;pwd=Spiderman!!!;pooling=false;");

            IDS.EBSTCRM.Base.ReportGeneratorSavedReport report = null;
            sql.commandText = "ReportGenerator_getSavedReportFromIntegrationKey";
            sql.parameters.AddWithValue("@IntegrationKey", Key);
            sql.parameters.AddWithValue("@Name", Name);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read())
            {
                report = new IDS.EBSTCRM.Base.ReportGeneratorSavedReport(ref dr, true);
            }
            dr.Close();
            sql.reset();

            if (report == null)
            {
                throw new Exception("Rapporten \"" + Name + "\" og nøgle \"" + Key + "\" mathcer ikke en aktiv SOAP rapport! (0x01)");
            }

            if (report.IntegrationRejectedDate != null)
            {
                throw new Exception("Rapporten \"" + Name + "\" og nøgle \"" + Key + "\" mathcer ikke en aktiv SOAP rapport! (0x02)");
            }

            if (report.IntegrationApprovedDate == null)
            {
                throw new Exception("Rapporten \"" + Name + "\" og nøgle \"" + Key + "\" mathcer ikke en aktiv SOAP rapport! (0x03)");
            }

            sql.commandType = System.Data.CommandType.Text;
            sql.commandText = report.SQL;

            dr = sql.executeReader;

            XmlObject root = new XmlObject("Results");
            XmlObject currentObject = null;


            int depth = 0;

            List<XmlObject> objectDepths = new List<XmlObject>();

        NextResultSet:

            while (dr.Read())
            {
                depth = 0;
                currentObject = null;
                int startingIndex = 0;

                while (startingIndex < dr.FieldCount)
                {
                    int endingIndex = 0;

                    XmlObject o = new XmlObject(dr, startingIndex, out endingIndex);

                    if (currentObject != null)
                        currentObject.Children.Add(o);
                    else
                        objectDepths.Add(o);

                    currentObject = o;

                    startingIndex = endingIndex;
                }
            }


            if (dr.NextResult())
                goto NextResultSet;

            dr.Close();
            sql.reset();

            string xml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n<result>";
            foreach (XmlObject o in objectDepths)
            {
                xml += o.ToString();
            }

            xml += "</result>";



            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xml);
            return xmlDocument;
        }

        [WebMethod]
        public DataSet[] ExecuteReport(string Name, string Key)
        {
            IDS.EBSTCRM.Base.SQLBase sql = new IDS.EBSTCRM.Base.SQLBase(System.Configuration.ConfigurationManager.AppSettings["connectionString"]);

            IDS.EBSTCRM.Base.ReportGeneratorSavedReport report = null;
            sql.commandText = "ReportGenerator_getSavedReportFromIntegrationKey";
            sql.parameters.AddWithValue("@IntegrationKey", Key);
            sql.parameters.AddWithValue("@Name", Name);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read())
            {
                report = new Base.ReportGeneratorSavedReport(ref dr, true);
            }
            dr.Close();
            sql.reset();

            if (report == null)
            {
                throw new Exception("Rapporten \"" + Name + "\" og nøgle \"" + Key + "\" mathcer ikke en aktiv SOAP rapport! (0x01)");
            }

            if (report.IntegrationRejectedDate != null)
            {
                throw new Exception("Rapporten \"" + Name + "\" og nøgle \"" + Key + "\" mathcer ikke en aktiv SOAP rapport! (0x02)");
            }

            if (report.IntegrationApprovedDate == null)
            {
                throw new Exception("Rapporten \"" + Name + "\" og nøgle \"" + Key + "\" mathcer ikke en aktiv SOAP rapport! (0x03)");
            }

            sql.commandType = System.Data.CommandType.Text;

            string tempSQL = report.SQL;

            if (tempSQL.ToLower().Contains("left join tilskudsinformation"))
            {
                tempSQL = tempSQL.Replace("left join Tilskudsinformation on ", "left join Tilskudsinformation_Hent_Web() on ");
            }

            //** Replace function
            if(tempSQL.ToLower().Contains("tilskudsinformation_hent_filter"))
            {
                tempSQL = tempSQL.Replace("Tilskudsinformation_Hent_Filter", "Tilskudsinformation_Hent_Web");
            }

            sql.commandText = tempSQL; // report.SQL;


            dr = sql.executeReader;

            List<DataSet> retval = new List<DataSet>();

            NextResultSet:

            retval.Add(new DataSet());
            List<Row> Rows = new List<Row>();

            while (dr.Read())
            {
                Rows.Add(new Row());
                List<PropertyValue> Cols = new List<PropertyValue>();
                for (int i = 0; i < dr.FieldCount; i++)
                {
                    Cols.Add(new PropertyValue(dr.GetName(i), dr.GetDataTypeName(i), dr[i]));     
                }
                Rows[Rows.Count - 1].Columns = Cols.ToArray();
                
            }
            retval[retval.Count - 1].Rows = Rows.ToArray();

            if (dr.NextResult())
                goto NextResultSet;

            dr.Close();
            sql.reset();

            return retval.ToArray();
        }

        [WebMethod]
        public List<TinyCompany> GetTinyCompanies()
        {
            List<TinyCompany> r = new List<TinyCompany>();
            r.Add(new TinyCompany() { Name = "Hansens Is", Id = 10 });
            r[0].Adresses = new List<TinyCompany.Address>();
            r[0].Adresses.Add(new TinyCompany.Address() { No = "10A", Street = "Isvejen", Zipcode = 2200 });
            r[0].Adresses.Add(new TinyCompany.Address() { No = "5", Street = "Halvejen", Zipcode = 9000 });
            r[0].Adresses.Add(new TinyCompany.Address() { No = "89", Street = "Jerngade", Zipcode = 5100 });

            r.Add(new TinyCompany() { Name = "Peters gulerødder", Id = 11 });
            r[1].Adresses = new List<TinyCompany.Address>();
            r[1].Adresses.Add(new TinyCompany.Address() { No = "15", Street = "Markvejen", Zipcode = 7000 });
            r[1].Adresses.Add(new TinyCompany.Address() { No = "44", Street = "Stålbuen", Zipcode = 2800 });

            return r;
        }

        public class TinyCompany
        {
            public string Name { get; set; }
            public int Id { get; set; }

            public List<Address> Adresses { get; set; }

            public class Address
            {
                public string Street { get; set; }
                public string No { get; set; }
                public int Zipcode { get; set; }
            }
        }

        [Serializable()]
        public class XmlObject
        {
            public static string CreateValidXMLTagName(string data)
            {
                //string re = @"[^\x09\x0A\x0D\x20-\xD7FF\xE000-\xFFFD\x10000-x10FFFF]";
                return System.Text.RegularExpressions.Regex.Replace(data, "[^0-9A-Za-z_ÆØÅæøå]", "_");
            }

            [Serializable()]
            public class XmlProperty
            {
                public string Name { get; set; }
                public string Value { get; set; }
                public Integration.DataType DataType { get; set; }

                public XmlProperty()
                {

                }

                public XmlProperty(string name, object value, string dataType)
                {
                    this.Name = name;

                    switch (dataType)
                    {
                        case "bit":
                            DataType = Integration.DataType.Boolean;
                            Value = value == DBNull.Value ? "{NULL}" : IDS.EBSTCRM.Base.TypeCast.ToBool(value) == true ? "TRUE" : "FALSE";
                            break;

                        case "int":
                            DataType = Integration.DataType.Integer;
                            Value = value == DBNull.Value ? "{NULL}" : IDS.EBSTCRM.Base.TypeCast.ToInt(value).ToString("#");
                            break;

                        case "float":
                            DataType = Integration.DataType.Float;
                            Value = value == DBNull.Value ? "{NULL}" : IDS.EBSTCRM.Base.TypeCast.ToDecimal(value).ToString("#,##0.00");
                            break;

                        case "varchar":
                        case "nvarchar":
                            DataType = Integration.DataType.String;
                            Value = value == DBNull.Value ? "{NULL}" : IDS.EBSTCRM.Base.TypeCast.ToString(value);
                            break;

                        case "datetime":
                            DataType = Integration.DataType.DateTime;
                            Value = value == DBNull.Value ? "{NULL}" : IDS.EBSTCRM.Base.TypeCast.ToDateTime(value).ToString("dd-MM-yyyy HH:mm:ss");
                            break;

                        default:
                            DataType = Integration.DataType.Any;
                            Value = value == DBNull.Value ? "{NULL}" : IDS.EBSTCRM.Base.TypeCast.ToString(value);
                            break;
                    }
                }
            }

            public string Tag { get; set; }
            public string Content { get; set; }
            public int Depth { get; set; }

            public List<XmlProperty> Properties { get; set; }
            public List<XmlObject> Children { get; set; }

            public XmlObject(SqlDataReader dr, int startingIndex, out int endingIndex)
            {
                Children = new List<XmlObject>();
                Properties = new List<XmlProperty>();

                endingIndex = startingIndex;

                this.Tag = dr.GetName(startingIndex);
                this.Tag = System.Text.RegularExpressions.Regex.Split(this.Tag, "@@unify\\:")[1];

                while (endingIndex < dr.FieldCount && this.Tag == System.Text.RegularExpressions.Regex.Split(dr.GetName(endingIndex), "@@unify\\:")[1])
                {
                    this.Properties.Add(new XmlProperty(
                            System.Text.RegularExpressions.Regex.Split(dr.GetName(endingIndex), "@@unify\\:")[0],
                            dr[endingIndex],
                            dr.GetDataTypeName(endingIndex)));
                    endingIndex++;
                }

            }

            public XmlObject()
            {
                Properties = new List<XmlProperty>();
                Children = new List<XmlObject>();
            }

            public XmlObject(string tag)
            {
                this.Tag = tag;
                Children = new List<XmlObject>();
                Properties = new List<XmlProperty>();
            }

            public XmlObject(SqlDataReader dr, int index, int depth)
            {
                this.Tag = dr.GetName(index);
                if (this.Tag.IndexOf("@@unify:") > -1)
                    this.Tag = System.Text.RegularExpressions.Regex.Split(this.Tag, "@@unify\\:")[1];

                Children = new List<XmlObject>();
                Properties = new List<XmlProperty>();
            }

            public XmlObject(string tag, string content)
            {
                this.Content = content;
                this.Tag = tag;
                Children = new List<XmlObject>();
                Properties = new List<XmlProperty>();
            }

            public string ToString()
            {
                XmlObject[] children = this.Children.Where(p => p.Children.Count > 0 || (p.Properties.Where(p2 => p2.Value != null && p2.Value != "").ToArray().Length > 0)).ToArray();
                XmlProperty[] properties = this.Properties.Where(p => p.Value != null && p.Value != "").ToArray();

                if (children.Length + properties.Length == 0) return "";


                string mtag = XmlObject.CreateValidXMLTagName(this.Tag);


                string t = "<" + mtag + ">";
                foreach (XmlProperty p in properties)
                {
                    string ptag = XmlObject.CreateValidXMLTagName(p.Name);

                    t += "\n<" + ptag + " DataType=\"" + p.DataType + "\">" + HttpContext.Current.Server.HtmlEncode(p.Value) + "</" + ptag + ">";
                }

                foreach (XmlObject o in children)
                {
                    t += o.ToString();
                }

                t += "</" + mtag + ">";

                return t;
            }

        }
    }
}
