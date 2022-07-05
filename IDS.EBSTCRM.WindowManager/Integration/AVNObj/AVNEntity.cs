using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IDS.EBSTCRM.WindowManager.Integration
{
    [Serializable]
    public class AVNEntity
    {
        #region Enums

        public enum dateFormat
        {
            DDMMYYYY,
            MMDDYYYY,
            YYYYMMDD
        }

        public enum decimalSeparator
        {
            Dot,
            Comma
        }

        #endregion

        #region Properties

        public int AVNTypeId { get; set; }
        public int? EntityId { get; set; }

        public string Name { get; set; }
        public AVNType.parentType ParentType { get; set; }
        public int? ParentId { get; set; }
        
        public dateFormat DateFormat { get; set; }
        public decimalSeparator DecimalSeparator { get; set; }

        public AVNField[] Fields { get; set; }

        public Sharing.Organisation[] SharedWithOrganisations { get; set; }
        public Sharing.Usergroup[] SharedWithUsergroups { get; set; }

        public DateTime[] Reminders { get; set; }

        #endregion

        #region Constructors

        public AVNEntity()
        {

        }

        #endregion
    }
}