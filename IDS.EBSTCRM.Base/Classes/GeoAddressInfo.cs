using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IDS.EBSTCRM.Base
{
    [Serializable()]
    public class GeoAddressInfo
    {
        public string[] Adresses { get; set; }
        public int TotalAdresses { get; set; }
        public int MappedAdresses { get; set; }

        public GeoAddressInfo()
        {

        }

    }
}
