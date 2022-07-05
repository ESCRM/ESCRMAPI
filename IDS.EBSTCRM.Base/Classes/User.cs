using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Web;

namespace IDS.EBSTCRM.Base {
    public enum UserRoles {
        Consultant = 0,
        SectionLeader = 10,
        CEO = 20,
        Accountant = 25,
        Administrator = 30,
        GlobalStatistics = 40,
        GlobalAdministrator = 50,
        SystemOwner = 60

    }

    /// <summary>
    /// User with Early Warning Rights and properties
    /// </summary>
    [Serializable()]
    public class EarlyWarningUser : User {
        public string OrganisationName;

        public EarlyWarningUser()
            : base() {

        }

        public EarlyWarningUser(ref SqlDataReader dr)
            : base(ref dr, false) {
            OrganisationName = TypeCast.ToString(dr["OrganisationName"]);
        }
    }

    /// <summary>
    /// Deleted User
    /// </summary>
    [Serializable()]
    public class UserDeleted : User {
        public string DeletedByUser = "";
        public DateTime DateDeleted;

        public UserDeleted() {
        }

        public UserDeleted(ref SqlDataReader dr)
            : base(ref dr, false) {
            DeletedByUser = TypeCast.ToString(dr["DeletedByUser"]);
            DateDeleted = TypeCast.ToDateTime(dr["DeletedDate"]);
        }
    }

    /// <summary>
    /// User object
    /// Stores user data, such as Organisation, and user security rights
    /// </summary>
    [Serializable()]
    public class User : EventLogBase {
        public string Id = "";
        public int OrganisationId = 0;
        public string Firstname = "";
        public string Lastname = "";
        public string Username = "";
        public string Password = "";
        public string Email = "";
        public string LicensePlates = "";
        public UserRoles UserRole = UserRoles.Consultant;
        public Organisation Organisation = null;
        public bool AllowPersonalFolder;

        public string ExchangeURL = "";
        public string ExchangeUsername = "";
        public string ExchangePassword = "";
        public string ExchangeDomain = "";
        public bool ExchangeFormbasedLogin = false;

        public DateTime ExchangeCurrentCalendarDate = DateTime.Today;

        public string Initials = "";

        public DateTime LastLogin = DateTime.Now.AddYears(-100);
        //public DateTime? PreviousLastLoginDate { get; set; }

        public bool ShowNews = true;

        public bool EarlyWarningUser = false;
        public bool RecieveEmailOnExportToUserEvaluation = false;

        public object SyncExchangeMailDate = null;

        public bool AutomaticSyncOnLogin = false;
        public bool NoWelcome = false;

        public bool UseStartMenu = true;
        public bool UseTopMenu = true;
        public bool UseWindowShadows = true;
        public int UseCustomDesktopBackground = 0;
        public bool ShowHomeAtStartup = true;
        public string UseCustomDesktopBackgroundStyle = "";

        public DateTime PasswordExpires;
        public bool MustChangePasswordAtLogin;

        public bool AcceptsTransferredCompanies;

        public object DeletedDate;
        public string DeletedByUserInfo;

        public int ListviewDataWarningAt;

        public List<UserProperty> Properties = null;

        //New properties
        public bool AllowImportData = false;
        public bool AllowMassRegistrations = false;
        public bool AllowMailgroupModifications = false;
        public bool AllowImpersonation = false;

        public bool FreeUser = false;
        public bool InvoiceAdvising = false;

        // Log Types
        public List<LogType> LogTypes {
            get {
                SQLDB sql = new SQLDB();
                return sql.GetLogType();
            }
        }

        //Integration to W2l
        public string W2L_Username { get; set; }
        public string W2L_Password { get; set; }

        // Integration til FVDB (KL)
        public int PNummer { get; set; }
        public string CellPhone { get; set; }



        // Time Registration
        public string Team { get; set; }
        public string Manager { get; set; }



        //Browser data
        public string BrowserData { get; set; }

        public bool StayInDesktopVersion { get; set; }

        public void SetBrowserData(HttpBrowserCapabilities r) {
            BrowserData = r.Browser + " " + r.Version + " (" + DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss") + ")";
        }

        public void SetProperty(ref SQLDB sql, string Property, string Value) {
            sql.UserPropertiesSet(this.Id, Property, Value);
            for (int i = 0; i < Properties.Count; i++) {
                if (Properties[i].Property.ToLower() == Property.ToLower()) {
                    Properties[i].Value = Value;
                    return;
                }
            }

            Properties.Add(new UserProperty(Property.ToLower(), Value));
        }

