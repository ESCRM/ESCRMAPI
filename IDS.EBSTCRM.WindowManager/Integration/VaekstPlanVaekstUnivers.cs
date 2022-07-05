using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using IDS.EBSTCRM.Base;

namespace IDS.EBSTCRM.WindowManager.Integration
{
    [Serializable()]
    public class VaekstPlanVaekstUnivers
    {
        // Constructor
        public VaekstPlanVaekstUnivers()
        {
        }
 
        public VaekstPlanVaekstUnivers(ref System.Data.SqlClient.SqlDataReader dr)
        {
            Virksomhedsleder = TypeCast.ToString(lv(ref dr, "2348_Virksomhedsleder"));
            Virksomhedsfakta = TypeCast.ToString(lv(ref dr, "2431_Virksomhedsfakta"));
            Vaekstkonsulent = new Konsulent() { Navn = TypeCast.ToString(lv(ref dr, "2352_Vækstkonsulent")), OrganisationId = TypeCast.ToInt(dr["OrganisationId"]) };
            Virksomhed = new Virksomhed() { Navn = TypeCast.ToString(lv(ref dr, "Name")), Leder = TypeCast.ToString(lv(ref dr, "2348_Virksomhedsleder")) };
 
            StartDato = TypeCast.ToDateTime(lv(ref dr, "2350_Start dato"));
            AfsluttetDato = TypeCast.ToDateTimeLoose(lv(ref dr, "2354_Afsluttet dato"));
 
            Vaekstambitioner = new List<Vaekstambition>();
            Vaekstambitioner.Add(new Vaekstambition() { KortSigt = TypeCast.ToString(lv(ref dr, "2365_Kort sigt")), LangSigt = TypeCast.ToString(lv(ref dr, "2366_Lang sigt")) });
 
            #region Væksthjul
 
            Vaeksthjul = new Vaeksthjul();
 
            Vaeksthjul.Branding = TypeCast.ToInt(lv(ref dr, "2389_Branding"));
            Vaeksthjul.EjerkredsOgBestyrelse = TypeCast.ToInt(lv(ref dr, "2392_Ejerkreds og betyrelse"));
            Vaeksthjul.Faciliteter = TypeCast.ToInt(lv(ref dr, "2409_Faciliteter"));
            Vaeksthjul.Finansiering = TypeCast.ToInt(lv(ref dr, "2397_Finansiering"));
            Vaeksthjul.Forretningsgange = TypeCast.ToInt(lv(ref dr, "2404_Forretninsgange"));
            Vaeksthjul.ForretningsIde = TypeCast.ToInt(lv(ref dr, "2372_Forretningside"));
            Vaeksthjul.ForretningsModel = TypeCast.ToInt(lv(ref dr, "2380_Forretningsmodel"));
            Vaeksthjul.ITSystemer = TypeCast.ToInt(lv(ref dr, "2405_IT-systemer"));
            Vaeksthjul.JuridiskeForhold = TypeCast.ToInt(lv(ref dr, "2408_Juridiske forhold"));
            Vaeksthjul.KommunikationOgPR = TypeCast.ToInt(lv(ref dr, "2385_Kommunikation og PR"));
            Vaeksthjul.KundePortefoelje = TypeCast.ToInt(lv(ref dr, "2384_Kundeportefølje"));
            Vaeksthjul.LeveranceOgProjektstyring = TypeCast.ToInt(lv(ref dr, "2401_Leverance og projektstyring"));
 
            Vaeksthjul.Markedsfoering = TypeCast.ToInt(lv(ref dr, "2377_Markedsføring"));
            Vaeksthjul.Markedsposition = TypeCast.ToInt(lv(ref dr, "2388_Markedsposition"));
 
            Vaeksthjul.Medarbejdere = TypeCast.ToInt(lv(ref dr, "2396_Medarbejdere"));
            Vaeksthjul.Modus = TypeCast.ToString(lv(ref dr, "2370_Modus"));
            Vaeksthjul.Netvaerk = TypeCast.ToInt(lv(ref dr, "2373_Netværk"));
            Vaeksthjul.OEkonomistyring = TypeCast.ToInt(lv(ref dr, "2393_Økonomistyring"));
            Vaeksthjul.ProduktPortefoelje = TypeCast.ToInt(lv(ref dr, "2376_Produktportefølje"));
            Vaeksthjul.Salg = TypeCast.ToInt(lv(ref dr, "2381_Salg"));
            Vaeksthjul.Samarbejdspartnere = TypeCast.ToInt(lv(ref dr, "2400_Samarbejdspartnere"));
 
            
 
            #endregion
 
            #region Fokus områder
 
            Fokusomraader = new List<string>();
            string fko = TypeCast.ToString(lv(ref dr, "2414_Fokusområde 1"));
            if (fko != "") Fokusomraader.Add(fko);
 
            fko = TypeCast.ToString(lv(ref dr, "2416_Fokusområde 2"));
            if (fko != "") Fokusomraader.Add(fko);
 
            fko = TypeCast.ToString(lv(ref dr, "2418_Fokusområde 3"));
            if (fko != "") Fokusomraader.Add(fko);
 
            #endregion
 
            // OPRETTET AF MHJ        
            #region AnsatteOmsaetningEksport

            AnsatteOmsaetningEksport = new List<AnsatteOmsaetningEksport>();

            AnsatteOmsaetningEksport.Add(new AnsatteOmsaetningEksport(1, TypeCast.ToString(lv(ref dr, "2434_Ansatte Omsætning Eksport1"))));
            AnsatteOmsaetningEksport.Add(new AnsatteOmsaetningEksport(2, TypeCast.ToString(lv(ref dr, "2435_Ansatte Omsætning Eksport2"))));
            ////Rettet string name, TR/IDS
            //string aoe = TypeCast.ToString(lv(ref dr, "2434_Ansatte Omsætning Eksport1"));
            //if (aoe != "") AnsatteOmsaetningEksport.Add(aoe);

            //aoe = TypeCast.ToString(lv(ref dr, "2435_Ansatte Omsætning Eksport2"));
            //if (aoe != "") AnsatteOmsaetningEksport.Add(aoe);
  
            #endregion


            // OPRETTET AF MHJ / TILSVARENDE BLOT MED VÆKSTPLANER ER SLETTET, DA JEG IKKE KUNNE FINDE DE VARIABLER        
            #region Vækstaktiviteter

            // Flyttet vækstaktiveter fra List<string> til klasse liste, hvor alle 3 mulige er medtaget, uanset om de er udfyldt eller ej.

            Vaekstaktiviteter = new List<VaekstAktivitet>();

            //Rettet typen til klasse og nummer
            Vaekstaktiviteter.Add(new VaekstAktivitet(1, TypeCast.ToString(lv(ref dr, "2422_Vækstaktivitet 1"))));
            Vaekstaktiviteter.Add(new VaekstAktivitet(2, TypeCast.ToString(lv(ref dr, "2424_Vækstaktivitet 2"))));
            Vaekstaktiviteter.Add(new VaekstAktivitet(3, TypeCast.ToString(lv(ref dr, "2426_Vækstaktivitet 3"))));
 
            #endregion

            #region Handling, Ydelser og Investeringer

            // Har oprettet klasse til dette segment i stedet for 3x string variabler, som ellers ville
            // kunne give nogle sjove resultater, hvor data blev forskudt i rækken, hvis et felt ikke
            // er udfyldt.

            HandlingerYdelserOgInvesteringer = new List<HandlingYdelseOgInvestering>();

            HandlingerYdelserOgInvesteringer.Add(new HandlingYdelseOgInvestering(1, TypeCast.ToString(lv(ref dr, "2444_Handlinger 1")),
                                                                                    TypeCast.ToString(lv(ref dr, "2445_Ydelser 1")),
                                                                                    TypeCast.ToString(lv(ref dr, "2446_Investering 1"))));

            HandlingerYdelserOgInvesteringer.Add(new HandlingYdelseOgInvestering(2, TypeCast.ToString(lv(ref dr, "2447_Handlinger 2")),
                                                                                    TypeCast.ToString(lv(ref dr, "2448_Ydelser 2")),
                                                                                    TypeCast.ToString(lv(ref dr, "2449_Investering 2"))));

            HandlingerYdelserOgInvesteringer.Add(new HandlingYdelseOgInvestering(3, TypeCast.ToString(lv(ref dr, "2450_Handlinger 3")),
                                                                                    TypeCast.ToString(lv(ref dr, "2451_Ydelser 3")),
                                                                                    TypeCast.ToString(lv(ref dr, "2452_Investering 3"))));
            #endregion

            //// OPRETTET AF MHJ        
            //#region HandlingerYdelserInvesteringer1
 
            //HandlingerYdelserInvesteringer1 = new List<string>();
            //string vpl = TypeCast.ToString(lv(ref dr, "2444_Handlinger 1"));
            //if (vpl != "") HandlingerYdelserInvesteringer1.Add(vpl);
 
            //vpl = TypeCast.ToString(lv(ref dr, "2445_Ydelser 1"));
            //if (vpl != "") HandlingerYdelserInvesteringer1.Add(vpl);
 
            //vpl = TypeCast.ToString(lv(ref dr, "2446_Investering 1"));
            //if (vpl != "") HandlingerYdelserInvesteringer1.Add(vpl);
 
            //#endregion
 
            //// OPRETTET AF MHJ        
            //#region HandlingerYdelserInvesteringer2
 
            //HandlingerYdelserInvesteringer2 = new List<string>();
            //string vpl = TypeCast.ToString(lv(ref dr, "2447_Handlinger 2"));
            //if (vpl != "") HandlingerYdelserInvesteringer2.Add(vpl);
 
            //vpl = TypeCast.ToString(lv(ref dr, "2448_Ydelser 2"));
            //if (vpl != "") HandlingerYdelserInvesteringer2.Add(vpl);
 
            //vpl = TypeCast.ToString(lv(ref dr, "2449_Investering 2"));
            //if (vpl != "") HandlingerYdelserInvesteringer2.Add(vpl);
 
            //#endregion

            //// OPRETTET AF MHJ        
            //#region HandlingerYdelserInvesteringer3
 
            //HandlingerYdelserInvesteringer3 = new List<string>();
            //string vpl = TypeCast.ToString(lv(ref dr, "2450_Handlinger 3"));
            //if (vpl != "") HandlingerYdelserInvesteringer3.Add(vpl);
 
            //vpl = TypeCast.ToString(lv(ref dr, "2451_Ydelser 3"));
            //if (vpl != "") HandlingerYdelserInvesteringer3.Add(vpl);
 
            //vpl = TypeCast.ToString(lv(ref dr, "2452_Investering 3"));
            //if (vpl != "") HandlingerYdelserInvesteringer3.Add(vpl);
 
            //#endregion
                         
            #region Henvisninger
 
            if (dr.NextResult())
            {
 
                Henvisninger = new List<Henvisning>();
 
                while (dr.Read())
                {
                    Henvisninger.Add(new Henvisning(ref dr));
                }
            }
 
            #endregion
        }
 
