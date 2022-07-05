using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IDS.EBSTCRM.WindowManager.Integration
{
    [Serializable()]
    public class OutlookUserInfo
    {
        public string Id { get; set; }
        public string Initials { get; set; }
        public string Username { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string OrganisatioName { get; set; }
        public int OrganisationId { get; set; }
        public int UserLevel { get; set; }
        public bool EarlyWarningAccess { get; set; }

        public OutlookUserInfo()
        {

        }

        public OutlookUserInfo(IDS.EBSTCRM.Base.User u)
        {
            Id  = u.Id;
            Initials = u.Initials;

            Username = u.Username;
            Firstname = u.Firstname;
            Lastname = u.Lastname;
            OrganisatioName = u.Organisation.Name;
            OrganisationId = u.OrganisationId;
            UserLevel = (int)u.UserRole;
            EarlyWarningAccess = u.EarlyWarningUser;
        }
    }
}