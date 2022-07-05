using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Collections.Generic;

namespace IDS.EBSTCRM.WindowManager
{
    public class multipleRowsManager
    {
        public string Id = "";
        public List<string> Buttons = new List<string>();

        public multipleRowsManager(string id)
        {
            Id = id;
        }

        public string Render(int rows)
        {
            string html = "<table border=\"0\" cellpadding=\"0\" cellspacing=\"4\" style=\"width:100%;\"><tr>";
            int counter = 0;

            //Fix sorting
            List<string>[] btnArr = new List<string>[rows];
            int ct = 0;
            int step = 0;
            decimal steps = Math.Floor((decimal)Buttons.Count / rows);

            foreach (string b in Buttons)
            {
                if (btnArr[ct] == null) btnArr[ct] = new List<string>();
                //b.Name += " " + step + " - " + steps;
                btnArr[ct].Add(b);

                step++;
                if (step >= steps) { step = 0; ct++; }
                if (ct >= btnArr.Length) ct = 0;

            }

            int buttonCount = Buttons.Count;
            Buttons.Clear();

            ct = 0;
            step = 0;
            for (int bt = 0; bt < buttonCount; bt++)
            {

                if (btnArr.Length > ct)
                {
                    if (btnArr[ct] == null) btnArr[ct] = new List<string>();
                    if (btnArr[ct].Count > step)
                        Buttons.Add(btnArr[ct][step]);
                }


                ct++;
                if (ct == btnArr.Length) { ct = 0; step++; }

            }

            string width = ((int)100 / rows).ToString();

            foreach (string b in Buttons)
            {
                if (counter % rows == 0) html += "</tr><tr>";
                html += "<td style=\"width:" + width + "%;\">" + b + "</td>";

                counter++;
            }

            html += "</tr></table>";

            return html;
        }

    }
}
