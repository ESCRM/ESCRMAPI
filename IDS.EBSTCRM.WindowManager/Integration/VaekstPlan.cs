using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using IDS.EBSTCRM.Base;

namespace IDS.EBSTCRM.WindowManager.Integration
{

    [Serializable]
    public class Vaekstplan
    {
        // Constructor
        public Vaekstplan()
        {
        }

        public Vaekstplan(ref System.Data.SqlClient.SqlDataReader dr)
        {
            Virksomhedsleder = TypeCast.ToString(lv(ref dr, "4_Virksomhedsleder"));
            Vaekstkonsulent = new Konsulent() { Navn = TypeCast.ToString(lv(ref dr, "5_Vækstkonsulent")), OrganisationId = TypeCast.ToInt(dr["OrganisationId"]) };
            Virksomhed = new Integration.Virksomhed() { Navn = TypeCast.ToString(lv(ref dr, "SMVCompanyName")), Leder = TypeCast.ToString(lv(ref dr, "4_Virksomhedsleder")) };

            StartDato = TypeCast.ToDateTime(lv(ref dr, "74_Start dato"));
            AfsluttetDato = TypeCast.ToDateTimeLoose(lv(ref dr, "75_Afsluttet dato"));

            Vaekstambitioner = new List<Vaekstambition>();
            Vaekstambitioner.Add(new Vaekstambition() { KortSigt = TypeCast.ToString(lv(ref dr, "17_Kort sigt")), LangSigt = TypeCast.ToString(lv(ref dr, "18_Lang sigt")) });

            #region Væksthjul

            Vaeksthjul = new Vaeksthjul();

            Vaeksthjul.Branding = TypeCast.ToInt(lv(ref dr, "64_Branding"));
            Vaeksthjul.EjerkredsOgBestyrelse = TypeCast.ToInt(lv(ref dr, "24_Ejerkreds og betyrelse"));
            Vaeksthjul.Faciliteter = TypeCast.ToInt(lv(ref dr, "68_Faciliteter"));
            Vaeksthjul.Finansiering = TypeCast.ToInt(lv(ref dr, "65_Finansiering"));
            Vaeksthjul.Forretningsgange = TypeCast.ToInt(lv(ref dr, "26_Forretninsgange"));
            Vaeksthjul.ForretningsIde = TypeCast.ToInt(lv(ref dr, "59_Forretningside"));
            Vaeksthjul.ForretningsModel = TypeCast.ToInt(lv(ref dr, "11_Forretningsmodel"));
            Vaeksthjul.ITSystemer = TypeCast.ToInt(lv(ref dr, "67_IT-systemer"));
            Vaeksthjul.JuridiskeForhold = TypeCast.ToInt(lv(ref dr, "27_Juridiske forhold"));
            Vaeksthjul.KommunikationOgPR = TypeCast.ToInt(lv(ref dr, "63_Kommunikation og PR"));
            Vaeksthjul.KundePortefoelje = TypeCast.ToInt(lv(ref dr, "12_Kundeportefølje"));
            Vaeksthjul.LeveranceOgProjektstyring = TypeCast.ToInt(lv(ref dr, "66_Leverance og projektstyring"));

            Vaeksthjul.Markedsfoering = TypeCast.ToInt(lv(ref dr, "61_Markedsføring"));
            Vaeksthjul.Markedsposition = TypeCast.ToInt(lv(ref dr, "13_Markedsposition"));

            Vaeksthjul.Medarbejdere = TypeCast.ToInt(lv(ref dr, "15_Medarbejdere"));
            Vaeksthjul.Modus = TypeCast.ToString(lv(ref dr, "52_Modus"));
            Vaeksthjul.Netvaerk = TypeCast.ToInt(lv(ref dr, "60_Netværk"));
            Vaeksthjul.OEkonomistyring = TypeCast.ToInt(lv(ref dr, "25_Økonomistyring"));
            Vaeksthjul.ProduktPortefoelje = TypeCast.ToInt(lv(ref dr, "23_Produktportefølje"));
            Vaeksthjul.Salg = TypeCast.ToInt(lv(ref dr, "62_Salg"));
            Vaeksthjul.Samarbejdspartnere = TypeCast.ToInt(lv(ref dr, "16_Samarbejdspartnere"));

            

            #endregion

            #region Fokus områder

            Fokusomraader = new List<string>();
            string fko = TypeCast.ToString(lv(ref dr, "34_Fokusområde 1"));
            if (fko != "") Fokusomraader.Add(fko);

            fko = TypeCast.ToString(lv(ref dr, "54_Fokusområde 2"));
            if (fko != "") Fokusomraader.Add(fko);

            fko = TypeCast.ToString(lv(ref dr, "56_Fokusområde 3"));
            if (fko != "") Fokusomraader.Add(fko);

            #endregion

            #region Vækstplaner

            Vaekstplaner = new List<string>();
            string vpl = TypeCast.ToString(lv(ref dr, "78_Vækstplan 1"));
            if (vpl != "") Vaekstplaner.Add(vpl);

            vpl = TypeCast.ToString(lv(ref dr, "81_Vækstplan 2"));
            if (vpl != "") Vaekstplaner.Add(vpl);

            vpl = TypeCast.ToString(lv(ref dr, "82_Vækstplan 3"));
            if (vpl != "") Vaekstplaner.Add(vpl);

            #endregion

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

        // Vækstplan 0-n
        public List<string> Vaekstplaner { get; set; }

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

    [Serializable]
    public class Virksomhed
    {
        public string Navn { get; set; }
        public string Leder { get; set; }

        public Virksomhed()
        {
        }

        public Virksomhed(string navn, string leder)
        {
            this.Navn = navn;
            this.Leder = leder;
        }
    }

    [Serializable]
    public class Konsulent
    {

        public string Navn { get; set; }
        public int OrganisationId { get; set; }
    }

    [Serializable]
    public class Vaekstambition
    {
        public string KortSigt { get; set; }
        public string LangSigt { get; set; }
    }

    [Serializable]
    public class Vaeksthjul
    {
        public enum ModusTyper
        {
            Normal,
            Omvendt,
            PåHovedet
        }

        //Modus
        public string Modus { get; set; }

        // Forretningskoncept
        public int ForretningsIde { get; set; }
        public int ProduktPortefoelje { get; set; }
        public int ForretningsModel { get; set; }
        public int KundePortefoelje { get; set; }
        public int Markedsposition { get; set; }
        // Kunderelationer
        public int Netvaerk { get; set; }
        public int Markedsfoering { get; set; }
        public int Salg { get; set; }
        public int KommunikationOgPR { get; set; }
        public int Branding { get; set; }
        // Organisation
        public int EjerkredsOgBestyrelse { get; set; }
        public int Medarbejdere { get; set; }
        public int Samarbejdspartnere { get; set; }
        public int Forretningsgange { get; set; }
        public int JuridiskeForhold { get; set; }
        // Virksomhedsdrift
        public int OEkonomistyring { get; set; }
        public int Finansiering { get; set; }
        public int LeveranceOgProjektstyring { get; set; }
        public int ITSystemer { get; set; }
        public int Faciliteter { get; set; }
    }

    [Serializable]
    public class Henvisning
    {
        public string OrganisationEllerKonsulent { get; set; }
        //public List<KontaktInformation> KontaktInformation { get; set; }
        public string KontaktInfo { get; set; }

        public Henvisning()
        {

        }

        public Henvisning(ref System.Data.SqlClient.SqlDataReader dr)
        {
            OrganisationEllerKonsulent = TypeCast.ToString(dr["OrganisationEllerKonsulent"]);

            KontaktInfo = "";

            string kti = TypeCast.ToString(dr["Phone1"]);
            if (kti != "") KontaktInfo += (KontaktInfo == "" ? "" : "\r\n") + kti;

            kti = TypeCast.ToString(dr["Phone2"]);
            if (kti != "") KontaktInfo += (KontaktInfo == "" ? "" : "\r\n") + kti;

            kti = TypeCast.ToString(dr["Email"]);
            if (kti != "") KontaktInfo += (KontaktInfo == "" ? "" : "\r\n") + kti;

            kti = TypeCast.ToString(dr["Address"]);
            if (kti != "") KontaktInfo += (KontaktInfo == "" ? "" : "\r\n") + kti;

            kti = (TypeCast.ToString(dr["Zipcode"]) + " " + TypeCast.ToString(dr["City"])).Trim();
            if (kti != "") KontaktInfo += (KontaktInfo == "" ? "" : "\r\n") + kti;
        }
    }

    [Serializable]
    public class KontaktInformation
    {
        public enum KontaktInformationsType
        {
            Email,
            Telefon,
            Mobil,
            Adresse,
            PostNr,
            By,
            Land
        }

        public KontaktInformationsType Type { get; set; }
        public string KontaktData { get; set; }
    }
}