using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IDS.EBSTCRM.WindowManager.Integration
{
    [Serializable]
    public partial class Sharing
    {
        [Serializable]
        public class Usergroup
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public sharing SharingState { get; set; }

            public Usergroup()
            {

            }

            public Usergroup(IDS.EBSTCRM.Base.AVNEntityDefaultShared dfs)
            {
                this.Id = dfs.UsergroupId;
                this.Name = dfs.UserGroupName;
                this.SharingState = dfs.ReadWriteState == 1 ? sharing.Read : dfs.ReadWriteState == 2 ? sharing.ReadWrite : sharing.None;
            }

            public Usergroup(IDS.EBSTCRM.Base.Usergroup ug)
            {
                this.Id = ug.Id;
                this.Name = ug.Name;
                this.SharingState = sharing.None;
            }
        }
    }
}