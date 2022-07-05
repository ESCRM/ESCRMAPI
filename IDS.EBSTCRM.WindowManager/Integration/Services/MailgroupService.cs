using IDS.EBSTCRM.Base;
using IDS.EBSTCRM.WindowManager.Integration.Models;
using System.Collections.Generic;

namespace IDS.EBSTCRM.WindowManager.Integration.Services
{
    public class MailgroupService
    {
        /// <summary>
        /// Returns accesslevel for Mailgroup
        /// -1   =  Nothing found
        ///  0   =  Group not accessible if exists
        ///  1   =  Readaccess
        ///  2   =  Write
        ///  3   =  Owning org
        /// </summary>
        /// <param name="user"></param>
        /// <param name="mailgroupId"></param>
        /// <returns></returns>
        public static int UserAccessLevelForMailgroup(User user, int mailgroupId)
        {
            if (user == null || mailgroupId<1)
                return 0;

            var context = new SQLDB();
            // Get list of active mailgroups fro user
            //List<Mailgroup> mgs = context.Mailgroups_getMailgroupsByOrgId(user.OrganisationId, 0, false);

            List<Mailgroup> mgs = context.Mailgroups_getMailgroupsFromOrganisationId(user.OrganisationId);

            // if owner and active return owner access
            bool owner = mgs.Exists(m => m.id == mailgroupId && m.organisationId==user.OrganisationId && m.DeactivatedDate is null);                       
            if(owner)
            {
                context.Dispose();
                context = null;
                return 3;
            }

            // If deactivated return read access
            bool ownerDeactivated = mgs.Exists(m => m.id == mailgroupId && m.organisationId == user.OrganisationId && m.DeactivatedDate != null);
            if (ownerDeactivated)
            {
                context.Dispose();
                context = null;
                return 1;
            }

            // If we get here mail group not owned by user organisation, check if shared
            List<MailgroupShared> smgs = context.Mailgroup_GetSharing(mailgroupId);

            // If writeable return write access
            bool write = smgs.Exists(sm => sm.MailgroupId == mailgroupId && sm.SharedWithOrganisationId == user.OrganisationId && sm.Writeable==true);
            if (write)
            {            
                context.Dispose();
                context = null;
                return 2;
            }

            // if shared readable return read access
            bool read = smgs.Exists(sm => sm.MailgroupId == mailgroupId && sm.SharedWithOrganisationId == user.OrganisationId && sm.Writeable == false);
            if (write)
            {
                return 1;
            }

            // nothing found return 0
            return 0;
        }

    }
}