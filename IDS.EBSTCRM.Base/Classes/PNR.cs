using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IDS.EBSTCRM.Base.Classes {

    public class PNRShards {
        public int? total { get; set; }
        public int? successful { get; set; }
        public int? failed { get; set; }
    }

    public class PNRPeriode {
        public string gyldigFra { get; set; }
        public string gyldigTil { get; set; }
    }

    public class PNRNavne {
        public string navn { get; set; }
        public PNRPeriode periode { get; set; }
        public DateTime sidstOpdateret { get; set; }
    }

    public class PNRPeriode2 {
        public string gyldigFra { get; set; }
        public object gyldigTil { get; set; }
    }

    public class PNRKommune {
        public int? kommuneKode { get; set; }
        public string kommuneNavn { get; set; }
        //public PNRPeriode2 periode { get; set; }
        //public DateTime sidstOpdateret { get; set; }
    }

    public class PNRPeriode3 {
        public string gyldigFra { get; set; }
        public string gyldigTil { get; set; }
    }

    public class PNRBeliggenhedsadresse {
        public string landekode { get; set; }
        public object fritekst { get; set; }
        public int? vejkode { get; set; }
        public PNRKommune kommune { get; set; }
        public int? husnummerFra { get; set; }
        public string adresseId { get; set; }
        public DateTime? sidstValideret { get; set; }
        public object husnummerTil { get; set; }
        public object bogstavFra { get; set; }
        public object bogstavTil { get; set; }
        public object etage { get; set; }
        public object sidedoer { get; set; }
        public object conavn { get; set; }
        public object postboks { get; set; }
        public string vejnavn { get; set; }
        public object bynavn { get; set; }
        public int? postnummer { get; set; }
        public string postdistrikt { get; set; }
        public PNRPeriode3 periode { get; set; }
        public DateTime sidstOpdateret { get; set; }
    }

    public class PNRPeriode4 {
        public string gyldigFra { get; set; }
        public object gyldigTil { get; set; }
    }

    public class PNRLivsforloeb {
        public PNRPeriode4 periode { get; set; }
        public DateTime sidstOpdateret { get; set; }
    }

    public class PNRPeriode5 {
        public string gyldigFra { get; set; }
        public string gyldigTil { get; set; }
    }

    public class PNRHovedbranche {
        public string branchekode { get; set; }
        public string branchetekst { get; set; }
        public PNRPeriode5 periode { get; set; }
        public DateTime sidstOpdateret { get; set; }
    }

    public class PNRAarsbeskaeftigelse {
        public int aar { get; set; }
        public object antalInklusivEjere { get; set; }
        public object antalAarsvaerk { get; set; }
        public object antalAnsatte { get; set; }
        public DateTime sidstOpdateret { get; set; }
        public string intervalKodeAntalInklusivEjere { get; set; }
        public string intervalKodeAntalAarsvaerk { get; set; }
        public string intervalKodeAntalAnsatte { get; set; }
    }

    public class PNRKvartalsbeskaeftigelse {
        public int aar { get; set; }
        public int kvartal { get; set; }
        public object antalAarsvaerk { get; set; }
        public object antalAnsatte { get; set; }
        public DateTime sidstOpdateret { get; set; }
        public string intervalKodeAntalAarsvaerk { get; set; }
        public string intervalKodeAntalAnsatte { get; set; }
    }

    //** Ny klasse erstmaanedsbeskaeftigelse, ESCRM-211/213
    public class PNRErstMaanedsbeskaeftigelse
    {
        public int aar { get; set; }
        public int maaned { get; set; }
        public object antalAarsvaerk { get; set; }
        public object antalAnsatte { get; set; }
        //public DateTime sidstOpdateret { get; set; }
        //public string intervalKodeAntalAarsvaerk { get; set; }
        //public string intervalKodeAntalAnsatte { get; set; }
    }

    public class PNRPeriode6 {
        public string gyldigFra { get; set; }
        public string gyldigTil { get; set; }
    }

    public class PNRVirksomhedsrelation {
        public int? cvrNummer { get; set; }
        public PNRPeriode6 periode { get; set; }
        public DateTime sidstOpdateret { get; set; }
    }

    public class PNRPeriode7 {
        public string gyldigFra { get; set; }
        public object gyldigTil { get; set; }
    }

    public class PNRNyesteNavn {
        public string navn { get; set; }
        public PNRPeriode7 periode { get; set; }
        public DateTime sidstOpdateret { get; set; }
    }

    public class PNRPeriode8 {
        public string gyldigFra { get; set; }
        public object gyldigTil { get; set; }
    }

    public class PNRKommune2 {
        public int kommuneKode { get; set; }
        public string kommuneNavn { get; set; }
        public PNRPeriode8 periode { get; set; }
        public DateTime sidstOpdateret { get; set; }
    }

    public class PNRPeriode9 {
        public string gyldigFra { get; set; }
        public object gyldigTil { get; set; }
    }

    public class PNRNyesteBeliggenhedsadresse {
        public string landekode { get; set; }
        public object fritekst { get; set; }
        public int vejkode { get; set; }
        public PNRKommune2 kommune { get; set; }
        public int husnummerFra { get; set; }
        public string adresseId { get; set; }
        public DateTime sidstValideret { get; set; }
        public object husnummerTil { get; set; }
        public object bogstavFra { get; set; }
        public object bogstavTil { get; set; }
        public object etage { get; set; }
        public object sidedoer { get; set; }
        public object conavn { get; set; }
        public object postboks { get; set; }
        public string vejnavn { get; set; }
        public object bynavn { get; set; }
        public int postnummer { get; set; }
        public string postdistrikt { get; set; }
        public PNRPeriode9 periode { get; set; }
        public DateTime sidstOpdateret { get; set; }
    }

    public class PNRPeriode10 {
        public string gyldigFra { get; set; }
        public object gyldigTil { get; set; }
    }

    public class PNRNyesteHovedbranche {
        public string branchekode { get; set; }
        public string branchetekst { get; set; }
        public PNRPeriode10 periode { get; set; }
        public DateTime sidstOpdateret { get; set; }
    }

    public class PNRPeriode11 {
        public string gyldigFra { get; set; }
        public object gyldigTil { get; set; }
    }

    public class PNRNyesteBibranche1 {
        public string branchekode { get; set; }
        public string branchetekst { get; set; }
        public PNRPeriode11 periode { get; set; }
        public DateTime sidstOpdateret { get; set; }
    }

    public class PNRNyesteAarsbeskaeftigelse {
        public int aar { get; set; }
        public object antalInklusivEjere { get; set; }
        public object antalAarsvaerk { get; set; }
        public object antalAnsatte { get; set; }
        public DateTime sidstOpdateret { get; set; }
        public string intervalKodeAntalInklusivEjere { get; set; }
        public string intervalKodeAntalAarsvaerk { get; set; }
        public string intervalKodeAntalAnsatte { get; set; }
    }

    public class PNRNyesteKvartalsbeskaeftigelse {
        public int aar { get; set; }
        public int kvartal { get; set; }
        public object antalAarsvaerk { get; set; }
        public object antalAnsatte { get; set; }
        public DateTime sidstOpdateret { get; set; }
        public string intervalKodeAntalAarsvaerk { get; set; }
        public string intervalKodeAntalAnsatte { get; set; }
    }

    public class PNRProduktionsEnhedMetadata {
        //public PNRNyesteBeliggenhedsadresse nyesteBeliggenhedsadresse { get; set; }
        public PNRNyesteHovedbranche nyesteHovedbranche { get; set; }
        public PNRNyesteBibranche1 nyesteBibranche1 { get; set; }
        public object nyesteBibranche2 { get; set; }
        public object nyesteBibranche3 { get; set; }
        public List<object> nyesteKontaktoplysninger { get; set; }
        public int nyesteCvrNummerRelation { get; set; }
        public NyesteAarsbeskaeftigelse nyesteAarsbeskaeftigelse { get; set; }
        public NyesteKvartalsbeskaeftigelse nyesteKvartalsbeskaeftigelse { get; set; }
        public string sammensatStatus { get; set; }
        public PNRNyesteNavn nyesteNavn { get; set; }
    }

    public class PNRVrproduktionsEnhed {
        public int pNummer { get; set; }
        public List<PNRNavne> navne { get; set; }
        public List<PNRBeliggenhedsadresse> beliggenhedsadresse { get; set; }
        public object brancheAnsvarskode { get; set; }
        public bool reklamebeskyttet { get; set; }

        //public List<object> telefonNummer { get; set; }
        //public List<object> telefaxNummer { get; set; }
        //public List<object> elektroniskPost { get; set; }
        //public List<PNRLivsforloeb> livsforloeb { get; set; }
        //public List<PNRHovedbranche> hovedbranche { get; set; }
        //public List<object> bibranche1 { get; set; }
        //public List<object> bibranche2 { get; set; }
        //public List<object> bibranche3 { get; set; }
        //public List<PNRAarsbeskaeftigelse> aarsbeskaeftigelse { get; set; }
        //public List<Kvartalsbeskaeftigelse> kvartalsbeskaeftigelse { get; set; }
        
        //** Nyt felt erstmaanedsbeskaeftigelse, ESCRM-211/213
        public List<PNRErstMaanedsbeskaeftigelse> erstmaanedsbeskaeftigelse { get; set; }
        public List<PNRVirksomhedsrelation> virksomhedsrelation { get; set; }
        //public List<object> attributter { get; set; }
        //public List<object> deltagerRelation { get; set; }
        //public PNRProduktionsEnhedMetadata produktionsEnhedMetadata { get; set; }
        //public int samtId { get; set; }
        //public bool fejlRegistreret { get; set; }
        //public int dataAdgang { get; set; }
        //public object enhedsNummer { get; set; }
        //public string enhedstype { get; set; }
        //public DateTime sidstIndlaest { get; set; }
        //public DateTime sidstOpdateret { get; set; }
        //public bool fejlVedIndlaesning { get; set; }
        //public object naermesteFremtidigeDato { get; set; }
        //public object fejlBeskrivelse { get; set; }
        //public string virkningsAktoer { get; set; }
    }

    public class PNRSource {
        public PNRVrproduktionsEnhed VrproduktionsEnhed { get; set; }
    }

    public class PNRHit {
        public string _index { get; set; }
        public string _type { get; set; }
        public string _id { get; set; }
        public double? _score { get; set; }
        public PNRSource _source { get; set; }
    }

    public class PNRHits {
        public int? total { get; set; }
        public double? max_score { get; set; }
        public List<PNRHit> hits { get; set; }
    }

    public class PNRRootObject {
        public string _scroll_id { get; set; }
        public int? took { get; set; }
        public bool timed_out { get; set; }
        public PNRShards _shards { get; set; }
        public PNRHits hits { get; set; }
    }
}
