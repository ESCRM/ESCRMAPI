using System;
using System.Collections.Generic;
using IDS.EBSTCRM.Base;

namespace IDS.EBSTCRM.WindowManager.Integration {
    [Serializable()]
    public class VaekstPlanEU {

        public VaekstPlanEU() { }

        public VaekstPlanEU(ref System.Data.SqlClient.SqlDataReader dr) {
            Virksomhedsleder = TypeCast.ToString(lv(ref dr, "39203_Virksomhedsleder"));
            Virksomhedsfakta = TypeCast.ToString(lv(ref dr, "39214_Virksomhedsfakta"));
            Vaekstkonsulent = new Konsulent() { Navn = TypeCast.ToString(lv(ref dr, "39205_Vækstkonsulent")), OrganisationId = TypeCast.ToInt(dr["OrganisationId"]) };
            Virksomhed = new Virksomhed() { Navn = TypeCast.ToString(lv(ref dr, "Name")), Leder = TypeCast.ToString(lv(ref dr, "39203_Virksomhedsleder")) };

            StartDato = TypeCast.ToDateTime(lv(ref dr, "41357_Start dato"));
            AfsluttetDato = TypeCast.ToDateTimeLoose(lv(ref dr, "41359_Afsluttet dato"));

            Vaekstambitioner = new List<Vaekstambition>();
            Vaekstambitioner.Add(new Vaekstambition() { KortSigt = TypeCast.ToString(lv(ref dr, "39219_Kort sigt")), LangSigt = TypeCast.ToString(lv(ref dr, "39220_Lang sigt")) });

            #region Væksthjul

            Vaeksthjul = new Vaeksthjul();

            Vaeksthjul.Branding = TypeCast.ToInt(lv(ref dr, "39247_Branding"));
            Vaeksthjul.EjerkredsOgBestyrelse = TypeCast.ToInt(lv(ref dr, "39250_Ejerkreds og betyrelse"));
            Vaeksthjul.Faciliteter = TypeCast.ToInt(lv(ref dr, "39267_Faciliteter"));
            Vaeksthjul.Finansiering = TypeCast.ToInt(lv(ref dr, "39255_Finansiering"));
            Vaeksthjul.Forretningsgange = TypeCast.ToInt(lv(ref dr, "39262_Forretninsgange"));
            Vaeksthjul.ForretningsIde = TypeCast.ToInt(lv(ref dr, "39230_Forretningside"));
            Vaeksthjul.ForretningsModel = TypeCast.ToInt(lv(ref dr, "39238_Forretningsmodel"));
            Vaeksthjul.ITSystemer = TypeCast.ToInt(lv(ref dr, "39263_IT-systemer"));
            Vaeksthjul.JuridiskeForhold = TypeCast.ToInt(lv(ref dr, "39266_Juridiske forhold"));
            Vaeksthjul.KommunikationOgPR = TypeCast.ToInt(lv(ref dr, "39243_Kommunikation og PR"));
            Vaeksthjul.KundePortefoelje = TypeCast.ToInt(lv(ref dr, "39242_Kundeportefølje"));
            Vaeksthjul.LeveranceOgProjektstyring = TypeCast.ToInt(lv(ref dr, "39259_Leverance og projektstyring"));

            Vaeksthjul.Markedsfoering = TypeCast.ToInt(lv(ref dr, "39235_Markedsføring"));
            Vaeksthjul.Markedsposition = TypeCast.ToInt(lv(ref dr, "39246_Markedsposition"));

            Vaeksthjul.Medarbejdere = TypeCast.ToInt(lv(ref dr, "39254_Medarbejdere"));
            Vaeksthjul.Modus = TypeCast.ToString(lv(ref dr, "39228_Modus"));
            Vaeksthjul.Netvaerk = TypeCast.ToInt(lv(ref dr, "39231_Netværk"));
            Vaeksthjul.OEkonomistyring = TypeCast.ToInt(lv(ref dr, "39251_Økonomistyring"));
            Vaeksthjul.ProduktPortefoelje = TypeCast.ToInt(lv(ref dr, "39234_Produktportefølje"));
            Vaeksthjul.Salg = TypeCast.ToInt(lv(ref dr, "39239_Salg"));
            Vaeksthjul.Samarbejdspartnere = TypeCast.ToInt(lv(ref dr, "39258_Samarbejdspartnere"));

            #endregion

            #region Fokus områder

            Fokusomraader = new List<string>();
            string fko = TypeCast.ToString(lv(ref dr, "39272_Fokusområde 1"));
            if (fko != "") Fokusomraader.Add(fko);

            fko = TypeCast.ToString(lv(ref dr, "39274_Fokusområde 2"));
            if (fko != "") Fokusomraader.Add(fko);

            fko = TypeCast.ToString(lv(ref dr, "39276_Fokusområde 3"));
            if (fko != "") Fokusomraader.Add(fko);

            #endregion

            // OPRETTET AF MHJ        
            #region AnsatteOmsaetningEksport

            AnsatteOmsaetningEksport = new List<AnsatteOmsaetningEksport>();
            AnsatteOmsaetningEksport.Add(new AnsatteOmsaetningEksport(1, TypeCast.ToString(lv(ref dr, "39223_Ansatte Omsætning Eksport1"))));
            AnsatteOmsaetningEksport.Add(new AnsatteOmsaetningEksport(2, TypeCast.ToString(lv(ref dr, "39224_Ansatte Omsætning Eksport2"))));

            #endregion


            // OPRETTET AF MHJ / TILSVARENDE BLOT MED VÆKSTPLANER ER SLETTET, DA JEG IKKE KUNNE FINDE DE VARIABLER        
            #region Vækstaktiviteter

            // Flyttet vækstaktiveter fra List<string> til klasse liste, hvor alle 3 mulige er medtaget, uanset om de er udfyldt eller ej.

            Vaekstaktiviteter = new List<VaekstAktivitet>();

            //Rettet typen til klasse og nummer
            Vaekstaktiviteter.Add(new VaekstAktivitet(1, TypeCast.ToString(lv(ref dr, "39280_Vækstaktivitet 1"))));
            Vaekstaktiviteter.Add(new VaekstAktivitet(2, TypeCast.ToString(lv(ref dr, "39282_Vækstaktivitet 2"))));
            Vaekstaktiviteter.Add(new VaekstAktivitet(3, TypeCast.ToString(lv(ref dr, "39284_Vækstaktivitet 3"))));

            #endregion

            #region Handling, Ydelser og Investeringer

            // Har oprettet klasse til dette segment i stedet for 3x string variabler, som ellers ville
            // kunne give nogle sjove resultater, hvor data blev forskudt i rækken, hvis et felt ikke
            // er udfyldt.

            HandlingerYdelserOgInvesteringer = new List<HandlingYdelseOgInvestering>();

            HandlingerYdelserOgInvesteringer.Add(new HandlingYdelseOgInvestering(1, TypeCast.ToString(lv(ref dr, "39292_Handlinger 1")),
                                                                                    TypeCast.ToString(lv(ref dr, "39293_Ydelser 1")),
                                                                                    TypeCast.ToString(lv(ref dr, "39294_Investering 1"))));

            HandlingerYdelserOgInvesteringer.Add(new HandlingYdelseOgInvestering(2, TypeCast.ToString(lv(ref dr, "39296_Handlinger 2")),
                                                                                    TypeCast.ToString(lv(ref dr, "39297_Ydelser 2")),
                                                                                    TypeCast.ToString(lv(ref dr, "39298_Investering 2"))));

            HandlingerYdelserOgInvesteringer.Add(new HandlingYdelseOgInvestering(3, TypeCast.ToString(lv(ref dr, "39300_Handlinger 3")),
                                                                                    TypeCast.ToString(lv(ref dr, "39301_Ydelser 3")),
                                                                                    TypeCast.ToString(lv(ref dr, "39302_Investering 3"))));
            #endregion

            #region Henvisninger

            if (dr.NextResult()) {
                Henvisninger = new List<Henvisning>();
                while (dr.Read()) {
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
        private object lv(ref System.Data.SqlClient.SqlDataReader dr, string token) {
            return LookupValue(ref dr, token);
        }
        private object LookupValue(ref System.Data.SqlClient.SqlDataReader dr, string token) {
            string t = token.Substring(token.IndexOf("_") + 1).ToLower();

            // Match a token
            for (int i = 0; i < dr.FieldCount; i++) {
                string tt = dr.GetName(i).ToLower();
                if (tt.IndexOf("_") > -1)
                    tt = tt.Substring(tt.IndexOf("_") + 1);

                if (t == tt)
                    return dr[i];
            }

            return null;
        }
    }
}