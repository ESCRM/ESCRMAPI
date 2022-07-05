using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IDS.EBSTCRM.WindowManager.Integration
{
    /// <summary>
    /// Gets organisations for Outlook integration
    /// </summary>
    public class Outlook_Organisation
    {
        public int Id { get; set; }
        public int ParentId { get; set; }
        public string Name { get; set; }

        public Outlook_Organisation[] Children { get; set; }

        public Outlook_Organisation()
        {


        }

        public Outlook_Organisation(int id, int parentId, string name)
        {
            Id = id;
            ParentId = parentId;
            Name = name;
        }
    }
}