        // Hoved 0-1
        public string Virksomhedsleder { get; set; }
        public string Virksomhedsfakta { get; set; }        
        public Konsulent Vaekstkonsulent { get; set; }
        public DateTime StartDato { get; set; }
        public DateTime? AfsluttetDato { get; set; }
        public Virksomhed Virksomhed { get; set; }
 
        // Vækstambition 0-1
        public List<Vaekstambition> Vaekstambitioner { get; set; }
 
        // Væksthjul 0-1
        public Vaeksthjul Vaeksthjul { get; set; }
 
        // Fokusområder 0-n
        public List<string> Fokusomraader { get; set; }
 
        // AnsatteOmsaetningEksport 0-n
        public List<AnsatteOmsaetningEksport> AnsatteOmsaetningEksport { get; set; }
        
        // Vaekstaktiviteter
        public List<VaekstAktivitet> Vaekstaktiviteter { get; set; }
        
        // HandlingerYdelserInvesteringer1
        //public List<string> HandlingerYdelserInvesteringer1 { get; set; }
        
        //// HandlingerYdelserInvesteringer2
        //public List<string> HandlingerYdelserInvesteringer2 { get; set; }
        
        //// HandlingerYdelserInvesteringer3
        //public List<string> HandlingerYdelserInvesteringer3 { get; set; }

