using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

namespace IDS.EBSTCRM.Base {
    /// <summary>
    /// Email Manager
    /// Handles all emails sent from the CRM system 
    /// </summary>
    public static class EmailManager {
        public static string emailBody = "<html>" +
                                        "<head></head>" +
                                        "<body style=\"" +
                                        " margin:0px;" +
                                        " font-family:arial;" +
                                        " font-size:12px;" +
                                        " color:black;" +
                                        " \">" +
                                            " <div style=\"" +
                                            " padding:12px;" +
                                            " background-color:white;" +
                                            " font-family:arial;" +
                                            " font-size:12px;" +
                                            " color:black;" +
                                            " height:100%;" +
                                            " \">" +
                                                " %content%" +
                                            " </div>" +
                                        "</body>" +
                                        "</html>";

        public static void SendEmail(string Sender, string Receiver, string Subject, string Body) {
            SendEmail(Sender, Receiver, null, Subject, Body, false);
        }
        public static void SendEmail(string Sender, string Receiver, string ReplyTo, string Subject, string Body) {
            SendEmail(Sender, Receiver, ReplyTo, Subject, Body, false);
        }

        public static void SendEmail(string Sender, string Receiver, string Subject, string Body, bool ForcefullySendToCorrectEmail) {
            SendEmail(Sender, Receiver, null, Subject, Body, ForcefullySendToCorrectEmail, null);
        }

        public static void SendEmail(string Sender, string Receiver, string ReplyTo, string Subject, string Body, bool ForcefullySendToCorrectEmail, bool ShowFooter) {
            SendEmail(Sender, Receiver, ReplyTo, Subject, Body, ForcefullySendToCorrectEmail, null, ShowFooter);
        }

        public static void SendEmail(string Sender, string Receiver, string ReplyTo, string Subject, string Body, bool ForcefullySendToCorrectEmail) {
            SendEmail(Sender, Receiver, ReplyTo, Subject, Body, ForcefullySendToCorrectEmail, null);
        }

        public static void SendEmail(string Sender, string Receiver, string ReplyTo, string Subject, string Body, bool ForcefullySendToCorrectEmail, System.Collections.Generic.List<System.Net.Mail.Attachment> Attachments, bool ShowFooter = true) {
            string html = emailBody;

            string SandBox = "";
            string olMesage = "";

            string footer = "<div style=\"font-size:11px;color:gray;\"><br><br><b>Bemærk:</b> Denne email er sendt automatisk fra CRM-systemet.<br><br>For at logge ind på CRM systemet, gå da til <a href=\"http://www.escrm.dk\">www.escrm.dk</a>.</div>";

            string conn = System.Configuration.ConfigurationManager.AppSettings["connectionString"];
            string[] args = conn.Split(';');
            foreach (string a in args) {
                string[] ag = a.Trim().Split('=');
                if (ag[0].Trim().ToLower() == "initial catalog") {
                    if (ag[1].Trim().ToUpper() != "ESCRM_DRIFT" && ag[1].Trim().ToUpper() != "EBSTCRM_CRM2") {
                        SandBox = "<p style=\"color:#000000; background-color:#fffcce; padding:3px; border:solid 1px #000000;\"><b>BEMÆRK: Denne email er afsendt fra sandkassen!</b></p>";
                    }
                    break;
                }
            }

            //Use overuling email?
            string overRulingEmailAddress = TypeCast.ToString(System.Configuration.ConfigurationManager.AppSettings["OverRulingEmailAddress"]);
            if (overRulingEmailAddress != "" && overRulingEmailAddress.IndexOf("@") > -1 && !ForcefullySendToCorrectEmail) {
                olMesage = "<p style=\"color:#b00000; background-color:#fffcce; padding:3px; border:solid 1px #000000;\"><b>BEMÆRK: Modtageren af denne email \"" + Receiver + "\" er blevet erstattet med \"" + overRulingEmailAddress + "\"!</b></p>";
                Receiver = overRulingEmailAddress;
            }

            html = html.Replace("%hostname%", System.Configuration.ConfigurationManager.AppSettings["CurrentHost"]);
            html = html.Replace("%header%", Subject);
            html = html.Replace("%content%", olMesage + SandBox + Body + SandBox + olMesage + (ShowFooter ? footer : ""));

            System.Net.Mail.MailMessage m = new System.Net.Mail.MailMessage();
            try {

                // - This code commented during contact agreement email
                //if (ReplyTo != null && ReplyTo != "") {
                //    m.ReplyToList.Add(new System.Net.Mail.MailAddress(Sender));
                //    m.From = new System.Net.Mail.MailAddress("info@escrm.dk");
                //} else {
                //    m.From = new System.Net.Mail.MailAddress(Sender);
                //}

                if (ReplyTo != null && ReplyTo != "") {
                    m.ReplyToList.Add(new System.Net.Mail.MailAddress(ReplyTo));
                } 

                m.From = new System.Net.Mail.MailAddress(Sender);
                m.Subject = Subject;
                m.Body = html;
                m.IsBodyHtml = true;

                if (Attachments != null) {
                    foreach (System.Net.Mail.Attachment ma in Attachments)
                        m.Attachments.Add(ma);
                }

                string[] toEmails = Receiver.Split(';');
                foreach (string t in toEmails) {
                    try {
                        System.Net.Mail.MailAddress mailAddress = new System.Net.Mail.MailAddress(t);
                        if (m.To.Contains(mailAddress) == false)
                            m.To.Add(mailAddress);
                    } catch { }
                }

                string smtpServer = TypeCast.ToString(System.Configuration.ConfigurationManager.AppSettings["SMTPServer"]);
                if (smtpServer == "") smtpServer = "172.16.2.31";

                System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient(smtpServer);

                //Set mail to be saved to disc for storage and delivery
                string delFolder = TypeCast.ToString(System.Configuration.ConfigurationManager.AppSettings["SMTPDeliveryDirectory"]);
                if (delFolder != "" && System.IO.Directory.Exists(delFolder)) {
                    smtp.PickupDirectoryLocation = delFolder;
                    smtp.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.SpecifiedPickupDirectory;
                }

                smtp.Send(m);
            } catch (Exception exp) {
                //IDS.EBSTCRM.Base.ExceptionMail.SendException(exp);
                //Something went wrong - save the message to disc, and let Exchange Server Pick it up.
                System.Diagnostics.Debug.WriteLine(exp.Message);
            }
        }
    }
}