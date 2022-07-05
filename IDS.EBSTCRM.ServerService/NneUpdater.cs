// Service that updates CRM companies' name, address, and phone from NNE webservice.

using System;
using System.Collections.Generic;
using IDS.EBSTCRM.Base;
using System.Configuration;
using System.Diagnostics;

namespace IDS.EBSTCRM.ServerService {
    partial class NneUpdater : System.ServiceProcess.ServiceBase {
        public static volatile System.Timers.Timer timer;    // public static is thread safe
        private volatile bool active = false;
        private volatile bool pleasestop = false;
        private volatile bool debugging = false;

        public NneUpdater() {
            InitializeComponent();
        }

        // This Start is only for debugging
        public void Start() {
            //debugging = true;
            NneUpdateRun(null, null);
        }

        protected override void OnStart(string[] args) {
            ConfigurationManager.RefreshSection("appSettings");

            // First run is 1 minute after starting the service, later runs are delayed as specified in settings.
            timer = new System.Timers.Timer(60000);
            timer.AutoReset = false;
            timer.Elapsed += new System.Timers.ElapsedEventHandler(NneUpdateRun);
            timer.Start();
        }

        protected override void OnStop() {
            pleasestop = true;
            timer.Stop();
            do {   // Wait until our work thread has finished. (We wait before testing active for the unlikely case that
                // the timer has triggered but not yet started the work thread).
                RequestAdditionalTime(250);
                System.Threading.Thread.Sleep(100);
            }
            while (active);
            timer.Dispose();
        }

        private void NneUpdateRun(object Source, System.Timers.ElapsedEventArgs e) {
            active = true;
            try {
                NneUpdateAll();
            } catch (Exception ex) {
                try {
                    // Fake a user object just to get a text line right in the exception mail:
                    User u = new User();
                    u.Username = "NNE updater";
                    if (!debugging)
                        ExceptionMail.SendException(ex, u);
                } catch (Exception ex2) {
                    this.EventLog.WriteEntry(ex.Message, System.Diagnostics.EventLogEntryType.Error);
                    this.EventLog.WriteEntry(ex2.Message, System.Diagnostics.EventLogEntryType.Error);
                }
            }
            // Restart timer, but only after completion, so that we don't get multiple threads working concurrently:
            timer.Interval = int.Parse(ConfigurationManager.AppSettings["NneHoursBetweenRuns"]) * 60 * 60 * 1000;
            timer.Enabled = !pleasestop;
            active = false;
        }

        private void NneUpdateAll() {
            SQLDB db = new SQLDB(ConfigurationManager.AppSettings["ConnStr"]);
            try {
                User u = db.Users_Login(ConfigurationManager.AppSettings["NneCrmUser"], ConfigurationManager.AppSettings["NneCrmPassword"]);
                if (u == null)
                    throw new Exception("Login to CRM failed");

                // Check all companies except deleted or staging:
                foreach (Company c in db.Companies_GetAll_ReallyAll_Almost(u.OrganisationId)) {
                    if (pleasestop)
                        break;

                    Debug.WriteLine("Testing company " + c.Id);

                    List<DynamicField> fields = db.dynamicFields_getFieldsForOrganisationFromSQL(u.Organisation);
                    int cvr = 0;
                    int pno = 0;
                    foreach (TableColumnWithValue col in c.DynamicTblColumns) {
                        col.MatchDynamicField(fields);
                        if (col.DynamicField.DataLink == "NNECompanyCVR")
                            cvr = TypeCast.ToInt(col.Value);
                        if (col.DynamicField.DataLink == "NNECompanyPNR")
                            pno = TypeCast.ToInt(col.Value);
                    }

                    //if (cvr == 15731796)
                    //{

                    //}

                    if (c.NNEId != 0)
                        NneUpdateOne(db, u, c, c.NNEId);
                    else {
                        if (cvr != 0 || pno != 0) {
                            Debug.WriteLine("Searching NNE for cpr/pno");
                            Base.NNE.CompanyBasic cb = NNESearcher.NNEFindCompanyFromCvrOrPNo(cvr, pno);
                            if (cb != null) {
                                NneUpdateOne(db, u, c, cb.tdcId);
                            }
                        }
                    }
                    // just skip those companies we can't find
                }
            } finally {
                db.Dispose();
            }
        }

