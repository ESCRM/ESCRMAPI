using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IDS.EBSTCRM.WindowManager.Integration
{
    public partial class Sharing
    {
        public enum sharing
        {
            None,
            Read,
            ReadWrite
        }

        [Serializable]
        public class Organisation
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public sharing SharingState { get; set; }

            public Organisation()
            {

            }

            public Organisation(IDS.EBSTCRM.Base.AVNEntityDefaultShared dfs)
            {
                this.Id = dfs.OrganisationId;
                this.Name = dfs.SharedWithOrganisationName;
                this.SharingState = dfs.ReadWriteState == 1 ? sharing.Read : dfs.ReadWriteState == 2 ? sharing.ReadWrite : sharing.None;
            }

            public Organisation(IDS.EBSTCRM.Base.Organisation org)
            {
                this.Id = org.Id;
                this.Name = org.Name;
                this.SharingState = sharing.None;
            }
        }
    }
}