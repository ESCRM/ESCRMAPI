using IDS.EBSTCRM.Base;
using IDS.EBSTCRM.Base.Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web.Script.Services;
using System.Web.Services;
using Microsoft.Office.Interop.Excel;
using System.Data;
using DataTable = System.Data.DataTable;

namespace IDS.EBSTCRM.WindowManager.Integration
{
    /// <summary>
    /// Summary description for TilskudsinformationWS, ESCRM-98/99
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]

    [ScriptService]
    public class TilskudsinformationWS : System.Web.Services.WebService
    {
        /// <summary>
        /// Web metode : Add tilskudsinformation som enkelt data
        /// </summary>
        /// <param name="CVR"></param>
        /// <param name="Kontakt"></param>
        /// <param name="Program"></param>
        /// <param name="BevillingsDato"></param>
        /// BevillingsNavn omdøbt, ESCRM-176/177
        /// <param name="Titel"></param>
        /// <param name="Form"></param>
        /// <param name="Tilskudsbeloeb"></param>
        /// Emne omdøbt, ESCRM-176/177
        /// <param name="Overordnet_indsats"></param>
        /// <param name="Status"></param>
        /// <param name="CreatedBy"></param>
        /// <param name="Ansvarlig"></param>
        /// Sagsnummer omdøbt, ESCRM-176/177
        /// <param name="JournalNummer"></param>
        /// <param name="Beskrivelse"></param>
        /// <param name="SystemAdgang"></param>
        /// Nyt felt, ESCRM-176/177
        /// <param name="PNummer"></param>
        /// /// Nye felter økonomi
        /// <param name="Oko_AnsoegtBeloeb"></param>
        /// <param name="Oko_TilsagnsBeloeb"></param>
        /// <param name="Oko_EgenFinansiering"></param>
        /// <param name="Oko_Formaal"></param>
        /// <param name="Oko_Projektkonsulent"></param>
        /// Nye felter ekstern rådgiver
        /// <param name="Raadgiver_Firmanavn"></param>
        /// <param name="Raadgiver_CVR"></param>
        /// <returns></returns>
        [WebMethod(Description = "Følgende felter skal sendes: cvr,program,bevillingsdato,titel,form,status,createdby,ansvarlig,systemadgang,afslutningsdato,kontakt,tilskudsbeloeb,overordnet_indsats,journalnummer,beskrivelse,informations_hjemmeside,pnummer,oko_ansoegtbeloeb,oko_tilsagnsbeloeb,oko_egenfinansiering,oko_formaal,oko_projektkonsulent,raadgiver_firmanavn,raadgiver_cvr, checkprogramnavn, checkprogramguid, dubletcheck=0")]
        public string AddTilskudsinformationSingle(string CheckProgramNavn, Guid CheckProgramGuid, int SystemAdgang, int CVR, string Program, DateTime BevillingsDato, string Titel, string Form, string Status, int Ansvarlig, DateTime? AfslutningsDato, int PNummer, decimal Oko_AnsoegtBeloeb, decimal Oko_TilsagnsBeloeb, decimal Oko_EgenFinansiering, string Oko_Formaal, string Oko_Projektkonsulent, string Raadgiver_Firmanavn, int Raadgiver_CVR, string Kontakt = "", decimal Tilskudsbeloeb = 0, string Overordnet_indsats = "", string JournalNummer = "", string Beskrivelse = "", string Informations_Hjemmeside = "")
        {
            string returnResult = string.Empty;
            SQLDB sql = new SQLDB();

            try
            {
                //** Lave check på programnavn og guide
                if (!sql.CanImportTilskudsinformation(CheckProgramNavn, CheckProgramGuid) && Program == CheckProgramNavn)
                {
                    //** Hop tilbage med fejl tekst
                    returnResult = "Der opstod en fejl med check";
                }
                else
                {
                    if (CVR.ToString().Length != 8)
                    {
                        returnResult = "CVR skal være 8 tegn";
                    }
                    else if (PNummer.ToString().Length != 10)
                    {
                        returnResult = "P-Nummer skal være 10 tegn";
                    }
                    else
                    {
                        //** Kald private function
                        //** Nye felter, ESCRM176/177
                        //** Double check er hardcoded til 1, CreatedBy er systemGuid, CVR sendes som Int, ESCRM-234/235
                        Guid SystemGuid = Guid.Parse("161D0A22-39E3-4D17-A0DA-FEA05D93C210");
                        int DoubleCheck = 0; // MGR: duplicate control is handled in database creation/update (ESCRM-357/358) 
                        returnResult = InsertTilskudsinformation(CVR.ToString(), Program, BevillingsDato, Titel, Form, Status, SystemGuid, Ansvarlig, SystemAdgang, AfslutningsDato, PNummer, Oko_AnsoegtBeloeb, Oko_TilsagnsBeloeb, Oko_EgenFinansiering, Oko_Formaal, Oko_Projektkonsulent, Raadgiver_Firmanavn, Raadgiver_CVR, Kontakt, Tilskudsbeloeb, Overordnet_indsats, JournalNummer, Beskrivelse, Informations_Hjemmeside, DoubleCheck);
                    }
                }
            }
            catch (Exception ex)
            {
                returnResult = "Der opstod en fejl";
            }

            return returnResult;
        }

        /// <summary>
        /// Web metode : Add tilskudsinformation som Excel
        /// </summary>
        /// <param name="excelFil"></param>
        /// <param name="DubletCheck"></param>
        /// <returns></returns>
        [WebMethod(Description = "Excel skal sendes som DataTable og skal indeholde følgende felter: cvr,program,bevillingsdato,titel,form,status,createdby,ansvarlig,systemadgang,afslutningsdato,kontakt,tilskudsbeloeb,overordnet_indsats,journalnummer,beskrivelse,informations_hjemmeside,pnummer,oko_ansoegtbeloeb,oko_tilsagnsbeloeb,oko_egenfinansiering,oko_formaal,oko_projektkonsulent,raadgiver_firmanavn,raadgiver_cvr, check_programnavn, check_programguid")]
        public string AddTilskudsinformationExcel(DataTable excelFile, int DubletCheck = 0)
        {
            string returnResult = string.Empty;
            StringBuilder sb = new StringBuilder();
            Boolean errorOccured = false;
            SQLDB sql = new SQLDB();

            try
            {

                //** Løb alle rækker igennem
                foreach (DataRow row in excelFile.Rows)
                {
                    //** Lave check på programnavn og guid, ESCRM-176/177
                    if (!sql.CanImportTilskudsinformation(row["Check_ProgramNavn"].ToString(), Guid.Parse(row["Check_ProgramGuid"].ToString())) && row["Check_ProgramNavn"].ToString() == row["Program"].ToString())
                    {
                        //** Skriv fejlen besked
                        sb.Append("Der opstod en fejl med check");

                        //** Fortsæt
                        continue;
                    }

                    //** Find alle data
                    string cvr = row["cvr"].ToString();
                    string kontakt = row["kontakt"].ToString();
                    string program = row["program"].ToString();
                    DateTime bevillingsdato = DateTime.Parse(row["bevillingsdato"].ToString());

                    //** Bevillingsnavn omdøb, ESCRM-176/177
                    //string bevillingsnavn = row["bevillingsnavn"].ToString();
                    string titel = row["titel"].ToString();

                    string form = row["form"].ToString();
                    string status = row["status"].ToString();
                    Guid createdby = Guid.Parse(row["createdby"].ToString());
                    int ansvarlig = int.Parse(row["ansvarlig"].ToString());
                    int systemadgang = int.Parse(row["systemadgang"].ToString());
                    DateTime afslutningsDato = DateTime.Parse(row["afslutningsdato"].ToString());
                    decimal tilskudsbeloeb = decimal.Parse(row["tilskudsbeloeb"].ToString());

                    //** Emne omdøbt, ESCRM-176/177
                    //string emne = row["emne"].ToString();
                    string overordnet_indsats = row["overordnet_indsats"].ToString();

                    //** Sagsnummer omdøbt, ESCRM-176/177
                    //string sagsnummer = row["sagsnummer"].ToString();
                    string journalnummer = row["journalnummer"].ToString();

                    string beskrivelse = row["beskrivelse"].ToString();
                    string informations_hjemmeside = row["informations_hjemmeside"].ToString();

                    //**Nye felter økonomi, ESCRM-176/177
                    int pnummer = int.Parse(row["pnummer"].ToString());
                    decimal oko_ansoegtbeloeb = decimal.Parse(row["Oko_ansoegtbeloeb"].ToString());
                    decimal oko_tilsagnsbeloeb = decimal.Parse(row["Oko_tilsagnsbeloeb"].ToString());
                    decimal oko_egenfinansiering = decimal.Parse(row["Oko_egenfinansiering"].ToString());
                    string oko_formaal = row["Oko_formaal"].ToString();
                    string oko_projektkonsulent = row["Oko_projektkonsulent"].ToString();
                    //**Nye felter ekstern rådgiver, ESCRM-176/177
                    string raadgiver_firmanavn = row["raadgiver_firmanavn"].ToString();
                    int raadgiver_cvr = int.Parse(row["raadgiver_cvr"].ToString());

                    //** Skriv data til databasen
                    //string result = InsertTilskudsinformation(cvr, program, bevillingsdato, bevillingsnavn, form, status, createdby, ansvarlig, systemadgang, afslutningsDato, kontakt, tilskudsbeloeb, emne, sagsnummer, beskrivelse, informations_hjemmeside, DubletCheck);
                    string result = InsertTilskudsinformation(cvr, program, bevillingsdato, titel, form, status, createdby, ansvarlig, systemadgang, afslutningsDato, pnummer, oko_ansoegtbeloeb, oko_tilsagnsbeloeb, oko_egenfinansiering, oko_formaal, oko_projektkonsulent, raadgiver_firmanavn, raadgiver_cvr, kontakt, tilskudsbeloeb, overordnet_indsats, journalnummer, beskrivelse, informations_hjemmeside, DubletCheck);

                    //** Test retur resultat
                    if (result == "Fejl")
                    {
                        sb.Append("Der opstod en fejl med CVR: " + cvr + Environment.NewLine);
                        errorOccured = true;
                    }
                    //** Test for findes allerede
                    if (result == "Tilskudsinformation findes allerede")
                    {
                        sb.Append("CVR: " + cvr + @" findes allerede." + Environment.NewLine);
                        errorOccured = true;
                    }
                }

                //** Tilføjet for ESCRM-98/108
                returnResult = "Success";
            }
            catch (Exception ex)
            {
                returnResult = "Fejl: " + ex.Message;
            }

            //** Returner fejl hvis de opstod
            if (errorOccured)
                returnResult = sb.ToString();

            return returnResult;
        }

        /// <summary>
        /// PF : Kald stored procedure
        /// </summary>
        /// <returns></returns>
        private string InsertTilskudsinformation(string CVR, string Program, DateTime BevillingsDato, string Titel, string Form, string Status, Guid CreatedBy, int Ansvarlig, int SystemAdgang, DateTime? AfslutningDato, int PNummer, decimal Oko_AnsoegtBeloeb, decimal Oko_TilsagnsBeloeb, decimal Oko_Egenfinansiering, string Oko_Formaal, string Oko_Projektkonsulent, string Raadgiver_Firmanavn, int Raadgiver_CVR, string Kontakt = "", decimal Tilskudsbeloeb = 0, string Overordnet_indsats = "", string JournalNummer = "", string Beskrivelse = "", string Informations_Hjemmeside = "", int DubletCheck = 0)
        {
            string returnResult = string.Empty;

            try
            {
                //** Definer sql filen
                SQLDB sql = new SQLDB();

                //** Opret ny tilskudsinformation
                Tilskudsinformation nyTilskudsinformation = new Tilskudsinformation();
                nyTilskudsinformation.CVR = CVR;
                nyTilskudsinformation.Kontakt = Kontakt;
                nyTilskudsinformation.Program = Program;
                nyTilskudsinformation.BevillingsDato = BevillingsDato;
                //** BevillingsNavn omdøbt, ESCRM-176/177
                //nyTilskudsinformation.BevillingsNavn = BevillingsNavn;
                nyTilskudsinformation.Titel = Titel;

                nyTilskudsinformation.Form = Form;
                nyTilskudsinformation.TilskudsBeloeb = Tilskudsbeloeb;
                //** Emne omdøbt, ESCRM-176/177
                //nyTilskudsinformation.Emne = Emne;
                nyTilskudsinformation.Overordnet_indsats = Overordnet_indsats;

                nyTilskudsinformation.Status = Status;
                nyTilskudsinformation.CreatedBy = CreatedBy;
                nyTilskudsinformation.Ansvarlig = Ansvarlig;
                //** Sagsnummer omdøbt, ESCRM-167/177
                //nyTilskudsinformation.SagsNummer = SagsNummer;
                nyTilskudsinformation.JournalNummer = JournalNummer;

                nyTilskudsinformation.Beskrivelse = @Beskrivelse;
                //** Find systemadgang via enum
                if (SystemAdgang == 1)
                    nyTilskudsinformation.SystemAdgang = Tilskudsinformation.API_AdgangsNiveau.Alle;
                if (SystemAdgang == 2)
                    nyTilskudsinformation.SystemAdgang = Tilskudsinformation.API_AdgangsNiveau.Ikke_tilladt;
                //** Tilføjet for ESCRM-98/108
                nyTilskudsinformation.AfslutningsDato = AfslutningDato;
                nyTilskudsinformation.Informations_Hjemmeside = Informations_Hjemmeside;

                //** Nyt felt
                nyTilskudsinformation.PNummer = PNummer;
                //** Nye felter økonomi, ESCRM-176/177
                nyTilskudsinformation.Oko_AnsoegtBeloeb = Oko_AnsoegtBeloeb;
                nyTilskudsinformation.Oko_TilsagnsBeloeb = Oko_TilsagnsBeloeb;
                nyTilskudsinformation.Oko_EgenFinansiering = Oko_Egenfinansiering;
                nyTilskudsinformation.Oko_Formaal = Oko_Formaal;
                nyTilskudsinformation.Oko_Projektkonsulent = Oko_Projektkonsulent;
                //** Nye felter ekstern rådgiver
                nyTilskudsinformation.Raadgiver_Firmanavn = Raadgiver_Firmanavn;
                nyTilskudsinformation.Raadgiver_CVR = Raadgiver_CVR;

                //** Skal der laves dublet check
                if (DubletCheck == 1)
                {
                    ///
                    if (sql.TilskudsinformationDubletCheck(nyTilskudsinformation))
                    {
                        return "Tilskudsinformation findes allerede";
                    }
                }

                //** Kald stored procedure for at oprette tilskudsinformation
                if (sql.AddTilskudsinformation(nyTilskudsinformation))
                    returnResult = "Success";
                else
                    returnResult = "Fejl";
            }
            catch (Exception ex)
            {
                returnResult = "Fejl\r\n" + ex.Message;
            }

            return returnResult;
        }
    }
}