        private void NneUpdateOne(SQLDB db, User u, Company c, int nneid) {

            int delay = int.Parse(ConfigurationManager.AppSettings["NneIgnoreCompanyUpdatedWithinDays"]);
            DateTime limit = DateTime.Now.AddDays(-delay);

            // Don't update companies that have been updated recently:
            // TODO: Add exception, when company is recently imported
            if (c.LastUpdated != null && (DateTime)c.LastUpdated > limit || (DateTime)c.DateStamp > limit) {
                Debug.WriteLine("Update time doesn't allow a new update");
                return;
            }

            Debug.WriteLine("Getting details from NNE");

            Base.NNE.Company nnecomp = NNESearcher.NNEGetCompany(ref db, ref u, nneid); // db and u are not used
            if (nnecomp == null)    // Not found in NNE:
            {
                Debug.WriteLine("Company details not found in NNE");
                return;             // Should we delete the company??
            }

            bool updateneeded = false;

            // If NNE Id is 0 and a company was found, update the key
            if (c.NNEId == 0) {
                c.NNEId = nnecomp.tdcId;
                updateneeded = true;
            }

            // This search through the columns parallels what happens in WindowsManager\ajax\SMVInstantSearch.aspx.cs for manually fetching from NNE
            foreach (TableColumnWithValue col in c.DynamicTblColumns) {

                switch (col.DynamicField.DataLink) {
                    case "NNECompanyName":
                        updateneeded |= TestField(col, nnecomp.officialName);
                        break;
                    case "NNECompanyAddress1":
                        updateneeded |= TestField(col, nnecomp.street);
                        break;
                    case "NNECompanyZipcode":
                        updateneeded |= TestField(col, nnecomp.zipCode.ToString());
                        break;
                    case "NNECompanyDistrict":
                        updateneeded |= TestField(col, nnecomp.district);
                        break;
                    case "NNECompanyStatusText":
                        updateneeded |= TestField(col, nnecomp.statusText);
                        break;
                    case "NNECompanyCity":
                        updateneeded |= TestField(col, nnecomp.district);
                        break;
                    case "NNECompanyPhone":
                        updateneeded |= TestField(col, nnecomp.phone);
                        break;
                    case "NNECompany":
                        updateneeded |= TestField(col, nnecomp.phone);
                        break;
                    case "Counties":
                        // NNE supplies a code and a lookup table for municipality. Find the name, and fix the single one that CRM names differently.
                        string nnemunip = "";
                        foreach (Base.NNE.ValueText vt in NNESearcher.NNEMunicipalities())
                            if (vt.value == nnecomp.geography.municipalityCode)
                                nnemunip = vt.text;
                        if (nnemunip == "Københavns")
                            nnemunip = "København";
                        updateneeded |= TestField(col, nnemunip);
                        break;
                    case "NNEAdditionalNames":      // At present (05 nov 2012), this field hasn't been created in the live db, only in "sandkassen"
                        updateneeded |= TestField(col, String.Join("\n", NNESearcher.NNEGetCompanyAdditionalNames(nneid)));
                        break;

                    case "CVRStartDate":
                        break;


                        // Just ignore all other fields.
                        // The following fields are fetched from NNE on new creations, but I've chosen not to update them:
                        //   NNEId/tdcId (pointless, if we can find by CVR or PNo now, we can do that the next time as well).
                        //   NNEEmployees/numberOfEmployees (often just the average of an interval, e.g. 10-100 employees becomes 55).
                        //   NNEYear/foundedYear (unlikely to change :-) )
                        //   NNETrade/tradeCode+" "+trade (requires extra call to NNE -- also, most existing data has either the code or the
                        //                                  text, not both, so we would end up updating almost everything)
                        //   NNECompanyType/companyForm (unlikely to change) (note that CRM uses abbreviations here, NNE long text)
                        //   NNECompanyCVR/cvrNo (unlikely to change)
                        //   NNECompanyFax/fax (is data quality ok?)
                        //   NNECompanyWeb/homepage (is data quality ok?)
                        //   NNECompanyEmail/email (is data quality ok?)
                        //   NNECompanyPNR/PNo (unlikely to change)
                }
            }

            if (updateneeded && !debugging) {
                c.CreatedById = u.Id;                   // used by company_update as the user responsible
                db.Company_Update(u, c);                // also puts company into staging

                db.CVR_UpdateCompany();                 // CVR information update for company organisation 1

                db.companies_verifyStructure(c.Id, 0);  // gets company out of staging
            }
        }

        // test whether this column needs update, and prepare the new value
        private bool TestField(TableColumnWithValue col, string nnevalue) {
            if (nnevalue == null || nnevalue == "" || col.ValueFormatted == nnevalue)
                return false;
            else {
                Debug.WriteLine("Col " + col.Name + " needs updating from " + col.ValueFormatted + " to " + nnevalue);
                col.Value = nnevalue;
                return true;
            }
        }
    }
}
