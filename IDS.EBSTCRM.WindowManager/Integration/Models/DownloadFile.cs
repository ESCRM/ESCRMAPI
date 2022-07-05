using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IDS.EBSTCRM.WindowManager.Integration.Models {
    public class DownloadFileResponse : Response {
        public int FileId { get; set; }
        public string Filename { get; set; }
        public byte[] Data { get; set; }
    }

    //** Er til at slette filer med, ESCRM-163
    public class DeleteFileResponse : Response
    {
        public int FileId { get; set; }
        public string Filename { get; set; }
        public int SprocStatus { get; set; }
        public byte[] Data { get; set; }
    }
}