        public string GetProperty(ref SQLDB sql, string Property) {
            return this.GetProperty(ref sql, Property, "");
        }
        public string GetProperty(ref SQLDB sql, string Property, string DefaltValue) {
            //if (Properties == null)
            Properties = sql.UserPropertiesGetAll(this.Id);

            foreach (UserProperty p in Properties) {
                if (p.Property.ToLower() == Property.ToLower())
                    return p.Value;
            }

            this.SetProperty(ref sql, Property, DefaltValue);
            return DefaltValue;
        }

        public string UserRoleFormated {
            get {
                switch (UserRole) {
                    case UserRoles.Administrator:
                        return "Administrator";

                    case UserRoles.CEO:
                        return "Direktør";

                    case UserRoles.Accountant:
                        return "Bogføringsmedarbejder";

                    case UserRoles.Consultant:
                        return "Konsulent";

                    case UserRoles.GlobalAdministrator:
                        return "Global administrator";

                    case UserRoles.GlobalStatistics:
                        return "Global statistiker";

                    case UserRoles.SectionLeader:
                        return "Mellemleder";

                    case UserRoles.SystemOwner:
                        return "Systemejer";
                }

                return "Ukendt";
            }
        }

        public User() {

        }

        public User(ref SqlDataReader dr, bool fillList) {
            Populate(ref dr, fillList);
        }

        public User(ref SqlDataReader dr) {
            Populate(ref dr, true);
        }

        public void ToHTMLObject(ref System.Web.UI.HtmlControls.HtmlGenericControl c) {
            if (!UseCustomDesktopBackgroundStyle.EndsWith(".css"))
                UseCustomDesktopBackgroundStyle = "default.css";

            c.InnerText = (this.Firstname + " " + this.Lastname).Trim();
            c.Attributes["uid"] = this.Id;
            c.Attributes["role"] = ((int)this.UserRole).ToString();
            c.Attributes["roleName"] = this.UserRoleFormated;
            c.Attributes["syncEmailsAtStart"] = this.AutomaticSyncOnLogin ? "true" : "";
            c.Attributes["earlywarning"] = this.EarlyWarningUser ? "true" : "";
            c.Attributes["backgroundImage"] = this.UseCustomDesktopBackground.ToString();
            c.Attributes["welcomeAtStart"] = this.ShowHomeAtStartup ? "true" : "";
            c.Attributes["syncEmails"] = this.AutomaticSyncOnLogin ? "true" : "";
            c.Attributes["backgroundImage"] = this.UseCustomDesktopBackground.ToString();
            c.Attributes["windowLayout"] = this.UseCustomDesktopBackgroundStyle;
            c.Attributes["useTopMenu"] = this.UseTopMenu ? "true" : "";
            c.Attributes["organisationType"] = ((int)this.Organisation.Type).ToString();

            //New properties
            c.Attributes["AllowImportData"] = this.AllowImportData ? "true" : "";
            c.Attributes["AllowMassRegistrations"] = this.AllowMassRegistrations ? "true" : "";
            c.Attributes["AllowMailgroupModifications"] = this.AllowMailgroupModifications ? "true" : "";
            c.Attributes["AllowImpersonation"] = this.AllowImpersonation ? "true" : "";
            c.Attributes["FreeUser"] = this.FreeUser ? "true" : "";
            c.Attributes["InvoiceAdvising"] = this.InvoiceAdvising ? "true" : "";

            c.Attributes["OrganisationId"] = this.OrganisationId.ToString();
            c.Attributes["OrganisationName"] = this.Organisation.Name.ToString();
            c.Attributes["UserName"] = this.Username.ToString();
        }

