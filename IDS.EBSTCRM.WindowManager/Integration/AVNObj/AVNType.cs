using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IDS.EBSTCRM.WindowManager.Integration
{
    [Serializable]
    public class AVNType
    {
        #region Enums

        public enum parentType
        {
            Contact,
            Company
        }

        #endregion

        #region Properties

        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public parentType ParentType { get; set; }
        public string CreatedBy { get; set; }
        public string LastUpdatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? LastUpdatedDate { get; set; }

        public List<Sharing.Organisation> DefaultSharedWithOrganisations { get; set; }
        public List<Sharing.Usergroup> DefaultSharedWithUsergroups { get; set; }

        #endregion

        #region Constructors

        public AVNType()
        {

        }

        public AVNType(IDS.EBSTCRM.Base.AdminAVN a)
        {
            this.Id = a.Id;
            this.Name = a.Name;
            this.Description = a.Description;
            this.ParentType = a.SaveToCompany ? parentType.Company : parentType.Contact;
            this.CreatedBy = a.CreatedByUser;
            this.LastUpdatedBy = a.LastUpdatedByUser;
            this.CreatedDate = a.DateCreated;
            this.LastUpdatedDate = a.LastUpdated;

            this.DefaultSharedWithOrganisations = new List<Sharing.Organisation>();
            this.DefaultSharedWithUsergroups = new List<Sharing.Usergroup>();

            foreach (IDS.EBSTCRM.Base.AVNEntityDefaultShared dfs in a.DefaultSharedWith)
            {
                if (dfs.OrganisationId > 0)
                {
                    this.DefaultSharedWithOrganisations.Add(new Sharing.Organisation(dfs));
                }
                else
                {
                    this.DefaultSharedWithUsergroups.Add(new Sharing.Usergroup(dfs));
                }
            }
        }

        #endregion
    }
}