using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using IDS.EBSTCRM.Base;

namespace IDS.EBSTCRM.WindowManager.Integration
{
    [Serializable]
    public class Businesscase
    {
        public int? Id { get; set; }
        //@OrganisationId int,
	    public string Name { get; set; }
	    //@UserId uniqueidentifier,
	    public int? SMVContactId  { get; set; }
	    public int? SMVCompanyId  { get; set; }
	    public int SharedWith { get; set; }
	    public string SharePointUrl { get; set; }
	    public string Pipeline { get; set; }
	    public string Navn { get; set; }
	    public string Kort_beskrivelse { get; set; }
	    public string Deltagende_virksomheder { get; set; }
        public string Subbrands { get; set; }
	    public string Ansvarlig { get; set; }
	    public DateTime? Startdato { get; set; }
        public DateTime? Slutdato { get; set; }
        public DateTime? Opfølgningsdato { get; set; }

        public Businesscase()
        {

        }

        public Businesscase(ref SqlDataReader dr, bool Sandkasse)
        {
            //@OrganisationId int,
	        //@UserId uniqueidentifier,
            Id = TypeCast.ToInt(dr["Id"]);
            Name = TypeCast.ToString(dr["Name"]);

            SMVContactId = TypeCast.ToInt(dr["SMVContactId"]);
            SMVCompanyId = TypeCast.ToInt(dr["SMVCompanyId"]);
            SharedWith = TypeCast.ToInt(dr["SharedWith"]);

            SharePointUrl = TypeCast.ToString(dr["12060_SharePointUrl"]);
            Pipeline = TypeCast.ToString(dr["12059_Pipeline"]);
            Navn = TypeCast.ToString(dr["12061_Navn"]);
            Kort_beskrivelse = TypeCast.ToString(dr["12063_Kort beskrivelse"]);
            Deltagende_virksomheder = TypeCast.ToString(dr["12064_Deltagende virksomheder"]);
            Ansvarlig = TypeCast.ToString(dr["12066_Ansvarlig"]);
            Startdato = TypeCast.ToDateTimeLoose(dr["12067_Startdato"]);
            Slutdato = TypeCast.ToDateTimeLoose(dr["12068_Slutdato"]);
            Opfølgningsdato = TypeCast.ToDateTimeLoose(dr["12069_Opfølgningsdato"]);

            if(Sandkasse)
                Subbrands = TypeCast.ToString(dr["17667_Subbrand"]);
            else
                Subbrands = TypeCast.ToString(dr["18965_Subbrand"]);
        }
    }
}