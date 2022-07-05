using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using IDS.EBSTCRM.Base;

namespace IDS.EBSTCRM.WindowManager.Integration
{
    /// <summary>
    /// Summary description for Demos
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class Demos : System.Web.Services.WebService
    {
        [WebMethod]
        public GeoAddressInfo GetMissingGeoCodeAddresses()
        {
            SQLDB sql = new SQLDB();

            GeoAddressInfo r = sql.GEOCodes_GetRandom250MissingAddresses();

            sql.Dispose();
            sql = null;

            return r;

        }

        [WebMethod]
        public void GeoCode_Update(IDS.EBSTCRM.Base.GeoCode Code)
        {
            SQLDB sql = new SQLDB();

            sql.GeoCode_Update(Code);

            sql.Dispose();
            sql = null;
        }

        [WebMethod]
        public string HelloWorld()
        {
            return "Hello World";
        }

        [WebMethod]
        public ExtraData HelloExtraData()
        {
            return new ExtraData() { Name = "Mr. Awesome!", Size = 1024 };
        }

        public class ExtraData
        {
            public string Name { get; set; }
            public int Size { get; set; }


        }
    }
}