        private void Populate(ref SqlDataReader dr, bool fillList) {
            Id = TypeCast.ToString(dr["Id"]);
            OrganisationId = TypeCast.ToInt(dr["OrganisationId"]);
            Firstname = TypeCast.ToString(dr["Firstname"]);
            Lastname = TypeCast.ToString(dr["Lastname"]);
            Username = TypeCast.ToString(dr["Username"]);
            Password = TypeCast.ToString(dr["Password"]);
            Email = TypeCast.ToString(dr["Email"]);
            LicensePlates = TypeCast.ToString(dr["LicensePlates"]);
            UserRole = (UserRoles)TypeCast.ToInt(dr["UserRole"]);
            AllowPersonalFolder = TypeCast.ToBool(dr["allowPersonalFolder"]);

            if ((int)UserRole > 0 && (int)UserRole < 10) {
                UserRole = (UserRoles)((int)UserRole * 10);
            }

            ExchangeURL = TypeCast.ToString(dr["ExchangeURL"]);
            ExchangeUsername = TypeCast.ToString(dr["ExchangeUsername"]);
            ExchangePassword = TypeCast.ToString(dr["ExchangePassword"]);
            ExchangeDomain = TypeCast.ToString(dr["ExchangeDomain"]);
            ExchangeFormbasedLogin = TypeCast.ToBool(dr["exchangeFormbasedLogin"]);

            W2L_Username = TypeCast.ToString(dr["W2L_Username"]);
            W2L_Password = TypeCast.ToString(dr["W2L_Password"]);

            Initials = TypeCast.ToString(dr["Initials"]);

            EarlyWarningUser = TypeCast.ToBool(dr["EarlyWarningUser"]);
            RecieveEmailOnExportToUserEvaluation = TypeCast.ToBool(dr["RecieveEmailOnExportToUserEvaluation"]);

            SyncExchangeMailDate = TypeCast.ToDateTimeOrNull(dr["SyncExchangeMailDate"]);

            AutomaticSyncOnLogin = TypeCast.ToBool(dr["AutomaticSyncOnLogin"]);
            NoWelcome = TypeCast.ToBool(dr["NoWelcome"]);

            object tmp = TypeCast.ToDateTimeOrNull(dr["LastLogin"]);
            if (tmp != null)
                LastLogin = (DateTime)tmp;

            ShowNews = TypeCast.ToBool(dr["showNews"]);

            UseStartMenu = TypeCast.ToBoolOrTrue(dr["UseStartMenu"]);
            UseTopMenu = TypeCast.ToBoolOrTrue(dr["UseTopMenu"]);
            UseWindowShadows = TypeCast.ToBoolOrTrue(dr["UseWindowShadows"]);
            UseCustomDesktopBackground = TypeCast.ToInt(dr["UseCustomDesktopBackground"]);
            ShowHomeAtStartup = TypeCast.ToBoolOrTrue(dr["ShowHomeAtStartup"]);
            UseCustomDesktopBackgroundStyle = TypeCast.ToString(dr["UseCustomDesktopBackgroundStyle"]);

            PasswordExpires = TypeCast.ToDateTime(dr["PasswordExpires"]);
            MustChangePasswordAtLogin = TypeCast.ToBool(dr["MustChangePasswordAtLogin"]);

            AcceptsTransferredCompanies = TypeCast.ToBool(dr["AcceptsTransferredCompanies"]);

            DeletedDate = TypeCast.ToDateTimeOrNull(dr["DeletedDate"]);
            DeletedByUserInfo = TypeCast.ToString(dr["DeletedByUserInfo"]);

            ListviewDataWarningAt = TypeCast.ToInt(dr["ListviewDataWarningAt"]);

            //New properties
            AllowImportData = TypeCast.ToBool(dr["AllowImportData"]);
            AllowMassRegistrations = TypeCast.ToBool(dr["AllowMassRegistrations"]);
            AllowMailgroupModifications = TypeCast.ToBool(dr["AllowMailgroupModifications"]);
            AllowImpersonation = TypeCast.ToBool(dr["AllowImpersonation"]);

            FreeUser = TypeCast.ToBool(dr["FreeUser"]);
            InvoiceAdvising = TypeCast.ToBool(dr["InvoiceAdvising"]);

            PNummer = TypeCast.ToInt(dr["PNumber"]);
            CellPhone = TypeCast.ToString(dr["CellPhone"]);

            // Team Registration
            Team = TypeCast.ToString(dr["Team"]);
            Manager = TypeCast.ToString(dr["Manager"]);

            //Fill list
            if (fillList) {
                if (dr.NextResult()) {
                    if (dr.Read()) {
                        Organisation = new Organisation(ref dr);
                    }
                }
            }

            if (!UseCustomDesktopBackgroundStyle.EndsWith(".css")) {
                UseCustomDesktopBackgroundStyle = "default.css";
            }
        }

        public override VisualItems GetVisualItemsForEventLog(string Event) {
            VisualItems retval = new VisualItems();

            retval.Text = eventToText(Event) + " brugeren " + Username;
            retval.Icon = "images/listviewIcons/user.png";

            retval.JavaScript = (Event == "DELETE" ?
                                    "alert('Brugeren er blevet slettet');"
                                    :
                                    "top.frames['root'].frames['frameGlobalAdmin'].organisations_CreateUser('" + Id + "');");



            return retval;
        }
    }

    /// <summary>
    /// User containing OrganisatioName
    /// </summary>
    [Serializable()]
    public class UserWithOrganisation : User {
        public string OrganisationName { get; set; }

        public UserWithOrganisation() {

        }

        public UserWithOrganisation(ref SqlDataReader dr)
            : base(ref dr, false) {
            OrganisationName = TypeCast.ToString(dr["organisationName"]);
        }
    }
}
