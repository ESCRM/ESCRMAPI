using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IDS.EBSTCRM.Base.Classes {

    public class CVR {

        public string Response { get; set; }
        public DateTime SidstOpdateret { get; set; }

        public int CVRNumber { get; set; }

        public string SammensatStatus { get; set; }
        public string StiftelsesDato { get; set; }
        public string VirkningsDato { get; set; }

        public int? KommuneKode { get; set; }
        public string KommuneNavn { get; set; }
        public bool? Reklamebeskyttet { get; set; }

        public string BrancheTekst { get; set; }
        public string BrancheKode { get; set; }
        public string TelefaxNummer { get; set; }
        public string TelefonNummer { get; set; }
        public string VirksomhedsformLang { get; set; }
        public string VirksomhedsformKort { get; set; }
        public string ElektroniskPost { get; set; }
        public string Binavne { get; set; }
        public string HjemmeSide { get; set; }
    }

    #region Model mapping with Json result
    public class Shards {
        public int? total { get; set; }
        public int? successful { get; set; }
        public int? failed { get; set; }
    }

    public class Periode {
        public string gyldigFra { get; set; }
        public object gyldigTil { get; set; }
    }

    public class Navne {
        public string navn { get; set; }
        public Periode periode { get; set; }
        public string sidstOpdateret { get; set; }
    }

    public class Periode2 {
        public string gyldigFra { get; set; }
        public object gyldigTil { get; set; }
    }

    public class Binavne {
        public string navn { get; set; }
        public Periode2 periode { get; set; }
        public string sidstOpdateret { get; set; }
    }

    public class Periode3 {
        public string gyldigFra { get; set; }
        public string gyldigTil { get; set; }
    }

    public class Kommune {
        public int? kommuneKode { get; set; }
        public string kommuneNavn { get; set; }
        public Periode3 periode { get; set; }
        public string sidstOpdateret { get; set; }
    }

    public class Periode4 {
        public string gyldigFra { get; set; }
        public string gyldigTil { get; set; }
    }

    public class Beliggenhedsadresse {
        public string landekode { get; set; }
        public object fritekst { get; set; }
        public int? vejkode { get; set; }
        public Kommune kommune { get; set; }
        public int? husnummerFra { get; set; }
        public string adresseId { get; set; }
        public string sidstValideret { get; set; }
        public object husnummerTil { get; set; }
        public string bogstavFra { get; set; }
        public object bogstavTil { get; set; }
        public string etage { get; set; }
        public object sidedoer { get; set; }
        public object conavn { get; set; }
        public object postboks { get; set; }
        public string vejnavn { get; set; }
        public object bynavn { get; set; }
        public int? postnummer { get; set; }
        public string postdistrikt { get; set; }
        public Periode4 periode { get; set; }
        public string sidstOpdateret { get; set; }
    }

    public class Periode5 {
        public string gyldigFra { get; set; }
        public object gyldigTil { get; set; }
    }

    public class TelefonNummer {
        public string kontaktoplysning { get; set; }
        public bool? hemmelig { get; set; }
        public Periode5 periode { get; set; }
        public string sidstOpdateret { get; set; }
    }

    public class Periode6 {
        public string gyldigFra { get; set; }
        public object gyldigTil { get; set; }
    }

    public class TelefaxNummer {
        public string kontaktoplysning { get; set; }
        public bool? hemmelig { get; set; }
        public Periode6 periode { get; set; }
        public string sidstOpdateret { get; set; }
    }

    public class Periode7 {
        public string gyldigFra { get; set; }
        public object gyldigTil { get; set; }
    }

    public class ElektroniskPost {
        public string kontaktoplysning { get; set; }
        public bool? hemmelig { get; set; }
        public Periode7 periode { get; set; }
        public string sidstOpdateret { get; set; }
    }

    public class Hjemmeside {
        public string kontaktoplysning { get; set; }
        public bool? hemmelig { get; set; }
        public Periode7 periode { get; set; }
        public string sidstOpdateret { get; set; }
    }

    public class Periode8 {
        public string gyldigFra { get; set; }
        public object gyldigTil { get; set; }
    }

    public class Livsforloeb {
        public Periode8 periode { get; set; }
        public string sidstOpdateret { get; set; }
    }

    public class Periode9 {
        public string gyldigFra { get; set; }
        public string gyldigTil { get; set; }
    }

    public class Hovedbranche {
        public string branchekode { get; set; }
        public string branchetekst { get; set; }
        public Periode9 periode { get; set; }
        public string sidstOpdateret { get; set; }
    }

    public class Periode10 {
        public string gyldigFra { get; set; }
        public object gyldigTil { get; set; }
    }

    public class Virksomhedsstatu {
        public string status { get; set; }
        public Periode10 periode { get; set; }
        public string sidstOpdateret { get; set; }
    }

    public class Periode11 {
        public string gyldigFra { get; set; }
        public object gyldigTil { get; set; }
    }

    public class Virksomhedsform {
        public int? virksomhedsformkode { get; set; }
        public string kortBeskrivelse { get; set; }
        public string langBeskrivelse { get; set; }
        public string ansvarligDataleverandoer { get; set; }
        public Periode11 periode { get; set; }
        public string sidstOpdateret { get; set; }
    }

    public class Aarsbeskaeftigelse {
        public int? aar { get; set; }
        public object antalInklusivEjere { get; set; }
        public object antalAarsvaerk { get; set; }
        public object antalAnsatte { get; set; }
        public string sidstOpdateret { get; set; }
        public string intervalKodeAntalInklusivEjere { get; set; }
        public string intervalKodeAntalAarsvaerk { get; set; }
        public string intervalKodeAntalAnsatte { get; set; }
    }

    public class Kvartalsbeskaeftigelse {
        public int? aar { get; set; }
        public int? kvartal { get; set; }
        public object antalAarsvaerk { get; set; }
        public object antalAnsatte { get; set; }
        public string sidstOpdateret { get; set; }
        public string intervalKodeAntalAarsvaerk { get; set; }
        public string intervalKodeAntalAnsatte { get; set; }
    }

    public class Maanedsbeskaeftigelse {
        public int? aar { get; set; }
        public int? maaned { get; set; }
        public object antalAarsvaerk { get; set; }
        public object antalAnsatte { get; set; }
        public string sidstOpdateret { get; set; }
        public string intervalKodeAntalAarsvaerk { get; set; }
        public string intervalKodeAntalAnsatte { get; set; }
    }

    public class Periode12 {
        public string gyldigFra { get; set; }
        public object gyldigTil { get; set; }
    }

    public class Vaerdier {
        public string vaerdi { get; set; }
        public Periode12 periode { get; set; }
        public string sidstOpdateret { get; set; }
    }

    public class Attributter {
        public int? sekvensnr { get; set; }
        public string type { get; set; }
        public string vaerditype { get; set; }
        public List<Vaerdier> vaerdier { get; set; }
    }

    public class Periode13 {
        public string gyldigFra { get; set; }
        public object gyldigTil { get; set; }
    }

    public class Penheder {
        public int? pNummer { get; set; }
        public Periode13 periode { get; set; }
        public string sidstOpdateret { get; set; }
    }

    public class Periode14 {
        public string gyldigFra { get; set; }
        public string gyldigTil { get; set; }
    }

    public class Navne2 {
        public string navn { get; set; }
        public Periode14 periode { get; set; }
        public string sidstOpdateret { get; set; }
    }

    public class Periode15 {
        public string gyldigFra { get; set; }
        public string gyldigTil { get; set; }
    }

    public class Kommune2 {
        public int? kommuneKode { get; set; }
        public string kommuneNavn { get; set; }
        public Periode15 periode { get; set; }
        public string sidstOpdateret { get; set; }
    }

    public class Periode16 {
        public string gyldigFra { get; set; }
        public string gyldigTil { get; set; }
    }

    public class Beliggenhedsadresse2 {
        public string landekode { get; set; }
        public string fritekst { get; set; }
        public int? vejkode { get; set; }
        public Kommune2 kommune { get; set; }
        public int? husnummerFra { get; set; }
        public string adresseId { get; set; }
        public string sidstValideret { get; set; }
        public int? husnummerTil { get; set; }
        public string bogstavFra { get; set; }
        public object bogstavTil { get; set; }
        public string etage { get; set; }
        public string sidedoer { get; set; }
        public string conavn { get; set; }
        public object postboks { get; set; }
        public string vejnavn { get; set; }
        public object bynavn { get; set; }
        public int? postnummer { get; set; }
        public string postdistrikt { get; set; }
        public Periode16 periode { get; set; }
        public string sidstOpdateret { get; set; }
    }

    public class Deltager {
        public object enhedsNummer { get; set; }
        public string enhedstype { get; set; }
        public int? forretningsnoegle { get; set; }
        public object organisationstype { get; set; }
        public string sidstIndlaest { get; set; }
        public string sidstOpdateret { get; set; }
        public List<Navne2> navne { get; set; }
        public List<Beliggenhedsadresse2> beliggenhedsadresse { get; set; }
    }

    public class Periode17 {
        public string gyldigFra { get; set; }
        public object gyldigTil { get; set; }
    }

    public class OrganisationsNavn {
        public string navn { get; set; }
        public Periode17 periode { get; set; }
        public string sidstOpdateret { get; set; }
    }

    public class Periode18 {
        public string gyldigFra { get; set; }
        public object gyldigTil { get; set; }
    }

    public class Vaerdier2 {
        public string vaerdi { get; set; }
        public Periode18 periode { get; set; }
        public string sidstOpdateret { get; set; }
    }

    public class Attributter2 {
        public int? sekvensnr { get; set; }
        public string type { get; set; }
        public string vaerditype { get; set; }
        public List<Vaerdier2> vaerdier { get; set; }
    }

    public class Periode19 {
        public string gyldigFra { get; set; }
        public string gyldigTil { get; set; }
    }

    public class Vaerdier3 {
        public string vaerdi { get; set; }
        public Periode19 periode { get; set; }
        public string sidstOpdateret { get; set; }
    }

    public class Attributter3 {
        public int? sekvensnr { get; set; }
        public string type { get; set; }
        public string vaerditype { get; set; }
        public List<Vaerdier3> vaerdier { get; set; }
    }

    public class MedlemsData {
        public List<Attributter3> attributter { get; set; }
    }

    public class Organisationer {
        public object enhedsNummerOrganisation { get; set; }
        public string hovedtype { get; set; }
        public List<OrganisationsNavn> organisationsNavn { get; set; }
        public List<Attributter2> attributter { get; set; }
        public List<MedlemsData> medlemsData { get; set; }
    }

    public class DeltagerRelation {
        public Deltager deltager { get; set; }
        public List<object> kontorsteder { get; set; }
        public List<Organisationer> organisationer { get; set; }
    }

    public class Periode20 {
        public string gyldigFra { get; set; }
        public object gyldigTil { get; set; }
    }

    public class NyesteNavn {
        public string navn { get; set; }
        public Periode20 periode { get; set; }
        public string sidstOpdateret { get; set; }
    }

    //** For test indtil videre
    public class NyesteNavn2
    {
        public string navn { get; set; }
    }

    public class Periode21 {
        public string gyldigFra { get; set; }
        public object gyldigTil { get; set; }
    }

    public class NyesteVirksomhedsform {
        public int? virksomhedsformkode { get; set; }
        public string kortBeskrivelse { get; set; }
        public string langBeskrivelse { get; set; }
        public string ansvarligDataleverandoer { get; set; }
        public Periode21 periode { get; set; }
        public string sidstOpdateret { get; set; }
    }

    public class Periode22 {
        public string gyldigFra { get; set; }
        public object gyldigTil { get; set; }
    }

    public class Kommune3 {
        public int? kommuneKode { get; set; }
        public string kommuneNavn { get; set; }
        public Periode22 periode { get; set; }
        public string sidstOpdateret { get; set; }
    }

    public class Periode23 {
        public string gyldigFra { get; set; }
        public object gyldigTil { get; set; }
    }

    public class NyesteBeliggenhedsadresse {
        public string landekode { get; set; }
        public object fritekst { get; set; }
        public int? vejkode { get; set; }
        public Kommune3 kommune { get; set; }
        public int? husnummerFra { get; set; }
        public string adresseId { get; set; }
        public string sidstValideret { get; set; }
        public object husnummerTil { get; set; }
        public string bogstavFra { get; set; }
        public object bogstavTil { get; set; }
        public object etage { get; set; }
        public object sidedoer { get; set; }
        public object conavn { get; set; }
        public object postboks { get; set; }
        public string vejnavn { get; set; }
        public object bynavn { get; set; }
        public int? postnummer { get; set; }
        public string postdistrikt { get; set; }
        public Periode23 periode { get; set; }
        public string sidstOpdateret { get; set; }
    }

    public class Periode24 {
        public string gyldigFra { get; set; }
        public object gyldigTil { get; set; }
    }

    public class NyesteHovedbranche {
        public string branchekode { get; set; }
        public string branchetekst { get; set; }
        public Periode24 periode { get; set; }
        public string sidstOpdateret { get; set; }
    }

    public class NyesteAarsbeskaeftigelse {
        public int? aar { get; set; }
        public object antalInklusivEjere { get; set; }
        public object antalAarsvaerk { get; set; }
        public object antalAnsatte { get; set; }
        public string sidstOpdateret { get; set; }
        public string intervalKodeAntalInklusivEjere { get; set; }
        public string intervalKodeAntalAarsvaerk { get; set; }
        public string intervalKodeAntalAnsatte { get; set; }
    }

    public class NyesteKvartalsbeskaeftigelse {
        public int? aar { get; set; }
        public int? kvartal { get; set; }
        public object antalAarsvaerk { get; set; }
        public object antalAnsatte { get; set; }
        public string sidstOpdateret { get; set; }
        public string intervalKodeAntalAarsvaerk { get; set; }
        public string intervalKodeAntalAnsatte { get; set; }
    }

    public class NyesteMaanedsbeskaeftigelse {
        public int? aar { get; set; }
        public int? maaned { get; set; }
        public object antalAarsvaerk { get; set; }
        public object antalAnsatte { get; set; }
        public string sidstOpdateret { get; set; }
        public string intervalKodeAntalAarsvaerk { get; set; }
        public string intervalKodeAntalAnsatte { get; set; }
    }
    
    //** Tilføjet for , ESCRM-211/213
    public class NyesteErstMaanedsbeskaeftigelse
    {
        public int? aar { get; set; }
        public int? maaned { get; set; }
        public object antalAarsvaerk { get; set; }
        public object antalAnsatte { get; set; }
        public string sidstOpdateret { get; set; }
        public string intervalKodeAntalAarsvaerk { get; set; }
        public string intervalKodeAntalAnsatte { get; set; }
    }

    public class VirksomhedMetadata {
        //** Tilføjet igen, ESCRM-211/213
        public NyesteNavn nyesteNavn { get; set; }
        //public NyesteVirksomhedsform nyesteVirksomhedsform { get; set; }
        public NyesteBeliggenhedsadresse nyesteBeliggenhedsadresse { get; set; }
        //public NyesteHovedbranche nyesteHovedbranche { get; set; }
        public object nyesteBibranche1 { get; set; }
        public object nyesteBibranche2 { get; set; }
        public object nyesteBibranche3 { get; set; }
        public object nyesteStatus { get; set; }
        public List<string> nyesteKontaktoplysninger { get; set; }
        public int? antalPenheder { get; set; }
        //public NyesteAarsbeskaeftigelse nyesteAarsbeskaeftigelse { get; set; }
        //public NyesteKvartalsbeskaeftigelse nyesteKvartalsbeskaeftigelse { get; set; }
        public NyesteMaanedsbeskaeftigelse nyesteMaanedsbeskaeftigelse { get; set; }
        public NyesteErstMaanedsbeskaeftigelse nyesteErstMaanedsbeskaeftigelse { get; set; }
        public string sammensatStatus { get; set; }
        public string stiftelsesDato { get; set; }
        public string virkningsDato { get; set; }
    }

    public class Vrvirksomhed {
        public int? cvrNummer { get; set; }
        public string sidstOpdateret { get; set; }
        public bool? reklamebeskyttet { get; set; }

        //public List<object> regNummer { get; set; }

        //public object brancheAnsvarskode { get; set; }
        
        //public List<Navne> navne { get; set; }

        public List<Binavne> binavne { get; set; }

        public VirksomhedMetadata virksomhedMetadata { get; set; }

        //public List<object> postadresse { get; set; }
        //public List<Beliggenhedsadresse> beliggenhedsadresse { get; set; }

        public List<TelefonNummer> telefonNummer { get; set; }
        
        public List<TelefaxNummer> telefaxNummer { get; set; }
        public List<ElektroniskPost> elektroniskPost { get; set; }
        public List<Hjemmeside> hjemmeside { get; set; }
        //public List<object> obligatoriskEmail { get; set; }
        //public List<Livsforloeb> livsforloeb { get; set; }

        public List<Hovedbranche> hovedbranche { get; set; }
        //public List<object> bibranche1 { get; set; }
        //public List<object> bibranche2 { get; set; }
        //public List<object> bibranche3 { get; set; }

        //public List<object> status { get; set; }
        //public List<Virksomhedsstatu> virksomhedsstatus { get; set; }

        public List<Virksomhedsform> virksomhedsform { get; set; }      
        public List<Aarsbeskaeftigelse> aarsbeskaeftigelse { get; set; }
        public List<Aarsbeskaeftigelse> nyesteErstMaanedsbeskaeftigelse { get; set; }

        //public List<Kvartalsbeskaeftigelse> kvartalsbeskaeftigelse { get; set; }
        //public List<Maanedsbeskaeftigelse> maanedsbeskaeftigelse { get; set; }
        //public List<Attributter> attributter { get; set; }
        //public List<Penheder> penheder { get; set; }
        //public List<DeltagerRelation> deltagerRelation { get; set; }
        //public List<object> fusioner { get; set; }
        //public List<object> spaltninger { get; set; }

        //public int? samtId { get; set; }
        //public bool? fejlRegistreret { get; set; }
        //public int? dataAdgang { get; set; }
        //public long enhedsNummer { get; set; }
        //public string enhedstype { get; set; }
        //public string sidstIndlaest { get; set; }
        //public bool? fejlVedIndlaesning { get; set; }
        //public object naermesteFremtidigeDato { get; set; }
        //public object fejlBeskrivelse { get; set; }
        //public string virkningsAktoer { get; set; }
    }

    public class Source {
        public Vrvirksomhed Vrvirksomhed { get; set; }
    }

    public class Hit {
        public string _index { get; set; }
        public string _type { get; set; }
        public string _id { get; set; }
        public string _score { get; set; }
        public Source _source { get; set; }
    }

    public class Hits {
        public string total { get; set; }
        public string max_score { get; set; }
        public List<Hit> hits { get; set; }
    }

    public class RootObject {
        public string _scroll_id { get; set; }
        public int? took { get; set; }
        public bool? timed_out { get; set; }
        public Shards _shards { get; set; }
        public Hits hits { get; set; }
    }
    #endregion

}
