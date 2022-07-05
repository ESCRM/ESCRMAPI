using System;
using System.Collections.Generic;
using System.Text;

namespace IDS.EBSTCRM.Base
{
    /// <summary>
    /// Generic Email Manager
    /// Sends emails without notification of SANDKASSE or LIVE version
    /// </summary>
    public static class GenericEmail
    {
        public static void SendEmail(string Sender, string Reciever, string Title, string Body, bool IsHTML)
        {
            System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient(System.Configuration.ConfigurationManager.AppSettings["SMTPServer"]);
            System.Net.Mail.MailMessage msg = new System.Net.Mail.MailMessage();

            msg.From = new System.Net.Mail.MailAddress(Sender);
            msg.Bcc.Add("escrm@ehsyd.dk");

            foreach (string adr in Reciever.Split(';'))
            {
                if (adr != null && adr != "" && adr.IndexOf("@") > -1)
                {
                    msg.To.Add(adr);
                }
            }

            msg.Subject = Title;

            msg.IsBodyHtml = IsHTML;
            msg.Body = Body;

            try { smtp.Send(msg); }
            catch (Exception exp) { ExceptionMail.SendException(exp); };
            smtp = null;
        }
    }
}
