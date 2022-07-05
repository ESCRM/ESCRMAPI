using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Services;
using IDS.EBSTCRM.Base;
using IDS.EBSTCRM.Base.Classes;

namespace IDS.EBSTCRM.WindowManager.Integration {
    /// <summary>
    /// Summary description for W2L
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class W2L : System.Web.Services.WebService {

        [WebMethod]
        public VaekstPlanVaekstUnivers HentVaekstplanVaekstUnivers(int Id) {
            ValidIPAddress();
            try {
                VaekstPlanVaekstUnivers retval = null;
                SQLBase sql = new SQLBase(System.Configuration.ConfigurationManager.AppSettings["connectionString"]);

                sql.commandText = "Integration_W2L_GetVaekstplanVaekstUnivers";
                sql.parameters.AddWithValue("@Id", Id);

                System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
                if (dr.Read()) {
                    retval = new VaekstPlanVaekstUnivers(ref dr);
                }
                dr.Close();
                sql.reset();

                sql.Dispose();
                sql = null;

                return retval;
            } catch (Exception err) {
                Context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                Context.Response.StatusDescription = err.Message;
                return null;
            }
        }

        [WebMethod]
        public Vaekstplan HentVaekstplan(int Id) {
            ValidIPAddress();
            try {
                Vaekstplan retval = null;
                SQLBase sql = new SQLBase(System.Configuration.ConfigurationManager.AppSettings["connectionString"]);

                sql.commandText = "Integration_W2L_GetVaekstplan";
                sql.parameters.AddWithValue("@Id", Id);

                System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
                if (dr.Read()) {
                    retval = new Vaekstplan(ref dr);
                }
                dr.Close();
                sql.reset();

                sql.Dispose();
                sql = null;

                return retval;
            } catch (Exception err) {
                Context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                Context.Response.StatusDescription = err.Message;
                return null;
            }
        }

        [WebMethod]
        public VaekstPlanEU HentVaekstplanEU(int Id) {
            ValidIPAddress();
            try {
                VaekstPlanEU retval = null;
                SQLBase sql = new SQLBase(System.Configuration.ConfigurationManager.AppSettings["connectionString"]);

                sql.commandText = "Integration_W2L_GetVaekstplanEU";
                sql.parameters.AddWithValue("@Id", Id);

                System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
                if (dr.Read()) {
                    retval = new VaekstPlanEU(ref dr);
                }
                dr.Close();
                sql.reset();

                sql.Dispose();
                sql = null;

                return retval;
            } catch (Exception err) {
                Context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                Context.Response.StatusDescription = err.Message;
                return null;
            }
        }

        [WebMethod]
        public Vaekstplan HentVaekstplan_DEMO() {
            ValidIPAddress();
            try {
                var pl = new Vaekstplan();
                pl.Virksomhedsleder = "Anders Hoffmann";

                pl.Vaekstkonsulent = new Konsulent();
                pl.Vaekstkonsulent.Navn = "Anders Lund Madsen";
                pl.Vaekstkonsulent.OrganisationId = 1;

                pl.StartDato = new DateTime(2012, 2, 2); // "02-02-2012";
                pl.AfsluttetDato = new DateTime(2012, 2, 5); //"05-12-2012";

                pl.Vaekstambitioner = new List<Vaekstambition>();
                pl.Vaekstambitioner.Add(new Vaekstambition() { KortSigt = "Ikke gå konkurs", LangSigt = "Blive milliardær!" });

                pl.Vaeksthjul = new Vaeksthjul();
                pl.Vaeksthjul.ForretningsIde = 0;
                pl.Vaeksthjul.ProduktPortefoelje = 0;
                pl.Vaeksthjul.ForretningsModel = 1;
                pl.Vaeksthjul.KundePortefoelje = 3;
                pl.Vaeksthjul.Markedsposition = 2;
                pl.Vaeksthjul.Netvaerk = 0;
                pl.Vaeksthjul.Markedsfoering = 4;
                pl.Vaeksthjul.Salg = 4;
                pl.Vaeksthjul.KommunikationOgPR = 2;
                pl.Vaeksthjul.Branding = 0;
                pl.Vaeksthjul.EjerkredsOgBestyrelse = 1;
                pl.Vaeksthjul.Medarbejdere = 3;
                pl.Vaeksthjul.Samarbejdspartnere = 3;
                pl.Vaeksthjul.Forretningsgange = 1;
                pl.Vaeksthjul.JuridiskeForhold = 0;
                pl.Vaeksthjul.OEkonomistyring = 4;
                pl.Vaeksthjul.Finansiering = 3;
                pl.Vaeksthjul.LeveranceOgProjektstyring = 4;
                pl.Vaeksthjul.ITSystemer = 4;
                pl.Vaeksthjul.Faciliteter = 1;

                pl.Fokusomraader = new List<string>();
                pl.Fokusomraader.Add("Hold tungen lige i munden");

                pl.Vaekstplaner = new List<string>();
                pl.Vaekstplaner.Add("Der skal arbejdes med en forretningsplan");

                pl.Henvisninger = new List<Henvisning>();
                pl.Henvisninger.Add(new Henvisning() {
                    OrganisationEllerKonsulent = "Skatteministreriet",
                    KontaktInfo = "Tester"
                });

                //pl.Henvisninger = new List<Henvisning>();
                //pl.Henvisninger.Add(new Henvisning() { OrganisationEllerKonsulent = "Skatteministreriet",
                //                                        KontaktInformation = new List<KontaktInformation>()
                //                                        });
                //pl.Henvisninger[0].KontaktInformation.Add(new KontaktInformation() { Type = KontaktInformation.KontaktInformationsType.Email, KontaktData = "test@ingenting.dk" });
                //pl.Henvisninger[0].KontaktInformation.Add(new KontaktInformation() { Type = KontaktInformation.KontaktInformationsType.Telefon, KontaktData = "39131000" });
                //pl.Henvisninger[0].KontaktInformation.Add(new KontaktInformation() { Type = KontaktInformation.KontaktInformationsType.Mobil, KontaktData = "51550006" });

                return pl;
            } catch (Exception err) {
                Context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                Context.Response.StatusDescription = err.Message;
                return null;
            }
        }

        private void ValidIPAddress() {
            //bool allowed = true;
            //var sql = new SQLDB();
            //var userIP = Context.Request.UserHostAddress;
            //var methodName = Context.Request.PathInfo.Replace("/", "");
            //var accessIP = sql.GetApiAccessIp(methodName);

            //if (!string.IsNullOrEmpty(accessIP)) {

            //    // Range of IP
            //    if (accessIP.IndexOf("-") > -1) {
            //        var ipRange = accessIP.Split('-');
            //        var startIP = ipRange[0];
            //        var endIP = ipRange[1];

                    
            //    }

            //    // Individual IP
            //    else {
            //        if (userIP != accessIP) {
            //            allowed = false;
            //        }
            //    }

            //    if (!allowed) {
            //        Context.Response.Clear();
            //        Context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            //        Context.Response.StatusDescription = "Your are not authorized to access the web service. Please contact the site administrator.";
            //        Context.Response.End();
            //    }
            //}
        }
    }
}
