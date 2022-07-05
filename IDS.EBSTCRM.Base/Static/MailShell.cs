using System;
using System.Collections.Generic;
using System.Text;

namespace IDS.EBSTCRM.Base
{
    /// <summary>
    /// Wraps email content into preformatted HTML
    /// </summary>
    public static class MailShell
    {
        public static string Shell = "<html>" +
                                "<head>" +
                                "</head>" +
                                "<body style=\"background-color:#white;font-family:Tahoma;font-size:12px; color:black\">" +
                                    "<h1 style=\"width:100%;display:inline;font-size:16px;background-color:#d9e9ff; border-bottom: solid 1px #7eadd3; color: #15428b;\">%subject%</h1>" +
                                    "<div style=\"padding:10px;margin:10px;color:Black;\">%content%</div>" +
                                "</body>" +
                                "</html>";

        public static string ParseMail(string Subject, string Content)
        {
            return Shell.Replace("%subject%", Subject).Replace("%content%", Content);
        }
    }
}
