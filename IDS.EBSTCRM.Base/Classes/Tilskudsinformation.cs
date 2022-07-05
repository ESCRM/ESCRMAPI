using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IDS.EBSTCRM.Base.Classes
{
    /// <summary>
    /// Tilføjet for ESCRM-9/40
    /// Ved ikke om den skal bruges i systemet
    /// Den er nu blevet public, ESCRM-9/89
    /// </summary>
    [Serializable()]
    public class Tilskudsinformation : EventLogBase
    {
        //** Enum tilføjet, ESCRM-9/89
        //** Enum flyttet ESCRM-98/99
        public enum API_AdgangsNiveau
        {
            Alle = 1,
            Ikke_tilladt = 2
        }

        #region Declarations
        private Guid tilskudsinformationId;
        private string cVR;
        private string kontakt;
        private string program;                 //** Tilføjet for ESCRM-98/99
        private int ansvarlig;                  //** Rettet for ESCRM-98/99
        private DateTime bevillingsDato;
        //** bevillingsnavn omdøbt, ESCRM-176/177
        //private string bevillingsNavn;
        private string titel;
        private string form;
        private decimal tilskudsBeloeb;
        //** Emne omdøbt, ESCRM-176/177
        //private string emne;
        private string overordnet_indsats;

        private string status;
        private Guid createdBy;                 //** Tilføjet for ESCRM-98/99
        //** Sagsnummer omdøbt, ESCRM-176/177
        //private string sagsNummer;
        private string journalNummer;

        private string beskrivelse;
        private API_AdgangsNiveau systemAdgang;
        private DateTime? afslutningsDato;       //** Tilføjet for ESCRM-90/108
        private string informations_Hjemmeside; //** Tilføjet for ESCRM-98/108
        private DateTime updatedTimestamp;      //** Tilføjet for ESCRM-118/119

        //** Nyt felt
        private int pNummer;
        //** Nye felter økonomi
        private decimal oko_AnsoegtBeloeb;
        private decimal oko_TilsagnsBeloeb;
        private decimal oko_EgenFinansiering;
        private string oko_Formaal;
        private string oko_Projektkonsulent;
        //** Nye felter rådgiver
        private string raadgiver_Firmanavn;
        private int raadgiver_CVR;
        #endregion

        #region Properties
        public Guid TilskudsinformationId
        {
            get { return tilskudsinformationId; }
            set { tilskudsinformationId = value; }
        }

        public string CVR
        {
            get { return cVR; }
            set { cVR = value; }
        }

        public string Kontakt
        {
            get { return kontakt; }
            set { kontakt = value; }
        }

        //** Tilføjet for ESCRM-98/99
        public string Program
        {
            get { return program; }
            set { program = value; }
        }

        //** Rettet for ESCRM-98/99
        public int Ansvarlig
        {
            get { return ansvarlig; }
            set { ansvarlig = value; }
        }

        public DateTime BevillingsDato
        {
            get { return bevillingsDato; }
            set { bevillingsDato = value; }
        }

        //** BevillingsNavn omdøbt, ESCRM-176/177
        //public string BevillingsNavn
        //{
        //    get { return bevillingsNavn; }
        //    set { bevillingsNavn = value; }
        //}
        public string Titel
        {
            get { return titel; }
            set { titel = value; }
        }

        public string Form
        {
            get { return form; }
            set { form = value; }
        }

        public decimal TilskudsBeloeb
        {
            get { return tilskudsBeloeb; }
            set { tilskudsBeloeb = value; }
        }

        //** Emne omdøbt, ESCRM-176/177
        //public string Emne
        //{
        //    get { return emne; }
        //    set { emne = value; }
        //}
        public string Overordnet_indsats
        {
            get { return overordnet_indsats; }
            set { overordnet_indsats = value; }
        }

        public string Status
        {
            get { return status; }
            set { status = value; }
        }

        //** Tilføjet for ESCRM-98/99
        public Guid CreatedBy
        {
            get { return createdBy; }
            set { createdBy = value; }
        }

        //** Sagsnummer omdøbt, ESCRM-176/177
        //public string SagsNummer
        //{
        //    get { return sagsNummer; }
        //    set { sagsNummer = value; }
        //}
        public string JournalNummer
        {
            get { return journalNummer; }
            set { journalNummer = value; }
        }

        public string Beskrivelse
        {
            get { return beskrivelse; }
            set { beskrivelse = value; }
        }

        public API_AdgangsNiveau SystemAdgang
        {
            get { return systemAdgang; }
            set { systemAdgang = value; }
        }

        //** Tilføjet for ESCRM-98/108
        public DateTime? AfslutningsDato
        {
            get { return afslutningsDato; }
            set { afslutningsDato = value; }
        }

        //** Tilføjet for ESCRM-98/108
        public string Informations_Hjemmeside
        {
            get { return informations_Hjemmeside; }
            set { informations_Hjemmeside = value; }
        }

        //** Tilføjet for ESCRM-118/119
        public DateTime UpdatedTimestamp
        {
            get { return updatedTimestamp; }
            set { updatedTimestamp = value; }
        }

        //** Nyt felt, ESCRM-176/177
        public int PNummer
        {
            get { return pNummer; }
            set { pNummer = value; }
        }

        //** Nye felter økonomi, ESCRM-176/177
        public decimal Oko_AnsoegtBeloeb
        {
            get { return oko_AnsoegtBeloeb; }
            set { oko_AnsoegtBeloeb = value; }
        }

        public decimal Oko_TilsagnsBeloeb
        {
            get { return oko_TilsagnsBeloeb; }
            set { oko_TilsagnsBeloeb = value; }
        }

        public decimal Oko_EgenFinansiering
        {
            get { return oko_EgenFinansiering; }
            set { oko_EgenFinansiering = value; }
        }

        public string Oko_Projektkonsulent
        {
            get { return oko_Projektkonsulent; }
            set { oko_Projektkonsulent = value; }
        }

        public string Oko_Formaal
        {
            get { return oko_Formaal; }
            set { oko_Formaal = value; }
        }

        //** Nye felter ekstern rådgiver, ESCRM-176/177
        public string Raadgiver_Firmanavn
        {
            get { return raadgiver_Firmanavn; }
            set { raadgiver_Firmanavn = value; }
        }

        public int Raadgiver_CVR
        {
            get { return raadgiver_CVR; }
            set { raadgiver_CVR = value; }
        }
        #endregion
    }
}
