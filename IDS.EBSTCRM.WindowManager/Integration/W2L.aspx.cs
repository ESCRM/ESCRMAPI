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

namespace IDS.EBSTCRM.WindowManager.Integration
{
    public partial class W2L1 : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Response.Expires = -1;
            Response.ExpiresAbsolute = DateTime.Now;

            if (IDS.EBSTCRM.WindowManager.Security.UserAccess.GetCurrentUser() != null)
            {
                User U = (User)IDS.EBSTCRM.WindowManager.Security.UserAccess.GetCurrentUser();
                //brugernavn|password|navn|email|kundeID|vækstforløbID|

                //Response.Redirect("http://www.startvaekst.dk/pls/wwwdata/vhjul_xml.vhlogin?i_str=" + IDS.EBSTCRM.Base.Encryption.DES_CBC_PKCS5.Encrypt("punch|Punch54321|Martin Exner Jensen|martin@lbe.dk|1|1|", "1234567890abcdefghij"));

                string ev = TypeCast.ToString(Request.QueryString["event"]);
                string type = TypeCast.ToString(Request.QueryString["type"]);
                string url = "";

                switch(ev)
                {
                    case "Login":

                        string fn = TypeCast.ToString(Server.UrlDecode(Request.QueryString["firstname"])).Replace("%26", "&");
                        string ln = TypeCast.ToString(Server.UrlDecode(Request.QueryString["Lastname"])).Replace("%26", "&");

                        //Response.Redirect("http://www.startvaekst.dk/pls/wwwdata/vhjul_xml.vhlogin?i_str=" + IDS.EBSTCRM.Base.Encryption.DES_CBC_PKCS5.Encrypt(U.W2L_Username + "|" + U.W2L_Password + "|" + U.Firstname + " " + U.Lastname + "|" + U.Email + "|" + Request.QueryString["SmvId"] + "|" + Request.QueryString["AvnId"] + "|", "12345678"));
                        url = "http://www.startvaekst.dk/pls/wwwdata/vhjul_xml.vhlogin?i_str=" + IDS.EBSTCRM.Base.Encryption.DES_CBC_PKCS5.Encrypt(U.W2L_Username + "|" + U.W2L_Password + "|" + fn + " " + ln + "|" + Request.QueryString["Email"] + "|" + Request.QueryString["SmvId"] + "|" + Request.QueryString["AvnId"] + "|", "12345678");
                        break;

                    case "DownloadPDF":
                        url = "http://www.startvaekst.dk/pls/wwwdata/vhjul_xml.vhplan?" + (type != "" ? "i_pnid=" + type + "&" : "") + "i_str=" + IDS.EBSTCRM.Base.Encryption.DES_CBC_PKCS5.Encrypt(Request.QueryString["AvnId"], "12345678");
                        break;

                    case "Save":
                        url = "http://www.startvaekst.dk/vaekstplan/0/44/0/" + IDS.EBSTCRM.Base.Encryption.DES_CBC_PKCS5.Encrypt(Request.QueryString["AvnId"] + "|" + Request.QueryString["SmvId"] + "|", "12345678");
                        break;

                    case "DownloadImage":
                        url = "http://www.startvaekst.dk/pls/wwwdata/vhjul_xml.vhimage?" + (type != "" ? "i_pnid=" + type + "&" : "") + "i_str=" + IDS.EBSTCRM.Base.Encryption.DES_CBC_PKCS5.Encrypt(Request.QueryString["AvnId"], "12345678");
                        break;
                }

                if (url != null && url != "")
                    Response.Redirect(url);
            }
        }

    }
}