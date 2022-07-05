using IDS.EBSTCRM.Base;
using System.Web.Services;
using System;

namespace IDS.EBSTCRM.WindowManager.Integration
{
    /// <summary>
    /// Summary description for AVN560_API
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class AVN560_API : System.Web.Services.WebService {

        [WebMethod]
        public void GetAVNEntities(string key) {
            var ip = string.Empty;
            try { ip = Context.Request.UserHostAddress; } catch (Exception) { ip = Context.Request.ServerVariables["REMOTE_ADDR"]; }
            key = key.ToLower();

            var result = "<error><code>401</code><message>Unauthorized</message></error>";
            var context = new SQLDB();
            var validKey = context.API_ValidateUserKey(key);
            if (validKey) {
                result = context.API_GetAVNEntities(key, 560, ip);
            }
            Context.Response.Clear();
            Context.Response.ContentType = "application/xml";
            Context.Response.Write(result);
        }
    }
}
