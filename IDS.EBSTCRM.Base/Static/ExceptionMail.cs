using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace IDS.EBSTCRM.Base {
    /// <summary>
    /// Handles all Exception Emails
    /// Forwards formatted Exceptions to the EmailManager and Sends them to the correct Personnel.
    /// </summary>
    public static class ExceptionMail {
        public static void SendException(Exception e) {
            SendException(e, new User(), null);
        }
        public static void SendException(Exception e, string reportGeneratorSQL) {
            SendException(e, new User(), reportGeneratorSQL);
        }

        public static void SendException(Exception e, Organisation O) {
            User U = new User();
            U.Organisation = O;
            U.OrganisationId = O.Id;
            SendException(e, U, null);
        }
        public static void SendException(Exception e, Organisation O, string reportGeneratorSQL) {
            User U = new User();
            U.Organisation = O;
            U.OrganisationId = O.Id;
            SendException(e, U, reportGeneratorSQL);
        }

        public static void SendException(Exception e, User U) {
            SendException(e, U, null);
        }

        public static void SendException(Exception e, User user, string reportGeneratorSQL, HttpRequest userRequest = null) {

            //System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient(System.Configuration.ConfigurationManager.AppSettings["SMTPServer"]);
            //System.Net.Mail.MailMessage msg = new System.Net.Mail.MailMessage();

            string From = System.Configuration.ConfigurationManager.AppSettings["ExceptionSender"];
            string SendTo = System.Configuration.ConfigurationManager.AppSettings["ExceptionRecipts"];
            string Subject = "Fejl i CRM: " + DateTime.Now.ToString("dd-MM-yyyy"); // TypeCast.ToString(e.Message);
            string Body = "";

            if (user != null) {
                Body = "User Id        : " + user.Id + "\r\n" +
                            "Username       : " + user.Username + "\r\n" +
                            "OrganisationId : " + user.OrganisationId + "\r\n" +
                            "Organisation   : " + (user.Organisation != null ? user.Organisation.Name : "Ukendt organisation") + "\r\n";
            }

            try {
                Body += "IP: " + HttpContext.Current.Request.UserHostAddress + "\r\n";
            } catch (Exception) { }

            //Add all Exceptions
            AddException:
            Body += e.Message + "\r\n\r\n" +
                    e.StackTrace;

            if (e.InnerException != null) {
                Body += "\r\n\r\n";
                e = e.InnerException;
                goto AddException;
            }

            //Add Report Generator SQL
            if (reportGeneratorSQL != null && reportGeneratorSQL != "") {
                Body += "\r\n\r\nREPORT GENERATOR EXCEPTION FAULT IN SQL:\r\n\r\n" + reportGeneratorSQL + "\r\n\r\n";
            }


            //Add User HttpRequest Object
            if (userRequest != null) {
                try {
                    // Query String
                    if (userRequest.QueryString != null && userRequest.QueryString.Count > 0) {
                        Body += "\r\n\r\nRequest Query String:\r\n";
                        var queryStrings = userRequest.QueryString;
                        for (var i = 0; i < queryStrings.Count; i++) {
                            Body += string.Format("{0}: {1}", queryStrings.GetKey(i), string.Join(" | ", queryStrings.GetValues(i))) + "\r\n";
                        }
                    }

                    // Request Form
                    if (userRequest.Form != null) {
                        Body += "\r\n\r\nRequest Form:\r\n";
                        var forms = userRequest.Form;
                        for (var i = 0; i < forms.Count; i++) {
                            Body += string.Format("{0}: {1}", forms.GetKey(i), string.Join(" | ", forms.GetValues(i))) + "\r\n";
                        }
                    }
                } catch (Exception) { }
            } else {
                try {

                    // HttpRequest Object
                    if (HttpContext.Current != null && HttpContext.Current.Request != null) {

                        // Query String
                        if (HttpContext.Current.Request.QueryString != null && HttpContext.Current.Request.QueryString.Count > 0) {
                            Body += "\r\n\r\nRequest Query String:\r\n";
                            var queryStrings = HttpContext.Current.Request.QueryString;
                            for (var i = 0; i < queryStrings.Count; i++) {
                                Body += string.Format("{0}: {1}", queryStrings.GetKey(i), string.Join(" | ", queryStrings.GetValues(i))) + "\r\n";
                            }
                        }

                        // Request Form
                        if (HttpContext.Current.Request.Form != null) {
                            Body += "\r\n\r\nRequest Form:\r\n";
                            var forms = HttpContext.Current.Request.Form;
                            for (var i = 0; i < forms.Count; i++) {
                                Body += string.Format("{0}: {1}", forms.GetKey(i), string.Join(" | ", forms.GetValues(i))) + "\r\n";
                            }
                        }
                    }
                } catch (Exception) { }
            }

            //Set mail to be saved to disc for storage and delivery
            EmailManager.SendEmail(From, SendTo, Subject, Body.Replace("\n", "<br>"), true);
        }
    }
}