        public List<HandlingYdelseOgInvestering> HandlingerYdelserOgInvesteringer { get; set; }
 
        // Henvisninger 0-n
        public List<Henvisning> Henvisninger { get; set; }
 
        // Get value matching name
        private object lv(ref System.Data.SqlClient.SqlDataReader dr, string token)
        {
            return LookupValue(ref dr, token);
        }
        private object LookupValue(ref System.Data.SqlClient.SqlDataReader dr, string token)
        {
            string t = token.Substring(token.IndexOf("_") + 1).ToLower();
 
            // Match a token
            for (int i = 0; i < dr.FieldCount; i++)
            {
                string tt = dr.GetName(i).ToLower();
                if(tt.IndexOf("_")>-1)
                    tt = tt.Substring(tt.IndexOf("_") + 1);
 
                if (t == tt)
                    return dr[i];
            }
 
            return null;
        }
    }


    // Lokale classes
    [Serializable()]
    public class AnsatteOmsaetningEksport
    {
        public int Nummer { get; set; }
        public string Eskport { get; set; }

        public AnsatteOmsaetningEksport()
        {

        }

        public AnsatteOmsaetningEksport(int nummer, string eskport)
        {
            this.Nummer = nummer;
            this.Eskport = eskport;
        }
    }
    
    [Serializable()]
    public class HandlingYdelseOgInvestering
    {
        public int Nummer { get; set; }
        public string Handling { get; set; }
        public string Ydelse { get; set; }
        public string Investering { get; set; }

