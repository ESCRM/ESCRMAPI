using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using IDS.EBSTCRM.Base;
using System.Collections.Generic;

namespace IDS.EBSTCRM.FrontEnd {
    public class Global : System.Web.HttpApplication {

        public static System.Collections.Specialized.NameValueCollection OnlineUsers = new System.Collections.Specialized.NameValueCollection();

        public static System.Collections.Specialized.NameValueCollection ChatMessages = new System.Collections.Specialized.NameValueCollection();

        public static System.Collections.Specialized.NameValueCollection ErrorMessages = new System.Collections.Specialized.NameValueCollection();

        public void Session_End(object sender, EventArgs e) {
            User U = (User)IDS.EBSTCRM.WindowManager.Security.UserAccess.GetCurrentUser();
            if (U != null) {
                try { Global.OnlineUsers.Remove(U.Id); } catch { }
            }
        }

        public void Session_Start(object sender, EventArgs e) {
            User U = (User)IDS.EBSTCRM.WindowManager.Security.UserAccess.GetCurrentUser();
            if (U != null) {
                try {
                    if (U.BrowserData == null)
                        U.SetBrowserData(Request.Browser);

                    Global.OnlineUsers.Add(U.Id + "|" + U.BrowserData, U.Organisation.Name + "\t" + U.Username + "\t" + U.Firstname + "\t" + U.Lastname + "\t" + U.Email + "\t" + U.BrowserData + "\t" + U.OrganisationId);
                } catch { }
            }
        }

        protected void Application_Start(object sender, EventArgs e) {

        }

        protected void Application_End(object sender, EventArgs e) {

        }

        protected void Application_BeginRequest(object sender, EventArgs e) {

        }

        /// <summary>
        /// Triggers when an error occurs during execution
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Application_Error(Object sender, System.EventArgs e) {

            System.Web.HttpContext context = HttpContext.Current;
            System.Exception exp = Context.Server.GetLastError();

            IDS.EBSTCRM.Base.User U = IDS.EBSTCRM.WindowManager.Security.UserAccess.GetCurrentUser();

            if (U != null && !exp.Message.Contains("The remote host closed the connection")) {

                //Dont alert about errors, if the problem resides at NNE
                if (exp is IDS.EBSTCRM.Base.NNE.NNEException) {
                } else {
                    ErrorMessages.Add(U.Id + "_" + System.Guid.NewGuid(), "Der er desværre opstået en fejl i CRM System.<br><br>Fejlen er logget og sendt til ESCRM teamet.<br><br><b>Detaljer:</b><br>" + exp.Message.Replace("\n", "<br>") + "<br>" + exp.StackTrace.Replace("\n", "<br>"));
                }
            }

            //TODO: Set emails in web.config
            ExceptionMail.SendException(exp, U, Request.Form["sql"]);
            context.Server.ClearError();
        }
    }
}