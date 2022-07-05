using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IDS.EBSTCRM.WindowManager.Integration.Models {

    [Serializable]
    public class UploadContactFile {
        public Credential Credential { get; set; }
        public int ContactId { get; set; }
        public string FileName { get; set; }
        public Shared Shared { get; set; }
        public byte[] FileBytes { get; set; }
        public UploadContactFile() {
            Shared = new Shared();
        }
    }

    [Serializable]
    public class Shared {
        public List<int> OrganisationId { get; set; }
    }
}