        public HandlingYdelseOgInvestering()
        {

        }

        public HandlingYdelseOgInvestering(int nummer, string handling, string ydelse, string investering)
        {
            this.Nummer = nummer;
            this.Handling = handling;
            this.Ydelse = ydelse;
            this.Investering = investering;
        }
    }

    [Serializable()]
    public class VaekstAktivitet
    {
        public int Nummer { get; set; }
        public string Aktivitet { get; set; }

        public VaekstAktivitet()
        {

        }

        public VaekstAktivitet(int nummer, string aktivitet)
        {
            this.Nummer = nummer;
            this.Aktivitet = aktivitet;
        }
    }
 
    //[Serializable]
    //public class Virksomhed
    //{
    //    public string Navn { get; set; }
    //    public string Leder { get; set; }
 
    //    public Virksomhed()
    //    {
    //    }
 
    //    public Virksomhed(string navn, string leder)
    //    {
    //        this.Navn = navn;
    //        this.Leder = leder;
    //    }
    //}
 
    //[Serializable]
    //public class Konsulent
    //{
 
    //    public string Navn { get; set; }
    //    public int OrganisationId { get; set; }
    //}
 
    //[Serializable]
    //public class Vaekstambition
    //{
    //    public string KortSigt { get; set; }
    //    public string LangSigt { get; set; }
    //}
 
    //[Serializable]
    //public class Vaeksthjul
    //{
    //    public enum ModusTyper
    //    {
    //        Normal,
    //        Omvendt,
    //        PåHovedet
    //    }
 
    //    //Modus
    //    public string Modus { get; set; }
 
    //    // Forretningskoncept
    //    public int ForretningsIde { get; set; }
    //    public int ProduktPortefoelje { get; set; }
    //    public int ForretningsModel { get; set; }
    //    public int KundePortefoelje { get; set; }
    //    public int Markedsposition { get; set; }
    //    // Kunderelationer
    //    public int Netvaerk { get; set; }
    //    public int Markedsfoering { get; set; }
    //    public int Salg { get; set; }
    //    public int KommunikationOgPR { get; set; }
    //    public int Branding { get; set; }
    //    // Organisation
    //    public int EjerkredsOgBestyrelse { get; set; }
    //    public int Medarbejdere { get; set; }
    //    public int Samarbejdspartnere { get; set; }
    //    public int Forretningsgange { get; set; }
    //    public int JuridiskeForhold { get; set; }
    //    // Virksomhedsdrift
    //    public int OEkonomistyring { get; set; }
    //    public int Finansiering { get; set; }
    //    public int LeveranceOgProjektstyring { get; set; }
    //    public int ITSystemer { get; set; }
    //    public int Faciliteter { get; set; }
    //}
 
    //[Serializable]
    //public class Henvisning
    //{
    //    public string OrganisationEllerKonsulent { get; set; }
    //    //public List<KontaktInformation> KontaktInformation { get; set; }
    //    public string KontaktInfo { get; set; }
 
    //    public Henvisning()
    //    {
 
    //    }
 
    //    public Henvisning(ref System.Data.SqlClient.SqlDataReader dr)
    //    {
    //        OrganisationEllerKonsulent = TypeCast.ToString(dr["OrganisationEllerKonsulent"]);
 
    //        KontaktInfo = "";
 
    //        string kti = TypeCast.ToString(dr["Phone1"]);
    //        if (kti != "") KontaktInfo += (KontaktInfo == "" ? "" : "\r\n") + kti;
 
    //        kti = TypeCast.ToString(dr["Phone2"]);
    //        if (kti != "") KontaktInfo += (KontaktInfo == "" ? "" : "\r\n") + kti;
 
    //        kti = TypeCast.ToString(dr["Email"]);
    //        if (kti != "") KontaktInfo += (KontaktInfo == "" ? "" : "\r\n") + kti;
 
    //        kti = TypeCast.ToString(dr["Address"]);
    //        if (kti != "") KontaktInfo += (KontaktInfo == "" ? "" : "\r\n") + kti;
 
    //        kti = (TypeCast.ToString(dr["Zipcode"]) + " " + TypeCast.ToString(dr["City"])).Trim();
    //        if (kti != "") KontaktInfo += (KontaktInfo == "" ? "" : "\r\n") + kti;
    //    }
    //}

    //[Serializable]
    //public class KontaktInformation
    //{
    //    public enum KontaktInformationsType
    //    {
    //        Email,
    //        Telefon,
    //        Mobil,
    //        Adresse,
    //        PostNr,
    //        By,
    //        Land
    //    }
 
    //    public KontaktInformationsType Type { get; set; }
    //    public string KontaktData { get; set; }
    //}
}
