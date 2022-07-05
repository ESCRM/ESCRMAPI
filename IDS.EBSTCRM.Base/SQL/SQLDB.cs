using System;
using System.Collections.Generic;
using System.Text;
using IDS.EBSTCRM.DBManager;
using System.Collections;
using System.Data.SqlClient;
using System.Linq;
using System.Data;
using System.Security;
using IDS.EBSTCRM.Base.Classes;
using System.Configuration;
using System.Diagnostics;

namespace IDS.EBSTCRM.Base {
    /// <summary>
    /// SQL Server Communacations Class
    /// All SQL Transactions pass thru this class
    /// </summary>
    public class SQLDB {

        #region Constructors & Declarations
        SQLBase sql;
        string connectionString = System.Configuration.ConfigurationManager.AppSettings["connectionString"];

        //** Har vi allerede kaldt searchCVR_Get, ESCRM-190/191
        private bool HasAlreadyRun = false;
        private List<SearchCVR>  GemteData = null;
        //bool restartRender=false;

        public SQLBase Base {
            get { return sql; }
        }

        public SQLDB() {

            New_(connectionString);
        }

        public SQLDB(string server, string initialDatabase, string username, string password, bool pooling) {
            New_("Data Source=" + server + ";Initial Catalog=" + initialDatabase + ";Uid=" + username + ";pwd=" + password + ";pooling=" + pooling.ToString() + ";");
        }

        public SQLDB(string dbConn) {
            New_(dbConn);
        }

        private void New_(string dbConn) {
            sql = new SQLBase(dbConn);
        }

        public void Dispose() {
            sql.Dispose();
            sql = null;
        }
        #endregion

        #region NNE Tracking

        /// <summary>
        /// Add an entry to the NNE Tracking statisitcs
        /// </summary>
        /// <param name="user"></param>
        public void NNE_Tracking_Add(User user) {
            sql.commandText = "NNE_Tracking_Add";
            sql.parameters.AddWithValue("@OrganisationId", user.OrganisationId);
            sql.parameters.AddWithValue("@userId", user.Id);
            sql.execute();
            sql.reset();
        }

        #endregion

        #region GeoCodes

        /// <summary>
        /// Add or Update a GeoCode in the database
        /// </summary>
        /// <param name="g"></param>
        public void GeoCode_Update(GeoCode g) {
            sql.commandText = "GeoCode_Update";
            if (g.Id > 0)
                sql.parameters.AddWithValue("@Id", g.Id);

            sql.parameters.AddWithValue("@InputAddress", g.InputAddress);
            sql.parameters.AddWithValue("@FormattedAddress", g.FormattedAddress);
            sql.parameters.AddWithValue("@Latitude", g.Latitude);
            sql.parameters.AddWithValue("@Longitude", g.Longitude);
            sql.parameters.AddWithValue("@ViewPortSWLat", g.ViewPortSWLat);
            sql.parameters.AddWithValue("@ViewPortSWLong", g.ViewPortSWLong);
            sql.parameters.AddWithValue("@ViewPortNELat", g.ViewPortNELat);
            sql.parameters.AddWithValue("@ViewPortNELong", g.ViewPortNELong);

            sql.execute();
            sql.reset();
        }

        /// <summary>
        /// Lookup a GeoCode from database
        /// </summary>
        /// <param name="InputAddress"></param>
        /// <returns></returns>
        public GeoCode GeoCode_Get(string InputAddress) {
            //No address - no result
            if (InputAddress == null || InputAddress.Trim() == "") return null;

            GeoCode retval = null;
            sql.commandText = "GeoCode_Get";
            sql.parameters.AddWithValue("@InputAddress", InputAddress.Trim());

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = new GeoCode(ref dr);
            }

            dr.Close();
            sql.reset();

            return retval;
        }

        public IDS.EBSTCRM.Base.GeoAddressInfo GEOCodes_GetRandom250MissingAddresses() {
            List<string> retval = new List<string>();
            IDS.EBSTCRM.Base.GeoAddressInfo gi = new GeoAddressInfo();

            sql.commandText = "GEOCodes_GetRandom250MissingAddresses";
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(TypeCast.ToString(dr["LookupAddress"]));
            }

            gi.Adresses = retval.ToArray();

            if (dr.NextResult()) {
                if (dr.Read()) {
                    gi.MappedAdresses = TypeCast.ToInt(dr["MappedAdresses"]);
                    gi.TotalAdresses = TypeCast.ToInt(dr["TotalAdresses"]);
                }
            }

            dr.Close();
            sql.reset();

            return gi;
        }

        #endregion

        #region PerformanceTimers

        /// <summary>
        /// Adds an entry to performance timers - OBSOLETE
        /// </summary>
        /// <param name="p"></param>
        public void PerformanceTimers_AddEntry(PerformanceTimer p) {
            sql.commandText = "PerformanceTimers_AddEntry";
            sql.parameters.AddWithValue("@OrganisationId", p.CurrentUser.OrganisationId);
            sql.parameters.AddWithValue("@userId", p.CurrentUser.Id);
            sql.parameters.AddWithValue("@PerformaceRegion", (int)p.CurrentPerformaceRegion);
            sql.parameters.AddWithValue("@TickCount", p.TimeUsed);
            sql.parameters.AddWithValue("@RowCount", p.RowCount);
            sql.execute();
            sql.reset();
        }

        #endregion

        #region widgets

        /// <summary>
        /// Gets all active widgets (displayed on desktop) for a single user
        /// </summary>
        /// <param name="UserId"></param>
        /// <returns></returns>
        public List<string> userWidgetsGetAll(string UserId) {
            List<string> retval = new List<string>();
            sql.commandText = "userWidgetsGetAll";
            sql.parameters.AddWithValue("@UserId", UserId);
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(TypeCast.ToString(dr["widgetName"]) + "@" + TypeCast.ToString(dr["X"]) + "@" + TypeCast.ToString(dr["Y"]));
            }

            dr.Close();
            sql.reset();

            return retval;
        }

        /// <summary>
        /// Adds a new widget to the desktop
        /// </summary>
        /// <param name="UserId"></param>
        /// <param name="widgetName"></param>
        /// <param name="SortOrder"></param>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        public void userWidgetsAdd(string UserId, string widgetName, int SortOrder, int X, int Y) {
            sql.commandText = "userWidgetsAdd";
            sql.parameters.AddWithValue("@UserId", UserId);
            sql.parameters.AddWithValue("@widgetName", widgetName);
            sql.parameters.AddWithValue("@SortOrder", SortOrder);
            if (X != 0 || Y != 0) sql.parameters.AddWithValue("@X", X);
            if (X != 0 || Y != 0) sql.parameters.AddWithValue("@Y", Y);

            sql.execute();
            sql.reset();
        }

        /// <summary>
        /// Removes a widget from the desktop
        /// </summary>
        /// <param name="UserId"></param>
        /// <param name="widgetName"></param>
        public void userWidgetsRemove(string UserId, string widgetName) {
            sql.commandText = "userWidgetsRemove";
            sql.parameters.AddWithValue("@UserId", UserId);
            sql.parameters.AddWithValue("@widgetName", widgetName);

            sql.execute();
            sql.reset();
        }

        #region FavoriteSMV, ESCRM-14/105
        /// <summary>
        /// Function : Get favorite SMV
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public List<ResentFavorites> FavoriteSMV_getFavoriteSMV(User user)
        {
            var resentFav = new List<ResentFavorites>();
            //sql.commandText = "FavoriteSMV_getFavoriteSMV";
            sql.commandText = "GetRecentFavoriteChanges";
            sql.parameters.AddWithValue("@userId", user.Id);

            using (SqlDataReader dr = sql.executeReader)
            {
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        resentFav.Add(new ResentFavorites()
                        {
                            CompanyId = TypeCast.ToInt(dr["companyId"]),
                            CompanyName = TypeCast.ToString(dr["Firmanavn"]),
                            Action = TypeCast.ToString(dr["action"]),
                            Description = TypeCast.ToString(dr["beskrivelse"]),
                            ChangedDate = TypeCast.ToString(dr["changeDate"]),
                            OrganisationId = TypeCast.ToString(dr["organizationId"]),
                            UserId = TypeCast.ToString(dr["userId"]),
                            Name = TypeCast.ToString(dr["Name"]),
                            Profile = TypeCast.ToString(dr["profile"]),
                            CompanyType = TypeCast.ToString(dr["CompanyType"])
                        });
                    }
                }
                if (!dr.IsClosed) { dr.Close(); dr.Dispose(); }
            }

            sql.reset();

            return resentFav;
        }

        /// <summary>
        /// Function : Get favorite SMV for search tab, ESCRM-122(106)
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public List<List<Tuple<string, string>>> FavoriteSMV_getFavoriteSMV_SearchTab(User user)
        {
            //var resentFav = new List<ResentFavorites>();
            var resultFav = new List<List<Tuple<string, string>>>();

            //** Rettet til ny Sproc fra Martin, ESCRM-122
            sql.commandText = "GetFavoritesByUserId";

            sql.parameters.AddWithValue("@userId", user.Id);

            using (SqlDataReader dr = sql.executeReader)
            {
                if (dr.HasRows)
                {
                    var columns = Enumerable.Range(0, dr.FieldCount).Select(dr.GetName).ToList();

                    while (dr.Read())
                    {
                        //currentFav.Clear();
                        var currentFav = new List<Tuple<string, string>>();

                        //** Vi prøver noget nyt. Den skal fylde en List<Tuple>
                        foreach (var item in columns)
                        {
                            currentFav.Add(new Tuple<string, string>(item.ToString(), dr[item.ToString()].ToString()));
                        }

                        resultFav.Add(currentFav);
                    }
                }
                if (!dr.IsClosed) { dr.Close(); dr.Dispose(); }
            }

            sql.reset();

            return resultFav;
        }
        #endregion        
        #endregion

        #region Finalcial pools

        /// <summary>
        /// Get all available financial pools for an organisation
        /// </summary>
        /// <param name="OrganisationId"></param>
        /// <returns></returns>
        public List<FinancialPool> finalcialPools_getAll(int OrganisationId) {
            List<FinancialPool> retval = new List<FinancialPool>();
            sql.commandText = "finalcialPools_getAll";
            sql.parameters.AddWithValue("@OrganisationId", OrganisationId);
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new FinancialPool(ref dr, false));
            }

            dr.Close();
            sql.reset();

            return retval;
        }

        /// <summary>
        /// Gets single financial pool from Id - used for editing its properties
        /// </summary>
        /// <param name="id"></param>
        /// <param name="OrganisationId"></param>
        /// <returns></returns>
        public FinancialPool finalcialPools_get(int id, int OrganisationId) {
            FinancialPool retval = null;
            sql.commandText = "finalcialPools_get";
            sql.parameters.AddWithValue("@id", id);
            if (OrganisationId > 0) sql.parameters.AddWithValue("@OrganisationId", OrganisationId);
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = new FinancialPool(ref dr, true);
            }

            dr.Close();
            sql.reset();

            return retval;
        }

        /// <summary>
        /// Updates shared state for a financial pool
        /// </summary>
        /// <param name="FinancialPoolId"></param>
        /// <param name="SharedWithOrganisationId"></param>
        public void finalcialPools_updateShare(int FinancialPoolId, int SharedWithOrganisationId) {
            sql.commandText = "finalcialPools_updateShare";
            sql.parameters.AddWithValue("@FinancialPoolId", FinancialPoolId);
            sql.parameters.AddWithValue("@SharedWithOrganisationId", SharedWithOrganisationId);

            sql.execute();
            sql.reset();
        }

        /// <summary>
        /// Creates or updates an financial pool
        /// </summary>
        /// <param name="U"></param>
        /// <param name="P"></param>
        /// <returns></returns>
        public int finalcialPools_update(User U, FinancialPool P) {
            int retval = 0;
            sql.commandText = "finalcialPools_update";
            if (P.Id > 0) sql.parameters.AddWithValue("@id", P.Id);
            sql.parameters.AddWithValue("@organisationId", U.OrganisationId);
            sql.parameters.AddWithValue("@name", P.Name);
            sql.parameters.AddWithValue("@shared", P.Shared);
            sql.parameters.AddWithValue("@createdby", U.Id);
            sql.parameters.AddWithValue("@ForceEvaluation", P.ForceEvaluation);
            sql.parameters.AddWithValue("@LimitedToSMV", P.LimitedToSMV);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = TypeCast.ToInt(dr["id"]);
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        /// <summary>
        /// Delete a financial pool
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="OrganisationId"></param>
        public void finalcialPools_delete(int Id, int OrganisationId) {
            sql.commandText = "finalcialPools_delete";
            sql.parameters.AddWithValue("@Id", Id);
            sql.parameters.AddWithValue("@OrganisationId", OrganisationId);

            sql.execute();
            sql.reset();
        }

        #endregion

        #region SMV ERRORS

        /// <summary>
        /// Gets SMV Company doublets (in theory there should be none)
        /// </summary>
        /// <returns></returns>
        public List<SMVDoubleCompany> SYSTEM_SMV_GetDoubles() {
            return SYSTEM_SMV_GetDoubles(0);
        }

        /// <summary>
        /// Gets SMV Company doublets (in theory there should be none)
        /// </summary>
        /// <returns></returns>
        public List<SMVDoubleCompany> SYSTEM_SMV_GetDoubles(int CompanyId) {
            List<SMVDoubleCompany> retval = new List<SMVDoubleCompany>();
            sql.commandText = "SYSTEM_SMV_GetDoubles";
            if (CompanyId > 0) sql.parameters.AddWithValue("@companyId", CompanyId);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new SMVDoubleCompany(ref dr));
            }

            dr.Close();
            sql.reset();

            return retval;
        }

        #endregion

        #region Visuals and user memory

        /// <summary>
        /// Adds a quicklink to the user taskbar menu
        /// </summary>
        /// <param name="ql"></param>
        /// <returns></returns>
        public int UserAddQuickLink(UserQuickLink ql) {
            int retval = 0;
            sql.commandText = "UserAddQuickLink";
            sql.parameters.AddWithValue("@Icon16", ql.Icon16);
            sql.parameters.AddWithValue("@Icon32", ql.Icon32);
            sql.parameters.AddWithValue("@Name", ql.Name);
            sql.parameters.AddWithValue("@OrganisationId", ql.OrganisationId);

            sql.parameters.AddWithValue("@SortOrderInt", ql.SortOrderInt);
            sql.parameters.AddWithValue("@URL", ql.URL);
            sql.parameters.AddWithValue("@UserId", ql.UserId);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = TypeCast.ToInt(dr["id"]);
            }

            dr.Close();
            sql.reset();

            return retval;
        }

        /// <summary>
        /// Removes a quicklink from the users taskbar menu
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="UserId"></param>
        public void UserRemoveQuickLink(int Id, string UserId) {
            sql.commandText = "UserRemoveQuickLink";
            sql.parameters.AddWithValue("@UserId", UserId);
            sql.parameters.AddWithValue("@Id", Id);
            sql.execute();
            sql.reset();
        }

        /// <summary>
        /// Sets a property value to an user
        /// </summary>
        /// <param name="UserId"></param>
        /// <param name="Property"></param>
        /// <param name="Value"></param>
        public void UserPropertiesSet(string UserId, string Property, string Value) {
            sql.commandText = "UserPropertiesSet";
            sql.parameters.AddWithValue("@UserId", UserId);
            sql.parameters.AddWithValue("@Property", Property.ToLower());
            sql.parameters.AddWithValue("@Value", Value);
            sql.execute();
            sql.reset();
        }

        /// <summary>
        /// Gets all properties stored on a user
        /// </summary>
        /// <param name="UserId"></param>
        /// <returns></returns>
        public List<UserProperty> UserPropertiesGetAll(string UserId) {
            List<UserProperty> retval = new List<UserProperty>();
            sql.commandText = "UserPropertiesGetAll";
            sql.parameters.AddWithValue("@UserId", UserId);
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new UserProperty(ref dr));
            }

            dr.Close();
            sql.reset();

            return retval;
        }

        /// <summary>
        /// Gets all quicklinks stored for a user
        /// </summary>
        /// <param name="UserId"></param>
        /// <returns></returns>
        public List<UserQuickLink> UserGetQuickLinks(string UserId) {
            List<UserQuickLink> retval = new List<UserQuickLink>();
            sql.commandText = "UserGetQuickLinks";
            sql.parameters.AddWithValue("@UserId", UserId);
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new UserQuickLink(ref dr));
            }

            dr.Close();
            sql.reset();

            return retval;
        }

        /// <summary>
        /// Get most used windows from user
        /// </summary>
        /// <param name="UserId"></param>
        /// <returns></returns>
        public List<UserMostViewedWindow> Users_GetMostUsedWindows(string UserId) {
            List<UserMostViewedWindow> retval = new List<UserMostViewedWindow>();
            sql.commandText = "Users_GetMostUsedWindows";
            sql.parameters.AddWithValue("@UserId", UserId);
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new UserMostViewedWindow(ref dr));
            }

            dr.Close();
            sql.reset();

            return retval;
        }

        /// <summary>
        /// Add a window to most used windows history
        /// </summary>
        /// <param name="UserId"></param>
        /// <param name="URL"></param>
        /// <param name="Title"></param>
        /// <param name="Icon"></param>
        /// <param name="Width"></param>
        /// <param name="Height"></param>
        /// <param name="Minimizeable"></param>
        /// <param name="Maximizeable"></param>
        /// <param name="Closeable"></param>
        /// <param name="Resizeable"></param>
        public void Users_AddWindow(string UserId, string URL, string Title, string Icon, int Width, int Height, bool Minimizeable, bool Maximizeable, bool Closeable, bool Resizeable) {
            sql.commandText = "Users_AddWindow";
            sql.parameters.AddWithValue("@UserId", UserId);
            sql.parameters.AddWithValue("@URL", URL);
            sql.parameters.AddWithValue("@Title", Title);
            sql.parameters.AddWithValue("@Icon", Icon);

            sql.parameters.AddWithValue("@Width", Width);
            sql.parameters.AddWithValue("@Height", Height);
            sql.parameters.AddWithValue("@Minimizeable", Minimizeable);
            sql.parameters.AddWithValue("@Maximizeable", Maximizeable);
            sql.parameters.AddWithValue("@Closeable", Closeable);
            sql.parameters.AddWithValue("@Resizeable", Resizeable);

            sql.execute();
            sql.reset();
        }

        #endregion

        #region Authentication

        /// <summary>
        /// Sets wether the users wants to see "welcome" when he or she logs in
        /// </summary>
        /// <param name="UserId"></param>
        /// <param name="NoWelcome"></param>
        public void User_SetShowWelcome(string UserId, bool NoWelcome) {
            sql.commandText = "User_SetShowWelcome";
            sql.parameters.AddWithValue("@UserId", UserId);
            sql.parameters.AddWithValue("@NoWelcome", NoWelcome);
            sql.execute();
            sql.reset();
        }

        /// <summary>
        /// Updates last login date for a user
        /// </summary>
        /// <param name="UserId"></param>
        public void Users_UpdateLastLogin(string UserId) {
            sql.commandText = "Users_UpdateLastLogin";
            sql.parameters.AddWithValue("@UserId", UserId);
            sql.execute();
            sql.reset();
        }

        /// <summary>
        /// Attempt to log in
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="ipAddress"></param>
        /// <param name="userAgent"></param>
        /// <returns></returns>
        public User Users_Login(string username, string password, string ipAddress = "", string userAgent = "") {
            User retval = null;
            sql.commandText = "Users_Login";
            sql.parameters.AddWithValue("@username", username);
            sql.parameters.AddWithValue("@password", password);
            if (!string.IsNullOrEmpty(ipAddress)) {
                sql.parameters.AddWithValue("@IPaddress", ipAddress);
            }
            if (!string.IsNullOrEmpty(userAgent)) {
                sql.parameters.AddWithValue("@UserAgent", userAgent);
            }
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = new User(ref dr);
            }

            //Make case sensitive pwd!
            if (retval != null)
                if (retval.Password != password) retval = null;

            dr.Close();
            sql.reset();

            return retval;
        }

        #endregion

        #region BugTracker

        /// <summary>
        /// Get news from bugtracker
        /// </summary>
        /// <param name="userLevel"></param>
        /// <returns></returns>
        public List<BugTracker> BugTracker_GetNews(int userLevel) {
            List<BugTracker> retval = new List<BugTracker>();
            sql.commandText = "BugTracker_GetNews";
            sql.parameters.AddWithValue("@userLevel", userLevel);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new BugTracker(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        #endregion

        #region AddressBook

        /// <summary>
        /// Get all entries from a users addressbook (used with travelusage)
        /// </summary>
        /// <param name="UserId"></param>
        /// <returns></returns>
        public List<AddressBookItem> addressbook_getAll(string UserId) {
            List<AddressBookItem> retval = new List<AddressBookItem>();
            sql.commandText = "addressbook_getAll";
            sql.parameters.AddWithValue("@UserId", UserId);


            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new AddressBookItem(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        /// <summary>
        /// Find addressbook item within a users addressbook registry
        /// </summary>
        /// <param name="UserId"></param>
        /// <param name="address"></param>
        /// <returns></returns>
        public List<AddressBookItem> addressbook_lookup(string UserId, string address) {
            List<AddressBookItem> retval = new List<AddressBookItem>();
            sql.commandText = "addressbook_lookup";
            sql.parameters.AddWithValue("@UserId", UserId);
            sql.parameters.AddWithValue("@address", address);


            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new AddressBookItem(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        /// <summary>
        /// Remove an address from a users addressbook
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="UserId"></param>
        public void addressbook_dropAddress(int Id, string UserId) {
            sql.commandText = "addressbook_dropAddress";
            sql.parameters.AddWithValue("@Id", Id);
            sql.parameters.AddWithValue("@UserId", UserId);
            sql.execute();
            sql.reset();
        }

        /// <summary>
        /// Gets a specific addressbook item, from a users addressbook library
        /// </summary>
        /// <param name="UserId"></param>
        /// <param name="Id"></param>
        /// <returns></returns>
        public AddressBookItem addressbook_getAddress(string UserId, int Id) {
            AddressBookItem retval = null;
            sql.commandText = "addressbook_getAddress";
            sql.parameters.AddWithValue("@UserId", UserId);
            sql.parameters.AddWithValue("@Id", Id);


            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = new AddressBookItem(ref dr);
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        /// <summary>
        /// Update or add an addressbook item
        /// </summary>
        /// <param name="U"></param>
        /// <param name="Address"></param>
        /// <returns></returns>
        public int addressbook_update(User U, AddressBookItem Address) {
            int retval = 0;

            sql.commandText = "addressbook_update";
            if (Address.Id > 0) sql.parameters.AddWithValue("@Id", Address.Id);
            sql.parameters.AddWithValue("@UserId", U.Id);
            sql.parameters.AddWithValue("@OrganisationId", U.OrganisationId);

            sql.parameters.AddWithValue("@Address", Address.Address);
            sql.parameters.AddWithValue("@Zipcode", Address.Zipcode);
            sql.parameters.AddWithValue("@City", Address.City);
            sql.parameters.AddWithValue("@Country", Address.Country);
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = TypeCast.ToInt(dr["id"]);
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        #endregion

        #region Advanced Notes
        public void AVN_DeleteAVNEntity(User U, int AvnId, int Id) {
            sql.commandText = "AVN_DeleteAVNEntity";
            sql.parameters.AddWithValue("@AvnId", AvnId);
            sql.parameters.AddWithValue("@EntityId", Id);
            sql.parameters.AddWithValue("@UserId", U.Id);
            sql.parameters.AddWithValue("@OrganisationId", U.OrganisationId);
            if (System.Web.HttpContext.Current != null && System.Web.HttpContext.Current.Request != null) {
                var ip = System.Web.HttpContext.Current.Request.UserHostAddress;
                sql.parameters.AddWithValue("@IPaddress", ip);
            }
            sql.execute();
            sql.reset();
        }

        /// <summary>
        /// Get AVN information
        /// </summary>
        /// <param name="user"></param>
        /// <param name="AvnId"></param>
        /// <param name="Id"></param>
        /// <param name="obfuscate"></param>
        /// <returns></returns>
        public AVN AVN_GetAVN(User user, int AvnId, int Id, bool obfuscate = true) {

            // OLD STORED PROCEDURE
            //sql.commandText = "z_avn_GetAVN_" + AvnId;
            //sql.parameters.AddWithValue("@id", Id);

            AVN retval = null;
            var fields = AVN_GetFields(AvnId);

            sql.commandText = "AVN_Get";
            sql.parameters.AddWithValue("@UserId", user.Id);
            sql.parameters.AddWithValue("@Id", Id);
            sql.parameters.AddWithValue("@AVNTypeId", AvnId);
            sql.parameters.AddWithValue("@OrganisationId", user.OrganisationId);
            sql.parameters.AddWithValue("@Obfuscate", obfuscate);
            if (System.Web.HttpContext.Current != null && System.Web.HttpContext.Current.Request != null) {
                var ip = System.Web.HttpContext.Current.Request.UserHostAddress;
                sql.parameters.AddWithValue("@IPaddress", ip);
            }
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = new AVN(ref dr, fields);
            }
            dr.Close();
            sql.reset();

            if (retval != null) {
                retval.SharedWithList = AVNEntity_GetSharedWith(AvnId, retval.Id);
                retval.Reminders = AVNEntity_GetReminders(AvnId, retval.Id, user.Id);
            }

            return retval;
        }

        /// <summary>
        /// Get value for one field of AVN
        /// </summary>
        /// <param name="user"></param>
        /// <param name="AvnId"></param>
        /// <param name="Id"></param>
        /// <param name="fieldname"></param>
        /// <returns></returns>
        public string AVN_Get(User user, int AvnId, int Id, string fieldname) {
            var result = string.Empty;
            sql.commandText = "AVN_Get";
            sql.parameters.AddWithValue("@UserId", user.Id);
            sql.parameters.AddWithValue("@Id", Id);
            sql.parameters.AddWithValue("@AVNTypeId", AvnId);
            sql.parameters.AddWithValue("@OrganisationId", user.OrganisationId);
            sql.parameters.AddWithValue("@Obfuscate", false);
            sql.parameters.AddWithValue("@FieldName", fieldname);
            if (System.Web.HttpContext.Current != null && System.Web.HttpContext.Current.Request != null) {
                var ip = System.Web.HttpContext.Current.Request.UserHostAddress;
                sql.parameters.AddWithValue("@IPaddress", ip);
            }
            SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                result = TypeCast.ToString(dr[fieldname]);
            }
            dr.Close();
            sql.reset();
            return result;
        }

        public List<int> AdminAVN_GetAVNsForRepair(int OrganisationId) {
            sql.commandText = "AdminAVN_GetAVNsForRepair";

            if (OrganisationId > 0) sql.parameters.AddWithValue("@OrganisationId", OrganisationId);

            List<int> retval = new List<int>();

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(TypeCast.ToInt(dr["Id"]));
            }

            dr.Close();
            sql.reset();

            return retval;
        }

        public List<DateTime> AVNEntity_GetReminders(int AvnTypeId, int AvnEntityId, string UserId) {
            sql.commandText = "AVNEntity_GetReminders";
            sql.parameters.AddWithValue("@AvnTypeId", AvnTypeId);
            sql.parameters.AddWithValue("@AvnEntityId", AvnEntityId);
            sql.parameters.AddWithValue("@UserId", UserId);

            List<DateTime> retval = new List<DateTime>();

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(TypeCast.ToDateTime(dr["reminderDate"]));
            }

            dr.Close();
            sql.reset();

            return retval;
        }

        public List<AVNEntityDefaultShared> AVNEntity_GetDefaultSharedWith(int AvnId, int OwnerOrganisationId) {
            sql.commandText = "AVNEntity_GetDefaultSharedWith";
            sql.parameters.AddWithValue("@AvnId", AvnId);
            sql.parameters.AddWithValue("@OwnerOrganisationId", OwnerOrganisationId);

            List<AVNEntityDefaultShared> retval = new List<AVNEntityDefaultShared>();

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new AVNEntityDefaultShared(ref dr));
            }

            dr.Close();
            sql.reset();

            return retval;
        }

        public List<AVNEntityShared> AVNEntity_GetSharedWith(int AvnId, int AvnEntityId) {
            sql.commandText = "AVNEntity_GetSharedWith";
            sql.parameters.AddWithValue("@AvnId", AvnId);
            sql.parameters.AddWithValue("@EntityId", AvnEntityId);

            List<AVNEntityShared> retval = new List<AVNEntityShared>();

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new AVNEntityShared(ref dr));
            }

            dr.Close();
            sql.reset();

            return retval;
        }

        public List<AVNEntity> AVN_GetAVNsFromContactOrCompany(User U, int CompanyId) {
            sql.commandText = "AVN_GetAVNsFromContactOrCompany";
            sql.parameters.AddWithValue("@UserId", U.Id);
            sql.parameters.AddWithValue("@OrganisationId", U.OrganisationId);
            sql.parameters.AddWithValue("@CompanyId", CompanyId);

            List<AVNEntity> retval = new List<AVNEntity>();

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new AVNEntity(ref dr));
            }

            dr.Close();
            sql.reset();

            return retval;
        }

        public List<AVNEntityWithReminder> AVN_getNotesWithReminders(User U, bool ShowExipredOnly) {
            sql.commandText = "AVN_getNotesWithReminders";
            sql.parameters.AddWithValue("@UserId", U.Id);
            sql.parameters.AddWithValue("@ShowExipredOnly", ShowExipredOnly);

            List<AVNEntityWithReminder> retval = new List<AVNEntityWithReminder>();

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new AVNEntityWithReminder(ref dr));
            }

            dr.Close();
            sql.reset();

            return retval;
        }

        public List<AVNEntityWithReminderWithContactData> AVN_getNotesWithRemindersAndContactData(User U, bool ShowExipredOnly) {
            sql.commandText = "AVN_getNotesWithRemindersAndContactData";
            sql.parameters.AddWithValue("@UserId", U.Id);
            sql.parameters.AddWithValue("@ShowExipredOnly", ShowExipredOnly);

            List<AVNEntityWithReminderWithContactData> retval = new List<AVNEntityWithReminderWithContactData>();

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new AVNEntityWithReminderWithContactData(ref dr));
            }

            dr.Close();
            sql.reset();

            return retval;
        }

        /// <summary>
        /// AVN update function
        /// </summary>
        /// <param name="user"></param>
        /// <param name="avn"></param>
        /// <returns></returns>
        public int AVN_UpdateAVN(User user, AVN avn) {

            /* OLD ROUTINE
            sql.commandText = "z_avn_UpdateAVN_" + avn.TypeId;
            if (avn.Id > 0) sql.parameters.AddWithValue("@id", avn.Id);
            sql.parameters.AddWithValue("@OrganisationId", U.OrganisationId);
            sql.parameters.AddWithValue("@UserId", U.Id);
            sql.parameters.AddWithValue("@Name", avn.Name);
            if (avn.SMVContactId > 0) sql.parameters.AddWithValue("@SMVContactId", avn.SMVContactId);
            if (avn.SMVCompanyId > 0) sql.parameters.AddWithValue("@SMVCompanyId", avn.SMVCompanyId);
            sql.parameters.AddWithValue("@SharedWith", avn.SharedWith);

            foreach (AVNFieldWithValue f in avn.Fields) {
                sql.parameters.AddWithValue("@" + f.DatabaseColumn.Replace(" ", "_").Replace("[", "_").Replace("]", "_").Replace("(", "_").Replace("-", "_").Replace(")", "_").Replace(".", "_").Replace(":", "_").Replace("/", "_").Replace("\\", "_").Replace(",", "_"), f.DatabaseValue);
            }

            int retval = 0;

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = TypeCast.ToInt(dr["id"]);
            }

            dr.Close();
            sql.reset();
            */

            // FETCH OLD VALUES 
            var c = AVN_GetAVN(user, avn.TypeId, avn.Id);
            var classifications = GetDataClassifications();

            // NEW ROUTINE
            int returnValue = 0;
            try {
                var fieldXmlValue = "<FieldValues>";
                foreach (AVNFieldWithValue f in avn.Fields) {
                    fieldXmlValue = fieldXmlValue + "<Field>";
                    fieldXmlValue = fieldXmlValue + "<TableName>z_avn_" + avn.TypeId + "</TableName>";
                    fieldXmlValue = fieldXmlValue + "<FieldId>" + SecurityElement.Escape(Convert.ToString(f.Id)) + "</FieldId>";
                    fieldXmlValue = fieldXmlValue + "<FieldName>" + SecurityElement.Escape(Convert.ToString(f.DatabaseColumn)) + "</FieldName>";
                    fieldXmlValue = fieldXmlValue + "<Value>" + SecurityElement.Escape(Convert.ToString(f.DatabaseValue)) + "</Value>";

                    var anyUpdate = false;
                    if (c != null && c.Fields != null) {

                        var olddynamicfield = c.Fields.FirstOrDefault(w => w.Id == f.Id);
                        if (olddynamicfield != null) {

                            var oldvalue = Convert.ToString(olddynamicfield.Value);
                            var newvalue = Convert.ToString(f.DatabaseValue);

                            // Don't make update when data is obfuscated
                            if (newvalue.Contains("****")) {
                                anyUpdate = false;
                            } else {
                                if (olddynamicfield.DataType == "bit") {
                                    if (newvalue == "1" || string.Equals(newvalue, "true", StringComparison.CurrentCultureIgnoreCase)) { newvalue = "true"; } else { newvalue = "false"; }
                                }

                                //if (newvalue.ToLower() != oldvalue.ToLower()) {
                                if (!string.Equals(newvalue, oldvalue, StringComparison.Ordinal)) {
                                    fieldXmlValue = fieldXmlValue + "<AnyUpdate>1</AnyUpdate>";
                                    anyUpdate = true;
                                }
                            }
                        }

                        // WHEN NO UPDATE
                        if (!anyUpdate) {
                            fieldXmlValue = fieldXmlValue + "<AnyUpdate>0</AnyUpdate>";
                        }
                    } else {
                        fieldXmlValue = fieldXmlValue + "<AnyUpdate>0</AnyUpdate>";
                    }

                    fieldXmlValue = fieldXmlValue + "</Field>";
                }
                fieldXmlValue = fieldXmlValue + "</FieldValues>";

                var sqlConnection = new SqlConnection(connectionString);
                using (var cmd = new SqlCommand("AVN_Update", sqlConnection)) {
                    cmd.Parameters.AddWithValue("@UserId", user.Id);
                    cmd.Parameters.AddWithValue("@OrganisationId", user.OrganisationId);
                    cmd.Parameters.AddWithValue("@Id", avn.Id);
                    cmd.Parameters.AddWithValue("@TypeId", avn.TypeId);
                    cmd.Parameters.AddWithValue("@Name", avn.Name);
                    cmd.Parameters.AddWithValue("@SharedWith", avn.SharedWith);
                    cmd.Parameters.AddWithValue("@FieldValues", fieldXmlValue);
                    if (avn.SMVContactId > 0) cmd.Parameters.AddWithValue("@SMVContactId", avn.SMVContactId);
                    if (avn.SMVCompanyId > 0) cmd.Parameters.AddWithValue("@SMVCompanyId", avn.SMVCompanyId);
                    if (System.Web.HttpContext.Current != null && System.Web.HttpContext.Current.Request != null) {
                        var ip = System.Web.HttpContext.Current.Request.UserHostAddress;
                        cmd.Parameters.AddWithValue("@IPaddress", ip);
                    }

                    cmd.CommandType = CommandType.StoredProcedure;
                    if (sqlConnection.State == ConnectionState.Closed) { sqlConnection.Open(); }
                    using (SqlDataReader dr = cmd.ExecuteReader()) {
                        if (dr.HasRows) {
                            while (dr.Read()) {
                                returnValue = TypeCast.ToInt(dr["ID"]);
                            }
                        }
                    }
                }
                if (sqlConnection.State == ConnectionState.Open) {
                    sqlConnection.Close();
                    sqlConnection.Dispose();
                }

                // Update Sharewith
                foreach (AVNEntityShared s in avn.SharedWithList) {
                    AVNEntity_UpdateSharedWith(avn.TypeId, returnValue, s.OrganisationId, s.UsergroupId, s.ReadWriteState);
                }

                // Save Reminders
                foreach (DateTime dt in avn.Reminders) {
                    AVNEntity_UpdateReminder(avn.TypeId, returnValue, dt, user.Id);
                }
            } catch (Exception ex) { throw ex; }

            return returnValue;
        }

        /// <summary>
        /// Dismiss AVN reminders
        /// </summary>
        /// <param name="ReminderId"></param>
        public void AVNEntity_DismissReminder(int ReminderId) {
            sql.commandText = "AVNEntity_DismissReminder";
            sql.parameters.AddWithValue("@Id", ReminderId);
            sql.execute();
            sql.reset();
        }

        /// <summary>
        /// Save AVN reminders
        /// </summary>
        /// <param name="AVNTypeId"></param>
        /// <param name="AvnEntityId"></param>
        /// <param name="dt"></param>
        /// <param name="UserId"></param>
        public void AVNEntity_UpdateReminder(int AVNTypeId, int AvnEntityId, DateTime dt, string UserId) {
            sql.commandText = "AVNEntity_UpdateReminder";
            sql.parameters.AddWithValue("@AVNTypeId", AVNTypeId);
            sql.parameters.AddWithValue("@AvnEntityId", AvnEntityId);
            sql.parameters.AddWithValue("@userId", UserId);
            sql.parameters.AddWithValue("@ReminderDate", dt);
            sql.execute();
            sql.reset();
        }

        /// <summary>
        /// Update of AVN whom it's shared with
        /// </summary>
        /// <param name="AvnId"></param>
        /// <param name="AvnEntityId"></param>
        /// <param name="OrganisationId"></param>
        /// <param name="UsergroupId"></param>
        /// <param name="ReadWriteState"></param>
        public void AVNEntity_UpdateSharedWith(int AvnId, int AvnEntityId, int OrganisationId, int UsergroupId, int ReadWriteState) {
            sql.commandText = "AVNEntity_UpdateSharedWith";
            sql.parameters.AddWithValue("@AvnId", AvnId);
            sql.parameters.AddWithValue("@EntityId", AvnEntityId);
            if (OrganisationId > 0) sql.parameters.AddWithValue("@OrganisationId", OrganisationId);
            if (UsergroupId > 0) sql.parameters.AddWithValue("@UsergroupId", UsergroupId);
            sql.parameters.AddWithValue("@ReadWriteState", ReadWriteState);
            sql.execute();
            sql.reset();
        }

        /// <summary>
        /// Update default shared with for AVN
        /// </summary>
        /// <param name="AvnId"></param>
        /// <param name="OwnerOrganisationId"></param>
        /// <param name="OrganisationId"></param>
        /// <param name="UsergroupId"></param>
        /// <param name="ReadWriteState"></param>
        public void AVNEntity_UpdateDefaultSharedWith(int AvnId, int OwnerOrganisationId, int OrganisationId, int UsergroupId, int ReadWriteState) {
            sql.commandText = "AVNEntity_UpdateDefaultSharedWith";
            sql.parameters.AddWithValue("@AvnId", AvnId);
            sql.parameters.AddWithValue("@OwnerOrganisationId", OwnerOrganisationId);
            if (OrganisationId > 0) sql.parameters.AddWithValue("@OrganisationId", OrganisationId);
            if (UsergroupId > 0) sql.parameters.AddWithValue("@UsergroupId", UsergroupId);
            sql.parameters.AddWithValue("@ReadWriteState", ReadWriteState);
            sql.execute();
            sql.reset();
        }

        public void AdminAVNDeleteAVN(User U, int AvnId) {
            sql.commandText = "AdminAVNDeleteAVN";
            sql.parameters.AddWithValue("@AvnId", AvnId);
            sql.parameters.AddWithValue("@UserId", U.Id);
            sql.execute();
            sql.reset();
        }

        public int AdminAVN_Save(User U, AdminAVN avn) {

            sql.commandText = "AdminAVN_Save";
            if (avn.Id > 0) sql.parameters.AddWithValue("@id", avn.Id);
            sql.parameters.AddWithValue("@OrganisationId", avn.OrganisationId);
            sql.parameters.AddWithValue("@UserId", U.Id);
            sql.parameters.AddWithValue("@Name", avn.Name);
            sql.parameters.AddWithValue("@Description", avn.Description);
            if (avn.BackgroundColor != "") sql.parameters.AddWithValue("@BackgroundColor", avn.BackgroundColor);
            if (avn.DisabledBackgroundColor != "") sql.parameters.AddWithValue("@DisabledBackgroundColor", avn.DisabledBackgroundColor);
            if (avn.Icon != "") sql.parameters.AddWithValue("@Icon", avn.Icon);
            if (avn.Category != "" && avn.Category != null) sql.parameters.AddWithValue("@Category", avn.Category);
            sql.parameters.AddWithValue("@Status", avn.Status);
            sql.parameters.AddWithValue("@SaveToCompany", avn.SaveToCompany);

            sql.parameters.AddWithValue("@OnLoad", avn.OnLoad);
            sql.parameters.AddWithValue("@OnNewLoad", avn.OnNewLoad);
            sql.parameters.AddWithValue("@OnExistingLoad", avn.OnExistingLoad);
            sql.parameters.AddWithValue("@OnBeforeSave", avn.OnBeforeSave);
            sql.parameters.AddWithValue("@OnAfterCopy", avn.OnAfterCopy);

            sql.parameters.AddWithValue("@VisibleInReportGenerator", avn.VisibleInReportGenerator);
            sql.parameters.AddWithValue("@AllowChangingExpiration", avn.AllowChangingExpiration);
            //** Sæt geografi, ESCRM-11/183
            sql.parameters.AddWithValue("@Geografi", avn.Geografi);

            int retval = 0;
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = TypeCast.ToInt(dr["id"]);
            }

            dr.Close();
            sql.reset();

            //Save default sharing
            foreach (AVNEntityDefaultShared s in avn.DefaultSharedWith) {
                AVNEntity_UpdateDefaultSharedWith(retval, U.OrganisationId, s.OrganisationId, s.UsergroupId, s.ReadWriteState);
            }

            return retval;
        }

        public void AVN_updateAdminAVNShare(int AvnId, int SharedWithOrganisationId) {
            sql.commandText = "AVN_updateAdminAVNShare";
            sql.parameters.AddWithValue("@AvnId", AvnId);
            sql.parameters.AddWithValue("@SharedWithOrganisationId", SharedWithOrganisationId);
            sql.execute();
            sql.reset();
        }

        public AdminAVN AdminAVN_GetAVN(User U, int Id) {
            sql.commandText = "AdminAVN_GetAVN";
            sql.parameters.AddWithValue("@UserId", U.Id);
            sql.parameters.AddWithValue("@Id", Id);

            AdminAVN retval = null;

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = new AdminAVN(ref dr);
            }

            dr.Close();
            sql.reset();

            if (retval != null)
                retval.DefaultSharedWith = AVNEntity_GetDefaultSharedWith(retval.Id, U.OrganisationId);

            return retval;
        }

        public List<AdminAVN> AVN_GetCreateAbleAVNs(User U) {
            sql.commandText = "AVN_GetCreateAbleAVNs";
            sql.parameters.AddWithValue("@UserId", U.Id);
            sql.parameters.AddWithValue("@OrganisationId", U.OrganisationId);

            List<AdminAVN> retval = new List<AdminAVN>();

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new AdminAVN(ref dr));
            }

            dr.Close();
            sql.reset();

            return retval;
        }

        public List<AdminAVN> AVN_GetNotesFromCategory(User U, string CategoryName, int CategoryOrganisationId) {
            sql.commandText = "AVN_GetNotesFromCategory";
            sql.parameters.AddWithValue("@UserId", U.Id);
            sql.parameters.AddWithValue("@OrganisationId", U.OrganisationId);
            sql.parameters.AddWithValue("@CategoryName", CategoryName);
            sql.parameters.AddWithValue("@CategoryOrganisationId", CategoryOrganisationId);

            List<AdminAVN> retval = new List<AdminAVN>();

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new AdminAVN(ref dr));
            }

            dr.Close();
            sql.reset();

            return retval;
        }

        public List<AdminAVN> AdminAVN_GetAVNs(User U) {
            sql.commandText = "AdminAVN_GetAVNs";
            sql.parameters.AddWithValue("@UserId", U.Id);
            sql.parameters.AddWithValue("@OrganisationId", U.OrganisationId);

            List<AdminAVN> retval = new List<AdminAVN>();

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new AdminAVN(ref dr));
            }

            dr.Close();
            sql.reset();

            return retval;
        }

        public List<AdminAVN> AdminAVN_GetAVNs(int OrganisationId) {
            sql.commandText = "AdminAVN_GetAVNs";
            sql.parameters.AddWithValue("@OrganisationId", OrganisationId);

            List<AdminAVN> retval = new List<AdminAVN>();

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new AdminAVN(ref dr));
            }

            dr.Close();
            sql.reset();

            return retval;
        }

        public void AdminAVN_UpdateFields(int AvnID, User U, ref List<AVNField> fields) {
            for (int i = 0; i < fields.Count; i++) {

                sql.commandText = "AdminAVN_UpdateField";

                if (fields[i].Id > 0) sql.parameters.AddWithValue("@id", fields[i].Id);
                sql.parameters.AddWithValue("@AvnId", AvnID);
                sql.parameters.AddWithValue("@DatabaseColumn", fields[i].DatabaseColumn);

                sql.parameters.AddWithValue("@x", fields[i].X);
                sql.parameters.AddWithValue("@y", fields[i].Y);
                sql.parameters.AddWithValue("@width", fields[i].Width);
                sql.parameters.AddWithValue("@height", fields[i].Height);

                sql.parameters.AddWithValue("@fieldType", fields[i].FieldType);
                sql.parameters.AddWithValue("@RequiredState", fields[i].RequiredState);

                sql.parameters.AddWithValue("@tabIndex", TypeCast.ToInt(fields[i].TabIndex));
                sql.parameters.AddWithValue("@alternateText", TypeCast.ToString(fields[i].AlternateText));

                sql.parameters.AddWithValue("@OuterCSS", fields[i].OuterCSS);
                sql.parameters.AddWithValue("@InnerCSS", fields[i].InnerCSS);

                sql.parameters.AddWithValue("@Icon", fields[i].Icon);
                sql.parameters.AddWithValue("@StatisticsType", fields[i].Statistics);
                sql.parameters.AddWithValue("@ListIndex", fields[i].ListIndex);
                sql.parameters.AddWithValue("@ListWidth", fields[i].ListWidth);

                sql.parameters.AddWithValue("@DataClassificationId", fields[i].DataClassificationId);
                sql.parameters.AddWithValue("@AnonymizationId", fields[i].AnonymizationId);

                System.Data.SqlClient.SqlDataReader dr = sql.executeReader;

                if (dr.Read()) {
                    fields[i].Id = TypeCast.ToInt(dr["id"]);
                }
                dr.Close();
                dr = null;
                sql.reset();

                if ((fields[i].FieldType == "dropdown" || fields[i].FieldType == "listview" || fields[i].FieldType == "map")) {
                    int c = 0;
                    foreach (string s in fields[i].CustomFieldValues) {
                        if ((s != "" && s != null) || c < fields[i].CustomFieldValues.Count - 1 || c == 0) {
                            this.AvnFieldId_addCustomValue(fields[i].Id, s, false);
                        }
                        c++;
                    }
                } else if (fields[i].FieldType == "button" || fields[i].FieldType == "linkbutton" || fields[i].FieldType == "sqllabel") {
                    foreach (string s in fields[i].CustomFieldValues) {
                        this.AvnFieldId_addCustomValue(fields[i].Id, s, true);
                    }
                }
            }
        }

        public void AvnFieldId_addCustomValue(int AvnFieldId, string value, bool IgnoreDoubles) {
            sql.commandType = System.Data.CommandType.StoredProcedure;
            sql.commandText = "AVNFields_addCustomValue" + (IgnoreDoubles ? "IgnoreDouble" : "");
            sql.parameters.AddWithValue("@AvnFieldId", AvnFieldId);
            sql.parameters.AddWithValue("@value", value);
            sql.execute();
            sql.reset();
        }

        public List<string> AVNFields_getCustomValues(int AVNFieldId) {
            List<string> retval = new List<string>();
            sql.commandText = "AVNFields_getCustomValues";
            sql.parameters.AddWithValue("@AVNFieldId", AVNFieldId);
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(TypeCast.ToString(dr["value"]));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        /// <summary>
        /// Get AVN Fields information
        /// </summary>
        /// <param name="AvnID"></param>
        /// <returns></returns>
        public List<AVNField> AVN_GetFields(int AvnID) {
            var fields = new List<AVNField>();
            sql.commandText = "AdminAVN_GetFields";
            sql.parameters.AddWithValue("@AvnID", AvnID);
            SqlDataReader dr = sql.executeReader;
            while (dr.Read()) { fields.Add(new AVNField(ref dr)); }
            dr.Close();
            sql.reset();
            return fields;
        }

        public void AdminAVN_DropField(int AvnID, int Id) {
            sql.commandText = "AdminAVN_DropField";
            sql.parameters.AddWithValue("@AvnID", AvnID);
            sql.parameters.AddWithValue("@Id", Id);
            sql.execute();
            sql.reset();
        }

        /// <summary>
        /// Add AVNEntitySharedCounty, ESCRM-11/183
        /// </summary>
        /// <param name="entityId">entityId/param>
        /// <param name="contactId">contactId</param>
        /// <param name="sharingLevel">sharingLevel</param>
        public void Add_AVNEntitySharedCounty(int entityId, int contactId, int companyId, int AvnId, int sharingLevel)
        {
            //** Hvis vi kun har contactId skal vi finde companyId
            if (contactId > 0 && companyId == 0)
            {
                companyId = ContactGetCompanyId(contactId);
            }

            sql.commandText = "Add_AVNEntitySharedCounty";
            sql.parameters.AddWithValue("@EntityId", entityId);
            sql.parameters.AddWithValue("@contactId", contactId);
            sql.parameters.AddWithValue("@companyId", companyId);
            sql.parameters.AddWithValue("@sharingLevel", sharingLevel);
            sql.parameters.AddWithValue("@AvnId", AvnId);
            sql.execute();
            sql.reset();
        }

        /// <summary>
        /// Get AVNEntitySharedCounty, ESCRM-11/183
        /// </summary>
        /// <param name="entityId">entityId</param>
        /// <param name="avnEntityId">avnEntityId</param>
        public int Get_AVNEntitySharedCounty(int entityId, int avnEntityId, int avnId)
        {
            int retval = -1;

            sql.commandText = "Get_AVNEntitySharedCounty";
            sql.parameters.AddWithValue("@EntityId", entityId);
            sql.parameters.AddWithValue("@avnEntityId", avnEntityId);
            sql.parameters.AddWithValue("@avnId", avnId);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read())
            {
                retval = TypeCast.ToInt(dr["SharingLevel"]);
            }

            dr.Close();
            sql.reset();

            return retval;
        }

        /// <summary>
        /// Scalar Function : AVN_GetAccessLevelByUserId, ESCRM-220/221
        /// </summary>
        /// <param name="U"></param>
        /// <returns></returns>
        public int AVN_GetAccessLevelByUserId(User U, int AvnEntityId, int AvnId, int EntityId)
        {
            int retval = 0;

            try
            {
                using (var connection = new SqlConnection(sql.ConnectionString))
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = "SELECT dbo.AVN_GetAccessLevelByUserId(@userId,@avnentityid,@avnid,@entityid) AS AvnAccess";

                    command.Parameters.AddWithValue("@userId", U.Id);
                    command.Parameters.AddWithValue("@avnentityid", AvnEntityId);
                    command.Parameters.AddWithValue("@avnid", AvnId);
                    command.Parameters.AddWithValue("@entityid", EntityId);

                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        retval = int.Parse(reader["AvnAccess"].ToString()); // Remember Type Casting is required here it has to be according to database column data type

                    }
                    reader.Close();
                    command.Dispose();
                    connection.Close();
                }
            }
            catch(Exception exp)
            {
                string error = exp.Message;
                retval = 0;
            }

            return retval;
        }
        #endregion

        #region UserGroups
        /// <summary>
        /// Create / Update an Usergroup
        /// </summary>
        /// <param name="U">Updating/Creating User Identity</param>
        /// <param name="g">Usergroup to create/update</param>
        /// <returns>The Id for the group</returns>
        public int UserGroups_Update(User U, Usergroup g, string[] UsersInGroup, string[] UsersNotInGroup) {
            int retval = 0;

            sql.commandText = "UserGroups_Update";
            if (g.Id > 0) sql.parameters.AddWithValue("@Id", g.Id);
            sql.parameters.AddWithValue("@OrganisationId", U.OrganisationId);
            sql.parameters.AddWithValue("@UserId", U.Id);
            sql.parameters.AddWithValue("@Name", g.Name);
            sql.parameters.AddWithValue("@Description", g.Description);
            sql.parameters.AddWithValue("@Icon", g.Icon);
            sql.parameters.AddWithValue("@SharedWith", g.SharedWith);
            sql.parameters.AddWithValue("@Internal", g.Internal);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;

            if (dr.Read()) {
                retval = TypeCast.ToInt(dr["id"]);
            }

            dr.Close();
            sql.reset();

            //Add sharing
            if (g.SharedWith == 0 && g.SharedWithOrganisations != null) {
                foreach (int i in g.SharedWithOrganisations) {
                    UserGroups_UpdateShare(retval, i);
                }
            }

            //Remove any users
            foreach (string u in UsersNotInGroup) {
                if (u != "" && u != null)
                    UserGroups_RemoveUser(retval, u);
            }

            //Add any users
            foreach (string u in UsersInGroup) {
                if (u != "" && u != null)
                    UserGroups_AddUser(U, retval, u);
            }

            return retval;
        }

        /// <summary>
        /// Gets all available Usergrups for a given user
        /// </summary>
        /// <param name="U">User</param>
        /// <returns>Array of UserGroups</returns>
        public List<Usergroup> UserGroups_GetGroups(User U) {
            sql.commandText = "UserGroups_GetGroups";
            sql.parameters.AddWithValue("@UserId", U.Id);
            sql.parameters.AddWithValue("@organisationId", U.OrganisationId);

            List<Usergroup> retval = new List<Usergroup>();

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new Usergroup(ref dr));
            }

            dr.Close();
            sql.reset();

            return retval;
        }

        /// <summary>
        /// Get a single usergroup from Id
        /// </summary>
        /// <param name="U">User identity loading</param>
        /// <param name="Id">Group Id</param>
        /// <returns>Usergroup</returns>
        public Usergroup UserGroups_GetGroup(User U, int Id) {
            sql.commandText = "UserGroups_GetGroup";
            sql.parameters.AddWithValue("@UserId", U.Id);
            sql.parameters.AddWithValue("@organisationId", U.OrganisationId);
            sql.parameters.AddWithValue("@Id", Id);

            Usergroup retval = null;

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = new Usergroup(ref dr);
            }

            dr.Close();
            sql.reset();

            return retval;
        }

        /// <summary>
        /// Update Usergroup Sharing amonst Organisations
        /// </summary>
        /// <param name="UserGroupId">UserGroup Id To Share</param>
        /// <param name="SharedWithOrganisationId">Share with Organisation</param>
        public void UserGroups_UpdateShare(int UserGroupId, int SharedWithOrganisationId) {
            sql.commandText = "UserGroups_UpdateShare";
            sql.parameters.AddWithValue("@UserGroupId", UserGroupId);
            sql.parameters.AddWithValue("@SharedWithOrganisationId", SharedWithOrganisationId);
            sql.execute();
            sql.reset();
        }

        /// <summary>
        /// Deletes an usergroup
        /// </summary>
        /// <param name="U"></param>
        /// <param name="UserGroupId"></param>
        public void UserGroups_Delete(User U, int UserGroupId) {
            sql.commandText = "UserGroups_Delete";
            sql.parameters.AddWithValue("@Id", UserGroupId);
            sql.parameters.AddWithValue("@UserId", U.Id);
            sql.parameters.AddWithValue("@OrganisationId", U.OrganisationId);
            sql.execute();
            sql.reset();
        }

        /// <summary>
        /// Add a user the an UserGroup
        /// </summary>
        /// <param name="U"></param>
        /// <param name="UserGroupId"></param>
        /// <param name="UserId"></param>
        public void UserGroups_AddUser(User U, int UserGroupId, string UserId) {
            sql.commandText = "UserGroups_AddUser";
            sql.parameters.AddWithValue("@UserGroupId", UserGroupId);
            sql.parameters.AddWithValue("@UserId", UserId);
            sql.parameters.AddWithValue("@AddedBy", U.Id);
            sql.execute();
            sql.reset();
        }

        /// <summary>
        /// Remove a user from an UserGroup
        /// </summary>
        /// <param name="UserGroupId"></param>
        /// <param name="UserId"></param>
        public void UserGroups_RemoveUser(int UserGroupId, string UserId) {
            sql.commandText = "UserGroups_RemoveUser";
            sql.parameters.AddWithValue("@UserGroupId", UserGroupId);
            sql.parameters.AddWithValue("@UserId", UserId);
            sql.execute();
            sql.reset();
        }

        /// <summary>
        /// Gets users attached to an UserGroup
        /// </summary>
        /// <param name="UserGroupId"></param>
        /// <returns></returns>
        public List<UserWithOrganisation> UserGroups_GetUsers(int UserGroupId) {
            sql.commandText = "UserGroups_GetUsers";
            sql.parameters.AddWithValue("@UserGroupId", UserGroupId);

            List<UserWithOrganisation> retval = new List<UserWithOrganisation>();

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new UserWithOrganisation(ref dr));
            }

            dr.Close();
            sql.reset();

            return retval;
        }

        /// <summary>
        /// Gets Usergroups bound / related to a given user
        /// </summary>
        /// <param name="U"></param>
        /// <param name="LoggedInOrganisationId"></param>
        /// <returns></returns>
        public List<UserGroupBoundToUser> UserGroups_GetGroupsBoundToUser(User U, int LoggedInOrganisationId) {
            return UserGroups_GetGroupsBoundToUser(U.Id, U.OrganisationId, LoggedInOrganisationId);
        }

        /// <summary>
        /// Gets Usergroups bound / related to a given user
        /// </summary>
        /// <param name="UserId"></param>
        /// <param name="OrganisationId"></param>
        /// <param name="LoggedInOrganisationId"></param>
        /// <returns></returns>
        public List<UserGroupBoundToUser> UserGroups_GetGroupsBoundToUser(string UserId, int OrganisationId, int LoggedInOrganisationId) {
            sql.commandText = "UserGroups_GetGroupsBoundToUser";
            if (UserId != null && UserId != "") sql.parameters.AddWithValue("@userId", UserId);
            sql.parameters.AddWithValue("@organisationId", OrganisationId);
            sql.parameters.AddWithValue("@LoggedInOrganisationId", LoggedInOrganisationId);

            List<UserGroupBoundToUser> retval = new List<UserGroupBoundToUser>();

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new UserGroupBoundToUser(ref dr));
            }

            dr.Close();
            sql.reset();

            return retval;
        }
        #endregion

        #region UserGroups Custom Rights

        public List<Usergroup> UsergroupsCustomRightsGetGroups(int customRightId) {
            sql.commandText = "UsergroupsCustomRightsGetGroups";
            sql.parameters.AddWithValue("@customRightId", customRightId);

            List<Usergroup> retval = new List<Usergroup>();

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new Usergroup(ref dr));
            }

            dr.Close();
            sql.reset();

            return retval;
        }

        public void UsergrouosCustomRightsDeleteGroup(int customRightId, int usergroupId) {
            sql.commandText = "UsergrouosCustomRightsDeleteGroup";
            sql.parameters.AddWithValue("@customRightId", customRightId);
            sql.parameters.AddWithValue("@usergroupId", usergroupId);
            sql.execute();
            sql.reset();
        }
        public void usergroupsCustomRightsAddUserGroup(int customRightId, int usergroupId) {
            sql.commandText = "usergroupsCustomRightsAddUserGroup";
            sql.parameters.AddWithValue("@customRightId", customRightId);
            sql.parameters.AddWithValue("@usergroupId", usergroupId);
            sql.execute();
            sql.reset();
        }

        public List<UsergroupCustomRight> User_GetCustomRightsMembership(User U) {
            List<UsergroupCustomRight> retval = new List<UsergroupCustomRight>();

            sql.commandText = "User_GetCustomRightsMembership";
            sql.parameters.AddWithValue("@UserId", U.Id);
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new UsergroupCustomRight(ref dr));
            }

            dr.Close();
            sql.reset();

            return retval;
        }

        /// <summary>
        /// Gets all available Usergrups for a given user
        /// </summary>
        /// <param name="U">User</param>
        /// <returns>Array of UserGroups</returns>
        public List<UsergroupCustomRight> UsergroupsCustomRights_GetAll(User U) {
            List<UsergroupCustomRight> retval = new List<UsergroupCustomRight>();
            sql.commandText = "UsergroupsCustomRights_GetAll";
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new UsergroupCustomRight(ref dr));
            }

            dr.Close();
            sql.reset();

            return retval;
        }

        #endregion

        #region Companies
        public CompanyTransferInformation companies_IsTransferred(int CompanyId, int organisationId, string userId) {
            CompanyTransferInformation retval = null;
            sql.commandText = "companies_IsTransferred";
            sql.parameters.AddWithValue("@CompanyId", CompanyId);
            sql.parameters.AddWithValue("@organisationId", organisationId);
            sql.parameters.AddWithValue("@userId", userId);
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = new CompanyTransferInformation(ref dr);
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public string CompanyGetCVR(int CompanyId, int organisationId) {
            string retval = "";
            sql.commandText = "z_Companies_GetCVR_" + organisationId;
            sql.parameters.AddWithValue("@CompanyId", CompanyId);
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = TypeCast.ToString(dr[1]);
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public List<string> ContactGetCompanyAndContactName(int ContactId, int organisationId) {
            List<string> retval = new List<string>();

            sql.commandText = "z_Contacts_GetName_" + organisationId;
            sql.parameters.AddWithValue("@ContactId", ContactId);
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;

            if (dr.Read()) {
                retval.Add(TypeCast.ToString(dr[1]) + " " + TypeCast.ToString(dr[2]));
                try {
                    retval.Add(TypeCast.ToString(dr[3]));
                } catch {
                    retval.Add("");
                }
            }

            dr.Close();
            sql.reset();

            return retval;
        }

        public string ContactGetName(int ContactId, int organisationId) {
            string retval = "";
            sql.commandText = "z_Contacts_GetName_" + organisationId;
            sql.parameters.AddWithValue("@ContactId", ContactId);
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = TypeCast.ToString(dr[1]) + " " + TypeCast.ToString(dr[2]);
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public string CompanyGetName(int CompanyId, int organisationId) {
            string retval = "";
            sql.commandText = "z_Companies_GetName_" + organisationId;
            sql.parameters.AddWithValue("@CompanyId", CompanyId);
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = TypeCast.ToString(dr[1]);
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public class CompanyNameAndId {
            public int Id;
            public string Name;

            public CompanyNameAndId(int id, string name) {
                Id = id;
                Name = name;
            }
        }

        public CompanyNameAndId CompanyGetNameFromEmail(string email, int organisationId) {
            CompanyNameAndId retval = null;
            sql.commandText = "z_Companies_GetNameFromEmail_" + organisationId;
            sql.parameters.AddWithValue("@email", email);
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = new CompanyNameAndId(TypeCast.ToInt(dr[0]), TypeCast.ToString(dr[1]));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public struct ExistingCompanyAndContactInfo {
            public int CompanyId;
            public int ContactId;

            public ExistingCompanyAndContactInfo(ref System.Data.SqlClient.SqlDataReader dr) {
                CompanyId = TypeCast.ToInt(dr["CompanyOwnerId"]);
                ContactId = TypeCast.ToInt(dr["ContactId"]);
            }
        }

        public void ImportLogAdd(int CompanyId, int ContactId, string ImportedBy) {
            sql.commandText = "ImportLogAdd";
            sql.parameters.AddWithValue("@CompanyId", CompanyId);
            sql.parameters.AddWithValue("@ContactId", ContactId);
            sql.parameters.AddWithValue("@ImportedBy", ImportedBy);

            sql.execute();

            sql.reset();
        }

        public ExistingCompanyAndContactInfo CompanyOrContactExists(Organisation O, string cvr, string cpr, string pnr, int notThisComanyId, int notThisContactId) {
            ExistingCompanyAndContactInfo retval = new ExistingCompanyAndContactInfo();
            sql.commandText = "CompanyOrContactExists"; // +O.Id;
            if (cvr != "") sql.parameters.AddWithValue("@cvr", cvr);
            if (cpr != "") sql.parameters.AddWithValue("@cpr", cpr);
            if (pnr != "") sql.parameters.AddWithValue("@pnr", pnr);
            if (notThisComanyId > 0) sql.parameters.AddWithValue("@notThisCompanyId", notThisComanyId);
            if (notThisContactId > 0) sql.parameters.AddWithValue("@notThisContactId", notThisContactId);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = new ExistingCompanyAndContactInfo(ref dr);
            }
            dr.Close();
            sql.reset();

            return retval;
        } //'120465-1236',null

        public lastUpdatedBy contacts_getLastUpdatedOrCreated(int companyId) {
            lastUpdatedBy retval = null;
            sql.commandText = "contacts_getLastUpdatedOrCreated";
            sql.parameters.AddWithValue("@companyId", companyId);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = new lastUpdatedBy(ref dr);
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public List<Company> Companies_QuickSearch(int organisationId, string userId, string query, int type, int searchIn, string SortOrder, string sortAsc) {
            List<Company> retval = new List<Company>();
            sql.commandText = "z_companies_quickSearch_" + organisationId;
            sql.parameters.AddWithValue("@organisationId", organisationId);
            sql.parameters.AddWithValue("@userId", userId);

            if (type > -1) sql.parameters.AddWithValue("@type", type);
            sql.parameters.AddWithValue("@searchIn", searchIn);

            sql.parameters.AddWithValue("@query", query);

            if (SortOrder != "" && SortOrder != null) sql.parameters.AddWithValue("@SortOrder", SortOrder);
            if (sortAsc != "" && sortAsc != null) sql.parameters.AddWithValue("@sortAsc", sortAsc);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new Company(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public List<Company> Companies_LocateDoubles(int organisationId, string userId, string query, int type, int notThisId) {
            List<Company> retval = new List<Company>();
            sql.commandText = "z_companies_LocateDoubles_" + organisationId;
            sql.parameters.AddWithValue("@organisationId", organisationId);
            sql.parameters.AddWithValue("@userId", userId);

            sql.parameters.AddWithValue("@query", query);
            sql.parameters.AddWithValue("@type", type);

            if (notThisId > 0) sql.parameters.AddWithValue("@notThisId", notThisId);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new Company(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public List<Company> Companies_GetLooseCompanies(string userId) {
            List<Company> retval = new List<Company>();
            sql.commandText = "Companies_GetLooseCompanies";
            sql.commandType = System.Data.CommandType.StoredProcedure;
            sql.parameters.AddWithValue("@userId", userId);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new Company(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public List<Contact> Contacts_getLooseContacts(string userId) {
            List<Contact> retval = new List<Contact>();
            sql.commandText = "Contacts_getLooseContacts";
            sql.commandType = System.Data.CommandType.StoredProcedure;
            sql.parameters.AddWithValue("@userId", userId);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new Contact(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        // Get all companies for this org, except
        // - in staging
        // - deleted
        // - with companytype != 0
        // - with pnumber != null
        // Just a few companies are returned, actually.
        public List<Company> Companies_GetAll(int organisationId) {
            List<Company> retval = new List<Company>();
            sql.commandText = "SELECT * FROM [dbo].[Companies_" + organisationId + "]() as Companies where companytype=0 and companies.[z_companies_1_P-nummer_1] is null"; // and Companies.[z_companies_1_CVR-nummer_1]='26801656'";
            sql.commandType = System.Data.CommandType.Text;

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new Company(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        // Get all companies for this org, except in staging or deleted.
        public List<Company> Companies_GetAll_ReallyAll_Almost(int organisationId) {
            List<Company> retval = new List<Company>();
            sql.commandText = "SELECT * FROM [dbo].[Companies_" + organisationId + "]() as Companies";
            sql.commandType = System.Data.CommandType.Text;

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new Company(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public void UpdateCompanyPnumber(int id, string pnr) {
            sql.commandText = "update [dbo].[Companies_1]() set [z_companies_1_Land_1]='Denmark', [z_companies_1_P-Nummer_1]='" + pnr + "' where companyId=" + id; // and Companies.[z_companies_1_CVR-nummer_1]='26801656'";
            sql.commandType = System.Data.CommandType.Text;
            sql.execute();
            sql.reset();
            sql.commandType = System.Data.CommandType.StoredProcedure;
        }

        public List<int> companies_GetCompaniesWithSamePNR(int OrganisationId, int CompanyId, string pnr) {
            List<int> retval = new List<int>();
            sql.commandText = "z_GetCompaniesWithSamePNR_" + OrganisationId;
            if (CompanyId > 0) sql.parameters.AddWithValue("@companyId", CompanyId);
            sql.parameters.AddWithValue("@pnr", pnr);
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(TypeCast.ToInt(dr["companyId"]));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        //** Ny metode, ESCRM-190/191
        public List<int> companies_GetCompaniesWithSameCVR_PNR(int OrganisationId, int CompanyId, string pnr, string cvr)
        {
            List<int> retval = new List<int>();
            sql.commandText = "GetCompaniesWithSameCVR_PNR";
            if (CompanyId > 0) sql.parameters.AddWithValue("@companyId", CompanyId);
            sql.parameters.AddWithValue("@pnr", pnr);
            sql.parameters.AddWithValue("@cvr", cvr);
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read())
            {
                retval.Add(TypeCast.ToInt(dr["companyId"]));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public List<int> companies_GetCompaniesWithSameCVR(int OrganisationId, int CompanyId) {
            List<int> retval = new List<int>();
            sql.commandText = "z_GetCompaniesWithSameCVR_" + OrganisationId;
            sql.parameters.AddWithValue("@companyId", CompanyId);
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(TypeCast.ToInt(dr["companyId"]));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public List<int> contacts_GetContactsWithSameCPR(int OrganisationId, int ContactId, string cpr) {
            List<int> retval = new List<int>();
            sql.commandText = "z_GetContactsWithSameCPR_" + OrganisationId;
            sql.parameters.AddWithValue("@contactId", ContactId);
            sql.parameters.AddWithValue("@cpr", TypeCast.FixCPR(cpr));
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(TypeCast.ToInt(dr["contactId"]));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public List<int> GetCompaniesWithSamePNRAndCVR(string PNR, string CVR) {
            List<int> retval = new List<int>();
            sql.commandText = "GetCompaniesWithSamePNRAndCPR";
            sql.parameters.AddWithValue("@PNR", PNR);
            sql.parameters.AddWithValue("@CVR", CVR);
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(TypeCast.ToInt(dr["CompanyId"]));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public List<int> GetCompaniesWithSamePNR(string PNR) {
            List<int> retval = new List<int>();
            sql.commandText = "GetCompaniesWithSamePNR";
            sql.parameters.AddWithValue("@PNR", PNR);
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(TypeCast.ToInt(dr["CompanyId"]));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public List<int> GetCompaniesWithSameCVR(string CVR) {
            List<int> retval = new List<int>();
            sql.commandText = "GetCompaniesWithSameCVR";
            sql.parameters.AddWithValue("@CVR", CVR);
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(TypeCast.ToInt(dr["CompanyId"]));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public List<int> GetContactsWithSameEmail(string Email) {
            return GetContactsWithSameEmail(0, Email);
        }

        public List<int> GetContactsWithSameEmail(int CompanyId, string Email) {
            List<int> retval = new List<int>();
            sql.commandText = "GetContactsWithSameEmail";
            if (CompanyId > 0) sql.parameters.AddWithValue("@CompanyId", CompanyId);
            sql.parameters.AddWithValue("@Email", Email);
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(TypeCast.ToInt(dr["ContactId"]));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public List<int> GetCompaniesWithSameEmail(string Email) {
            return GetCompaniesWithSameEmail(0, Email);
        }

        public List<int> GetCompaniesWithSameEmail(int CompanyId, string Email) {
            List<int> retval = new List<int>();
            sql.commandText = "GetCompaniesWithSameEmail";
            if (CompanyId > 0) sql.parameters.AddWithValue("@CompanyId", CompanyId);
            sql.parameters.AddWithValue("@Email", Email);
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(TypeCast.ToInt(dr["CompanyId"]));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public bool Company_CanBeDeleted(int companyId) {
            bool retval = true;
            sql.commandText = "Company_CanBeDeleted";
            sql.parameters.AddWithValue("@companyId", companyId);
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = TypeCast.ToBool(dr["CanDelete"]);
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public Company Company_Get(int organisationId, int companyId, User user, bool obfuscate = true) {

            Company retval = null;
            sql.commandText = "Company_Get";
            sql.parameters.AddWithValue("@UserId", user.Id);
            sql.parameters.AddWithValue("@companyId", companyId);
            sql.parameters.AddWithValue("@OrganisationId", organisationId);
            sql.parameters.AddWithValue("@Obfuscate", obfuscate);
            if (System.Web.HttpContext.Current != null && System.Web.HttpContext.Current.Request != null) {
                var ip = System.Web.HttpContext.Current.Request.UserHostAddress;
                sql.parameters.AddWithValue("@IPaddress", ip);
            }
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = new Company(ref dr);
            }
            dr.Close();
            sql.reset();
            return retval;
        }

        public string Company_Get(User user, int companyId, string fieldname) {
            var result = string.Empty;
            sql.commandText = "Company_Get";
            sql.parameters.AddWithValue("@UserId", user.Id);
            sql.parameters.AddWithValue("@companyId", companyId);
            sql.parameters.AddWithValue("@OrganisationId", user.OrganisationId);
            sql.parameters.AddWithValue("@Obfuscate", false);
            sql.parameters.AddWithValue("@FieldName", fieldname);
            if (System.Web.HttpContext.Current != null && System.Web.HttpContext.Current.Request != null) {
                var ip = System.Web.HttpContext.Current.Request.UserHostAddress;
                sql.parameters.AddWithValue("@IPaddress", ip);
            }
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                result = TypeCast.ToString(dr[fieldname]);
            }
            dr.Close();
            sql.reset();
            return result;
        }

        public Company Company_Get_FromStaging(int organisationId, int companyId) {
            Company retval = null;
            sql.commandText = "z_companies_get_FromStaging_" + organisationId;
            sql.parameters.AddWithValue("@companyId", companyId);
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = new Company(ref dr);
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public void company_abandon(int companyId, string userId) {
            sql.commandText = "company_abandon";
            sql.parameters.AddWithValue("@companyId", companyId);
            sql.parameters.AddWithValue("@userId", userId);

            sql.execute();

            sql.reset();
        }

        public void company_unabandon(int companyId, string userId) {
            sql.commandText = "company_unabandon";
            sql.parameters.AddWithValue("@companyId", companyId);
            sql.parameters.AddWithValue("@userId", userId);

            sql.execute();

            sql.reset();
        }

        public void company_delete(int companyId, User U) {
            sql.commandText = "company_delete";
            sql.parameters.AddWithValue("@companyId", companyId);
            sql.parameters.AddWithValue("@userId", U.Id);
            sql.parameters.AddWithValue("@OrganisationId", U.OrganisationId);
            if (System.Web.HttpContext.Current != null && System.Web.HttpContext.Current.Request != null) {
                var ip = System.Web.HttpContext.Current.Request.UserHostAddress;
                sql.parameters.AddWithValue("@IPaddress", ip);
            }
            sql.execute();

            sql.reset();
        }

        public void companies_convert(User U, Company company) {
            sql.commandText = "companies_convert";
            sql.parameters.AddWithValue("@CompanyId", company.Id);
            sql.parameters.AddWithValue("@state", company.Type);

            sql.execute();

            sql.reset();

            Events_addToEventLog(U.OrganisationId, U.Id, "UPDATE", "COMPANIES", company.Id.ToString(), company);

        }

        /// <summary>
        /// Company_Update also puts the company into staging.  Use companies_verifyStructure to move the company out of staging.
        /// Note: company.CreatedById must be set by the caller to the user responsible for the update, before running Company_Update.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="company"></param>
        /// <returns></returns>
        public int Company_Update(User user, Company company) {

            // FETCH OLD VALUES 
            var c = Company_Get(user.OrganisationId, company.Id, user);
            if (c == null) c = Company_Get_FromStaging(user.OrganisationId, company.Id);
            var classifications = GetDataClassifications();

            int returnValue = 0;
            try {
                var fieldXmlValue = "<FieldValues>";
                foreach (TableColumnWithValue t in company.DynamicTblColumns) {
                    if (t.DynamicField != null) {
                        fieldXmlValue = fieldXmlValue + "<Field>";
                        fieldXmlValue = fieldXmlValue + "<TableName>" + SecurityElement.Escape(t.DynamicField.DatabaseTable) + "</TableName>";
                        fieldXmlValue = fieldXmlValue + "<FieldId>" + SecurityElement.Escape(t.DynamicField.Id.ToString()) + "</FieldId>";
                        fieldXmlValue = fieldXmlValue + "<Value>" + SecurityElement.Escape(Convert.ToString(t.Value)) + "</Value>";

                        var anyUpdate = false;
                        if (c != null && c.DynamicTblColumns != null) {

                            string[] tmp = t.DynamicField.DatabaseTable.Split('_');
                            int ownerId = TypeCast.ToInt(tmp[tmp.Length - 1]);

                            var olddynamicfield = c.DynamicTblColumns.FirstOrDefault(w => w.Name == (t.DynamicField.DatabaseTable + "_" + t.DynamicField.DatabaseColumn + "_" + ownerId));
                            if (olddynamicfield != null) {

                                var oldvalue = Convert.ToString(olddynamicfield.Value);
                                var newvalue = Convert.ToString(t.Value);

                                // Don't make update when data is obfuscated
                                if (newvalue.Contains("****")) {
                                    anyUpdate = false;
                                } else {
                                    if (olddynamicfield.DataType == "bit") {
                                        if (newvalue == "1" || string.Equals(newvalue, "true", StringComparison.CurrentCultureIgnoreCase)) { newvalue = "true"; } else { newvalue = "false"; }
                                    }
                                    //if (newvalue.ToLower() != oldvalue.ToLower()) {
                                    if (!string.Equals(newvalue, oldvalue, StringComparison.Ordinal)) {
                                        fieldXmlValue = fieldXmlValue + "<AnyUpdate>1</AnyUpdate>";
                                        anyUpdate = true;
                                    }
                                }
                            }

                            // WHEN NO UPDATE
                            if (!anyUpdate) {
                                fieldXmlValue = fieldXmlValue + "<AnyUpdate>0</AnyUpdate>";
                            }
                        } else {
                            fieldXmlValue = fieldXmlValue + "<AnyUpdate>0</AnyUpdate>";
                        }

                        fieldXmlValue = fieldXmlValue + "</Field>";
                    }
                }
                fieldXmlValue = fieldXmlValue + "</FieldValues>";

                var sqlConnection = new SqlConnection(connectionString);
                using (var cmd = new SqlCommand("Company_Update", sqlConnection)) {
                    cmd.Parameters.AddWithValue("@UserId", user.Id);
                    cmd.Parameters.AddWithValue("@OrganisationId", user.OrganisationId);
                    cmd.Parameters.AddWithValue("@CompanyId", TypeCast.ToIntOrDBNull(company.Id));
                    cmd.Parameters.AddWithValue("@CompanyType", company.Type);
                    cmd.Parameters.AddWithValue("@FieldValues", fieldXmlValue);
                    cmd.Parameters.AddWithValue("@CompanyCreatedById", company.CreatedById);
                    cmd.Parameters.AddWithValue("@CompanyNNEId", company.NNEId);
                    if (System.Web.HttpContext.Current != null && System.Web.HttpContext.Current.Request != null) {
                        var ip = System.Web.HttpContext.Current.Request.UserHostAddress;
                        cmd.Parameters.AddWithValue("@IPaddress", ip);
                    }

                    cmd.CommandType = CommandType.StoredProcedure;
                    if (sqlConnection.State == ConnectionState.Closed) { sqlConnection.Open(); }
                    using (SqlDataReader dr = cmd.ExecuteReader()) {
                        if (dr.HasRows) {
                            while (dr.Read()) {
                                returnValue = TypeCast.ToInt(dr["CompanyId"]);
                            }
                        }
                    }
                }
                if (sqlConnection.State == ConnectionState.Open) {
                    sqlConnection.Close();
                    sqlConnection.Dispose();
                }
            } catch (Exception ex) { throw ex; }

            return returnValue;
        }

        private List<string> Companies_GetUpdateArguments(int OrganisationId) {
            List<string> retval = new List<string>();
            sql.commandText = "Companies_GetUpdateArguments";
            sql.parameters.AddWithValue("@OrganisationId", OrganisationId);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(TypeCast.ToString(dr["PARAMETER_NAME"]).ToLower());
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        /// <summary>
        /// Opdater smv's antal ansatte på CVR nummer i CVR tabellen, ESCRM-127/128. Opdateret ESCRM-11/183, ESCRM-211/213
        /// </summary>
        /// <param name="CVR"></param>
        /// <param name="FirmaNavn"></param>
        /// <param name="AntalAnsatte"></param>
        /// <param name="AntalAarsvaerk"></param>
        /// <param name="Aar"></param>
        /// <returns></returns>
        public bool UpdateNyesteErstMaanedBeskaeftigelseOnCVR(string companyCVR, string companyName, string companyEmployees, string companyManYears, string companyMonth, string companyYear)
        {
            bool result = true;

            try
            {
                sql.commandText = "CVR_Update_NyesteErstMaanedBeskaeftigelse";
                sql.parameters.AddWithValue("@CVR", companyCVR);
                sql.parameters.AddWithValue("@FirmaNavn", companyName);
                sql.parameters.AddWithValue("@AntalAnsatte", (companyEmployees != string.Empty) ? companyEmployees:null);
                sql.parameters.AddWithValue("@AntalAarsvaerk", (companyManYears != string.Empty) ? companyManYears.Replace(".","").Replace(",",".") : null);
                sql.parameters.AddWithValue("@Aar", (companyYear != string.Empty) ? companyYear: null);
                sql.parameters.AddWithValue("@Maaned", (companyMonth != string.Empty) ? companyMonth: null);

                sql.execute();

                sql.reset();
            }
            catch (Exception ex)
            {
                result = false;
            }

            return result;
        }

        /// <summary>
        /// Opdater smv's antal ansatte på CVR nummer, ESCRM-127/128. Opdateret ESCRM-11/183, ESCRM-211/213
        /// </summary>
        /// <param name="CVR"></param>
        /// <param name="P-Nummer"></param>
        /// <param name="AntalAnsatte"></param>
        /// <param name="AntalAarsvaerk"></param>
        /// <param name="Maaned"></param>
        /// <param name="Aar"></param>
        /// <returns></returns>
        public bool UpdateEmployeeNumberOnPNR(string companyCVR, string companyPNR, string Aar, string Maaned, string AntalAarsvaerk, string AntalAnsatte)
        {
            bool result = true;

            try
            {
                sql.commandText = "CVR_PNR_Update_Employee";
                sql.parameters.AddWithValue("@companyCVR", companyCVR);
                sql.parameters.AddWithValue("@companyPNR", companyPNR);
                sql.parameters.AddWithValue("@aar", Aar);
                sql.parameters.AddWithValue("@maaned", Maaned);
                sql.parameters.AddWithValue("@antalaarsvaerk", TypeCast.ToDecimal(AntalAarsvaerk));
                sql.parameters.AddWithValue("@antalansatte", AntalAnsatte);

                sql.execute();

                sql.reset();
            }
            catch (Exception ex)
            {
                result = false;
            }

            return result;
        }

        /// <summary>
        /// Company can abandon
        /// </summary>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public bool Company_CanAbandon(int companyId)
        {
            bool retval = true;
            sql.commandText = "Company_CanAbandon";
            sql.parameters.AddWithValue("@companyId", companyId);
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read())
            {
                //** Hent resultat
                string value = dr["CanAbandon"].ToString();
                if (value == "0")
                    retval = false;
                else
                    retval = true;
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        /// <summary>
        /// Contact get companyId, ESCRM-11/183
        /// </summary>
        /// <param name="ContactId"></param>
        /// <returns></returns>
        public int ContactGetCompanyId(int ContactId)
        {
            int retval = 0;

            sql.commandText = "Contact_GetCompanyId";
            sql.parameters.AddWithValue("@ContactId", ContactId);
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read())
            {
                retval = TypeCast.ToInt(dr["CompanyId"].ToString());
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        /// <summary>
        /// Opdater Company udfra CVR og PNR, ESCRM-211/213
        /// </summary>
        /// <param name="CVR"></param>
        /// <returns></returns>
        public bool CVRCompanyUpdate(int companyCVR)
        {
            bool result = true;

            try
            {
                sql.commandText = "cvr_company_update";
                sql.parameters.AddWithValue("@companyCVR", companyCVR);

                sql.execute();

                sql.reset();
            }
            catch (Exception ex)
            {
                result = false;
            }

            return result;
        }
        #endregion

        #region Contacts
        /// <summary>
        /// SQL : Get first contact from company, ESCRM-122
        /// </summary>
        /// <param name="CompanyId"></param>
        /// <returns></returns>
        public int GetFirstContactFromCompany(int CompanyId)
        {
            int result = 0;

            sql.commandText = "Contact_GetFirstForCompany";
            sql.parameters.AddWithValue("@CompanyId", CompanyId);
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read())
            {
                result = TypeCast.ToInt(dr["ContactId"]);
            }
            dr.Close();
            sql.reset();

            return result;
        }

        public Contact Contacts_GetContactAllOrganisationFields(User U, int ContactId) {
            Contact retval = null;
            sql.commandText = "z_Contacts_GetAllOrganisationFields_" + U.OrganisationId;
            sql.parameters.AddWithValue("@contactId", ContactId);
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = new Contact(ref dr);
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public List<Contact> Contacts_GetContactsFromCompanyAllOrganisationFields(User U, int CompanyId) {
            List<Contact> retval = new List<Contact>();
            sql.commandText = "z_Contacts_GetContactsFromCompanyAllOrganisationFields_" + U.OrganisationId;
            sql.parameters.AddWithValue("@CompanyId", CompanyId);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new Contact(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public List<TinyContactListItem> Contacts_GetContactsWithSameEmail(User U, int ContactId, string Email) {
            List<TinyContactListItem> retval = new List<TinyContactListItem>();
            sql.commandText = "z_GetContactsWithSameEmail_" + U.OrganisationId;
            sql.parameters.AddWithValue("@ContactId", ContactId);
            sql.parameters.AddWithValue("@Email", Email);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new TinyContactListItem(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public int companies_verifyStructure(int CompanyId, int ContactId) {
            int retval = 0;
            sql.commandText = "z_companies_verifyStructure";

            if (ContactId > 0) sql.parameters.AddWithValue("@ContactId", ContactId);
            if (CompanyId > 0) sql.parameters.AddWithValue("@CompanyId", CompanyId);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = TypeCast.ToInt(dr["status"]);
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public void contacts_DeleteTransfer(int ContactId, User U) {
            sql.commandText = "contacts_DeleteTransfer";
            sql.parameters.AddWithValue("@ContactId", ContactId);
            sql.parameters.AddWithValue("@UserId", U.Id);

            sql.execute();

            sql.reset();
        }

        public void contacts_AcceptTransfer(int ContactId, User U, string Reason, string directLinkToContact) {
            string uid = "";

            sql.commandText = "contacts_AcceptTransfer";
            sql.parameters.AddWithValue("@ContactId", ContactId);
            sql.parameters.AddWithValue("@Reason", Reason);
            sql.parameters.AddWithValue("@UserId", U.Id);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                uid = TypeCast.ToString(dr["senderUserId"]);
            }
            dr.Close();

            sql.reset();

            //Send an email to the reciever if its a person
            if (uid != "") {
                User UU = this.Organisations_getUser(uid);

                if (UU != null) {
                    Contact ct = this.Contact_Get(U.OrganisationId, ContactId, U);
                    int CompanyId = ct.CompanyId;


                    string companyName = this.CompanyGetName(CompanyId, U.OrganisationId);
                    string contactName = this.ContactGetName(ContactId, U.OrganisationId);

                    string conn = System.Configuration.ConfigurationManager.AppSettings["connectionString"];
                    string[] args = conn.Split(';');
                    string url = "https://www.escrm.dk";

                    foreach (string a in args) {
                        string[] ag = a.Trim().Split('=');
                        if (ag[0].Trim().ToLower() == "initial catalog") {
                            if (ag[1].Trim().ToUpper() != "ESCRM_DRIFT" && ag[1].Trim().ToUpper() != "EBSTCRM_CRM2") {
                                url = "https://sandkasse.escrm.dk";
                            }
                            break;
                        }
                    }

                    string html = "Kære " + UU.Firstname + ",<br><br>" +
                        "Jeg har modtaget din anvisning af <b><a href=\"" + url + "/?ol=" + directLinkToContact + "\">" + contactName + " / " + companyName + "</a></b>, som jeg har accepteret.<br><br>" +
                        Reason.Replace("\n", "<br>") + "<br><br>" +
                        "Mange hilsner,<br><br>" +
                        U.Firstname + " " + U.Lastname + "<br>" +
                        U.Email + "<br>" +
                        U.Organisation.Name;

                    EmailManager.SendEmail("info@escrm.dk", UU.Email, "Accepteret anvisning i CRM Systemet", html);
                }
            }
        }

        public void contacts_RejectTransfer(int ContactId, User U, string Reason, string directLinkToContact) {
            string uid = "";
            sql.commandText = "contacts_RejectTransfer";
            sql.parameters.AddWithValue("@ContactId", ContactId);
            sql.parameters.AddWithValue("@Reason", Reason);
            sql.parameters.AddWithValue("@UserId", U.Id);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                uid = TypeCast.ToString(dr["senderUserId"]);
            }
            dr.Close();

            sql.reset();

            //Send an email to the reciever if its a person
            if (uid != "") {
                User UU = this.Organisations_getUser(uid);

                if (UU != null) {
                    Contact ct = this.Contact_Get(U.OrganisationId, ContactId, U);
                    int CompanyId = ct.CompanyId;


                    string companyName = this.CompanyGetName(CompanyId, U.OrganisationId);
                    string contactName = this.ContactGetName(ContactId, U.OrganisationId);

                    string conn = System.Configuration.ConfigurationManager.AppSettings["connectionString"];
                    string[] args = conn.Split(';');
                    string url = "https://www.escrm.dk";

                    foreach (string a in args) {
                        string[] ag = a.Trim().Split('=');
                        if (ag[0].Trim().ToLower() == "initial catalog") {
                            if (ag[1].Trim().ToUpper() != "ESCRM_DRIFT" && ag[1].Trim().ToUpper() != "EBSTCRM_CRM2") {
                                url = "https://sandkasse.escrm.dk";
                            }
                            break;
                        }
                    }

                    string html = "Kære " + UU.Firstname + ",<br><br>" +
                        "Jeg har modtaget din anvisning af <b><a href=\"" + url + "/?ol=" + directLinkToContact + "\">" + contactName + " / " + companyName + "</a></b>.<br><br>" +
                        "Jeg er desværre nødsaget til at afvise din anvisning pga.:<br><br>" +
                        Reason.Replace("\n", "<br>") + "<br><br>" +
                        "Mange hilsner,<br><br>" +
                        U.Firstname + " " + U.Lastname + "<br>" +
                        U.Email + "<br>" +
                        U.Organisation.Name;

                    EmailManager.SendEmail("info@escrm.dk", UU.Email, "Afvisning af anvisning i CRM Systemet", html);
                }
            }
        }

        public void companies_AcceptTransfer(int CompanyId, User U, string Reason) {
            sql.commandText = "companies_AcceptTransfer";
            sql.parameters.AddWithValue("@CompanyId", CompanyId);
            sql.parameters.AddWithValue("@Reason", Reason);
            sql.parameters.AddWithValue("@UserId", U.Id);

            sql.execute();

            sql.reset();
        }

        public void Contacts_TransferChangeContact(int OldContactId, int NewContactId) {
            sql.commandText = "Contacts_TransferChangeContact";
            sql.parameters.AddWithValue("@OldContactId", OldContactId);
            sql.parameters.AddWithValue("@NewContactId", NewContactId);

            sql.execute();

            sql.reset();
        }

        public int Contacts_TransferToOrganisation(int ContactId, string SenderUserId, int SenderOrganisationId,
                                            int ReceiverOrganisationId, string ReceiverUserId, string AcceptedByUserId,
                                            string TransferReason, string directLinkToContact, bool transfer) {
            int retval = 0;
            sql.commandText = "Contacts_TransferToOrganisation";

            sql.parameters.AddWithValue("@ContactId", ContactId);
            sql.parameters.AddWithValue("@SenderUserId", SenderUserId);
            sql.parameters.AddWithValue("@SenderOrganisationId", SenderOrganisationId);
            sql.parameters.AddWithValue("@ReceiverOrganisationId", ReceiverOrganisationId);
            if (ReceiverUserId != null && ReceiverUserId != "") sql.parameters.AddWithValue("@ReceiverUserId", ReceiverUserId);
            if (AcceptedByUserId != null && AcceptedByUserId != "") sql.parameters.AddWithValue("@AcceptedByUserId", AcceptedByUserId);
            sql.parameters.AddWithValue("@TransferReason", TransferReason);
            sql.parameters.AddWithValue("@Transfer", transfer);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = TypeCast.ToInt(dr["status"]);
            }
            dr.Close();
            sql.reset();


            if (!transfer) {
                //Send an email to the reciever if its a person
                if (ReceiverUserId != "") {
                    User UU = this.Organisations_getUser(ReceiverUserId);
                    User U = this.Organisations_getUser(SenderUserId);

                    if (UU != null) {
                        Contact ct = this.Contact_Get(SenderOrganisationId, ContactId, U);
                        int CompanyId = ct.CompanyId;

                        string companyName = this.CompanyGetName(CompanyId, U.OrganisationId);
                        string contactName = this.ContactGetName(ContactId, U.OrganisationId);

                        string conn = System.Configuration.ConfigurationManager.AppSettings["connectionString"];
                        string[] args = conn.Split(';');
                        string url = "https://www.escrm.dk";

                        foreach (string a in args) {
                            string[] ag = a.Trim().Split('=');
                            if (ag[0].Trim().ToLower() == "initial catalog") {
                                if (ag[1].Trim().ToUpper() != "ESCRM_DRIFT" && ag[1].Trim().ToUpper() != "EBSTCRM_CRM2") {
                                    url = "https://sandkasse.escrm.dk";
                                }
                                break;
                            }
                        }

                        string html = "Kære " + UU.Firstname + ",<br><br>" +
                            "Jeg har anvist kontaktpersonen <b><a href=\"" + url + "/?ol=" + directLinkToContact + "\">" + contactName + " / " + companyName + "</a></b> til dig.<br><br>" +
                            "Grunden til, at jeg anviser vedkommende til dig er:<br><br>" +
                            TransferReason.Replace("\n", "<br>") + "<br><br>" +
                            "Mange hilsner,<br><br>" +
                            U.Firstname + " " + U.Lastname + "<br>" +
                            U.Email + "<br>" +
                            U.Organisation.Name;

                        EmailManager.SendEmail("info@escrm.dk", UU.Email, U.Email, "Ny anvisning i CRM Systemet", html);
                    }
                }
            }

            return retval;
        }

        //public int companies_transferToOrganisation(int CompanyId, string SenderUserId, int SenderOrganisationId, 
        //                                            int ReceiverOrganisationId, string ReceiverUserId, string AcceptedByUserId, 
        //                                            string TransferReason)
        //{
        //    int retval = 0;
        //    sql.commandText = "companies_transferToOrganisation";

        //    sql.parameters.AddWithValue("@CompanyId", CompanyId);
        //    sql.parameters.AddWithValue("@SenderUserId", SenderUserId);
        //    sql.parameters.AddWithValue("@SenderOrganisationId", SenderOrganisationId);
        //    sql.parameters.AddWithValue("@ReceiverOrganisationId", ReceiverOrganisationId);
        //    if(ReceiverUserId != null && ReceiverUserId != "") sql.parameters.AddWithValue("@ReceiverUserId", ReceiverUserId);
        //    if (AcceptedByUserId != null && AcceptedByUserId != "") sql.parameters.AddWithValue("@AcceptedByUserId", AcceptedByUserId);
        //    sql.parameters.AddWithValue("@TransferReason", TransferReason);

        //    System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
        //    if (dr.Read())
        //    {
        //        retval = TypeCast.ToInt(dr["status"]);
        //    }
        //    dr.Close();
        //    sql.reset();


        //    //Send an email to the reciever if its a person
        //    if (ReceiverUserId != "")
        //    {
        //        User UU = this.Organisations_getUser(ReceiverUserId);
        //        User U = this.Organisations_getUser(SenderUserId);

        //        if (UU != null)
        //        {
        //            string companyName = this.CompanyGetName(CompanyId, U.OrganisationId);

        //            string html = "Kære " + UU.Firstname + ",<br><br>" +
        //                "Jeg har anvist virksomheden <b>" + companyName + "</b> til dig.<br><br>" +
        //                "Grunden til, at jeg anviser den til dig er:<br><br>" +
        //                TransferReason.Replace("\n", "<br>") + "<br><br>" +
        //                "Mange hilsner,<br><br>" +
        //                U.Firstname + " " + U.Lastname + "<br>" +
        //                U.Email + "<br>" +
        //                U.Organisation.Name;

        //            EmailManager.SendEmail(U.Email, UU.Email , "Ny anvisning i CRM Systemet", html);
        //        }
        //    }


        //    return retval;
        //}

        public Contact Contacts_GetContact_ForPartner(User U, int ContactId) {
            Contact retval = null;
            sql.commandText = "z_Contacts_GetContact_ForPartner_" + U.OrganisationId;
            sql.parameters.AddWithValue("@ContactId", ContactId);
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = new Contact(ref dr);
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public List<ContactAndPartnerList> Contacts_Get_Contacts_FromPartner(User U, int PartnerId) {
            List<ContactAndPartnerList> retval = new List<ContactAndPartnerList>();
            sql.commandText = "z_Contacts_Get_Contacts_FromPartner_" + U.OrganisationId;
            sql.parameters.AddWithValue("@PartnerId", PartnerId);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new ContactAndPartnerList(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public List<ContactAndPartnerList> Contacts_Get_Contacts_FromPartner_AllColumns(User U, int PartnerId) {
            List<ContactAndPartnerList> retval = new List<ContactAndPartnerList>();
            sql.commandText = "z_Contacts_Get_Contacts_FromPartner_AllColumns_" + U.OrganisationId;
            sql.parameters.AddWithValue("@PartnerId", PartnerId);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new ContactAndPartnerList(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public List<PartnerContactForList> Contacts_GetPartnerAssociationsForEntireCompany(int CompanyId) {
            List<PartnerContactForList> retval = new List<PartnerContactForList>();
            sql.commandText = "Contacts_GetPartnerAssociationsForEntireCompany";
            sql.parameters.AddWithValue("@CompanyId", CompanyId);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new PartnerContactForList(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public List<PartnerContactForList> Contacts_GetPartnerAssociations(int ContactId) {
            List<PartnerContactForList> retval = new List<PartnerContactForList>();
            sql.commandText = "Contacts_GetPartnerAssociations";
            sql.parameters.AddWithValue("@ContactId", ContactId);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new PartnerContactForList(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public EvaluationInfo contacts_getEvaluationInformation(User U, int CompanyId, int ContactId) {
            EvaluationInfo retval = null;
            sql.commandText = "contacts_get" + (U.Organisation.Type == Organisation.OrganisationType.County ? "Local" : "") + "EvaluationInformation";
            sql.parameters.AddWithValue("@ContactId", ContactId);
            sql.parameters.AddWithValue("@CompanyId", CompanyId);
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = new EvaluationInfo(ref dr);
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public Contact Contacts_isSentToEvaluation(User U, int CompanyId) {
            Contact retval = null;
            sql.commandText = "z_Contacts_isSentTo" + (U.Organisation.Type == Organisation.OrganisationType.County ? "Local" : "") + "Evaluation_" + U.OrganisationId;
            sql.parameters.AddWithValue("@organisationId", U.OrganisationId);
            sql.parameters.AddWithValue("@companyId", CompanyId);
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = new Contact(ref dr);
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public int contacts_sendContactToEvaluation(User U, int ContactId, int CompanyId, string type) {
            int retval = 0;
            sql.commandText = "contacts_sendContactTo" + (U.Organisation.Type == Organisation.OrganisationType.County ? "Local" : "") + "Evaluation";
            sql.parameters.AddWithValue("@organisationId", U.OrganisationId);
            sql.parameters.AddWithValue("@userId", U.Id);
            sql.parameters.AddWithValue("@contactId", ContactId);
            sql.parameters.AddWithValue("@companyId", CompanyId);
            sql.parameters.AddWithValue("@type", type);
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = TypeCast.ToInt(dr["status"]);
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public void contacts_moveToCompany(int oldCompany, int newCompany) {
            contacts_moveToCompany(oldCompany, newCompany, 0);
        }
        public void contacts_moveToCompany(int oldCompany, int newCompany, int ContactId) {
            sql.commandText = "contacts_moveToCompany";
            sql.parameters.AddWithValue("@oldCompany", oldCompany);
            sql.parameters.AddWithValue("@newCompany", newCompany);
            if (ContactId > 0) sql.parameters.AddWithValue("@ContactId", ContactId);

            sql.execute();

            sql.reset();

        }
        public void log_moveToCompany(User U, int oldCompany, int newCompany) {
            sql.commandText = "log_moveToCompany";
            sql.parameters.AddWithValue("@oldCompany", oldCompany);
            sql.parameters.AddWithValue("@newCompany", newCompany);
            sql.parameters.AddWithValue("@userId", U.Id);
            sql.execute();
            sql.reset();
        }
        public void contacts_abandon(User U, Contact C) {
            if (C != null && U != null) {
                sql.commandText = "contacts_abandon";
                sql.parameters.AddWithValue("@ContactId", C.Id);
                sql.parameters.AddWithValue("@CompanyId", C.CompanyId);
                sql.parameters.AddWithValue("@userId", U.Id);

                sql.execute();

                sql.reset();

                Events_addToEventLog(U.OrganisationId, U.Id, "ABANDON", "CONTACTS", C.Id.ToString(), C);
            }

        }

        public void contacts_unabandon(User U, Contact C) {
            sql.commandText = "contacts_unabandon";
            sql.parameters.AddWithValue("@ContactId", C.Id);
            sql.parameters.AddWithValue("@CompanyId", C.CompanyId);
            sql.parameters.AddWithValue("@userId", U.Id);

            sql.execute();

            sql.reset();

            Events_addToEventLog(U.OrganisationId, U.Id, "UNABANDON", "CONTACTS", C.Id.ToString(), C);

        }

        public void contacts_delete(User U, Contact C) {
            sql.commandText = "contacts_delete";
            sql.parameters.AddWithValue("@ContactId", C.Id);
            sql.parameters.AddWithValue("@CompanyId", C.CompanyId);
            sql.parameters.AddWithValue("@userId", U.Id);
            sql.parameters.AddWithValue("@OrganisationId", U.OrganisationId);
            if (System.Web.HttpContext.Current != null && System.Web.HttpContext.Current.Request != null) {
                var ip = System.Web.HttpContext.Current.Request.UserHostAddress;
                sql.parameters.AddWithValue("@IPaddress", ip);
            }
            sql.execute();
            sql.reset();
            Events_addToEventLog(U.OrganisationId, U.Id, "DELETE", "CONTACTS", C.Id.ToString(), C);
        }

        public void contacts_delete(User U, int ContactId, int CompanyId) {
            sql.commandText = "contacts_delete";
            sql.parameters.AddWithValue("@ContactId", ContactId);
            sql.parameters.AddWithValue("@CompanyId", CompanyId);
            sql.parameters.AddWithValue("@userId", U.Id);

            sql.execute();

            sql.reset();

            Events_addToEventLog(U.OrganisationId, U.Id, "DELETE", "CONTACTS", ContactId.ToString(), null);

        }

        public void contacts_bindToCompany(int ContactId, int CompanyId) {
            sql.commandText = "contacts_bindToCompany";
            sql.parameters.AddWithValue("@ContactId", ContactId);
            sql.parameters.AddWithValue("@CompanyId", CompanyId);

            sql.execute();

            sql.reset();
        }

        public List<Contact> Contacts_GetIncompleteContacts(User U) {

            List<Contact> retval = new List<Contact>();
            sql.commandText = "z_Contacts_GetIncompleteContacts_" + U.OrganisationId;
            sql.parameters.AddWithValue("@organisationId", U.OrganisationId);
            sql.parameters.AddWithValue("@UserId", U.Id);

            try {
                System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
                while (dr.Read()) {
                    retval.Add(new Contact(ref dr));
                }
                dr.Close();
            } catch (Exception E) {
                IDS.EBSTCRM.Base.ExceptionMail.SendException(E);
            }


            sql.reset();

            return retval;
        }

        public List<ContactTransferInfo> CompanyContacts_IsTransferred(int CompanyId, int OrganisationId) {

            List<ContactTransferInfo> retval = new List<ContactTransferInfo>();
            sql.commandText = "CompanyContacts_IsTransferred";
            sql.parameters.AddWithValue("@CompanyId", CompanyId);
            sql.parameters.AddWithValue("@OrganisationId", OrganisationId);

            try {
                System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
                while (dr.Read()) {
                    retval.Add(new ContactTransferInfo(ref dr));
                }
                dr.Close();
            } catch (Exception E) {
                IDS.EBSTCRM.Base.ExceptionMail.SendException(E);
            }


            sql.reset();

            return retval;
        }

        public List<ContactTransferred> Contacts_GetTransferred(User U) {

            List<ContactTransferred> retval = new List<ContactTransferred>();
            sql.commandText = "z_Contacts_Transferred_" + U.OrganisationId;
            sql.parameters.AddWithValue("@organisationId", U.OrganisationId);
            sql.parameters.AddWithValue("@userId", U.Id);

            try {
                System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
                while (dr.Read()) {
                    retval.Add(new ContactTransferred(ref dr));
                }
                dr.Close();
            } catch (Exception E) {
                IDS.EBSTCRM.Base.ExceptionMail.SendException(E);
            }


            sql.reset();

            return retval;
        }

        public List<ContactTransferred> Contacts_GetTransferredOut(User U) {

            List<ContactTransferred> retval = new List<ContactTransferred>();
            sql.commandText = "z_Contacts_TransferredOut_" + U.OrganisationId;
            sql.parameters.AddWithValue("@organisationId", U.OrganisationId);
            sql.parameters.AddWithValue("@userId", U.Id);

            try {
                System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
                while (dr.Read()) {
                    retval.Add(new ContactTransferred(ref dr));
                }
                dr.Close();
            } catch (Exception E) {
                IDS.EBSTCRM.Base.ExceptionMail.SendException(E);
            }


            sql.reset();

            return retval;
        }

        public List<ContactRecent> Contacts_GetRecent(User u) {
            //sql.commandText = "z_Contacts_Recent_" + u.OrganisationId;
            //sql.parameters.AddWithValue("@userId", u.Id);

            // Get dynamic columns
            var dfFields = GetDynamicFieldsforOrganisation(u.Organisation, "contacts", true);
            var sqlColumns = new List<SQLColumnItem>();

            foreach (DynamicField f in dfFields) {
                if (TypeCast.ToInt(f.ShowInListview) > 0 && f.OrganisationId == u.Organisation.Id) {
                    string col = "";
                    if (f.DataSource.IndexOf('_') > 0) {
                        col = f.DataSource + "_" + f.DataSource.Split('_')[2];
                    } else {
                        col = f.DatabaseTable + "_" + f.DatabaseColumn + "_" + f.BaseOrganisationId;
                    }

                    //Add the columns desired in a sorted manor!
                    bool inserted = false;
                    int index = f.ListviewIndex;

                    if (sqlColumns.Count == 0) {
                        sqlColumns.Add(new SQLColumnItem(col, f.ListviewIndex));
                    } else {
                        for (int ii = 0; ii < sqlColumns.Count; ii++) {
                            int cpIndex = sqlColumns[ii].ListIndex;
                            if (inserted == false && index < cpIndex) {
                                inserted = true;
                                sqlColumns.Insert(ii > sqlColumns.Count ? sqlColumns.Count : ii, new SQLColumnItem(col, f.ListviewIndex));
                            }
                        }

                        if (inserted == false) { sqlColumns.Add(new SQLColumnItem(col, f.ListviewIndex)); }
                    }
                }
            }

            var recents = new List<ContactRecent>();
            sql.commandText = "Contacts_Recent";
            sql.parameters.AddWithValue("@userId", u.Id);
            sql.parameters.AddWithValue("@OrganisationId", u.OrganisationId);
            sql.parameters.AddWithValue("@DynamicFieldsColumn", SQLColumnItemsToString(sqlColumns));
            try {
                SqlDataReader dr = sql.executeReader;
                while (dr.Read()) { recents.Add(new ContactRecent(ref dr)); }
                dr.Close();
            } catch (Exception E) {
                IDS.EBSTCRM.Base.ExceptionMail.SendException(E);
            }
            sql.reset();
            return recents;
        }

        public List<DynamicField> GetDynamicFieldsforOrganisation(Organisation O, string Type, bool ForListview) {
            if (ForListview) {
                List<DynamicField> fields = dynamicFields_getFieldsForOrganisationFromSQL(O, Type);
                for (int i = fields.Count - 1; i >= 0; i--) {
                    if (fields[i].NoInherit(O.Id) && fields[i].BaseOrganisationId != fields[i].OrganisationId)
                        fields.RemoveAt(i);
                }
                return fields;
            } else
                return dynamicFields_getFieldsForOrganisationFromSQL(O, Type);
        }

        public string SQLColumnItemsToString(List<SQLColumnItem> List) {
            string items = "";
            foreach (SQLColumnItem s in List) {
                if (s.SortingName != "" && s.SortingName != null)
                    items += s.Column;
            }
            return items;
        }

        public List<Contact> Contacts_GetRecentContacts(User U) {

            List<Contact> retval = new List<Contact>();
            sql.commandText = "z_Contacts_GetRecentContacts_" + U.OrganisationId;
            sql.parameters.AddWithValue("@organisationId", U.OrganisationId);
            sql.parameters.AddWithValue("@userId", U.Id);

            try {
                System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
                while (dr.Read()) {
                    retval.Add(new Contact(ref dr));
                }
                dr.Close();
            } catch (Exception E) {
                IDS.EBSTCRM.Base.ExceptionMail.SendException(E);
            }


            sql.reset();

            return retval;
        }

        public List<ContactSendToEvaluationList> Contacts_Get_Contacts_SentToEvaluation_AllColumns_EntireYear(User U, int year, string UserId) {
            List<ContactSendToEvaluationList> retval = new List<ContactSendToEvaluationList>();
            sql.commandText = "z_Contacts_Get_Contacts_SentTo" + (U.Organisation.Type == Organisation.OrganisationType.County ? "Local" : "") + "Evaluation_AllColumns_EntireYear_" + U.OrganisationId;
            sql.parameters.AddWithValue("@year", year);
            if (UserId != "" && UserId != null) sql.parameters.AddWithValue("@UserId", UserId);
            sql.parameters.AddWithValue("@organisationId", U.OrganisationId);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new ContactSendToEvaluationList(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public List<Contact> Contacts_GetContacts_FromCompany(int organisationId, int companyId) {

            //List<Contact> retval = new List<Contact>();
            //sql.commandText = "z_Contacts_Get_Contacts_FromCompany_" + organisationId;
            //sql.parameters.AddWithValue("@companyId", companyId);
            //System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            //while (dr.Read()) {
            //    retval.Add(new Contact(ref dr));
            //}
            //dr.Close();
            //sql.reset();
            //return retval;

            List<Contact> retval = new List<Contact>();
            sql.commandText = "ContactsFromCompany_Get";
            sql.parameters.AddWithValue("@OrganisationId", organisationId);
            sql.parameters.AddWithValue("@CompanyId", companyId);
            SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new Contact(ref dr));
            }
            dr.Close();
            sql.reset();
            return retval;
        }

        public List<Contact> Contacts_QuickSearch_old(int organisationId, string userId, string query, int type, int searchIn, string SortOrder, string sortAsc) {
            return Contacts_QuickSearch_old(organisationId, userId, query, type, searchIn, SortOrder, sortAsc, "", 0);
        }

        public List<Contact> Contacts_QuickSearch_old(int organisationId, string userId, string query, int type, int searchIn, string SortOrder, string sortAsc, string freesearchBefore, int laxSearchCount) {
            return Contacts_QuickSearch_old(organisationId, userId, query, type, searchIn, SortOrder, sortAsc, freesearchBefore, laxSearchCount, null);
        }

        public List<Contact> Contacts_QuickSearch_old(int organisationId, string userId, string query, int type, int searchIn, string SortOrder, string sortAsc, string freesearchBefore, int laxSearchCount, List<Contact> results) {
            List<Contact> retval = results;
            if (retval == null) retval = new List<Contact>();
            sql.commandText = "z_contacts_quickSearch_" + organisationId;
            sql.parameters.AddWithValue("@organisationId", organisationId);
            sql.parameters.AddWithValue("@userId", userId);

            if (type > -1) sql.parameters.AddWithValue("@type", type);
            sql.parameters.AddWithValue("@searchIn", searchIn);

            sql.parameters.AddWithValue("@query", query);

            if (SortOrder != "" && SortOrder != null) sql.parameters.AddWithValue("@SortOrder", SortOrder);
            if (sortAsc != "" && sortAsc != null) sql.parameters.AddWithValue("@sortAsc", sortAsc);

            sql.parameters.AddWithValue("@laxSearchCount", laxSearchCount);
            sql.parameters.AddWithValue("@freesearchBefore", freesearchBefore);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                AddToContactsIfNotAdded(ref retval, new Contact(ref dr));
                //retval.Add(new Contact(ref dr));
            }
            dr.Close();
            sql.reset();

            if ((retval.Count == 0 && laxSearchCount < 4)) // || freesearchBefore=="")
            {
                if (freesearchBefore == "") {
                    return Contacts_QuickSearch_old(organisationId, userId, query, type, searchIn, SortOrder, sortAsc, "%", laxSearchCount, retval);
                } else {
                    return Contacts_QuickSearch_old(organisationId, userId, query, type, searchIn, SortOrder, sortAsc, "", laxSearchCount + 1, retval);
                }
            }

            return retval;
        }

        private void AddToContactsIfNotAdded(ref List<Contact> cts, Contact c) {
            foreach (Contact ct in cts) {
                if (ct.Id == c.Id) return;
            }

            cts.Add(c);
        }

        public int Contacts_createContactHierachy(int organisationId, int contactId) {
            int retval = 0;
            sql.commandText = "z_Contacts_createContactHierachy";
            sql.parameters.AddWithValue("@organisationId", organisationId);
            sql.parameters.AddWithValue("@contactId", contactId);
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = TypeCast.ToInt(dr["ContactId"]);
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public bool Contacts_CanBeDeleted(int ContactId) {
            bool retval = true;
            sql.commandText = "Contacts_CanBeDeleted";
            sql.parameters.AddWithValue("@ContactId", ContactId);
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = TypeCast.ToBool(dr["CanDelete"]);
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public List<Contact> Contacts_GetAll(int organisationId) {
            List<Contact> retval = new List<Contact>();
            sql.commandText = "z_contacts_getall_" + organisationId;
            sql.parameters.AddWithValue("@orgId", organisationId);
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new Contact(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        /// <summary>
        /// Get company information
        /// </summary>
        /// <param name="organisationId"></param>
        /// <param name="contactId"></param>
        /// <param name="obfuscate"></param>
        /// <returns></returns>
        public Contact Contact_Get(int organisationId, int contactId, User user, bool obfuscate = true) {
            /*
            Contact retval = null;
            sql.commandText = "z_contacts_get_" + organisationId;
            sql.parameters.AddWithValue("@contactId", contactId);
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = new Contact(ref dr);
            }
            dr.Close();
            sql.reset();
            return retval;
            */

            Contact retval = null;
            sql.commandText = "Contact_Get";
            sql.parameters.AddWithValue("@UserId", user.Id);
            sql.parameters.AddWithValue("@ContactId", contactId);
            sql.parameters.AddWithValue("@OrganisationId", organisationId);
            sql.parameters.AddWithValue("@Obfuscate", obfuscate);
            if (System.Web.HttpContext.Current != null && System.Web.HttpContext.Current.Request != null) {
                var ip = System.Web.HttpContext.Current.Request.UserHostAddress;
                sql.parameters.AddWithValue("@IPaddress", ip);
            }

            SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = new Contact(ref dr);
            }
            dr.Close();
            sql.reset();
            return retval;
        }

        /// <summary>
        /// Get company information
        /// </summary>
        /// <param name="user"></param>
        /// <param name="contactId"></param>
        /// <param name="fieldname"></param>
        /// <returns></returns>
        public string Contact_Get(User user, int contactId, string fieldname) {
            var result = string.Empty;
            sql.commandText = "Contact_Get";
            sql.parameters.AddWithValue("@UserId", user.Id);
            sql.parameters.AddWithValue("@ContactId", contactId);
            sql.parameters.AddWithValue("@OrganisationId", user.OrganisationId);
            sql.parameters.AddWithValue("@Obfuscate", false);
            sql.parameters.AddWithValue("@FieldName", fieldname);
            if (System.Web.HttpContext.Current != null && System.Web.HttpContext.Current.Request != null) {
                var ip = System.Web.HttpContext.Current.Request.UserHostAddress;
                sql.parameters.AddWithValue("@IPaddress", ip);
            }

            SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                result = TypeCast.ToString(dr[fieldname]);
            }
            dr.Close();
            sql.reset();
            return result;
        }

        public Contact Contact_Get_FromStaging(int organisationId, int contactId) {
            Contact retval = null;
            sql.commandText = "z_Contacts_Get_FromStaging_" + organisationId;
            sql.parameters.AddWithValue("@contactId", contactId);
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = new Contact(ref dr);
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public Contact Contact_GetSingleFromContactsAndCompanies(int organisationId, int contactId) {
            Contact retval = null;
            sql.commandText = "z_Contacts_GetSingleFromContactsAndCompanies_" + organisationId;
            sql.parameters.AddWithValue("@contactId", contactId);
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = new Contact(ref dr);
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        /// <summary>
        /// Save or Update contact information
        /// </summary>
        /// <param name="user">User information object</param>
        /// <param name="contact">Contact information object</param>
        /// <returns></returns>
        public int Contact_Update(User user, Contact contact) {

            // FETCH OLD VALUES 
            Contact c = Contact_Get(user.OrganisationId, contact.Id, user);
            if (c == null) c = Contact_Get_FromStaging(user.OrganisationId, contact.Id);
            var classifications = GetDataClassifications();

            /*List<string> AvailbleArgs = Contacts_GetUpdateArguments(user.OrganisationId);
            sql.commandText = "z_contacts_updateContact_" + user.OrganisationId;
            sql.parameters.AddWithValue("@ContactId", TypeCast.ToIntOrDBNull(contact.Id));
            sql.parameters.AddWithValue("@CompanyOwnerId", contact.CompanyId);
            sql.parameters.AddWithValue("@ContactOrganisationId", contact.OrganisationId);
            sql.parameters.AddWithValue("@ContactCreatedById", contact.CreatedById);
            sql.parameters.AddWithValue("@ContactType", contact.Type);

            int retval = 0;
            foreach (TableColumnWithValue t in c.DynamicTblColumns) {
                string arg = "@" + TypeCast.PrepareArgument(t.Name).ToLower();
                if (AvailbleArgs.Contains(arg)) {
                    sql.parameters.AddWithValue(arg, t.Value);
                }
            }

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = TypeCast.ToInt(dr["ContactId"]);
            }
            dr.Close();
            sql.reset();

            Events_addToEventLog(user.OrganisationId, user.Id, contact.Id == retval ? "UPDATE" : "CREATE", "CONTACTS", retval.ToString(), contact);
            */

            // NEW ROUTINE
            int returnValue = 0;
            try {
                var fieldXmlValue = "<FieldValues>";
                foreach (TableColumnWithValue t in contact.DynamicTblColumns) {
                    if (t.DynamicField != null) {
                        fieldXmlValue = fieldXmlValue + "<Field>";
                        fieldXmlValue = fieldXmlValue + "<TableName>" + SecurityElement.Escape(t.DynamicField.DatabaseTable) + "</TableName>";
                        fieldXmlValue = fieldXmlValue + "<FieldId>" + SecurityElement.Escape(t.DynamicField.Id.ToString()) + "</FieldId>";
                        fieldXmlValue = fieldXmlValue + "<Value>" + SecurityElement.Escape(Convert.ToString(t.Value)) + "</Value>";

                        var anyUpdate = false;
                        if (c != null && c.DynamicTblColumns != null) {

                            string[] tmp = t.DynamicField.DatabaseTable.Split('_');
                            int ownerId = TypeCast.ToInt(tmp[tmp.Length - 1]);

                            var olddynamicfield = c.DynamicTblColumns.FirstOrDefault(w => w.Name == (t.DynamicField.DatabaseTable + "_" + t.DynamicField.DatabaseColumn + "_" + ownerId));
                            if (olddynamicfield != null) {

                                var oldvalue = Convert.ToString(olddynamicfield.Value);
                                var newvalue = Convert.ToString(t.Value);

                                // Don't make update when data is obfuscated
                                if (newvalue.Contains("****")) {
                                    anyUpdate = false;
                                } else {
                                    if (olddynamicfield.DataType == "bit") {
                                        if (newvalue == "1" || string.Equals(newvalue, "true", StringComparison.CurrentCultureIgnoreCase)) { newvalue = "true"; } else { newvalue = "false"; }
                                    }

                                    //if (newvalue.ToLower() != oldvalue.ToLower()) {
                                    if (!string.Equals(newvalue, oldvalue, StringComparison.Ordinal)) {
                                        fieldXmlValue = fieldXmlValue + "<AnyUpdate>1</AnyUpdate>";
                                        anyUpdate = true;
                                    }
                                }
                            }

                            // WHEN NO UPDATE
                            if (!anyUpdate) {
                                fieldXmlValue = fieldXmlValue + "<AnyUpdate>0</AnyUpdate>";
                            }
                        } else {
                            fieldXmlValue = fieldXmlValue + "<AnyUpdate>0</AnyUpdate>";
                        }
                        fieldXmlValue = fieldXmlValue + "</Field>";
                    }
                }
                fieldXmlValue = fieldXmlValue + "</FieldValues>";

                var sqlConnection = new SqlConnection(connectionString);
                using (var cmd = new SqlCommand("Contact_Update", sqlConnection)) {
                    cmd.Parameters.AddWithValue("@UserId", user.Id);
                    cmd.Parameters.AddWithValue("@OrganisationId", user.OrganisationId);
                    cmd.Parameters.AddWithValue("@CompanyOwnerId", contact.CompanyId);
                    cmd.Parameters.AddWithValue("@ContactId", TypeCast.ToIntOrDBNull(contact.Id));
                    cmd.Parameters.AddWithValue("@ContactType", contact.Type);
                    cmd.Parameters.AddWithValue("@FieldValues", fieldXmlValue);
                    cmd.Parameters.AddWithValue("@ContactCreatedById", contact.CreatedById);
                    if (System.Web.HttpContext.Current != null && System.Web.HttpContext.Current.Request != null) {
                        var ip = System.Web.HttpContext.Current.Request.UserHostAddress;
                        cmd.Parameters.AddWithValue("@IPaddress", ip);
                    }

                    cmd.CommandType = CommandType.StoredProcedure;
                    if (sqlConnection.State == ConnectionState.Closed) { sqlConnection.Open(); }
                    using (SqlDataReader dr = cmd.ExecuteReader()) {
                        if (dr.HasRows) {
                            while (dr.Read()) {
                                returnValue = TypeCast.ToInt(dr["ContactId"]);
                            }
                        }
                    }
                }
                if (sqlConnection.State == ConnectionState.Open) {
                    sqlConnection.Close();
                    sqlConnection.Dispose();
                }
            } catch (Exception ex) { throw ex; }

            return returnValue;
        }

        private List<string> Contacts_GetUpdateArguments(int OrganisationId) {
            List<string> retval = new List<string>();
            sql.commandText = "Contacts_GetUpdateArguments";
            sql.parameters.AddWithValue("@OrganisationId", OrganisationId);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(TypeCast.ToString(dr["PARAMETER_NAME"]).ToLower());
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public List<ContactHistoryItem> Company_GetContactHistoryData(User U, int CompanyId) {
            List<ContactHistoryItem> retval = new List<ContactHistoryItem>();
            //sql.commandText = "Company_GetContactHistoryData";
            sql.commandText = "Company_GetContactHistoryData_V2"; //** Denne her er version 2, ESCRM-9/42
            sql.parameters.AddWithValue("@OrganisationId", U.OrganisationId);
            sql.parameters.AddWithValue("@UserId", U.Id);
            sql.parameters.AddWithValue("@CompanyId", CompanyId);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new ContactHistoryItem(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        /// <summary>
        /// Get all contact by email, ESCRM-273/276
        /// </summary>
        /// <param name="emailList"></param>
        /// <returns></returns>
        public List<Tuple<int, string, string>> ContactsByEmail(List<string> emailList)
        {
            List<Tuple<int, string, string>> result = new List<Tuple<int, string, string>>();

            try
            {
                sql.commandText = "ContactsByEmail";

                //** Fyld data i user-defined table type, udtEmails
                DataTable dt = new DataTable();
                DataColumn dc = new DataColumn("Email", typeof(String));
                dt.Columns.Add(dc);

                foreach (var item in emailList)
                {
                    DataRow drow = dt.NewRow();
                    drow[0] = item;
                    dt.Rows.Add(drow);
                }

                SqlParameter param = new SqlParameter("@Emails", SqlDbType.Structured)
                {
                    TypeName = "dbo.udtEmails",
                    Value = dt
                };

                sql.parameters.Add(param);

                using (SqlDataReader dr = sql.executeReader)
                {
                    if (dr.HasRows)
                    {
                        while (dr.Read())
                        {
                            int contactId = TypeCast.ToInt(dr[0].ToString());
                            string email = TypeCast.ToString(dr[1]);
                            string navn = TypeCast.ToString(dr[2]);

                            Tuple<int, string, string> tpl = new Tuple<int, string, string>(contactId, email, navn);

                            result.Add(tpl);
                        }
                    }
                    if (!dr.IsClosed) { dr.Close(); dr.Dispose(); }
                }
            }
            catch(Exception ex)
            {
                string error = ex.Message;
            }

            sql.reset();

            return result;
        }
        #endregion

        #region Counties

        public List<County> County_getCounties(string SortOrder, string sortAsc) {
            List<County> retval = new List<County>();
            sql.commandText = "Counties_getCounties";
            if (SortOrder != "" && SortOrder != null) sql.parameters.AddWithValue("@SortOrder", SortOrder);
            if (sortAsc != "" && sortAsc != null) sql.parameters.AddWithValue("@sortAsc", sortAsc);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new County(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public County Counties_getCountyFromNNECode(int NNECode) {
            County retval = null;
            sql.commandText = "Counties_getCountyFromNNECode";
            sql.parameters.AddWithValue("@NNECode", NNECode);
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = new County(ref dr);
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public County County_getCounty(int id) {
            County retval = null;
            sql.commandText = "Counties_getCounty";
            sql.parameters.AddWithValue("@id", id);
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = new County(ref dr);
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public int Counties_CreateUpdate(User U, County c) {
            sql.commandText = "Counties_createCounty";
            sql.parameters.AddWithValue("@Id", TypeCast.ToIntOrNull(c.id));
            sql.parameters.AddWithValue("@Name", TypeCast.ToStringOrDBNull(c.name));
            sql.parameters.AddWithValue("@Url", TypeCast.ToStringOrDBNull(c.url));
            sql.parameters.AddWithValue("@Address", TypeCast.ToStringOrDBNull(c.address));
            sql.parameters.AddWithValue("@Zip", TypeCast.ToStringOrDBNull(c.zip));
            sql.parameters.AddWithValue("@NNECode", TypeCast.ToInt(c.NNECode));

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            int countyId = 0;
            if (dr.Read()) {
                countyId = TypeCast.ToInt(dr["Status"]);
            }

            c.id = countyId;

            dr.Close();
            sql.reset();

            if (U != null)
                Events_addToEventLog(U.OrganisationId, U.Id, c.id == countyId ? "UPDATE" : "CREATE", "COUNTIES", countyId.ToString(), c);


            return countyId;
        }

        public void County_deleteCounty(User U, int id) {
            sql.commandText = "Counties_delete";
            sql.parameters.AddWithValue("@Id", TypeCast.ToIntOrNull(id));
            sql.execute();
            sql.reset();

            Events_addToEventLog(U.OrganisationId, U.Id, "DELETE", "COUNTIES", id.ToString(), null);
        }


        #endregion

        #region Organisations
        public OrganisationNews Organisations_GetNews(int OrganisationId) {
            sql.commandText = "Organisations_GetNews";
            sql.parameters.AddWithValue("@OrganisationId", OrganisationId);
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            OrganisationNews retval = null;
            if (dr.Read()) {
                retval = new OrganisationNews(ref dr);
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public void Organisations_UpdateNews(int OrganisationId, string News, string UserId, bool IsPublic) {
            if (IsPublic)
                Organisations_UpdatePublicNews(OrganisationId, News, UserId);
            else
                Organisations_UpdatePrivateNews(OrganisationId, News, UserId);
        }

        public void Organisations_UpdatePrivateNews(int OrganisationId, string News, string UserId) {
            sql.commandText = "Organisations_UpdatePrivateNews";
            sql.parameters.AddWithValue("@UserId", UserId);
            sql.parameters.AddWithValue("@OrganisationId", OrganisationId);
            sql.parameters.AddWithValue("@News", News);

            sql.execute();

            sql.reset();
        }

        public void Organisations_UpdatePublicNews(int OrganisationId, string News, string UserId) {
            sql.commandText = "Organisations_UpdatePublicNews";
            sql.parameters.AddWithValue("@UserId", UserId);
            sql.parameters.AddWithValue("@OrganisationId", OrganisationId);
            sql.parameters.AddWithValue("@News", News);

            sql.execute();

            sql.reset();
        }

        public int user_changePassword(User U, string CurrentPassword, string NewPassword) {
            sql.commandText = "user_changePassword";
            sql.parameters.AddWithValue("@userId", U.Id);
            sql.parameters.AddWithValue("@oldPassword", CurrentPassword);
            sql.parameters.AddWithValue("@newPassword", NewPassword);
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            int retval = -3;
            if (dr.Read()) {
                retval = TypeCast.ToInt(dr["Status"]);
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public void Organisations_SetPasswordChange(string UserId, string ControlKey) {
            sql.commandText = "Organisations_SetPasswordChange";
            sql.parameters.AddWithValue("@UserId", UserId);
            sql.parameters.AddWithValue("@ControlKey", ControlKey);

            sql.execute();

            sql.reset();
        }

        public bool Organisations_CanUserAutoChangePassword(string UserId, string ControlKey) {
            sql.commandText = "Organisations_CanUserAutoChangePassword";
            sql.parameters.AddWithValue("@userId", UserId);
            if (ControlKey != null && ControlKey != "") sql.parameters.AddWithValue("@ControlKey", ControlKey);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            bool retval = false;
            if (dr.Read()) {
                retval = true;
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public void user_deleteUser(string UserToBeDeletedId, string UserId) {
            sql.commandText = "user_deleteUser";
            sql.parameters.AddWithValue("@id", UserToBeDeletedId);
            sql.parameters.AddWithValue("@UserId", UserId);

            sql.execute();

            sql.reset();
        }

        public string Organisations_createUser(User creator, User u) {
            sql.commandText = "Organisations_createUser";
            sql.parameters.AddWithValue("@userId", TypeCast.ToStringOrDBNull(u.Id));
            sql.parameters.AddWithValue("@organisationId", u.OrganisationId);
            sql.parameters.AddWithValue("@firstname", u.Firstname);
            sql.parameters.AddWithValue("@lastname", u.Lastname);

            if (u.Username != "") sql.parameters.AddWithValue("@username", u.Username);
            if (u.Password != "") sql.parameters.AddWithValue("@password", u.Password);

            sql.parameters.AddWithValue("@email", u.Email);
            sql.parameters.AddWithValue("@licensePlates", u.LicensePlates);
            sql.parameters.AddWithValue("@userrole", (int)u.UserRole);
            sql.parameters.AddWithValue("@allowPersonalFolder", u.AllowPersonalFolder);

            sql.parameters.AddWithValue("@ExchangeURL", u.ExchangeURL);
            sql.parameters.AddWithValue("@ExchangeUsername", u.ExchangeUsername);
            sql.parameters.AddWithValue("@ExchangePassword", u.ExchangePassword);
            sql.parameters.AddWithValue("@ExchangeDomain", u.ExchangeDomain);
            sql.parameters.AddWithValue("@ExchangeFormbasedLogin", u.ExchangeFormbasedLogin);
            sql.parameters.AddWithValue("@Initials", u.Initials);
            sql.parameters.AddWithValue("@EarlyWarningUser", u.EarlyWarningUser);
            sql.parameters.AddWithValue("@RecieveEmailOnExportToUserEvaluation", u.RecieveEmailOnExportToUserEvaluation);

            sql.parameters.AddWithValue("@W2L_Username", u.W2L_Username);
            sql.parameters.AddWithValue("@W2L_Password", u.W2L_Password);

            sql.parameters.AddWithValue("@UseStartMenu", u.UseStartMenu);
            sql.parameters.AddWithValue("@UseTopMenu", u.UseTopMenu);
            sql.parameters.AddWithValue("@UseWindowShadows", u.UseWindowShadows);
            sql.parameters.AddWithValue("@UseCustomDesktopBackground", u.UseCustomDesktopBackground);
            sql.parameters.AddWithValue("@ShowHomeAtStartup", u.ShowHomeAtStartup);
            sql.parameters.AddWithValue("@UseCustomDesktopBackgroundStyle", u.UseCustomDesktopBackgroundStyle);

            sql.parameters.AddWithValue("@NoWelcome", u.NoWelcome);

            if (u.MustChangePasswordAtLogin) sql.parameters.AddWithValue("@PasswordExpires", u.PasswordExpires);
            sql.parameters.AddWithValue("@MustChangePasswordAtLogin", u.MustChangePasswordAtLogin);

            sql.parameters.AddWithValue("@AcceptsTransferredCompanies", u.AcceptsTransferredCompanies);

            sql.parameters.AddWithValue("@creatorId", creator.Id);

            sql.parameters.AddWithValue("@ListviewDataWarningAt", u.ListviewDataWarningAt);

            sql.parameters.AddWithValue("@AllowImportData", u.AllowImportData);
            sql.parameters.AddWithValue("@AllowMassRegistrations", u.AllowMassRegistrations);
            sql.parameters.AddWithValue("@AllowMailgroupModifications", u.AllowMailgroupModifications);
            sql.parameters.AddWithValue("@AllowImpersonation", u.AllowImpersonation);
            sql.parameters.AddWithValue("@FreeUser", u.FreeUser);
            sql.parameters.AddWithValue("@InvoiceAdvising", u.InvoiceAdvising);

            sql.parameters.AddWithValue("@PNumber", u.PNummer);
            sql.parameters.AddWithValue("@CellPhone", u.CellPhone);

            if (!string.IsNullOrEmpty(u.Team)) { sql.parameters.AddWithValue("@Team", u.Team); }
            if (!string.IsNullOrEmpty(u.Manager)) { sql.parameters.AddWithValue("@Manager", u.Manager); }

            string retval = "";

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = TypeCast.ToString(dr["Status"]);
            }

            dr.Close();

            u.Id = retval;

            sql.reset();

            Events_addToEventLog(creator.OrganisationId, creator.Id, retval == u.Id ? "UPDATE" : "CREATE", "USERS", retval, u);

            return retval;
        }

        public int Organisations_CreateUpdate(User creator, Organisation org) {
            sql.commandText = "Organisations_CreateUpdate";

            if (org.Id > 0) sql.parameters.AddWithValue("@Id", org.Id);
            sql.parameters.AddWithValue("@ParentId", TypeCast.ToIntOrDBNull(org.ParentId));
            sql.parameters.AddWithValue("@Name", org.Name);
            sql.parameters.AddWithValue("@Type", org.Type);
            if (org.ContractSigned != null) sql.parameters.AddWithValue("@ContractSigned", org.ContractSigned);

            //New Invoice properties
            sql.parameters.AddWithValue("@CVR", org.CVR);
            sql.parameters.AddWithValue("@Street", org.Street);
            sql.parameters.AddWithValue("@ZipCode", org.ZipCode);
            sql.parameters.AddWithValue("@City", org.City);
            sql.parameters.AddWithValue("@Email", org.Email);
            sql.parameters.AddWithValue("@Phone", org.Phone);
            sql.parameters.AddWithValue("@Note", org.Note);
            sql.parameters.AddWithValue("@SendInvoice", org.SendInvoice);
            if (org.CRMContractSigned != null) sql.parameters.AddWithValue("@CRMContractSigned", TypeCast.ToDateTime(org.CRMContractSigned));
            if (org.CRMContractCancelled != null) sql.parameters.AddWithValue("@CRMContractCancelled", TypeCast.ToDateTime(org.CRMContractCancelled));
            sql.parameters.AddWithValue("@Attn", org.Attn);
            sql.parameters.AddWithValue("@Ref", org.Ref);
            sql.parameters.AddWithValue("@ContactPerson", org.ContactPerson);
            sql.parameters.AddWithValue("@ContactPersonEmail", org.ContactPersonEmail);
            sql.parameters.AddWithValue("@ContactPerson2", org.ContactPerson2);
            sql.parameters.AddWithValue("@ContactPersonEmail2", org.ContactPersonEmail2);
            sql.parameters.AddWithValue("@InvoiceMethod", org.InvoiceMethod);
            sql.parameters.AddWithValue("@EAN", org.EAN);
            sql.parameters.AddWithValue("@AltCVR", org.AltCVR);
            sql.parameters.AddWithValue("@AltName", org.AltName);
            sql.parameters.AddWithValue("@AltStreet", org.AltStreet);
            sql.parameters.AddWithValue("@AltZipCode", org.AltZipCode);
            sql.parameters.AddWithValue("@AltCity", org.AltCity);
            sql.parameters.AddWithValue("@AltPhone", org.AltPhone);
            sql.parameters.AddWithValue("@AltEmail", org.AltEmail);
            sql.parameters.AddWithValue("@AltEAN", org.AltEAN);

            if (org.PNumber > 0) { sql.parameters.AddWithValue("@PNumber", org.PNumber); }
            if (!string.IsNullOrEmpty(org.DPOUserId)) { sql.parameters.AddWithValue("@DPOUserId", org.DPOUserId); }
            sql.parameters.AddWithValue("@EnableContactAgreement", org.EnableContactAgreement);
            sql.parameters.AddWithValue("@AllowHourlyTimesheet", org.AllowHourlyTimesheet);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            int orgId = 0;
            if (dr.Read()) {
                orgId = TypeCast.ToInt(dr["Status"]);
            }
            dr.Close();
            sql.reset();

            if (creator != null)
                Events_addToEventLog(creator.OrganisationId, creator.Id, orgId == org.Id ? "UPDATE" : "CREATE", "ORGANISATIONS", orgId.ToString(), org);

            return orgId;
        }

        public List<Organisation> Organisations_getAllOrganisations() {
            List<Organisation> retval = new List<Organisation>();
            sql.commandText = "Organisations_getAllOrganisations";
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new Organisation(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public List<Organisation> Organisations_getOrganisations(object parentId) {
            List<Organisation> retval = new List<Organisation>();
            sql.commandText = "Organisations_getOrganisations";
            sql.parameters.AddWithValue("@parentId", TypeCast.ToIntOrDBNull(parentId));
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new Organisation(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public Organisation Organisations_getOrganisation(int id) {
            Organisation retval = null;
            sql.commandText = "Organisations_getOrganisation";
            sql.parameters.AddWithValue("@id", id);
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = new Organisation(ref dr);
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public List<User> Organisations_getUsers(int organisationId, string SortOrder, string sortAsc) {
            return Organisations_getUsers(organisationId, SortOrder, sortAsc, false);
        }

        public List<User> Organisations_getUsers(int organisationId, string SortOrder, string sortAsc, bool IncludeDeleted) {
            List<User> retval = new List<User>();
            sql.commandText = "Organisations_getUsers";
            sql.parameters.AddWithValue("@organisationId", organisationId);
            sql.parameters.AddWithValue("@SortOrder", SortOrder);
            sql.parameters.AddWithValue("@sortAsc", sortAsc);
            sql.parameters.AddWithValue("@IncludeDeleted", IncludeDeleted);
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new User(ref dr, false));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public string[] Organisations_getAllUsernames() {
            List<string> retval = new List<string>();
            sql.commandText = "Organisations_getAllUsernames";
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(TypeCast.ToString(dr["username"]));
            }
            dr.Close();
            sql.reset();

            return retval.ToArray();

        }

        public User Organisations_getUser(string userId) {
            return Organisations_getUser(userId, false);
        }
 
        public User Organisations_getUser(string userId, bool ignoreDeleted) {
            User retval = null;
            sql.commandText = "Organisations_getUser";
            sql.parameters.AddWithValue("@userId", userId);
            sql.parameters.AddWithValue("@ignoreDeleted", ignoreDeleted);
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = new User(ref dr);
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public User Organisations_GetUserFromUserName(string username) {
            User retval = null;
            sql.commandText = "Organisations_GetUserFromUserName";
            sql.parameters.AddWithValue("@username", username);
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = new User(ref dr, false);
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public void Organisations_delete(User U, int Id) {
            sql.commandText = "organisations_delete";
            sql.parameters.AddWithValue("@id", TypeCast.ToInt(Id));
            sql.execute();
            sql.reset();

            Events_addToEventLog(U.OrganisationId, U.Id, "DELETE", "ORGANISATIONS", Id.ToString(), null);
        }

        /// <summary>
        /// Create/Update standard deling
        /// </summary>
        /// <param name="OrganisationId"></param>
        /// <param name="StandardSharing"></param>
        public void Organisation_CreateUpdateStandardSharing(int OrganisationId, int StandardSharing, bool Noter, bool Dokumenter)
        {
            sql.commandText = "Organisations_CreateUpdateStandardSharing";
            sql.parameters.AddWithValue("@OrganisationId", OrganisationId);
            sql.parameters.AddWithValue("@StandardSharing", StandardSharing);
            sql.parameters.AddWithValue("@Noter", Noter);
            sql.parameters.AddWithValue("@Dokumenter", Dokumenter);

            sql.execute();

            sql.reset();
        }

        /// <summary>
        /// Get standard deling
        /// </summary>
        /// <param name="OrganisationId"></param>
        public int Organisation_GetStandardSharing(int OrganisationId, bool Noter, bool Dokumenter)
        {
            int result = 0;

            sql.commandText = "Organisations_GetStandardSharing";
            sql.parameters.AddWithValue("@OrganisationId", OrganisationId);
            sql.parameters.AddWithValue("@Noter", Noter);
            sql.parameters.AddWithValue("@Dokumenter", Dokumenter);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read())
            {
                result = TypeCast.ToInt(dr["StandardSharing"]);
            }
            dr.Close();

            sql.reset();

            return result;
        }
        #endregion

        #region Org_County_Association

        public int Org_County_createAssociation(int orgId, int countyId) {
            sql.commandText = "Org_County_createAssociation";
            sql.parameters.AddWithValue("@OrgId", orgId);
            sql.parameters.AddWithValue("@CountyId", countyId);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            int intStatus = -1;
            if (dr.Read()) {
                intStatus = TypeCast.ToInt(dr["Status"]);
            }
            dr.Close();
            sql.reset();

            return intStatus;
        }


        public List<County> Organisation_County_getAssociatedConties(int organisationId) {
            sql.commandText = "Organisation_County_getAssociatedConties";
            sql.parameters.AddWithValue("@organisationId", organisationId);

            List<County> countyIDS = new List<County>();
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;

            while (dr.Read()) {
                countyIDS.Add(new County(ref dr));
            }

            dr.Close();
            sql.reset();


            return countyIDS;
        }

        //public List<County> Org_County_getAssociatedCountyIds(int orgId)
        //{
        //    sql.commandText = "Org_County_getAssociatedCountyIds";
        //    sql.parameters.AddWithValue("@OrgId",orgId);

        //    ArrayList countyIDS = new ArrayList();
        //    System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
        //    while (dr.Read())
        //    { 
        //        countyIDS.Add(TypeCast.ToInt(dr["CountyId"]));
        //    }
        //    dr.Close();
        //    sql.reset();
        //    List<County> listCounty = new List<County>();

        //    if (countyIDS.Count > 0)
        //    {
        //        foreach (int i in countyIDS)
        //        {
        //            listCounty.Add(this.County_getCounty(i));
        //        }
        //    }
        //    return listCounty; 
        //}

        public void Org_County_deleteAssociation(int orgId, object countyId) {
            sql.commandText = "Org_County_deleteAssociation";
            sql.parameters.AddWithValue("@OrgId", orgId);
            sql.parameters.AddWithValue("@CountyId", TypeCast.ToIntOrDBNull(countyId));
            sql.execute();
            sql.reset();
        }

        #endregion

        #region Mailgroups
        public List<string> MailGroups_ExportContactEmails(int organisationId, int mailGroupId, string[] ids) {
            sql.commandText = "z_MailGroups_ExportContactEmails_" + organisationId;
            sql.parameters.AddWithValue("@organisationId", organisationId);
            sql.parameters.AddWithValue("@MailgroupId", mailGroupId);

            List<string> emails = new List<string>();

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                Contact ct = new Contact(ref dr);

                if (mailgrouIdInArray(ct.Id.ToString(), ids)) {
                    string email = "";

                    foreach (TableColumnWithValue t in ct.DynamicTblColumns) {
                        if (t.ValueFormatted != "" && email == "") {
                            email = t.ValueFormatted;
                        }
                    }
                    if (email != "")
                        emails.Add(email);
                }
            }

            dr.Close();
            sql.reset();

            return emails;
        }

        private bool mailgrouIdInArray(string id, string[] ids) {
            foreach (string s in ids) {
                if (s == id) return true;
            }
            return false;
        }

        public List<string> MailGroups_ExportContactEmails(int organisationId, int mailGroupId) {
            sql.commandText = "z_MailGroups_ExportContactEmails_" + organisationId;
            sql.parameters.AddWithValue("@organisationId", organisationId);
            sql.parameters.AddWithValue("@MailgroupId", mailGroupId);

            List<string> emails = new List<string>();

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                Contact ct = new Contact(ref dr);

                string email = "";

                foreach (TableColumnWithValue t in ct.DynamicTblColumns) {
                    if (t.ValueFormatted != "" && email == "") {
                        email = t.ValueFormatted;
                    }
                }
                if (email != "")
                    emails.Add(email);
            }

            dr.Close();
            sql.reset();

            return emails;
        }

        public void Mailgroups_MoveFolder(int Id, int ParentId) {
            sql.commandText = "Mailgroups_MoveFolder";
            sql.parameters.AddWithValue("@Id", Id);
            if (ParentId > 0) sql.parameters.AddWithValue("@ParentId", ParentId);
            sql.execute();
            sql.reset();
        }

        public void MailGroup_Move(int Id, int FolderId, int OrganisationId) {
            sql.commandText = "MailGroup_Move";
            sql.parameters.AddWithValue("@Id", Id);
            if (FolderId > 0) sql.parameters.AddWithValue("@FolderId", FolderId);
            sql.parameters.AddWithValue("@OrganisationId", OrganisationId);
            sql.execute();
            sql.reset();
        }

        public void Mailgroups_DeleteFolder(int Id, int OrganisationId, string UserId) {
            sql.commandText = "Mailgroups_DeleteFolder";
            sql.parameters.AddWithValue("@Id", Id);
            sql.parameters.AddWithValue("@OrganisationId", OrganisationId);
            sql.parameters.AddWithValue("@UserId", UserId);
            sql.execute();
            sql.reset();
        }

        public int Mailgroups_CreateFolder(int Id, int OrganisationId, string UserId, int ParentId, string Name) {
            int retval = 0;
            sql.commandText = "Mailgroups_CreateFolder";
            if (Id > 0) sql.parameters.AddWithValue("@Id", Id);
            sql.parameters.AddWithValue("@OrganisationId", OrganisationId);
            sql.parameters.AddWithValue("@UserId", UserId);
            if (ParentId > 0) sql.parameters.AddWithValue("@ParentId", ParentId);
            sql.parameters.AddWithValue("@Name", Name);
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;

            if (dr.Read())
                retval = TypeCast.ToInt(dr["id"]);

            dr.Close();
            sql.reset();


            return retval;
        }

        public List<MailgroupFolder> Mailgroups_getFolders(int orgId, int parentId) {
            sql.commandText = "Mailgroups_getFolders";
            sql.parameters.AddWithValue("@OrganisationId", orgId);
            if (parentId > 0) sql.parameters.AddWithValue("@parentId", parentId);

            List<MailgroupFolder> listMailgroup = new List<MailgroupFolder>();
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                listMailgroup.Add(new MailgroupFolder(ref dr));
            }
            dr.Close();
            sql.reset();

            return listMailgroup;
        }

        public List<Mailgroup> Mailgroups_getMailgroupsByOrgId(int orgId, int FolderId, bool ExcludeDisabled) {
            sql.commandText = "Mailgroups_getMailgroupsByOrganisationId";
            sql.parameters.AddWithValue("@OrganisationId", orgId);
            if (FolderId > 0) sql.parameters.AddWithValue("@FolderId", FolderId);
            sql.parameters.AddWithValue("@ExcludeDisabled", ExcludeDisabled);

            List<Mailgroup> listMailgroup = new List<Mailgroup>();
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                listMailgroup.Add(new Mailgroup(ref dr));
            }
            dr.Close();
            sql.reset();

            return listMailgroup;
        }

        /// <summary>
        /// Gets all mailgroups accessable by orgId
        /// </summary>
        /// <param name="orgId"></param>
        /// <returns></returns>
        public List<Mailgroup> Mailgroups_getMailgroupsFromOrganisationId(int orgId)
        {
            sql.commandText = "Mailgroups_getMailgroupsFromOrganisationId";
            sql.parameters.AddWithValue("@OrganisationId", orgId);

            List<Mailgroup> listMailgroup = new List<Mailgroup>();
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read())
            {
                listMailgroup.Add(new Mailgroup(ref dr));
            }
            dr.Close();
            sql.reset();

            return listMailgroup;
        }

        public List<Mailgroup> Mailgroups_getMailgroupsShared(int orgId, bool ExcludeDisabled) {
            sql.commandText = "Mailgroups_getMailgroupsShared";
            sql.parameters.AddWithValue("@OrganisationId", orgId);
            sql.parameters.AddWithValue("@ExcludeDisabled", ExcludeDisabled);

            List<Mailgroup> listMailgroup = new List<Mailgroup>();
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                listMailgroup.Add(new Mailgroup(ref dr));
            }
            dr.Close();
            sql.reset();

            return listMailgroup;
        }

        public Mailgroup Mailgroup_getMailgroup(int Id, int OrganisationId) {
            sql.commandText = "Mailgroup_getMailgroup";
            sql.parameters.AddWithValue("@Id", Id);
            sql.parameters.AddWithValue("@OrganisationId", OrganisationId);
            Mailgroup M = null;
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                M = new Mailgroup(ref dr);
            }
            dr.Close();
            sql.reset();

            return M;
        }

        public Mailgroup Mailgroups_getMailgroup(int Id, int OrganisationId) {
            sql.commandText = "Mailgroups_getMailgroup";
            sql.parameters.AddWithValue("@Id", Id);
            sql.parameters.AddWithValue("@OrganisationId", OrganisationId);

            Mailgroup M = null;
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                M = new Mailgroup(ref dr);
            }
            dr.Close();
            sql.reset();

            return M;
        }

        public Mailgroup Mailgroups_getMailgroupsFromFullPath(int OrganisationId, string Path) {
            sql.commandText = "Mailgroups_getMailgroupsFromFullPath";
            sql.parameters.AddWithValue("@OrganisationId", OrganisationId);

            if (!Path.StartsWith("/"))
                Path = "/" + Path;

            //Fix paths with leading or trailing spacers
            while (Path.IndexOf("/ ") > -1)
                Path = Path.Replace("/ ", "/");

            while (Path.IndexOf(" /") > -1)
                Path = Path.Replace(" /", "/");

            sql.parameters.AddWithValue("@Path", Path);
            Mailgroup M = null;
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                M = new Mailgroup(ref dr);
            }
            dr.Close();
            sql.reset();

            return M;
        }

        public List<Contact> Mailgroups_getContactsByMailgroupId(int mailgroupId, int organisationId) {
            sql.commandText = "z_Contacts_Get_Contacts_FromMailGroup_" + organisationId;
            sql.parameters.AddWithValue("@MailgroupId", mailgroupId);

            List<Contact> listContacts = new List<Contact>();

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                listContacts.Add(new Contact(ref dr));
            }

            dr.Close();
            sql.reset();

            return listContacts;

        }
        public List<MailgroupOnContact> Mailgroups_getMailgroupsFromContact(int contactId) {
            return Mailgroups_getMailgroupsFromContact(0, contactId);
        }

        public List<MailgroupOnContact> Mailgroups_getMailgroupsFromContact(int organisationId, int contactId) {
            sql.commandText = "Mailgroups_getMailgroupsFromContact";
            if (organisationId > 0) sql.parameters.AddWithValue("@organisationId", organisationId);
            sql.parameters.AddWithValue("@contactId", contactId);
            List<MailgroupOnContact> listMailgroup = new List<MailgroupOnContact>();
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                listMailgroup.Add(new MailgroupOnContact(ref dr));
            }
            dr.Close();
            sql.reset();

            return listMailgroup;
        }

        public int Mailgroup_Update(User U, Mailgroup m) {
            sql.commandText = "Mailgroup_CreateUpdate";
            sql.parameters.AddWithValue("@Id", TypeCast.ToIntOrDBNull(m.id));
            sql.parameters.AddWithValue("@name", m.name);
            sql.parameters.AddWithValue("@CreatedBy", m.createdBy);
            sql.parameters.AddWithValue("organisationId", m.organisationId);
            if (m.FolderId > 0) sql.parameters.AddWithValue("FolderId", m.FolderId);


            if (m.DeactivatedDate != null) {
                sql.parameters.AddWithValue("DeactivatedBy", m.DeactivatedBy);
                sql.parameters.AddWithValue("DeactivatedDate", m.DeactivatedDate);
            }

            sql.parameters.AddWithValue("@SharedWith", m.SharedWith);

            int retval = 0;

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = TypeCast.ToInt(dr["status"]);
            }
            dr.Close();
            sql.reset();

            Events_addToEventLog(U.OrganisationId, U.Id, m.id == retval ? "UPDATE" : "CREATE", "MAILGROUPS", retval.ToString(), m);

            return retval;
        }

        public List<MailgroupShared> Mailgroup_GetSharing(int MailgroupId) {
            sql.commandText = "Mailgroup_GetSharing";
            sql.parameters.AddWithValue("@Id", MailgroupId);
            List<MailgroupShared> listMailgroup = new List<MailgroupShared>();
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                listMailgroup.Add(new MailgroupShared(ref dr));
            }
            dr.Close();
            sql.reset();

            return listMailgroup;
        }

        public void Mailgroup_CreateSharing(MailgroupShared Sharing) {
            sql.commandText = "Mailgroup_CreateSharing";
            sql.parameters.AddWithValue("@MailgroupId", Sharing.MailgroupId);
            sql.parameters.AddWithValue("@SharedWithOrganisationId", Sharing.SharedWithOrganisationId);
            sql.parameters.AddWithValue("@Writeable", Sharing.Writeable);
            if (Sharing.SharedOrganisationFolderId > 0) sql.parameters.AddWithValue("@SharedOrganisationFolderId", Sharing.SharedOrganisationFolderId);
            sql.execute();
            sql.reset();
        }

        public void Mailgroup_Contact_deleteAssociation(int mailgroupId, object contactId) {
            sql.commandText = "Mailgroup_Contact_deleteAssociation";
            sql.parameters.AddWithValue("@MailgroupId", TypeCast.ToInt(mailgroupId));
            sql.parameters.AddWithValue("@ContactId", TypeCast.ToIntOrDBNull(contactId));
            sql.execute();
            sql.reset();
        }

        public void Mailgroups_moveToContact(int OldContactId, int NewContactId) {
            sql.commandText = "Mailgroups_moveToContact";
            sql.parameters.AddWithValue("@OldContactId", OldContactId);
            sql.parameters.AddWithValue("@NewContactId", NewContactId);
            sql.execute();
            sql.reset();
        }

        public void Mailgroup_delete(User U, int Id) {
            sql.commandText = "Mailgroups_delete";
            sql.parameters.AddWithValue("@id", Id);
            sql.parameters.AddWithValue("@userId", U.Id);
            sql.execute();
            sql.reset();

            Events_addToEventLog(U.OrganisationId, U.Id, "DELETE", "MAILGROUPS", Id.ToString(), null);
        }

        public int Mailgroup_Contact_createAssociation(int mailgroupId, int contactId, string UserId, bool FromBatchJob)
        {
            sql.commandText = "Mailgroup_Contact_createAssociation";
            sql.parameters.AddWithValue("@MailgroupId", mailgroupId);
            sql.parameters.AddWithValue("@ContactId", contactId);
            sql.parameters.AddWithValue("@UserId", UserId);
            sql.parameters.AddWithValue("@FromBatchJob", FromBatchJob);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            int intStatus = -1;
            if (dr.Read())
            {
                intStatus = TypeCast.ToInt(dr["Status"]);
            }
            dr.Close();
            sql.reset();

            return intStatus;
        }

        /// <summary>
        /// Mailgroup create association V2, ESCRM-273/275
        /// </summary>
        /// <param name="mailgroupId"></param>
        /// <param name="contactId"></param>
        /// <param name="UserId"></param>
        /// <param name="FromBatchJob"></param>
        /// <returns></returns>
        public int Mailgroup_Contact_createAssociation_V2(int mailgroupId, int contactId, string UserId, bool FromBatchJob)
        {
            sql.commandText = "Mailgroup_Contact_createAssociation_V2";
            sql.parameters.AddWithValue("@MailgroupId", mailgroupId);
            sql.parameters.AddWithValue("@ContactId", contactId);
            sql.parameters.AddWithValue("@UserId", UserId);
            sql.parameters.AddWithValue("@FromBatchJob", FromBatchJob);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            int intLastInsertedId = -1;
            if (dr.Read())
            {
                intLastInsertedId = TypeCast.ToInt(dr["LastInsertedId"]);
            }
            dr.Close();
            sql.reset();

            return intLastInsertedId;
        }

        /// <summary>
        /// Does mailgroupcontact for user exists, ESCRM-273/275
        /// </summary>
        /// <param name="mailgroupId"></param>
        /// <param name="contactId"></param>
        /// <param name="userId"></param>
        public bool MailgroupContactExists(int mailgroupId, object contactId, out int fmailgroupContactId)
        {
            bool retval = false;
            fmailgroupContactId = 0;

            sql.commandText = "MailgroupContactExists";
            sql.parameters.AddWithValue("@MailgroupId", TypeCast.ToInt(mailgroupId));
            sql.parameters.AddWithValue("@ContactId", TypeCast.ToIntOrDBNull(contactId));
            using (SqlDataReader dr = sql.executeReader)
            {
                if (dr.HasRows)
                {
                    dr.Read();
                    fmailgroupContactId = TypeCast.ToInt(dr[0]);

                    retval = true;
                }
                if (!dr.IsClosed) { dr.Close(); dr.Dispose(); }
            }
            sql.reset();

            return retval;
        }

        /// <summary>
        /// Get Mailgroupcontacts, ESCRM-273/275
        /// </summary>
        /// <returns></returns>
        public List<Tuple<string, string, string, string, string>> MailgroupContactsById(int mailgrouptId)
        {
            List<Tuple<string, string, string, string, string>> records = new List<Tuple<string, string, string, string, string>>();
            sql.commandText = "MailgroupContactsById";
            sql.parameters.AddWithValue("@MailgroupId", mailgrouptId);

            using (SqlDataReader dr = sql.executeReader)
            {
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        string mailgroupId = TypeCast.ToString(dr[0]);
                        string contactId = TypeCast.ToString(dr[1]);
                        string oprettetAf = TypeCast.ToString(dr[2]);
                        string datoStempel = TypeCast.ToString(dr[3]);
                        string fraBatch = TypeCast.ToString(dr[4]);

                        Tuple<string, string, string, string, string> tpl = new Tuple<string, string, string, string, string>(mailgroupId, contactId, oprettetAf, datoStempel, fraBatch);

                        records.Add(tpl);
                    }
                }
                if (!dr.IsClosed) { dr.Close(); dr.Dispose(); }
            }
            sql.reset();
            return records;
        }

        /// <summary>
        /// Get Mailgroupcontact by name, ESCRM-273/275
        /// </summary>
        /// <returns></returns>
        public int MailgroupsGetByName(string mailgrouptName)
        {
            int result = 0;

            sql.commandText = "MailgroupsGetByName";
            sql.parameters.AddWithValue("@MailgroupName", mailgrouptName);

            using (SqlDataReader dr = sql.executeReader)
            {
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        result = TypeCast.ToInt(dr[0]);
                    }
                }
                if (!dr.IsClosed) { dr.Close(); dr.Dispose(); }
            }
            sql.reset();

            return result;
        }
        #endregion

        #region Dynamic Fields

        public string dynamicFields_getFieldDataType(int OrganisationId, string DatabaseColumn, string DatabaseTable) {
            string retval = "";
            sql.commandType = System.Data.CommandType.StoredProcedure;
            sql.commandText = "dynamicFields_getFieldDataType";
            sql.parameters.AddWithValue("@organisationId", OrganisationId);
            sql.parameters.AddWithValue("@databaseColumn", DatabaseColumn);
            sql.parameters.AddWithValue("@databaseTable", DatabaseTable);
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = TypeCast.ToString(dr["fieldType"]);
            }
            dr.Close();
            sql.reset();
            return retval;
        }

        public void dynamicFields_clearCustomValues(int dynamicFieldId) {
            sql.commandType = System.Data.CommandType.StoredProcedure;
            sql.commandText = "dynamicFields_clearCustomValues";
            sql.parameters.AddWithValue("@dynamicFieldId", dynamicFieldId);
            sql.execute();
            sql.reset();
        }

        public void dynamicFields_addCustomValue(int dynamicFieldId, string value) {
            dynamicFields_addCustomValue(dynamicFieldId, value, false);
        }

        public void dynamicFields_addCustomValue(int dynamicFieldId, string value, bool ingoreDoubles) {
            sql.commandType = System.Data.CommandType.StoredProcedure;
            sql.commandText = "dynamicFields_addCustomValue";
            sql.parameters.AddWithValue("@dynamicFieldId", dynamicFieldId);
            sql.parameters.AddWithValue("@value", value);
            sql.parameters.AddWithValue("@ingoreDoubles", ingoreDoubles);
            sql.execute();
            sql.reset();
        }

        public void dynamicFields_addSharedWith(int dynamicFieldId, int SharedWithOrganisationId) {
            sql.commandType = System.Data.CommandType.StoredProcedure;
            sql.commandText = "dynamicFields_addSharedWith";
            sql.parameters.AddWithValue("@dynamicFieldId", dynamicFieldId);
            sql.parameters.AddWithValue("@SharedWithOrganisationId", SharedWithOrganisationId);
            sql.execute();
            sql.reset();
        }

        public dynamicCanvas dynamicCanvas_getCanvas(Organisation O, string CanvasType) {
            dynamicCanvas retval = null;
            sql.commandType = System.Data.CommandType.StoredProcedure;
            sql.commandText = "dynamicCanvas_getCanvas";
            sql.parameters.AddWithValue("@organisationId", O.Id);
            sql.parameters.AddWithValue("@CanvasType", CanvasType);
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = new dynamicCanvas(ref dr);
            }
            dr.Close();
            sql.reset();
            return retval;
        }

        public void dynamicCanvas_updateCanvas(dynamicCanvas C) {
            sql.commandType = System.Data.CommandType.StoredProcedure;
            sql.commandText = "dynamicCanvas_updateCanvas";
            sql.parameters.AddWithValue("@OrganisationId", C.OrganisationId);
            sql.parameters.AddWithValue("@CanvasType", C.CanvasType);
            sql.parameters.AddWithValue("@Width", C.Width);
            sql.parameters.AddWithValue("@Height", C.Height);
            sql.execute();
            sql.reset();

        }

        public List<DynamicField> dynamicFields_getFieldsForOrganisationFromSQL(int OrganisationId) {
            return dynamicFields_getFieldsForOrganisationFromSQL(OrganisationId, "");
        }
        public List<DynamicField> dynamicFields_getFieldsForOrganisationFromSQL(Organisation O) {
            return dynamicFields_getFieldsForOrganisationFromSQL(O, "");
        }
        public List<DynamicField> dynamicFields_getFieldsForOrganisationFromSQL(Organisation O, string Type) {
            return dynamicFields_getFieldsForOrganisationFromSQL(O.Id, Type);
        }
        public List<DynamicField> dynamicFields_getFieldsForOrganisationFromSQL(int OrganisationId, string Type) {
            List<DynamicField> retval = new List<DynamicField>();
            sql.commandText = "dynamicFields_getFieldsForOrganisation";
            sql.parameters.AddWithValue("@organisationId", OrganisationId);
            sql.parameters.AddWithValue("@type", TypeCast.ToStringOrDBNull(Type));
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new DynamicField(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }


        public List<DynamicField> dynamicFields_getFieldsFromOrganisationWithDataLinkOf(int OrganisationId, string DataLink) {
            List<DynamicField> retval = new List<DynamicField>();
            sql.commandText = "dynamicFields_getFieldsFromOrganisationWithDataLinkOf";
            sql.parameters.AddWithValue("@organisationId", OrganisationId);
            sql.parameters.AddWithValue("@DataLink", DataLink);
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new DynamicField(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public List<string> dynamicFields_GetPhysicalTableColumns(Organisation O, bool CompanyView) {
            List<string> retval = new List<string>();
            sql.commandText = "dynamicFields_GetPhysicalTableColumns";
            sql.parameters.AddWithValue("@organisationId", O.Id);
            sql.parameters.AddWithValue("@CompanyView", CompanyView);
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(TypeCast.ToString(dr["COLUMN_NAME"]));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public List<DynamicField> dynamicFields_FixDoubleTrouble(Organisation O) {
            List<DynamicField> DoublesRemoved = new List<DynamicField>();
            Redo:
            List<DynamicField> Fields = dynamicFields_getFieldsForOrganisationFromSQL(O);
            foreach (DynamicField df in Fields) {
                DynamicField DoubleField = dynamicFields_FixDoubleTrouble_InList(df, Fields);
                if (DoubleField != null) {
                    DoublesRemoved.Add(DoubleField);
                    dynamicFields_dropField(O, DoubleField.Id);
                    goto Redo;
                }
            }

            return DoublesRemoved;
        }

        private DynamicField dynamicFields_FixDoubleTrouble_InList(DynamicField f, List<DynamicField> Fields) {
            foreach (DynamicField df in Fields) {
                if (df.Id != f.Id && df.DatabaseColumn == f.DatabaseColumn && df.DatabaseTable == f.DatabaseTable && df.OrganisationId == f.OrganisationId) {
                    return df;
                }
            }

            return null;
        }

        public void dynamicFields_dropField(Organisation O, int fieldId) {
            dynamicFields_dropField(O, O, fieldId);
        }


        private void dynamicFields_dropField(Organisation BaseOrg, Organisation CurrentOrg, int fieldId) {
            sql.commandText = "dynamicFields_dropField";
            sql.parameters.AddWithValue("@organisationId", CurrentOrg.Id);
            sql.parameters.AddWithValue("@baseOrganisationId", BaseOrg.Id);
            sql.parameters.AddWithValue("@fieldId", fieldId);
            sql.execute();
            sql.reset();

            foreach (Organisation O in Organisations_getOrganisations(CurrentOrg.Id)) {
                dynamicFields_dropField(BaseOrg, O, fieldId);
            }
        }

        public List<string> dynamicFields_getCustomValues(int dynamicFieldId, bool onlyFromThisOrganisation) {
            List<string> retval = new List<string>();
            sql.commandText = "dynamicFields_getCustomValues";
            sql.parameters.AddWithValue("@dynamicFieldId", dynamicFieldId);
            sql.parameters.AddWithValue("@onlyFromThisOrganisation", onlyFromThisOrganisation);
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(TypeCast.ToString(dr["value"]));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public void dynamicFields_updateFields(User U, Organisation O, ref List<DynamicField> fields, bool resetPositionsForForeignOrganisations) {
            dynamicFields_updateFields(U, O, O, ref fields, resetPositionsForForeignOrganisations);
        }
        private void dynamicFields_updateFields(User U, Organisation BaseOrg, Organisation CurrentOrg, ref List<DynamicField> fields, bool resetPositionsForForeignOrganisations) {


            for (int i = 0; i < fields.Count; i++) {

                if (resetPositionsForForeignOrganisations || U.OrganisationId == CurrentOrg.Id) {
                    sql.commandText = "dynamicFields_updateField";
                    sql.parameters.AddWithValue("@resetPositionsForForeignOrganisations", true);
                } else {
                    sql.commandText = "dynamicFields_updateFieldKeepChildPositions";
                    sql.parameters.AddWithValue("@resetPositionsForForeignOrganisations", resetPositionsForForeignOrganisations);
                }

                if (fields[i].Id == 0)
                    fields[i].BaseOrganisationId = CurrentOrg.Id;

                if (fields[i].BaseOrganisationId == 0)
                    fields[i].BaseOrganisationId = BaseOrg.Id;

                if (fields[i].Id > 0) sql.parameters.AddWithValue("@id", fields[i].Id);
                sql.parameters.AddWithValue("@ownerId", (BaseOrg.Id != CurrentOrg.Id ? fields[i].Id : fields[i].OwnerId));
                sql.parameters.AddWithValue("@organisationId", CurrentOrg.Id);
                sql.parameters.AddWithValue("@baseOrganisationId", fields[i].BaseOrganisationId);
                sql.parameters.AddWithValue("@databaseTable", fields[i].DatabaseTable);
                sql.parameters.AddWithValue("@databaseColumn", fields[i].DatabaseColumn);
                sql.parameters.AddWithValue("@x1", fields[i].X1);
                sql.parameters.AddWithValue("@y1", fields[i].Y1);
                sql.parameters.AddWithValue("@width1", fields[i].Width1);
                sql.parameters.AddWithValue("@height1", fields[i].Height1);
                sql.parameters.AddWithValue("@x2", fields[i].X2);
                sql.parameters.AddWithValue("@y2", fields[i].Y2);
                sql.parameters.AddWithValue("@width2", fields[i].Width2);
                sql.parameters.AddWithValue("@height2", fields[i].Height2);
                sql.parameters.AddWithValue("@fieldType", fields[i].FieldType);
                sql.parameters.AddWithValue("@requiredState", fields[i].RequiredState);
                sql.parameters.AddWithValue("@searchAble", fields[i].SearchAble);
                sql.parameters.AddWithValue("@useInReports", fields[i].UseInReports);
                sql.parameters.AddWithValue("@viewState", fields[i].ViewState);
                sql.parameters.AddWithValue("@tabIndex1", TypeCast.ToInt(fields[i].TabIndex1));
                sql.parameters.AddWithValue("@tabIndex2", TypeCast.ToInt(fields[i].TabIndex2));
                sql.parameters.AddWithValue("@ShowInListview", TypeCast.ToIntOrDBNull(fields[i].ShowInListview));
                sql.parameters.AddWithValue("@ListviewIndex", TypeCast.ToIntOrDBNull(fields[i].ListviewIndex));
                sql.parameters.AddWithValue("@DataLink", TypeCast.ToString(fields[i].DataLink));
                sql.parameters.AddWithValue("@DataSource", TypeCast.ToString(fields[i].DataSource));
                sql.parameters.AddWithValue("@FollowAny", TypeCast.ToString(fields[i].FollowAny));
                sql.parameters.AddWithValue("@alternateText", TypeCast.ToString(fields[i].AlternateText));
                sql.parameters.AddWithValue("@noInheritance", fields[i].NoInherit_Obsolete);

                sql.parameters.AddWithValue("@OuterCSS", fields[i].OuterCSS);
                sql.parameters.AddWithValue("@InnerCSS", fields[i].InnerCSS);

                sql.parameters.AddWithValue("@DefaultFieldValue", fields[i].DefaultFieldValue);
                sql.parameters.AddWithValue("@SharedWith", (int)fields[i].SharedWith);

                sql.parameters.AddWithValue("@DataClassificationId", fields[i].DataClassificationId);
                sql.parameters.AddWithValue("@AnonymizationId", fields[i].AnonymizationId);

                sql.parameters.AddWithValue("@PositiveFieldValue", fields[i].PositiveFieldValue);
                sql.parameters.AddWithValue("@ReasonType", fields[i].ReasonType);
                sql.parameters.AddWithValue("@Reason", fields[i].Reason);

                if (fields[i].DatabaseColumn.IndexOf("Ingen") > -1) {
                    System.Diagnostics.Debug.Write("Ingen arv");
                }

                System.Data.SqlClient.SqlDataReader dr = sql.executeReader;

                if (dr.Read()) {
                    if (BaseOrg.Id == CurrentOrg.Id) fields[i].Id = TypeCast.ToInt(dr["id"]);
                }

                dr.Close();
                dr = null;
                sql.reset();

                if ((fields[i].FieldType == "dropdown" || fields[i].FieldType == "listview" || fields[i].FieldType == "map") && fields[i].OwnerId == 0) {
                    int c = 0;
                    foreach (string s in fields[i].CustomFieldValues) {
                        if ((s != "" && s != null) || c < fields[i].CustomFieldValues.Count - 1 || c == 0)
                            this.dynamicFields_addCustomValue(fields[i].Id, s);

                        c++;
                    }
                } else if (fields[i].FieldType == "button" || fields[i].FieldType == "linkbutton" || fields[i].FieldType == "sqllabel") {
                    this.dynamicFields_clearCustomValues(fields[i].Id);

                    foreach (string s in fields[i].CustomFieldValues) {
                        this.dynamicFields_addCustomValue(fields[i].Id, s, true);
                    }
                }

                //Partially shared field
                if (fields[i].SharedWith == DynamicField.ShareMethod.PartialShared && fields[i].SharedWithOrganisations != null && fields[i].SharedWithOrganisations.Count > 0) {
                    foreach (int orgId in fields[i].SharedWithOrganisations) {
                        this.dynamicFields_addSharedWith(fields[i].Id, orgId);
                    }
                }

            }

            //Update child organisations
            List<Organisation> orgs = this.Organisations_getOrganisations(CurrentOrg.Id);
            foreach (Organisation o in orgs) {
                dynamicFields_updateFields(U, BaseOrg, o, ref fields, resetPositionsForForeignOrganisations);
            }
        }

        public string[] dynamicFields_getAllStoredProcedures() {
            List<string> retval = new List<string>();
            sql.commandText = "dynamicFields_getStoredProcedures";
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(TypeCast.ToString(dr["SPECIFIC_NAME"]));
            }
            dr.Close();
            sql.reset();

            return retval.ToArray();
        }

        public string[] dynamicFields_getStoredProceduresById(int organisationId) {
            List<string> retval = new List<string>();
            sql.commandText = "dynamicFields_getStoredProcedures_byId";
            sql.parameters.AddWithValue("@Id", organisationId);
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                if (TypeCast.ToString(dr["SPECIFIC_NAME"]) != "")
                    retval.Add(TypeCast.ToString(dr["SPECIFIC_NAME"]));
            }
            dr.Close();
            sql.reset();

            return retval.ToArray();
        }

        public string[] dynamicFields_getGenericStoredProcedures() {
            List<string> retval = new List<string>();
            sql.commandText = "dynamicFields_getGenericStoredProcedures";
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(TypeCast.ToString(dr["SPECIFIC_NAME"]));
            }
            dr.Close();
            sql.reset();

            return retval.ToArray();
        }

        public string dynamicFields_getStoredProcedure(string name) {
            string retval = "";
            sql.commandType = System.Data.CommandType.StoredProcedure;
            sql.commandText = "dynamicFields_getStoredProcedure";
            sql.parameters.AddWithValue("@name", name);
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval += TypeCast.ToString(dr["Text"]);
            }
            dr.Close();
            sql.reset();
            return retval;
        }

        public bool dynamicFields_existsStoredProcedure(string name) {
            int retval = 0;
            sql.commandText = "dynamicFields_existsStoredProcedure";
            sql.parameters.AddWithValue("@name", name);
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = TypeCast.ToInt(dr["Exists"]);
            }
            dr.Close();
            sql.reset();

            return retval == 0 ? false : true;
        }

        public List<TableColumn> dynamicFields_getTableColumns(string tableName) {
            List<TableColumn> retval = new List<TableColumn>();
            sql.commandText = "dynamicFields_getTableColumns";
            sql.parameters.AddWithValue("@tableName", tableName);
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new TableColumn(TypeCast.ToString(dr["COLUMN_NAME"]), TypeCast.ToString(dr["DATA_TYPE"]), TypeCast.ToInt(dr["CHARACTER_MAXIMUM_LENGTH"])));
            }
            dr.Close();
            sql.reset();
            return retval;
        }

        public void dynamicFields_getTableColumnsRecursive(int organisationId, ref List<TableColumn> columns, DynamicField.DynamicTableName table) {
            Organisation org = Organisations_getOrganisation(organisationId);
            if (org != null) {
                if (org.ParentId != null)
                    dynamicFields_getTableColumnsRecursive(TypeCast.ToInt(org.ParentId), ref columns, table);

                List<TableColumn> tblCols = dynamicFields_getTableColumns(table.ToString() + "_" + org.Id);

                for (int i = 2; i < tblCols.Count; i++) {
                    columns.Add(tblCols[i]);
                }
            }

        }

        public void dynamicFields_dropStoredProcedure(string StoredProcedure) {
            try {
                sql.commandType = System.Data.CommandType.Text;
                sql.commandText = "IF EXISTS(SELECT 1 FROM sysobjects WHERE name = '" + StoredProcedure + "' AND USER_NAME(uid) = 'dbo')\n" +
                                                            "   DROP PROCEDURE dbo." + StoredProcedure + "";
                sql.execute();
                sql.reset();
            } catch (Exception exp) {
                sql.reset();
                //throw exp;
            }
        }

        public void dynamicFields_dropFunction(string Function) {
            try {
                sql.commandType = System.Data.CommandType.Text;
                sql.commandText = "IF EXISTS(SELECT 1 FROM sysobjects WHERE name = '" + Function + "' AND USER_NAME(uid) = 'dbo')\n" +
                                                            "   DROP FUNCTION dbo." + Function + "";
                sql.execute();
                sql.reset();
            } catch (Exception exp) {
                sql.reset();
                throw exp;
            }
        }

        public List<SQLLabelDataItem[]> generic_executeSQLForResultSet(string SQL) {
            List<SQLLabelDataItem[]> retval = new List<SQLLabelDataItem[]>();
            System.Data.SqlClient.SqlDataReader dr = null; // sql.executeReader;

            sql.commandType = System.Data.CommandType.Text;
            sql.commandText = SQL;
            dr = sql.executeReader;

            //int Rows = 0;
            //bool HeadersAdded = false;

            while (dr.Read()) {
                ////If there is more than one result (cols or rows), add headers
                //if ((dr.FieldCount > 1 || Rows > 0) && !HeadersAdded)
                //{
                //    string[] h = new string[dr.FieldCount];

                //    for (int i = 0; i < dr.FieldCount; i++)
                //    {
                //        h[i] = dr.GetName(i);
                //    }

                //    retval.Insert(0, h);
                //}

                SQLLabelDataItem[] r = new SQLLabelDataItem[dr.FieldCount];
                for (int i = 0; i < dr.FieldCount; i++) {

                    r[i] = new SQLLabelDataItem(dr.GetName(i), dr[i], dr.GetDataTypeName(i));
                }

                retval.Add(r);

                //Rows++;
            }

            dr.Close();
            sql.reset();

            return retval;
        }
        public List<SQLLabelDataItem[]> generic_executeStoredProcedureForResultSet(string SQL, SqlParameter[] Parameters) {
            List<SQLLabelDataItem[]> retval = new List<SQLLabelDataItem[]>();
            System.Data.SqlClient.SqlDataReader dr = null; // sql.executeReader;

            sql.commandType = System.Data.CommandType.StoredProcedure;
            sql.commandText = SQL;
            sql.parameters.AddRange(Parameters);
            dr = sql.executeReader;

            //int Rows = 0;
            //bool HeadersAdded = false;

            while (dr.Read()) {
                ////If there is more than one result (cols or rows), add headers
                //if ((dr.FieldCount > 1 || Rows > 0) && !HeadersAdded)
                //{
                //    string[] h = new string[dr.FieldCount];

                //    for (int i = 0; i < dr.FieldCount; i++)
                //    {
                //        h[i] = dr.GetName(i);
                //    }

                //    retval.Insert(0, h);
                //}

                SQLLabelDataItem[] r = new SQLLabelDataItem[dr.FieldCount];
                for (int i = 0; i < dr.FieldCount; i++) {

                    r[i] = new SQLLabelDataItem(dr.GetName(i), dr[i], dr.GetDataTypeName(i));
                }

                retval.Add(r);

                //Rows++;
            }

            dr.Close();
            sql.reset();

            return retval;
        }

        public void generic_executeSQL(string SQL) {
            try {
                sql.commandType = System.Data.CommandType.Text;
                sql.commandText = SQL;
                sql.execute();
                sql.reset();
            } catch (Exception exp) {
                ExceptionMail.SendException(new Exception(exp.Message + "\n\n\nSQL UPDATE:\n\n\n" + SQL));
                sql.reset();
                throw new Exception(exp.Message + "\n\n\nSQL UPDATE:\n\n\n" + SQL);
            }
        }

        public void dynamicFields_updateStoredProcedure(string StoredProcedure) {
            generic_executeSQL(StoredProcedure);
        }

        public void dynamicFields_refactorTable(string updateText) {
            try {
                sql.commandType = System.Data.CommandType.Text;
                sql.commandText = updateText;
                sql.execute();
                sql.reset();
            } catch (Exception exp) {
                sql.reset();
                throw new Exception(exp.Message + "\n\n\nSQL UPDATE:\n\n\n" + updateText);
            }
        }

        #endregion

        #region Dynamic Texts

        public List<DynamicText> Dynamictext_getDynamicTexts(string category, string sortOrder, string sortAsc) {

            List<DynamicText> retval = new List<DynamicText>();
            sql.commandText = "Dynamictext_getDynamictexts";
            sql.parameters.AddWithValue("@category", category);
            sql.parameters.AddWithValue("@sortOrder", sortOrder);
            sql.parameters.AddWithValue("@sortAsc", sortAsc);
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new DynamicText(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public List<DynamicText> Dynamictext_getAllDynamicTexts(string sortOrder, string sortAsc) {

            List<DynamicText> retval = new List<DynamicText>();
            sql.commandText = "Dynamictext_getAllDynamicTexts";
            sql.parameters.AddWithValue("@sortOrder", sortOrder);
            sql.parameters.AddWithValue("@sortAsc", sortAsc);
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new DynamicText(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public DynamicText Dynamictext_getDynamicText(int id) {
            DynamicText retval = null;
            sql.commandText = "Dynamictext_getDynamictext";
            sql.parameters.AddWithValue("@id", id);
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = new DynamicText(ref dr);
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public int Dynamictexts_createUpdateDynamictext(User U, DynamicText d) {
            sql.commandText = "Dynamictext_createDynamictext";
            sql.parameters.AddWithValue("@Id", TypeCast.ToIntOrNull(d.id));
            sql.parameters.AddWithValue("@headword", TypeCast.ToString(d.headword));
            sql.parameters.AddWithValue("@description", TypeCast.ToString(d.description));
            sql.parameters.AddWithValue("@category", TypeCast.ToString(d.category));
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            int dynamicTextId = 0;
            if (dr.Read()) {
                dynamicTextId = TypeCast.ToInt(dr["Status"]);
            }

            d.id = dynamicTextId;
            dr.Close();
            sql.reset();

            Events_addToEventLog(U.OrganisationId, U.Id, d.id == dynamicTextId ? "UPDATE" : "CREATE", "DYNAMICTEXTS", dynamicTextId.ToString(), d);

            return dynamicTextId;
        }

        public void Dynamictexts_deleteDynamictexts(User U, int id) {
            sql.commandText = "Dynamictext_delete";
            sql.parameters.AddWithValue("@Id", TypeCast.ToIntOrNull(id));
            sql.execute();
            sql.reset();

            Events_addToEventLog(U.OrganisationId, U.Id, "DELETE", "DYNAMICTEXTS", id.ToString(), null);
        }

        #endregion

        #region File_Dynamictext_Association

        public int File_Dynamictext_createAssociation(int fileId, int dynamicTextId) {
            sql.commandText = "File_Dynamictext_CreateAssociation";
            sql.parameters.AddWithValue("@FileId", fileId);
            sql.parameters.AddWithValue("@DynamicTextId", dynamicTextId);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            int intStatus = -1;
            if (dr.Read()) {
                intStatus = TypeCast.ToInt(dr["Status"]);
            }
            dr.Close();
            sql.reset();

            return intStatus;
        }

        public void File_Dynamictext_deleteAssociation(int fileId, object dynamicTextId) {
            sql.commandText = "File_DynamicText_deleteAssociation";
            sql.parameters.AddWithValue("@FileId", fileId);
            sql.parameters.AddWithValue("@DynamicTextId", TypeCast.ToIntOrDBNull(dynamicTextId));
            sql.execute();
            sql.reset();
        }

        #endregion

        #region ProjectTypes

        public void projectTypes_Primary_deleteProjectType(User U, int Id) {
            sql.commandText = "projectTypes_Primary_deleteProjectType";
            sql.parameters.AddWithValue("@Id", Id);

            sql.execute();

            sql.reset();

            Events_addToEventLog(U.OrganisationId, U.Id, "DELETE", "PROJECTTYPES_PRIMARY", Id.ToString(), null);
        }

        public List<ProjectTypePrimary> projectTypes_Primary_getProjectTypes(int organisationId, string SortOrder, string sortAsc) {
            List<ProjectTypePrimary> retval = new List<ProjectTypePrimary>();
            sql.commandText = "projectTypes_Primary_getProjectTypes";
            sql.parameters.AddWithValue("@organisationId", organisationId);
            sql.parameters.AddWithValue("@SortOrder", SortOrder);
            sql.parameters.AddWithValue("@sortAsc", sortAsc);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new ProjectTypePrimary(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public ProjectTypePrimary projectTypes_Primary_getProjectType(int organisationId, int Id) {
            ProjectTypePrimary retval = null;
            sql.commandText = "projectTypes_Primary_getProjectType";
            sql.parameters.AddWithValue("@Id", Id);
            sql.parameters.AddWithValue("@organisationId", organisationId);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = new ProjectTypePrimary(ref dr);
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public int projectTypes_Primary_UpdateProjectType(User U, ProjectTypePrimary P) {
            int retval = 0;

            sql.commandText = "projectTypes_Primary_UpdateProjectType";
            if (P.Id > 0) sql.parameters.AddWithValue("@Id", TypeCast.ToIntOrDBNull(P.Id));
            sql.parameters.AddWithValue("@Name", P.Name);
            sql.parameters.AddWithValue("@Description", P.Description);
            sql.parameters.AddWithValue("@AccountNo", P.AccountNo);
            sql.parameters.AddWithValue("@FinalSerialNo", P.FinalSerialNo);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = TypeCast.ToInt(dr["id"]);
            }
            dr.Close();
            sql.reset();

            Events_addToEventLog(U.OrganisationId, U.Id, P.Id == retval ? "UPDATE" : "CREATE", "PROJECTTYPES_PRIMARY", retval.ToString(), P);

            return retval;

        }

        public void projectTypes_Secondary_addExcludedUser(int OrganisationId, int ProjectTypeId, string UserId) {
            sql.commandText = "projectTypes_Secondary_addExcludedUser";
            sql.parameters.AddWithValue("@OrganisationId", OrganisationId);
            sql.parameters.AddWithValue("@ProjectTypeId", ProjectTypeId);
            sql.parameters.AddWithValue("@UserId", UserId);

            sql.execute();

            sql.reset();
        }

        public List<ProjectTypeUser> projectTypes_Secondary_getExcludedusers(int OrganisationId, int projectTypeId) {
            List<ProjectTypeUser> retval = new List<ProjectTypeUser>();
            sql.commandText = "projectTypes_Secondary_getExcludedusers";
            sql.parameters.AddWithValue("@organisationId", OrganisationId);
            if (projectTypeId > 0) sql.parameters.AddWithValue("@projectTypeId", projectTypeId);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new ProjectTypeUser(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }


        public void projectTypes_Secondary_DeleteProjectType(User U, int organsationId, int Id) {
            sql.commandText = "projectTypes_Secondary_DeleteProjectType";
            sql.parameters.AddWithValue("@Id", Id);
            sql.parameters.AddWithValue("@organsationId", organsationId);

            sql.execute();

            sql.reset();

            Events_addToEventLog(U.OrganisationId, U.Id, "DELETE", "PROJECTTYPES_SECONDARY", Id.ToString(), null);
        }

        public List<ProjectTypeSecondary> projectTypes_Secondary_getProjectTypes(int organisationId, int primaryProjectTypeId, string SortOrder, string sortAsc) {
            List<ProjectTypeSecondary> retval = new List<ProjectTypeSecondary>();
            sql.commandText = "projectTypes_Secondary_getProjectTypes";
            sql.parameters.AddWithValue("@organisationId", organisationId);
            sql.parameters.AddWithValue("@primaryProjectTypeId", primaryProjectTypeId);
            if (SortOrder != "") sql.parameters.AddWithValue("@SortOrder", SortOrder);
            if (sortAsc != "") sql.parameters.AddWithValue("@sortAsc", sortAsc);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new ProjectTypeSecondary(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public List<ProjectTypeSecondary> projectTypes_Secondary_getProjectTypesForRegistration(int organisationId, string UserId) {
            List<ProjectTypeSecondary> retval = new List<ProjectTypeSecondary>();
            sql.commandText = "projectTypes_Secondary_getProjectTypesForRegistration";
            sql.parameters.AddWithValue("@organisationId", organisationId);
            sql.parameters.AddWithValue("@UserId", UserId);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new ProjectTypeSecondary(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public ProjectTypeSecondary projectTypes_Secondary_getProjectTypeFromSerial(int organisationId, int serialNo) {
            ProjectTypeSecondary retval = null;
            sql.commandText = "projectTypes_Secondary_getProjectTypeFromSerial";
            sql.parameters.AddWithValue("@serialNo", serialNo);
            sql.parameters.AddWithValue("@organisationId", organisationId);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = new ProjectTypeSecondary(ref dr);
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public ProjectTypeSecondary projectTypes_Secondary_getProjectType(int organisationId, int Id) {
            ProjectTypeSecondary retval = null;
            sql.commandText = "projectTypes_Secondary_getProjectType";
            sql.parameters.AddWithValue("@Id", Id);
            sql.parameters.AddWithValue("@organisationId", organisationId);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = new ProjectTypeSecondary(ref dr);
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public int projectTypes_Secondary_UpdateProjectType(User U, ProjectTypeSecondary P) {
            int retval = 0;

            sql.commandText = "projectTypes_Secondary_UpdateProjectType";
            if (P.Id > 0) sql.parameters.AddWithValue("@Id", TypeCast.ToIntOrDBNull(P.Id));
            sql.parameters.AddWithValue("@Name", P.Name);
            sql.parameters.AddWithValue("@Description", P.Description);
            sql.parameters.AddWithValue("@OrganisationId", P.OrganisationId);
            sql.parameters.AddWithValue("@SerialNo", P.SerialNo);
            sql.parameters.AddWithValue("@primaryProjectTypeId", P.PrimaryProjectTypeId);
            if (P.UserGroupId > 0) {
                sql.parameters.AddWithValue("@UserGroupId", P.UserGroupId);
            }

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = TypeCast.ToInt(dr["id"]);
            }
            dr.Close();
            sql.reset();

            Events_addToEventLog(U.OrganisationId, U.Id, P.Id == retval ? "UPDATE" : "CREATE", "PROJECTTYPES_SECONDARY", retval.ToString(), P);

            return retval;

        }

        #endregion

        #region Files

        public List<FileFolder> fileArchive_getFolders(int organisationId, int FolderType, string userId) {
            return fileArchive_getFolders(organisationId, 0, FolderType, userId);
        }
        public List<FileFolder> fileArchive_getFolders(int organisationId, int parentId, int FolderType, object userId) {
            List<FileFolder> retval = new List<FileFolder>();
            sql.commandText = "fileArchive_getFolders";
            sql.parameters.AddWithValue("@organisationId", organisationId);
            sql.parameters.AddWithValue("@parentId", TypeCast.ToIntOrNull(parentId));
            sql.parameters.AddWithValue("@FolderType", TypeCast.ToInt(FolderType));
            if (FolderType != 0 && FolderType != 10) sql.parameters.AddWithValue("@userId", TypeCast.ToStringOrDBNull(userId));

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new FileFolder(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public FileFolder fileArchive_getFolderById(int Id) {
            FileFolder retval = new FileFolder();
            sql.commandText = "fileArchive_getFolderById";
            sql.parameters.AddWithValue("@Id", Id);
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval = new FileFolder(ref dr);
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public List<File> fileArchive_getFilesByFolderId(User U, int folderId, string SortOrder, string sortAsc) {
            List<File> retval = new List<File>();
            sql.commandText = "z_Filearchive_getFilesByFolderId_" + U.OrganisationId;
            sql.parameters.AddWithValue("@folderId", folderId);
            if (SortOrder != "" && SortOrder != null) sql.parameters.AddWithValue("@sortOrder", SortOrder);
            if (sortAsc != "" && sortAsc != null) sql.parameters.AddWithValue("@sortAsc", sortAsc);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new File(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public List<File> fileArchive_getFileByUserAndFolderType(User U, object folderType, object userId, string SortOrder, string sortAsc) {
            List<File> retval = new List<File>();
            sql.commandText = "z_Filearchive_getFilesByUserAndFolderType_" + U.OrganisationId;
            sql.parameters.AddWithValue("@folderType", TypeCast.ToIntOrDBNull(folderType));
            sql.parameters.AddWithValue("@userId", TypeCast.ToStringOrDBNull(userId));
            if (SortOrder != "" && SortOrder != null) sql.parameters.AddWithValue("@sortOrder", SortOrder);
            if (sortAsc != "" && sortAsc != null) sql.parameters.AddWithValue("@sortAsc", sortAsc);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new File(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public List<File> fileArchive_getFileByOrgIdAndFolderType(object folderType, int organisationId, string SortOrder, string sortAsc) {
            List<File> retval = new List<File>();
            sql.commandText = "z_Filearchive_getFilesByOrgIdAndFolderType_" + organisationId;
            sql.parameters.AddWithValue("@folderType", TypeCast.ToIntOrDBNull(folderType));
            sql.parameters.AddWithValue("@OrgId", TypeCast.ToInt(organisationId));
            if (SortOrder != "" && SortOrder != null) sql.parameters.AddWithValue("@sortOrder", SortOrder);
            if (sortAsc != "" && sortAsc != null) sql.parameters.AddWithValue("@sortAsc", sortAsc);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new File(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        //TODO: Hvad skal denne funktion bruges til?
        public int fileArchive_countFolders(int organisationId, int parentId, int FolderType, object userId) {
            sql.commandText = "fileArchive_countFolders";
            sql.parameters.AddWithValue("@organisationId", organisationId);
            sql.parameters.AddWithValue("@parentId", TypeCast.ToIntOrNull(parentId));
            sql.parameters.AddWithValue("@FolderType", TypeCast.ToInt(FolderType));
            sql.parameters.AddWithValue("@userId", TypeCast.ToStringOrDBNull(userId));

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;

            int retval = 0;

            if (dr.Read()) {
                retval = TypeCast.ToInt(dr["retval"]);
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public FileCRM2 fileArchive_getFileById(object Id) {
            FileCRM2 retval = new FileCRM2();
            sql.commandText = "fileArchive_getFileById";
            sql.parameters.AddWithValue("@Id", TypeCast.ToIntOrDBNull(Id));

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval = new FileCRM2(ref dr);
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public int fileArchive_CreateUpdateFolder(User U, FileFolder f) {
            sql.commandText = "fileArchive_CreateUpdateFolder";
            sql.parameters.AddWithValue("@Id", TypeCast.ToIntOrDBNull(f.Id));
            if (TypeCast.ToInt(f.ParentId) > 0) sql.parameters.AddWithValue("@parentId", TypeCast.ToIntOrDBNull(f.ParentId));
            sql.parameters.AddWithValue("@FolderType", f.FolderType);
            sql.parameters.AddWithValue("@name", f.Name);
            sql.parameters.AddWithValue("@userId", f.userId);
            sql.parameters.AddWithValue("@organisationId", f.organisationId);

            int retval = 0;

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = TypeCast.ToInt(dr["status"]);
            }
            dr.Close();
            sql.reset();

            Events_addToEventLog(U.OrganisationId, U.Id, f.Id == retval ? "UPDATE" : "CREATE", "FILEFOLDER", retval.ToString(), f);

            return retval;
        }

        //TODO: Hvad skal denne funktion bruges til?
        public int fileArchive_countFoldersByUserid(object Id, int folderType) {
            sql.commandText = "fileArchive_countFoldersByUserid";
            sql.parameters.AddWithValue("@userId", TypeCast.ToStringOrDBNull(Id));
            sql.parameters.AddWithValue("@folderType", TypeCast.ToInt(folderType));

            int retval = 0;

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = TypeCast.ToInt(dr["status"]);
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public void fileArchive_DeleteFolder(User U, int Id, object userId) {
            sql.commandText = "filearchive_DeleteFileFolder";
            sql.parameters.AddWithValue("@id", Id);
            sql.parameters.AddWithValue("@userId", TypeCast.ToStringOrDBNull(userId));
            sql.execute();
            sql.reset();

            Events_addToEventLog(U.OrganisationId, U.Id, "DELETE", "FILEFOLDERS", Id.ToString(), null);
        }

        public void filearchive_updateFileShare(int FileId, int SharedWithOrganisationId) {
            sql.commandText = "filearchive_updateFileShare";
            sql.parameters.AddWithValue("@FileId", FileId);
            sql.parameters.AddWithValue("@SharedWithOrganisationId", SharedWithOrganisationId);
            sql.execute();
            sql.reset();
        }

        public void filearchive_resetShared(int FileId) {
            sql.commandText = "filearchive_resetShared";
            sql.parameters.AddWithValue("@FileId", FileId);
            sql.execute();
            sql.reset();
        }

        public int filearchive_insertFile(User U, File F) {
            int retval = 0;


            sql.commandText = "filearchive_insertFile";
            sql.parameters.AddWithValue("@Id", TypeCast.ToIntOrDBNull(F.Id));
            sql.parameters.AddWithValue("@binary", TypeCast.ToInt(F.binary));
            sql.parameters.AddWithValue("@filename", TypeCast.ToString(F.filename));
            sql.parameters.AddWithValue("@contenttype", TypeCast.ToString(F.contenttype));
            sql.parameters.AddWithValue("@description", TypeCast.ToStringOrDBNull(F.description));
            sql.parameters.AddWithValue("@userId", TypeCast.ToString(F.userId));
            if (TypeCast.ToInt(F.FileFolder) > 0) sql.parameters.AddWithValue("@FileFolder", TypeCast.ToIntOrDBNull(F.FileFolder));
            sql.parameters.AddWithValue("@contentgroup", TypeCast.ToStringOrDBNull(F.contentgroup));
            sql.parameters.AddWithValue("@contentlength", TypeCast.ToInt(F.contentlength));
            sql.parameters.AddWithValue("@folderType", TypeCast.ToIntOrDBNull(F.folderType));
            sql.parameters.AddWithValue("@organisationId", TypeCast.ToInt(F.organisationId));
            sql.parameters.AddWithValue("@contactId", TypeCast.ToIntOrDBNull(F.contactId));
            sql.parameters.AddWithValue("@Category", F.Category);
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = TypeCast.ToInt(dr["status"]);
            }
            dr.Close();
            sql.reset();

            Events_addToEventLog(U.OrganisationId, U.Id, F.Id == retval ? "UPDATE" : "CREATE", "FILES", retval.ToString(), F);

            return retval;

        }

        public void filearchive_deleteBinary(int Id) {
            sql.commandText = "filearchive_deleteBinary";
            sql.parameters.AddWithValue("@Id", Id);
            sql.execute();
            sql.reset();
        }

        public int filearchive_insertBinary(Binary B) {
            int retval = 0;
            sql.commandText = "filearchive_InsertBinary";
            sql.parameters.AddWithValue("@Id", TypeCast.ToIntOrDBNull(B.Id));
            sql.parameters.AddWithValue("@data", B.Data);
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = TypeCast.ToInt(dr["status"]);
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public IDS.EBSTCRM.Base.Binary filearchive_getBinary(User U, object Id) {
            Binary retval = new Binary();
            sql.commandText = "filearchive_getBinary";
            sql.parameters.AddWithValue("@Id", TypeCast.ToIntOrDBNull(Id));
            sql.parameters.AddWithValue("@userId", U.Id);
            sql.parameters.AddWithValue("@organisationId", U.OrganisationId);
            if (System.Web.HttpContext.Current != null && System.Web.HttpContext.Current.Request != null) {
                var ip = System.Web.HttpContext.Current.Request.UserHostAddress;
                sql.parameters.AddWithValue("@IPaddress", ip);
            }
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) { retval = new Binary(ref dr); }
            dr.Close();
            sql.reset();
            return retval;
        }

        public void filearchive_moveFilesToContact(int OldContactId, int NewContactId) {
            sql.commandText = "filearchive_moveFilesToContact";
            sql.parameters.AddWithValue("@OldContactId", OldContactId);
            sql.parameters.AddWithValue("@NewContactId", NewContactId);
            sql.execute();
            sql.reset();
        }

        public void filearchive_DeleteFile(User U, int Id, object userId) {
            sql.commandText = "filearchive_DeleteFile";
            sql.parameters.AddWithValue("@Id", TypeCast.ToIntOrDBNull(Id));
            sql.parameters.AddWithValue("@UserId", TypeCast.ToStringOrDBNull(userId));
            sql.parameters.AddWithValue("@OrganisationId", U.OrganisationId);
            if (System.Web.HttpContext.Current != null && System.Web.HttpContext.Current.Request != null) {
                var ip = System.Web.HttpContext.Current.Request.UserHostAddress;
                sql.parameters.AddWithValue("@IPaddress", ip);
            }

            sql.execute();
            sql.reset();

            Events_addToEventLog(U.OrganisationId, U.Id, "DELETE", "FILES", Id.ToString(), null);
        }

        //** Ny metode der kun bliver brugt til WS, ESCRM-163
        public int filearchive_DeleteFileWS(User U, int Id, object userId)
        {
            int result = 0;

            SQLBase sql = new SQLBase(ConfigurationManager.AppSettings["ConnectionString"]);

            sql.commandText = "filearchive_DeleteFile";
            sql.parameters.AddWithValue("@Id", TypeCast.ToIntOrDBNull(Id));
            sql.parameters.AddWithValue("@UserId", TypeCast.ToStringOrDBNull(userId));
            sql.parameters.AddWithValue("@OrganisationId", U.OrganisationId);
            if (System.Web.HttpContext.Current != null && System.Web.HttpContext.Current.Request != null)
            {
                var ip = System.Web.HttpContext.Current.Request.UserHostAddress;
                sql.parameters.AddWithValue("@IPaddress", ip);
            }

            result = sql._execute();
            sql.reset();

            Events_addToEventLog(U.OrganisationId, U.Id, "DELETE", "FILES", Id.ToString(), null);

            return result;
        }

        public List<File> filearchive_QuickSearch(int organisationId, string userId, string query, object type, int folderType, string SortOrder, string sortAsc) {
            List<File> retval = new List<File>();
            sql.commandText = "z_Filearchive_Quicksearch_" + organisationId;
            sql.parameters.AddWithValue("@organisationId", TypeCast.ToInt(organisationId));
            sql.parameters.AddWithValue("@userId", TypeCast.ToString(userId));
            sql.parameters.AddWithValue("@query", TypeCast.ToString(query));
            sql.parameters.AddWithValue("@type", TypeCast.ToStringOrDBNull(type));
            sql.parameters.AddWithValue("@searchIn", TypeCast.ToInt(folderType));
            if (SortOrder != "" && SortOrder != null) sql.parameters.AddWithValue("@sortOrder", SortOrder);
            if (sortAsc != "" && sortAsc != null) sql.parameters.AddWithValue("@sortAsc", sortAsc);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                File f = new File(ref dr);
                f.Path = TypeCast.ToString(dr["path"]);
                retval.Add(f);

            }
            dr.Close();
            sql.reset();

            return retval;

        }

        public ArrayList filearchive_getDynamicTextIds(int fileId) {
            ArrayList retval = new ArrayList();
            sql.commandText = "filearchive_getDynamicTextIds";
            sql.parameters.AddWithValue("@FileId", fileId);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(TypeCast.ToInt(dr["DynamicTextId"]));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public List<DynamicText> filearchive_getDynamicTexts(int fileId) {
            List<DynamicText> retval = new List<DynamicText>();
            sql.commandText = "filearchive_getDynamicTexts";
            sql.parameters.AddWithValue("@FileId", TypeCast.ToInt(fileId));

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new DynamicText(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;

        }

        public List<FileCRM2> fileArchive_getFileByContactIdCRM2(int contactId, string UserId, int OrganisationId) {
            List<FileCRM2> retval = new List<FileCRM2>();
            sql.commandText = "fileArchive_getFileByContactIdCRM2";
            sql.parameters.AddWithValue("@contactId", TypeCast.ToIntOrDBNull(contactId));
            sql.parameters.AddWithValue("@UserId", UserId);
            sql.parameters.AddWithValue("@OrganisationId", OrganisationId);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new FileCRM2(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public List<File> fileArchive_getFilesByContactId(int contactId) {
            return fileArchive_getFilesByContactId(contactId, "", 0);
        }
        public List<File> fileArchive_getFilesByContactId(int contactId, string UserId, int OrganisationId) {
            List<File> retval = new List<File>();
            sql.commandText = "fileArchive_getFileByContactId";
            sql.parameters.AddWithValue("@contactId", TypeCast.ToIntOrDBNull(contactId));
            if (UserId != "") sql.parameters.AddWithValue("@UserId", UserId);
            if (OrganisationId > 0) sql.parameters.AddWithValue("@OrganisationId", OrganisationId);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new File(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }


        #endregion

        #region Note
        public List<Note> notes_getNotesFromContactOrCompany(int companyId, int contactId, User U) {
            List<Note> retval = new List<Note>();
            sql.commandText = "notes_getNotesFromContactOrCompany";
            sql.parameters.AddWithValue("@organisationId", U.OrganisationId);
            sql.parameters.AddWithValue("@userRole", (int)U.UserRole);
            sql.parameters.AddWithValue("@userId", U.Id);
            sql.parameters.AddWithValue("@companyId", companyId);
            sql.parameters.AddWithValue("@contactId", contactId);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new Note(ref dr, false));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public List<Note> notes_getNotesFromContact(int contactId) {
            return notes_getNotesFromContact(contactId, null);
        }

        public List<Note> notes_getNotesFromContact(int contactId, User U) {
            List<Note> retval = new List<Note>();
            sql.commandText = "notes_getNotesFromContact";
            if (U != null) {
                sql.parameters.AddWithValue("@organisationId", U.OrganisationId);
                sql.parameters.AddWithValue("@userRole", (int)U.UserRole);
                sql.parameters.AddWithValue("@userId", U.Id);
            }
            sql.parameters.AddWithValue("@contactId", contactId);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new Note(ref dr, false));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public List<NoteWithReminderAndContactData> notes_getNotesWithRemindersAndContactData(User U, bool ShowExipredOnly) {
            List<NoteWithReminderAndContactData> retval = new List<NoteWithReminderAndContactData>();
            sql.commandText = "notes_getNotesWithRemindersAndContactData";
            sql.parameters.AddWithValue("@userId", U.Id);
            sql.parameters.AddWithValue("@ShowExipredOnly", ShowExipredOnly);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new NoteWithReminderAndContactData(ref dr, false));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public List<NoteWithReminder> notes_getNotesWithReminders(User U, bool ShowExipredOnly) {
            List<NoteWithReminder> retval = new List<NoteWithReminder>();
            sql.commandText = "notes_getNotesWithReminders";
            sql.parameters.AddWithValue("@userId", U.Id);
            sql.parameters.AddWithValue("@ShowExipredOnly", ShowExipredOnly);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new NoteWithReminder(ref dr, false));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public List<Note> notes_getNotesFromCompany(int companyId, User U) {
            List<Note> retval = new List<Note>();
            sql.commandText = "notes_getNotesFromCompany";
            sql.parameters.AddWithValue("@organisationId", U.OrganisationId);
            sql.parameters.AddWithValue("@userRole", (int)U.UserRole);
            sql.parameters.AddWithValue("@userId", U.Id);
            sql.parameters.AddWithValue("@companyId", companyId);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new Note(ref dr, false));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public Note notes_getNote(User U, int Id) {
            Note retval = null;
            sql.commandText = "notes_getNote";
            sql.parameters.AddWithValue("@Id", Id);
            sql.parameters.AddWithValue("@userId", U.Id);
            sql.parameters.AddWithValue("@organisationId", U.OrganisationId);
            if (System.Web.HttpContext.Current != null && System.Web.HttpContext.Current.Request != null) {
                var ip = System.Web.HttpContext.Current.Request.UserHostAddress;
                sql.parameters.AddWithValue("@IPaddress", ip);
            }

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = new Note(ref dr, true);
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public int notes_updateNote(User U, Note n) {
            int retval = 0;
            sql.commandText = "notes_updateNote";
            sql.parameters.AddWithValue("@id", TypeCast.ToIntOrDBNull(n.id));
            sql.parameters.AddWithValue("@userId", U.Id);
            sql.parameters.AddWithValue("@organisationId", n.organisationId);
            sql.parameters.AddWithValue("@contactId", TypeCast.ToIntOrDBNull(n.contactId));
            sql.parameters.AddWithValue("@companyId", TypeCast.ToIntOrDBNull(n.companyId));
            sql.parameters.AddWithValue("@VisibleTo", n.VisibleTo);
            sql.parameters.AddWithValue("@isHighPriority", n.isHighPriority);
            sql.parameters.AddWithValue("@name", n.name);
            sql.parameters.AddWithValue("@note", n.note);
            if (n.ReminderDate != null) sql.parameters.AddWithValue("@ReminderDate", n.ReminderDate);
            if (n.ReminderDate != null && n.ReminderDismissed) sql.parameters.AddWithValue("@ReminderDismissed", n.ReminderDismissed);

            if (n.NoteType != "" && n.NoteType != null) sql.parameters.AddWithValue("@NoteType", n.NoteType);
            if (n.NoteType2 != "" && n.NoteType2 != null) sql.parameters.AddWithValue("@NoteType2", n.NoteType2);

            if (n.dateCreated != null) sql.parameters.AddWithValue("@dateCreated", n.dateCreated);
            if (n.NoteDate != null) sql.parameters.AddWithValue("@NoteDate", n.NoteDate);

            sql.parameters.AddWithValue("@NoteFromBatchJob", n.NoteFromBatchJob);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = TypeCast.ToInt(dr["id"]);
            }
            dr.Close();
            sql.reset();

            Events_addToEventLog(U.OrganisationId, U.Id, n.id == retval ? "UPDATE" : "CREATE", "NOTES", retval.ToString(), n);

            return retval;
        }

        public void notes_moveToCompany(int oldCompany, int newCompany) {
            sql.commandText = "notes_moveToCompany";
            sql.parameters.AddWithValue("@oldCompany", oldCompany);
            sql.parameters.AddWithValue("@newCompany", newCompany);
            sql.execute();
            sql.reset();
        }

        public void notes_moveToContact(int oldContact, int newContact) {
            sql.commandText = "notes_moveToContact";
            sql.parameters.AddWithValue("@oldContact", oldContact);
            sql.parameters.AddWithValue("@newContact", newContact);
            sql.execute();
            sql.reset();
        }
 
        public void avn_moveToContact(int oldContact, int newContact) {
            sql.commandText = "avn_moveToContact";
            sql.parameters.AddWithValue("@oldContact", oldContact);
            sql.parameters.AddWithValue("@newContact", newContact);
            sql.execute();
            sql.reset();
        }
        
        public void avn_moveToCompany(int oldCompany, int newCompany) {
            sql.commandText = "avn_moveToCompany";
            sql.parameters.AddWithValue("@oldCompany", oldCompany);
            sql.parameters.AddWithValue("@newCompany", newCompany);
            sql.execute();
            sql.reset();
        }
        
        public void notes_deleteNote(User user, int id) {
            sql.commandText = "notes_deleteNote";
            sql.parameters.AddWithValue("@Id", id);
            sql.parameters.AddWithValue("@UserId", user.Id);
            sql.parameters.AddWithValue("@OrganisationId", user.OrganisationId);
            if (System.Web.HttpContext.Current != null && System.Web.HttpContext.Current.Request != null) {
                var ip = System.Web.HttpContext.Current.Request.UserHostAddress;
                sql.parameters.AddWithValue("@IPaddress", ip);
            }

            sql.execute();
            sql.reset();
        }
        
        public void flags_moveToCompany(int oldCompany, int newCompany) {
            sql.commandText = "flags_moveToCompany";
            sql.parameters.AddWithValue("@oldCompany", oldCompany);
            sql.parameters.AddWithValue("@newCompany", newCompany);
            sql.execute();
            sql.reset();
        }
        
        public void notes_updateNoteShare(int NoteId, int SharedWithOrganisationId) {
            sql.commandText = "notes_updateNoteShare";
            sql.parameters.AddWithValue("@NoteId", NoteId);
            sql.parameters.AddWithValue("@SharedWithOrganisationId", SharedWithOrganisationId);
            sql.execute();
            sql.reset();
        }
        
        public void contracts_moveToContact(int oldContact, int newContact) {
            sql.commandText = "contracts_moveToContact";
            sql.parameters.AddWithValue("@oldContact", oldContact);
            sql.parameters.AddWithValue("@newContact", newContact);
            sql.execute();
            sql.reset();
        }
        
        public void flags_moveToContact(int oldContact, int newContact) {
            sql.commandText = "flags_moveToContact";
            sql.parameters.AddWithValue("@oldContact", oldContact);
            sql.parameters.AddWithValue("@newContact", newContact);
            sql.execute();
            sql.reset();
        }
        
        public void log_moveToContact(User U, int oldContact, int newContact) {
            sql.commandText = "log_moveToContact";
            sql.parameters.AddWithValue("@oldContact", oldContact);
            sql.parameters.AddWithValue("@newContact", newContact);
            sql.parameters.AddWithValue("@userId", U.Id);
            sql.execute();
            sql.reset();
        }
        #endregion

        #region AVN Note Categories

        public List<AvnNoteCategory> AVN_CategoriesWithNotes(User U) {
            List<AvnNoteCategory> retval = new List<AvnNoteCategory>();
            sql.commandText = "AVN_CategoriesWithNotes";
            sql.parameters.AddWithValue("@organisationId", U.OrganisationId);
            sql.parameters.AddWithValue("@UserId", U.Id);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new AvnNoteCategory(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public List<AvnNoteCategory> AvnNoteCategories_GetAll(User U) {
            return AvnNoteCategories_GetAll(U, 0);
        }

        public List<AvnNoteCategory> AvnNoteCategories_GetAll(User U, int NoteId) {
            List<AvnNoteCategory> retval = new List<AvnNoteCategory>();
            sql.commandText = "AVN_NoteCategories_GetAll";
            sql.parameters.AddWithValue("@organisationId", U.OrganisationId);
            if (NoteId > 0) sql.parameters.AddWithValue("@NoteId", NoteId);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new AvnNoteCategory(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public AvnNoteCategory AvnNoteCategories_Get(User U, int Id) {
            AvnNoteCategory retval = null;
            sql.commandText = "AVN_NoteCategories_Get";
            sql.parameters.AddWithValue("@categoryId", Id);
            sql.parameters.AddWithValue("@organisationId", U.OrganisationId);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = new AvnNoteCategory(ref dr);
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public int AvnNoteCategories_Update(User U, AvnNoteCategory n) {
            int retval = 0;
            sql.commandText = "AVN_noteCategories_Update";
            sql.parameters.AddWithValue("@categoryId", TypeCast.ToIntOrDBNull(n.Id));
            sql.parameters.AddWithValue("@userId", U.Id);
            sql.parameters.AddWithValue("@organisationId", U.OrganisationId);
            sql.parameters.AddWithValue("@categoryName", n.CategoryName);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = TypeCast.ToInt(dr["id"]);
            }
            dr.Close();
            sql.reset();

            Events_addToEventLog(U.OrganisationId, U.Id, n.Id == retval ? "UPDATE" : "CREATE", "AVNNOTECATEGORY", retval.ToString(), n);

            return retval;
        }

        public void AvnNoteCategories_Delete(User U, int id) {
            sql.commandText = "AVN_noteCategories_Delete";
            sql.parameters.AddWithValue("@categoryId", id);
            sql.parameters.AddWithValue("@organisationId", U.OrganisationId);
            sql.execute();
            sql.reset();
        }

        #endregion

        #region Note Categories

        public List<NoteCategory> noteCategories_GetAll(User U) {
            return noteCategories_GetAll(U, 0);
        }

        public List<NoteCategory> noteCategories_GetAll(User U, int NoteId) {
            List<NoteCategory> retval = new List<NoteCategory>();
            sql.commandText = "noteCategories_GetAll";
            sql.parameters.AddWithValue("@organisationId", U.OrganisationId);
            if (NoteId > 0) sql.parameters.AddWithValue("@NoteId", NoteId);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new NoteCategory(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public NoteCategory noteCategories_Get(User U, int Id) {
            NoteCategory retval = null;
            sql.commandText = "noteCategories_Get";
            sql.parameters.AddWithValue("@categoryId", Id);
            sql.parameters.AddWithValue("@organisationId", U.OrganisationId);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = new NoteCategory(ref dr);
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public int noteCategories_Update(User U, NoteCategory n) {
            int retval = 0;
            sql.commandText = "noteCategories_Update";
            sql.parameters.AddWithValue("@categoryId", TypeCast.ToIntOrDBNull(n.Id));
            sql.parameters.AddWithValue("@userId", U.Id);
            sql.parameters.AddWithValue("@organisationId", U.OrganisationId);
            sql.parameters.AddWithValue("@categoryName", n.CategoryName);
            sql.parameters.AddWithValue("@isDefault", n.IsDefault);
            sql.parameters.AddWithValue("@Level", (int)n.Level);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = TypeCast.ToInt(dr["id"]);
            }
            dr.Close();
            sql.reset();

            Events_addToEventLog(U.OrganisationId, U.Id, n.Id == retval ? "UPDATE" : "CREATE", "NOTECATEGORY", retval.ToString(), n);

            return retval;
        }

        public void noteCategories_Delete(User U, int id) {
            sql.commandText = "noteCategories_Delete";
            sql.parameters.AddWithValue("@categoryId", id);
            sql.parameters.AddWithValue("@organisationId", U.OrganisationId);
            sql.execute();
            sql.reset();
        }

        #endregion

        #region File Categories

        public List<FileCategory> fileCategories_GetAll(User U) {
            return fileCategories_GetAll(U, 0);
        }

        public List<FileCategory> fileCategories_GetAll(User U, int FileId) {
            List<FileCategory> retval = new List<FileCategory>();
            sql.commandText = "fileCategories_GetAll";
            sql.parameters.AddWithValue("@organisationId", U.OrganisationId);
            if (FileId > 0) sql.parameters.AddWithValue("@FileId", FileId);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new FileCategory(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public FileCategory fileCategories_Get(User U, int Id) {
            FileCategory retval = null;
            sql.commandText = "fileCategories_Get";
            sql.parameters.AddWithValue("@categoryId", Id);
            sql.parameters.AddWithValue("@organisationId", U.OrganisationId);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = new FileCategory(ref dr);
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public int fileCategories_Update(User U, FileCategory n) {
            int retval = 0;
            sql.commandText = "fileCategories_Update";
            sql.parameters.AddWithValue("@categoryId", TypeCast.ToIntOrDBNull(n.Id));
            sql.parameters.AddWithValue("@userId", U.Id);
            sql.parameters.AddWithValue("@organisationId", U.OrganisationId);
            sql.parameters.AddWithValue("@categoryName", n.CategoryName);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = TypeCast.ToInt(dr["id"]);
            }
            dr.Close();
            sql.reset();

            Events_addToEventLog(U.OrganisationId, U.Id, n.Id == retval ? "UPDATE" : "CREATE", "FILECATEGORY", retval.ToString(), n);

            return retval;
        }

        public void fileCategories_Delete(User U, int id) {
            sql.commandText = "fileCategories_Delete";
            sql.parameters.AddWithValue("@categoryId", id);
            sql.parameters.AddWithValue("@organisationId", U.OrganisationId);
            sql.execute();
            sql.reset();
        }

        #endregion

        #region CaseNumbers

        public void caseNumbers_removeFromProjectType(int OrganisationId, int Id, int SecondaryProjectTypeId) {
            sql.commandText = "caseNumbers_removeFromProjectType";
            sql.parameters.AddWithValue("@OrganisationId", OrganisationId);
            sql.parameters.AddWithValue("@SecondaryProjectTypeId", SecondaryProjectTypeId);
            sql.parameters.AddWithValue("@Id", Id);

            sql.execute();

            sql.reset();
        }

        public void caseNumbers_addToProjectType(int organisationId, int caseNumberId, int projectTypeId) {
            sql.commandText = "caseNumbers_addToProjectType";
            sql.parameters.AddWithValue("@OrganisationId", organisationId);
            sql.parameters.AddWithValue("@caseNumberId", caseNumberId);
            sql.parameters.AddWithValue("@projectTypeId", projectTypeId);

            sql.execute();

            sql.reset();
        }

        public void caseNumbers_deleteCaseNumber(User U, int OrganisationId, int Id) {
            sql.commandText = "caseNumbers_deleteCaseNumber";
            sql.parameters.AddWithValue("@OrganisationId", OrganisationId);
            sql.parameters.AddWithValue("@Id", Id);

            sql.execute();

            sql.reset();

            Events_addToEventLog(U.OrganisationId, U.Id, "DELETE", "CASENUMBERS", Id.ToString(), null);
        }

        public List<CaseNumber> caseNumbers_getCaseNumbers(int OrganisationId, string SortOrder, string sortAsc) {
            List<CaseNumber> retval = new List<CaseNumber>();
            sql.commandText = "caseNumbers_getCaseNumbers";
            sql.parameters.AddWithValue("@organisationId", OrganisationId);
            if (SortOrder != "") sql.parameters.AddWithValue("@SortOrder", SortOrder);
            if (sortAsc != "") sql.parameters.AddWithValue("@sortAsc", sortAsc);


            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new CaseNumber(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public List<CaseNumber> projectTypes_getCaseNumbers(int OrganisationId, int secondaryProjectTypeId, string SortOrder, string sortAsc, string UserId) {
            List<CaseNumber> retval = new List<CaseNumber>();
            sql.commandText = "projectTypes_getCaseNumbers";
            sql.parameters.AddWithValue("@organisationId", OrganisationId);
            sql.parameters.AddWithValue("@secondaryProjectTypeId", secondaryProjectTypeId);
            if (SortOrder != "") sql.parameters.AddWithValue("@SortOrder", SortOrder);
            if (sortAsc != "") sql.parameters.AddWithValue("@sortAsc", sortAsc);
            if (UserId.Length == 36) sql.parameters.AddWithValue("@UserId", UserId);


            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new CaseNumber(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public void projectTypes_Secondary_casenumbers_addExcludedUser(int OrganisationId, int projectTypeSecondaryId, int CasenumberId, string UserId) {
            sql.commandText = "projectTypes_Secondary_casenumbers_addExcludedUser";
            sql.parameters.AddWithValue("@OrganisationId", OrganisationId);
            sql.parameters.AddWithValue("@projectTypeSecondaryId", projectTypeSecondaryId);
            sql.parameters.AddWithValue("@CasenumberId", CasenumberId);
            sql.parameters.AddWithValue("@UserId", UserId);

            sql.execute();

            sql.reset();
        }

        public void casenumbers_addExcludedUser(int OrganisationId, int CasenumberId, string UserId) {
            sql.commandText = "casenumbers_addExcludedUser";
            sql.parameters.AddWithValue("@OrganisationId", OrganisationId);
            sql.parameters.AddWithValue("@CasenumberId", CasenumberId);
            sql.parameters.AddWithValue("@UserId", UserId);

            sql.execute();

            sql.reset();
        }

        public List<CaseNumberUser> projectTypes_Secondary_casenumbers_getExcludedusers(int OrganisationId, int projectTypeSecondaryId, int caseNumberId) {
            List<CaseNumberUser> retval = new List<CaseNumberUser>();
            sql.commandText = "projectTypes_Secondary_casenumbers_getExcludedusers";
            sql.parameters.AddWithValue("@organisationId", OrganisationId);
            sql.parameters.AddWithValue("@CasenumberId", caseNumberId);
            sql.parameters.AddWithValue("@projectTypeSecondaryId", projectTypeSecondaryId);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new CaseNumberUser(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public List<CaseNumberUser> casenumbers_getExcludedusers(int OrganisationId, int CasenumberId) {
            List<CaseNumberUser> retval = new List<CaseNumberUser>();
            sql.commandText = "casenumbers_getExcludedusers";
            sql.parameters.AddWithValue("@organisationId", OrganisationId);
            if (CasenumberId > 0) sql.parameters.AddWithValue("@CasenumberId", CasenumberId);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new CaseNumberUser(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public CaseNumber caseNumbers_getCaseNumberFromNumber(int OrganisationId, int Number) {
            CaseNumber retval = null;
            sql.commandText = "caseNumbers_getCaseNumberFromNumber";
            sql.parameters.AddWithValue("@OrganisationId", OrganisationId);
            sql.parameters.AddWithValue("@Number", Number);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = new CaseNumber(ref dr);
            }
            dr.Close();
            sql.reset();

            return retval;
        }
        public CaseNumber caseNumbers_getCaseNumber(int OrganisationId, int Id) {
            CaseNumber retval = null;
            sql.commandText = "caseNumbers_getCaseNumber";
            sql.parameters.AddWithValue("@OrganisationId", OrganisationId);
            sql.parameters.AddWithValue("@Id", Id);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = new CaseNumber(ref dr);
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public int caseNumbers_UpdateCaseNumber(User U, CaseNumber c, int projectTypeId) {
            int retval = 0;
            sql.commandText = "caseNumbers_UpdateCaseNumber";
            sql.parameters.AddWithValue("@id", TypeCast.ToIntOrDBNull(c.Id));
            sql.parameters.AddWithValue("@organisationId", c.OrganisationId);
            sql.parameters.AddWithValue("@casenumber", c.Number);
            sql.parameters.AddWithValue("@name", c.Name);
            sql.parameters.AddWithValue("@description", c.Description);
            if (projectTypeId > 0) sql.parameters.AddWithValue("@projectTypeId", projectTypeId);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = TypeCast.ToInt(dr["id"]);
            }

            c.Id = retval;

            dr.Close();
            sql.reset();

            Events_addToEventLog(U.OrganisationId, U.Id, c.Id == retval ? "UPDATE" : "CREATE", "CASENUMBERS", retval.ToString(), c);

            return retval;
        }

        #endregion

        #region TimeUsage
        public List<int> TimeUsage_getAvailableYears(int OrganisationId) {
            List<int> retval = new List<int>();
            sql.commandText = "TimeUsage_getAvailableyears";
            sql.parameters.AddWithValue("@OrganisationId", OrganisationId);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(TypeCast.ToInt(dr["year"]));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public List<TimeUsageOverview> TimeUsage_getOverviewForYear(int OrganisationId, int year) {
            List<TimeUsageOverview> retval = new List<TimeUsageOverview>();
            sql.commandText = "TimeUsage_getOverviewForYear";
            sql.parameters.AddWithValue("@OrganisationId", OrganisationId);
            if (year > 0) sql.parameters.AddWithValue("@year", year);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new TimeUsageOverview(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public List<TimeUsage> TimeUsage_getUserRegistrations(int OrganisationId, string userId, int year, string SortOrder, string sortAsc) {
            List<TimeUsage> retval = new List<TimeUsage>();
            sql.commandText = "TimeUsage_getUserRegistrations";
            sql.parameters.AddWithValue("@OrganisationId", OrganisationId);
            if (year > 0) sql.parameters.AddWithValue("@year", year);
            sql.parameters.AddWithValue("@userId", userId);
            sql.parameters.AddWithValue("@SortOrder", SortOrder);
            sql.parameters.AddWithValue("@sortAsc", sortAsc);


            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new TimeUsage(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public TimeUsage TimeUsage_getWeek(int OrganisationId, int Id) {
            TimeUsage retval = null;
            sql.commandText = "TimeUsage_getWeek";
            sql.parameters.AddWithValue("@OrganisationId", OrganisationId);
            sql.parameters.AddWithValue("@Id", Id);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = new TimeUsage(ref dr);
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public TimeUsage TimeUsage_getFirstAvailableWeek(int OrganisationId, string userId) {
            TimeUsage retval = null;
            sql.commandText = "TimeUsage_getFirstAvailableWeek";
            sql.parameters.AddWithValue("@OrganisationId", OrganisationId);
            sql.parameters.AddWithValue("@userId", userId);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = new TimeUsage(ref dr);
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public TimeUsage TimeUsage_getWeekFromDate(int OrganisationId, string userId, DateTime date, int direction) {
            TimeUsage retval = null;
            sql.commandText = "TimeUsage_getWeekFromDate";
            sql.parameters.AddWithValue("@OrganisationId", OrganisationId);
            sql.parameters.AddWithValue("@userId", userId);
            sql.parameters.AddWithValue("@date", date);
            sql.parameters.AddWithValue("@direction", direction);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = new TimeUsage(ref dr);
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public TimeUsage TimeUsage_getWeekFromWeek(int OrganisationId, string userId, int week, int year) {
            TimeUsage retval = null;
            sql.commandText = "TimeUsage_getWeekFromWeek";
            sql.parameters.AddWithValue("@OrganisationId", OrganisationId);
            sql.parameters.AddWithValue("@userId", userId);
            sql.parameters.AddWithValue("@week", week);
            sql.parameters.AddWithValue("@year", year);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = new TimeUsage(ref dr);
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public List<TimeUsageProject> timeUsage_getProjectsFromLastWeek(string userId, int OrganisationId, DateTime weekDate) {
            List<TimeUsageProject> retval = new List<TimeUsageProject>();
            sql.commandText = "timeUsage_getProjectsFromLastWeek";
            sql.parameters.AddWithValue("@OrganisationId", OrganisationId);
            sql.parameters.AddWithValue("@userId", userId);
            sql.parameters.AddWithValue("@weekDate", weekDate);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new TimeUsageProject(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }
        public TimeUsageProject timeUsage_getProject(int OrganisationId, string userId, int timeUsageId, int timeUsageProjectId) {
            TimeUsageProject retval = null;
            sql.commandText = "timeUsage_getProject";
            sql.parameters.AddWithValue("@OrganisationId", OrganisationId);
            sql.parameters.AddWithValue("@userId", userId);
            sql.parameters.AddWithValue("@timeUsageId", timeUsageId);
            sql.parameters.AddWithValue("@timeUsageProjectId", timeUsageProjectId);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = new TimeUsageProject(ref dr);
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public List<TimeUsageProject> timeUsage_getProjectsForWeek(int OrganisationId, string userId, int timeUsageId) {

            List<TimeUsageProject> retval = new List<TimeUsageProject>();
            if (timeUsageId <= 0) return retval;

            sql.commandText = "timeUsage_getProjectsForWeek";
            sql.parameters.AddWithValue("@OrganisationId", OrganisationId);
            sql.parameters.AddWithValue("@userId", userId);
            sql.parameters.AddWithValue("@timeUsageId", timeUsageId);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new TimeUsageProject(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public int TimeUsage_UpdateTimeUsage(User U, TimeUsage P) {
            int retval = 0;

            sql.commandText = "TimeUsage_UpdateTimeUsage";
            if (P.id > 0) sql.parameters.AddWithValue("@Id", TypeCast.ToIntOrDBNull(P.id));
            sql.parameters.AddWithValue("@organisationId", TypeCast.ToIntOrDBNull(P.organisationId));
            sql.parameters.AddWithValue("@userId", TypeCast.ToStringOrDBNull(P.userId));
            sql.parameters.AddWithValue("@weekDate", TypeCast.ToDateTime(P.weekDate));
            sql.parameters.AddWithValue("@weekNumber", TypeCast.ToIntOrDBNull(P.weekNumber));
            sql.parameters.AddWithValue("@year", TypeCast.ToIntOrDBNull(P.year));

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = TypeCast.ToInt(dr["id"]);
            }
            dr.Close();
            sql.reset();

            Events_addToEventLog(U.OrganisationId, U.Id, P.id == retval ? "UPDATE" : "CREATE", "TIMEUSAGE", retval.ToString(), P);

            return retval;

        }

        public void timeUsage_delete(int OrganisationId, int TimeUsageId) {
            sql.commandText = "timeUsage_delete";
            sql.parameters.AddWithValue("@OrganisationId", OrganisationId);
            sql.parameters.AddWithValue("@TimeUsageId", TimeUsageId);

            sql.execute();

            sql.reset();
        }

        public int TimeUsage_UpdateTimeUsageProject_MultipleAlike(TimeUsageProject P) {
            int retval = 0;

            sql.commandText = "TimeUsage_UpdateTimeUsageProject_MultipleAlike";
            if (P.id > 0) sql.parameters.AddWithValue("@Id", TypeCast.ToIntOrDBNull(P.id));
            sql.parameters.AddWithValue("@timeUsageId", TypeCast.ToInt(P.timeUsageId));
            sql.parameters.AddWithValue("@userId", TypeCast.ToString(P.userId));
            sql.parameters.AddWithValue("@organisationId", TypeCast.ToInt(P.organisationId));
            sql.parameters.AddWithValue("@dayOfWeek", TypeCast.ToInt(P.dayOfWeek));
            sql.parameters.AddWithValue("@timeSpent", TypeCast.ToDecimal(P.timeSpent));
            sql.parameters.AddWithValue("@SecondaryProjectTypeId", TypeCast.ToDecimal(P.SecondaryProjectTypeId));
            sql.parameters.AddWithValue("@CaseNumberId", TypeCast.ToDecimal(P.CaseNumberId));
            sql.parameters.AddWithValue("@Description", TypeCast.ToString(P.Description));
            sql.parameters.AddWithValue("@RowIndex", TypeCast.ToInt(P.RowIndex));
            sql.parameters.AddWithValue("@Status", TypeCast.ToInt(P.Status));

            //Counter posting ?
            if (P.CounterPostedBy != "") {
                sql.parameters.AddWithValue("@CounterPosted", P.CounterPosted);
                sql.parameters.AddWithValue("@CounterPostedBy", P.CounterPostedBy);
            }

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = TypeCast.ToInt(dr["id"]);
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public int TimeUsage_UpdateTimeUsageProject(TimeUsageProject P) {
            int retval = 0;

            sql.commandText = "TimeUsage_UpdateTimeUsageProject";
            if (P.id > 0) sql.parameters.AddWithValue("@Id", TypeCast.ToIntOrDBNull(P.id));
            sql.parameters.AddWithValue("@timeUsageId", TypeCast.ToInt(P.timeUsageId));
            sql.parameters.AddWithValue("@userId", TypeCast.ToString(P.userId));
            sql.parameters.AddWithValue("@organisationId", TypeCast.ToInt(P.organisationId));
            sql.parameters.AddWithValue("@dayOfWeek", TypeCast.ToInt(P.dayOfWeek));
            sql.parameters.AddWithValue("@timeSpent", TypeCast.ToDecimal(P.timeSpent));
            sql.parameters.AddWithValue("@SecondaryProjectTypeId", TypeCast.ToDecimal(P.SecondaryProjectTypeId));
            sql.parameters.AddWithValue("@CaseNumberId", TypeCast.ToDecimal(P.CaseNumberId));
            sql.parameters.AddWithValue("@Description", TypeCast.ToString(P.Description));
            sql.parameters.AddWithValue("@RowIndex", TypeCast.ToInt(P.RowIndex));

            //Counter posting ?
            if (P.CounterPostedBy != "") {
                sql.parameters.AddWithValue("@CounterPosted", P.CounterPosted);
                sql.parameters.AddWithValue("@CounterPostedBy", P.CounterPostedBy);
            }

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = TypeCast.ToInt(dr["id"]);
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public void TimeUsage_DropTimeUsageProject(string userId, int organisationId, int timeUsageId, int TimeUsageProjectId) {
            sql.commandText = "TimeUsage_DropTimeUsageProject";
            sql.parameters.AddWithValue("@userId", userId);
            sql.parameters.AddWithValue("@organisationId", organisationId);
            sql.parameters.AddWithValue("@timeUsageId", timeUsageId);
            sql.parameters.AddWithValue("@TimeUsageProjectId", TimeUsageProjectId);

            sql.execute();

            sql.reset();
        }

        public void TimeUsage_DropTimeUsageProjects(string userId, int organisationId, int timeUsageId, int secondaryProjectTypeId, int caseNumberId, int RowIndex) {
            sql.commandText = "TimeUsage_DropTimeUsageProjects";
            sql.parameters.AddWithValue("@userId", userId);
            sql.parameters.AddWithValue("@organisationId", organisationId);
            sql.parameters.AddWithValue("@timeUsageId", timeUsageId);
            sql.parameters.AddWithValue("@secondaryProjectTypeId", secondaryProjectTypeId);
            sql.parameters.AddWithValue("@caseNumberId", caseNumberId);
            sql.parameters.AddWithValue("@RowIndex", RowIndex);

            sql.execute();

            sql.reset();
        }

        public void TimeUsage_DropAllTimeUsageProjects(string userId, int organisationId, int timeUsageId) {
            sql.commandText = "TimeUsage_DropAllTimeUsageProjects";
            sql.parameters.AddWithValue("@userId", userId);
            sql.parameters.AddWithValue("@organisationId", organisationId);
            sql.parameters.AddWithValue("@timeUsageId", timeUsageId);

            sql.execute();

            sql.reset();
        }

        public void TimeUsage_SendToApproval(string userId, int organisationId, DateTime weekDate) {
            sql.commandText = "TimeUsage_SendToApproval";
            sql.parameters.AddWithValue("@userId", userId);
            sql.parameters.AddWithValue("@organisationId", organisationId);
            sql.parameters.AddWithValue("@weekDate", weekDate);

            sql.execute();

            sql.reset();
        }

        public void TimeUsage_SendToApprovalById(string userId, int organisationId, int id) {
            sql.commandText = "TimeUsage_SendToApprovalById";
            sql.parameters.AddWithValue("@userId", userId);
            sql.parameters.AddWithValue("@organisationId", organisationId);
            sql.parameters.AddWithValue("@id", id);
            sql.execute();
            sql.reset();
        }

        public List<TimeUsageAdmin> TimeUsage_getAdministrativeItems(int organisationId, int status, string userId, string sortOrder, string SortAsc) {
            return TimeUsage_getAdministrativeItems(organisationId, status, userId, 0, sortOrder, SortAsc);
        }
        public List<TimeUsageAdmin> TimeUsage_getAdministrativeItems(int organisationId, int status, string userId, int Year, string sortOrder, string SortAsc) {
            List<TimeUsageAdmin> retval = new List<TimeUsageAdmin>();
            sql.commandText = "TimeUsage_getAdministrativeItems";
            sql.parameters.AddWithValue("@OrganisationId", organisationId);
            if (userId != "" && userId != null) sql.parameters.AddWithValue("@userId", userId);
            sql.parameters.AddWithValue("@status", status);
            sql.parameters.AddWithValue("@sortOrder", sortOrder);
            sql.parameters.AddWithValue("@SortAsc", SortAsc);
            if (Year > 0) sql.parameters.AddWithValue("@Year", Year);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new TimeUsageAdmin(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public List<TimeUsageAdmin> TimeUsage_searchAdministrativeItems(int organisationId, string userId, int year, int weekNumber, bool pending, bool approved, bool declined, string sortOrder, string SortAsc) {
            List<TimeUsageAdmin> retval = new List<TimeUsageAdmin>();
            sql.commandText = "TimeUsage_searchAdministrativeItems";
            sql.parameters.AddWithValue("@OrganisationId", organisationId);
            if (userId != "" && userId != null) sql.parameters.AddWithValue("@userId", userId);
            if (year > 0) sql.parameters.AddWithValue("@year", year);
            if (weekNumber > 0) sql.parameters.AddWithValue("@weekNumber", weekNumber);
            sql.parameters.AddWithValue("@pending", pending);
            sql.parameters.AddWithValue("@approved", approved);
            sql.parameters.AddWithValue("@declined", declined);

            sql.parameters.AddWithValue("@sortOrder", sortOrder);
            sql.parameters.AddWithValue("@SortAsc", SortAsc);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new TimeUsageAdmin(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public List<HourUsageProjectTypeAndCaseNumber> TimeUsage_GetProjectTypesAndCaseNumbers(User U) {
            return hourRegistrationGetProjectTypesAndCaseNumbers(U);
        }
        public List<HourUsageProjectTypeAndCaseNumber> hourRegistrationGetProjectTypesAndCaseNumbers(User U) {
            List<HourUsageProjectTypeAndCaseNumber> retval = new List<HourUsageProjectTypeAndCaseNumber>();
            sql.commandText = "hourRegistrationGetProjectTypesAndCaseNumbers";
            sql.parameters.AddWithValue("@OrganisationId", U.OrganisationId);
            sql.parameters.AddWithValue("@userId", U.Id);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new HourUsageProjectTypeAndCaseNumber(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public void TimeUsage_SetApproval(User U, string adminId, string userId, int organisationId, DateTime weekDate, bool approved, string Reason) {
            sql.commandText = "TimeUsage_SetApproval";
            sql.parameters.AddWithValue("@adminId", adminId);
            sql.parameters.AddWithValue("@userId", userId);
            sql.parameters.AddWithValue("@organisationId", organisationId);
            sql.parameters.AddWithValue("@weekDate", weekDate);
            sql.parameters.AddWithValue("@approved", approved);
            sql.parameters.AddWithValue("@notApprovedReason", Reason);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            int retval = 0;
            if (dr.Read()) {
                retval = TypeCast.ToInt(dr["id"]);
            }
            dr.Close();
            sql.reset();

            Events_addToEventLog(U.OrganisationId, U.Id, approved ? "APPROVED" : "NOTAPPROVED", "TIMEUSAGE", retval.ToString(), TimeUsage_getWeek(U.OrganisationId, retval));
        }

        public void TimeUsage_SetApprovalById(User U, string adminId, int organisationId, int id, bool approved, string Reason) {
            sql.commandText = "TimeUsage_SetApprovalById";
            sql.parameters.AddWithValue("@adminId", adminId);
            sql.parameters.AddWithValue("@organisationId", organisationId);
            sql.parameters.AddWithValue("@id", id);
            sql.parameters.AddWithValue("@approved", approved);
            sql.parameters.AddWithValue("@notApprovedReason", Reason);

            sql.execute();

            sql.reset();

            Events_addToEventLog(U.OrganisationId, U.Id, approved ? "APPROVED" : "NOTAPPROVED", "TIMEUSAGE", id.ToString(), TimeUsage_getWeek(U.OrganisationId, id));
        }

        public void TimeUsage_sendTimeUsageRequestToUser(int organisationId, string userId, string adminId, DateTime weekDate, int weekNumber, int year) {
            sql.commandText = "TimeUsage_sendTimeUsageRequestToUser";
            sql.parameters.AddWithValue("@organisationId", organisationId);
            sql.parameters.AddWithValue("@userId", userId);
            sql.parameters.AddWithValue("@adminId", adminId);
            sql.parameters.AddWithValue("@weekDate", weekDate);
            sql.parameters.AddWithValue("@weekNumber", weekNumber);
            sql.parameters.AddWithValue("@year", year);

            sql.execute();

            sql.reset();
        }
        #endregion

        #region Mentors 

        public List<Mentor> Mentors_getMentorsFromContactOrCompany(int companyId, int contactId, User U) {
            List<Mentor> retval = new List<Mentor>();
            sql.commandText = "Mentors_getMentorsFromContactOrCompany";
            sql.parameters.AddWithValue("@organisationId", U.OrganisationId);
            if (companyId > 0) sql.parameters.AddWithValue("@companyId", companyId);
            if (contactId > 0) sql.parameters.AddWithValue("@contactId", contactId);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new Mentor(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public Mentor Mentors_getMentor(int Id) {
            Mentor retval = null;
            sql.commandText = "Mentors_getMentor";
            sql.parameters.AddWithValue("@Id", Id);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = new Mentor(ref dr);
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public int Mentors_CreateUpdate(User U, Mentor m) {
            int retval = 0;
            sql.commandText = "Mentors_CreateUpdate";
            sql.parameters.AddWithValue("@id", TypeCast.ToIntOrDBNull(m.Id));
            sql.parameters.AddWithValue("@organisationId", m.OrganisationId);
            sql.parameters.AddWithValue("@contactId", TypeCast.ToIntOrDBNull(m.ContactId));
            sql.parameters.AddWithValue("@companyId", TypeCast.ToIntOrDBNull(m.CompanyId));
            sql.parameters.AddWithValue("@firstname", m.Firstname);
            sql.parameters.AddWithValue("@lastname", m.Lastname);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = TypeCast.ToInt(dr["id"]);
            }
            dr.Close();
            sql.reset();

            Events_addToEventLog(U.OrganisationId, U.Id, m.Id == retval ? "UPDATE" : "CREATE", "MENTORS", retval.ToString(), m);

            return retval;
        }

        #endregion

        #region ExchangeServer

        public void Exchange_DeleteMailRule(string UserId, int Id) {
            sql.commandText = "Exchange_DeleteMailRule";
            sql.parameters.AddWithValue("@UserId", UserId);
            sql.parameters.AddWithValue("@Id", Id);
            sql.execute();
            sql.reset();
        }

        public int Exchange_UpdateMailRule(User U, MailRule m) {
            int retval = 0;

            sql.commandText = "Exchange_UpdateMailRule";

            sql.parameters.AddWithValue("@Id", m.Id);
            sql.parameters.AddWithValue("@UserId", U.Id);
            sql.parameters.AddWithValue("@OrganisationId", U.OrganisationId);
            sql.parameters.AddWithValue("@SenderEmail", m.SenderEmail);
            sql.parameters.AddWithValue("@ContactId", m.ContactId);
            sql.parameters.AddWithValue("@VisibleTo", m.VisibleTo);


            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = TypeCast.ToInt(dr["id"]);
            }
            dr.Close();
            sql.reset();

            return retval;
        }


        public MailRule Exchange_GetMailRule(User U, int Id) {
            MailRule retval = null;

            sql.commandText = "z_Exchange_GetMailRule_" + U.OrganisationId;
            sql.parameters.AddWithValue("@UserId", U.Id);
            sql.parameters.AddWithValue("@Id", Id);


            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = new MailRule(ref dr);
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public List<MailRule> Exchange_GetMailRules(User U) {
            List<MailRule> retval = new List<MailRule>();

            sql.commandText = "z_Exchange_GetMailRules_" + U.OrganisationId;
            sql.parameters.AddWithValue("@UserId", U.Id);


            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new MailRule(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public ExchangeMailItem Exchange_GetEmail(string UserId, int Id) {
            ExchangeMailItem retval = null;

            sql.commandText = "Exchange_GetEmail";
            sql.parameters.AddWithValue("@UserId", UserId);
            sql.parameters.AddWithValue("@Id", Id);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = new ExchangeMailItem(ref dr);
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public List<ExchangeMailFolder> Exchange_getFolders(string UserId, int ParentFolderId) {
            List<ExchangeMailFolder> retval = new List<ExchangeMailFolder>();

            sql.commandText = "Exchange_getFolders";
            sql.parameters.AddWithValue("@UserId", UserId);
            if (ParentFolderId > 0) sql.parameters.AddWithValue("@ParentFolderId", ParentFolderId);


            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new ExchangeMailFolder(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public List<ExchangeMail> Exchange_GetRecentMonthsEmails(string UserId, int MonthsBack, int FolderId) {
            List<ExchangeMail> retval = new List<ExchangeMail>();

            sql.commandText = "Exchange_GetRecentMonthsEmails";
            sql.parameters.AddWithValue("@UserId", UserId);
            sql.parameters.AddWithValue("@months", MonthsBack);
            if (FolderId > 0) sql.parameters.AddWithValue("@FolderId", FolderId);


            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new ExchangeMail(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public List<ExchangeMail> Exchange_GetEmails(string UserId, object DateStart, object DateEnd, int FolderId) {
            List<ExchangeMail> retval = new List<ExchangeMail>();

            sql.commandText = "Exchange_GetEmails";
            sql.parameters.AddWithValue("@UserId", UserId);
            if (DateStart != null) sql.parameters.AddWithValue("@DateStart", DateStart);
            if (DateEnd != null) sql.parameters.AddWithValue("@DateEnd", DateEnd);
            if (FolderId > 0) sql.parameters.AddWithValue("@FolderId", FolderId);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new ExchangeMail(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }
        public List<ExchangeMail> Exchange_GetEmailsFromContact(int ContactId) {
            return Exchange_GetEmailsFromContact("", ContactId, 0);
        }
        public List<ExchangeMail> Exchange_GetEmailsFromContact(string UserId, int ContactId, int OrganisationId) {
            List<ExchangeMail> retval = new List<ExchangeMail>();

            sql.commandText = "Exchange_GetEmailsFromContact";
            if (UserId != "") sql.parameters.AddWithValue("@UserId", UserId);
            sql.parameters.AddWithValue("@ContactId", ContactId);
            if (OrganisationId > 0) sql.parameters.AddWithValue("@OrganisationId", OrganisationId);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new ExchangeMail(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public void Exchange_SetUserAutoSync(string UserId, bool AutoSync) {
            sql.commandText = "Exchange_SetUserAutoSync";
            sql.parameters.AddWithValue("@UserId", UserId);
            sql.parameters.AddWithValue("@AutoSync", AutoSync);
            sql.execute();
            sql.reset();
        }

        public void Exchange_SetUserLastSyncDate(string UserId, DateTime Date) {
            sql.commandText = "Exchange_SetUserLastSyncDate";

            sql.parameters.AddWithValue("@UserId", UserId);
            sql.parameters.AddWithValue("@Date", Date);

            sql.execute();
            sql.reset();
        }

        public void Exchange_DeleteEmail(int Id, int OrganisationId, string UserId) {
            sql.commandText = "Exchange_DeleteEmail";
            sql.parameters.AddWithValue("@Id", Id);
            sql.parameters.AddWithValue("@OrganisationId", OrganisationId);
            sql.parameters.AddWithValue("@UserId", UserId);

            sql.execute();
            sql.reset();
        }

        public void Exchange_SetMailToContact(int Id, int ContactId, int VisibleTo) {
            sql.commandText = "Exchange_SetMailToContact";
            sql.parameters.AddWithValue("@Id", Id);
            if (ContactId > 0) sql.parameters.AddWithValue("@ContactId", ContactId);
            if (VisibleTo > 0) sql.parameters.AddWithValue("@VisibleTo", VisibleTo);

            sql.execute();
            sql.reset();

        }

        public List<MeetingReport> Exchange_getMeetingReportsFromCompany(string UserId, int CompanyId) {
            List<MeetingReport> retval = new List<MeetingReport>();

            sql.commandText = "Exchange_getMeetingReportsFromCompany";
            sql.parameters.AddWithValue("@UserId", UserId);
            sql.parameters.AddWithValue("@CompanyId", CompanyId);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new MeetingReport(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public MeetingReport Exchange_getMeetingReport(string UserId, string MeetingUrl) {
            MeetingReport retval = null;

            sql.commandText = "Exchange_getMeetingReport";
            sql.parameters.AddWithValue("@UserId", UserId);
            sql.parameters.AddWithValue("@MeetingUrl", MeetingUrl);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = new MeetingReport(ref dr);
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public int Exchange_MeetingReportUpate(User U, MeetingReport M) {
            int retval = 0;

            sql.commandText = "Exchange_MeetingReportUpate";
            if (M.Id > 0) sql.parameters.AddWithValue("@Id", M.Id);
            sql.parameters.AddWithValue("@OrganisationId", M.OrganisationId);
            sql.parameters.AddWithValue("@UserId", M.UserId);
            sql.parameters.AddWithValue("@CompanyId", M.CompanyId);
            sql.parameters.AddWithValue("@MeetingUrl", M.MeetingUrl);
            sql.parameters.AddWithValue("@Subject", M.Subject);
            sql.parameters.AddWithValue("@Location", M.Location);
            sql.parameters.AddWithValue("@DateStart", M.DateStart);
            sql.parameters.AddWithValue("@DateEnd", M.DateEnd);
            sql.parameters.AddWithValue("@Body", M.Body);
            sql.parameters.AddWithValue("@Timespent", M.Timespent);
            sql.parameters.AddWithValue("@PrimaryProjectTypeId", M.PrimaryProjectTypeId);
            sql.parameters.AddWithValue("@PrimaryProjectTypeName", M.PrimaryProjectTypeName);
            sql.parameters.AddWithValue("@SecondaryProjectTypeId", M.SecondaryProjectTypeId);
            sql.parameters.AddWithValue("@SecondaryProjectTypeName", M.SecondaryProjectTypeName);
            sql.parameters.AddWithValue("@SecondaryProjectTypeSerialNo", M.SecondaryProjectTypeSerialNo);
            sql.parameters.AddWithValue("@CaseNumberId", M.CaseNumberId);
            sql.parameters.AddWithValue("@CaseNumberRelationId", M.CaseNumberRelationId);
            sql.parameters.AddWithValue("@CaseNumber", M.CaseNumber);
            sql.parameters.AddWithValue("@CaseNumberName", M.CaseNumberName);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = TypeCast.ToInt(dr["status"]);
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public List<ExchangeAppointment> Exchange_getContactAppointments(int contactId) {
            return Exchange_getContactAppointments(null, contactId);
        }
        public List<ExchangeAppointment> Exchange_getContactAppointments(User U, int contactId) {
            List<ExchangeAppointment> retval = new List<ExchangeAppointment>();
            sql.commandText = "Exchange_getContactAppointments";
            if (U != null) sql.parameters.AddWithValue("@userId", U.Id);
            sql.parameters.AddWithValue("@contactId", contactId);
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new ExchangeAppointment(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public void Exchange_RemoveContactAppointment(string url, int contactId) {
            sql.commandText = "Exchange_RemoveContactAppointment";
            sql.parameters.AddWithValue("@url", url);
            if (contactId > 0) sql.parameters.AddWithValue("@contactId", contactId);

            sql.execute();
            sql.reset();
        }

        public List<Contact> Exchange_findContacts(User U, string query) {
            List<Contact> retval = new List<Contact>();
            sql.commandText = "z_Exchange_findContacts_" + U.OrganisationId;
            sql.parameters.AddWithValue("@query", query);
            sql.parameters.AddWithValue("@organisationId", U.OrganisationId);
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new Contact(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public List<Contact> Exchange_getContactEmailFromId(User U, int contactId) {
            List<Contact> retval = new List<Contact>();
            sql.commandText = "z_Exchange_getContactEmailFromId_" + U.OrganisationId;
            sql.parameters.AddWithValue("@contactId", contactId);
            sql.parameters.AddWithValue("@organisationId", U.OrganisationId);
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new Contact(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public List<User> Exchange_LocateUsers(User U, string query) {
            List<User> retval = new List<User>();
            sql.commandText = "Exchange_LocateUsers";
            sql.parameters.AddWithValue("@query", query);
            sql.parameters.AddWithValue("@organisationId", U.OrganisationId);
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new User(ref dr, false));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public void ExchangeServer_UpdateUserCredentials(User U) {
            sql.commandText = "ExchangeServer_UpdateUserCredentials";
            sql.parameters.AddWithValue("@userId", U.Id);
            sql.parameters.AddWithValue("@OrganisationId", U.OrganisationId);
            sql.parameters.AddWithValue("@ExchangeURL", U.ExchangeURL);
            sql.parameters.AddWithValue("@ExchangeUsername", U.ExchangeUsername);
            sql.parameters.AddWithValue("@ExchangePassword", U.ExchangePassword);
            sql.parameters.AddWithValue("@ExchangeDomain", U.ExchangeDomain);
            sql.parameters.AddWithValue("@ExchangeFormbasedLogin", U.ExchangeFormbasedLogin);
            sql.execute();
            sql.reset();

            //Events_addToEventLog(U.OrganisationId, U.Id, "UPDATE", "USER", U.Id.ToString(), U);
        }

        public void Exchange_moveContactAppointments(int OldContactId, int NewContactId) {
            sql.commandText = "Exchange_moveContactAppointments";
            sql.parameters.AddWithValue("@OldContactId", OldContactId);
            sql.parameters.AddWithValue("@NewContactId", NewContactId);
            sql.execute();
            sql.reset();
        }

        public void Exchange_deleteAppointment(User U, string Url) {
            sql.commandText = "Exchange_deleteAppointment";
            sql.parameters.AddWithValue("@userId", U.Id);
            sql.parameters.AddWithValue("@OrganisationId", U.OrganisationId);
            sql.parameters.AddWithValue("@Url", Url.Trim());
            sql.execute();
            sql.reset();

            //Events_addToEventLog(U.OrganisationId, U.Id, "DELETE", "EXCHANGE", U.Id.ToString(), U);
        }

        public int Exhcange_UpdateAppointment(User U, ExchangeAppointment A, string newUrl) {
            int retval = 0;

            sql.commandText = "Exhcange_UpdateAppointment";
            sql.parameters.AddWithValue("@id", A.id);
            sql.parameters.AddWithValue("@organisationId", A.organisationId);
            sql.parameters.AddWithValue("@userId", A.userId);
            sql.parameters.AddWithValue("@contactId", A.contactId);
            sql.parameters.AddWithValue("@contactEmail", A.contactEmail);
            sql.parameters.AddWithValue("@url", A.url);
            sql.parameters.AddWithValue("@newUrl", newUrl);
            sql.parameters.AddWithValue("@subject", A.subject);
            sql.parameters.AddWithValue("@location", A.location);
            sql.parameters.AddWithValue("@dateStart", A.dateStart);
            sql.parameters.AddWithValue("@dateEnd", A.dateEnd);
            sql.parameters.AddWithValue("@body", A.body);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = TypeCast.ToInt(dr["id"]);
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        #endregion

        #region Events

        public void Events_addToEventLog(int organisationId, string userId, string eventType, string eventOwner, string eventId, EventLogBase EventObject) {
            if (EventObject != null && sql != null) {
                sql.commandText = "events_addToEventLog";
                sql.parameters.AddWithValue("@organisationId", organisationId);
                sql.parameters.AddWithValue("@userId", userId);
                sql.parameters.AddWithValue("@eventType", eventType);
                sql.parameters.AddWithValue("@eventOwner", eventOwner);
                sql.parameters.AddWithValue("@eventId", eventId);


                EventLogBase.VisualItems V = EventObject.GetVisualItemsForEventLog(eventType);

                sql.parameters.AddWithValue("@icon", V.Icon);
                sql.parameters.AddWithValue("@text", V.Text);
                sql.parameters.AddWithValue("@javascript", V.JavaScript);

                sql.execute();
                sql.reset();
            }
        }

        public List<Event> Events_getRecentEventsFromUser(int organisationId, string userId) {
            List<Event> retval = new List<Event>();
            sql.commandText = "Events_getRecentEventsFromUser";
            sql.parameters.AddWithValue("@OrganisationId", organisationId);
            sql.parameters.AddWithValue("@userId", userId);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new Event(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        #endregion 

        #region Partners

        public List<PartnerEvaluationExport> extractPartnersForEvaluation(bool inDebug, bool extractAll, object DateStart, object DateEnd) {
            List<PartnerEvaluationExport> retval = new List<PartnerEvaluationExport>();
            sql.commandText = "z_Contacts_ExtractToUserFinalcialPoolEvaluation_1";
            sql.parameters.AddWithValue("@inDebug", inDebug);
            sql.parameters.AddWithValue("@extractAll", extractAll);
            if (DateStart != null) sql.parameters.AddWithValue("@DateStart", DateStart);
            if (DateEnd != null) sql.parameters.AddWithValue("@DateEnd", DateEnd);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new PartnerEvaluationExport(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public int Partners_CreatePartnerCompany(User U, PartnerCompany P) {
            sql.commandText = "Partners_CreatePartnerCompany";

            if (P.CompanyId > 0) sql.parameters.AddWithValue("@CompanyId", P.CompanyId);
            sql.parameters.AddWithValue("@OrganisationId", P.OrganisationId);
            sql.parameters.AddWithValue("@CompanyName", P.CompanyName);
            sql.parameters.AddWithValue("@CVR", P.CVR);
            sql.parameters.AddWithValue("@Address", P.Address);
            sql.parameters.AddWithValue("@Zipcode", P.Zipcode);
            sql.parameters.AddWithValue("@City", P.City);
            sql.parameters.AddWithValue("@County", P.County);
            sql.parameters.AddWithValue("@Memo", P.Memo);
            sql.parameters.AddWithValue("@IsPublicInstance", P.IsPublicInstance);
            sql.parameters.AddWithValue("@CreatedBy", P.CreatedBy);
            sql.parameters.AddWithValue("@Web", P.Web);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            int partnerCompanyId = 0;
            if (dr.Read()) {
                partnerCompanyId = TypeCast.ToInt(dr["Status"]);
            }
            dr.Close();
            sql.reset();

            // Events_addToEventLog(U.OrganisationId, U.Id, c.id == countyId ? "UPDATE" : "CREATE", "COUNTIES", countyId.ToString(), c);


            return partnerCompanyId;
        }

        public int Partners_CreatePartnerContact(User U, PartnerContact P) {
            sql.commandText = "Partners_CreatePartnerContact";
            if (P.ContactId > 0) sql.parameters.AddWithValue("@ContactId", P.ContactId);
            sql.parameters.AddWithValue("@OrganisationId", P.OrganisationId);
            sql.parameters.AddWithValue("@ContactType", P.ContactType);
            sql.parameters.AddWithValue("@Firstname", P.Firstname);
            sql.parameters.AddWithValue("@Lastname", P.Lastname);
            sql.parameters.AddWithValue("@Email", P.Email);
            sql.parameters.AddWithValue("@Phone1", P.Phone1);
            sql.parameters.AddWithValue("@Phone2", P.Phone2);
            sql.parameters.AddWithValue("@Nace", P.Nace);
            sql.parameters.AddWithValue("@OwnDescription", P.OwnDescription);
            sql.parameters.AddWithValue("@CompanyId", P.CompanyId);
            sql.parameters.AddWithValue("@CreatedBy", P.CreatedBy);

            sql.parameters.AddWithValue("@IsCooporative", P.IsCooporative);
            sql.parameters.AddWithValue("@IsMentor", P.IsMentor);
            sql.parameters.AddWithValue("@IsRedirect", P.IsRedirect);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            int partnerId = 0;
            if (dr.Read()) {
                partnerId = TypeCast.ToInt(dr["Status"]);
            }
            dr.Close();
            sql.reset();

            return partnerId;
        }

        public PartnerCompany Partners_getPartnerCompany(int CompanyId) {
            sql.commandText = "Partners_getPartnerCompany";
            sql.parameters.AddWithValue("@CompanyId", CompanyId);
            PartnerCompany P = null;
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                P = new PartnerCompany(ref dr);
            }
            dr.Close();
            sql.reset();

            return P;
        }

        public PartnerContact Partners_getPartnerContact(int ContactId) {
            sql.commandText = "Partners_getPartnerContact";
            sql.parameters.AddWithValue("@ContactId", ContactId);
            PartnerContact P = null;
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;

            while (dr.Read()) {
                P = new PartnerContact(ref dr);
            }
            dr.Close();
            sql.reset();

            return P;
        }

        public List<PartnerContact> Partners_getPartnerContactsFromCompany(int CompanyId) {
            sql.commandText = "Partners_getPartnerContactsFromCompany";
            sql.parameters.AddWithValue("@CompanyId", CompanyId);
            List<PartnerContact> P = new List<PartnerContact>();
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;

            while (dr.Read()) {
                P.Add(new PartnerContact(ref dr));
            }
            dr.Close();
            sql.reset();

            return P;
        }

        public PartnerContactForList Partners_getPartnerAndCompany(int ContactId) {
            PartnerContactForList retval = null;
            sql.commandText = "Partners_getPartnerAndCompany";
            sql.parameters.AddWithValue("@ContactId", ContactId);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = new PartnerContactForList(ref dr);
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public List<PartnerContactForList> Partners_GetRecent(User U) {
            List<PartnerContactForList> retval = new List<PartnerContactForList>();
            sql.commandText = "Partners_GetRecent";
            sql.parameters.AddWithValue("@organisationId", U.OrganisationId);
            sql.parameters.AddWithValue("@userId", U.Id);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new PartnerContactForList(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public List<PartnerContactForList> Partners_QuickSearch(int organisationId, string userId, string query, int searchIn, int searchFor, string SortOrder, string sortAsc, string Competences) {
            List<PartnerContactForList> retval = new List<PartnerContactForList>();
            sql.commandText = "Partners_QuickSearch";
            sql.parameters.AddWithValue("@organisationId", organisationId);
            sql.parameters.AddWithValue("@userId", userId);

            sql.parameters.AddWithValue("@searchIn", searchIn);
            sql.parameters.AddWithValue("@searchFor", searchFor);

            sql.parameters.AddWithValue("@query", query);

            if (Competences != "") {
                if (!Competences.StartsWith(","))
                    Competences = ',' + Competences;

                if (!Competences.EndsWith(","))
                    Competences += ',';

                sql.parameters.AddWithValue("@Competences", Competences);
            }

            if (SortOrder != "" && SortOrder != null) sql.parameters.AddWithValue("@SortOrder", SortOrder);
            if (sortAsc != "" && sortAsc != null) sql.parameters.AddWithValue("@sortAsc", sortAsc);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new PartnerContactForList(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public void Partners_Delete(User U, PartnerContact P) {
            sql.commandText = "Partners_Delete";
            sql.parameters.AddWithValue("@ContactId", P.ContactId);
            sql.parameters.AddWithValue("@CompanyId", P.CompanyId);
            sql.parameters.AddWithValue("@userId", TypeCast.ToStringOrDBNull(U.Id));
            sql.execute();
            sql.reset();

            Events_addToEventLog(U.OrganisationId, U.Id, "DELETE", "PARTNER", P.ContactId.ToString(), P);
        }

        public void Partner_Contact_moveAssociation(int contactRelationId, int newContactId) {
            sql.commandText = "Partner_Contact_moveAssociation";
            sql.parameters.AddWithValue("@ContactRelationId", TypeCast.ToInt(contactRelationId));
            sql.parameters.AddWithValue("@newContactId", TypeCast.ToInt(newContactId));

            sql.execute();
            sql.reset();
        }

        public int Partner_Contact_createAssociation(int partnerId, int contactId, string userId) {
            sql.commandText = "Partner_Contact_createAssociation";
            sql.parameters.AddWithValue("@PartnerId", partnerId);
            sql.parameters.AddWithValue("@ContactId", contactId);
            sql.parameters.AddWithValue("@userId", userId);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            int intStatus = -1;
            if (dr.Read()) {
                intStatus = TypeCast.ToInt(dr["Status"]);
            }
            dr.Close();
            sql.reset();

            return intStatus;
        }

        public int Partner_Contact_createAssociation(int partnerId, int contactId, string userId, bool IsMentor, bool IsCooporative, bool IsRedirect, string FinancialPool, int FinancialPoolId, decimal FinancialPoolAmount) {
            sql.commandText = "Partner_Contact_createAssociation";
            sql.parameters.AddWithValue("@PartnerId", partnerId);
            sql.parameters.AddWithValue("@ContactId", contactId);
            sql.parameters.AddWithValue("@userId", userId);

            sql.parameters.AddWithValue("@IsMentor", IsMentor);
            sql.parameters.AddWithValue("@IsCooporative", IsCooporative);
            sql.parameters.AddWithValue("@IsRedirect", IsRedirect);

            if (FinancialPool != "" && FinancialPool != null) sql.parameters.AddWithValue("@FinancialPool", FinancialPool);
            if (FinancialPoolId > 0) sql.parameters.AddWithValue("@FinancialPoolId", FinancialPoolId);
            if (FinancialPoolId > 0 && FinancialPoolAmount > 0) sql.parameters.AddWithValue("@FinancialPoolAmount", FinancialPoolAmount);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            int intStatus = -1;
            if (dr.Read()) {
                intStatus = TypeCast.ToInt(dr["Status"]);
            }
            dr.Close();
            sql.reset();

            return intStatus;
        }

        public void Partner_Contact_deleteAssociation(int partnerId, int contactId, string UserId) {
            sql.commandText = "Partner_Contact_deleteAssociation";
            if (partnerId > 0) sql.parameters.AddWithValue("@PartnerId", TypeCast.ToInt(partnerId));
            if (contactId > 0) sql.parameters.AddWithValue("@ContactId", TypeCast.ToInt(contactId));
            sql.parameters.AddWithValue("@userId", UserId);
            sql.execute();
            sql.reset();
        }

        public void Partner_Contact_deleteAssociationFromId(int Id, string UserId) {
            sql.commandText = "Partner_Contact_deleteAssociationFromId";
            sql.parameters.AddWithValue("@Id", Id);
            sql.parameters.AddWithValue("@userId", UserId);
            sql.execute();
            sql.reset();
        }
        #endregion

        #region Competences


        public List<Competence> competences_getCompetencesFromPartnerIdAndParentId(int PartnerId, int ParentId) {
            sql.commandText = "competences_getCompetencesFromPartnerIdAndParentId";
            sql.parameters.AddWithValue("@PartnerId", PartnerId);
            sql.parameters.AddWithValue("@ParentId", ParentId);
            List<Competence> retval = new List<Competence>();

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                Competence c = new Competence(ref dr);
                c.Check = TypeCast.ToBool(dr["checked"]);
                retval.Add(c);
            }

            dr.Close();
            sql.reset();

            return retval;
        }

        public List<Competence> competences_getCompetencesFromEWVolunteerIdAndParentId(int VolunteerId, int ParentId) {
            sql.commandText = "competences_getCompetencesFromEWVolunteerIdAndParentId";
            sql.parameters.AddWithValue("@VolunteerId", VolunteerId);
            sql.parameters.AddWithValue("@ParentId", ParentId);
            List<Competence> retval = new List<Competence>();

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                Competence c = new Competence(ref dr);
                c.Check = TypeCast.ToBool(dr["checked"]);
                retval.Add(c);
            }

            dr.Close();
            sql.reset();

            return retval;
        }

        public List<Competence> Competences_getCompetencesFromPartnerId(int PartnerId) {
            sql.commandText = "competences_getCompetencesFromPartnerId";
            sql.parameters.AddWithValue("@PartnerId", PartnerId);
            List<Competence> retval = new List<Competence>();

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                Competence c = new Competence(ref dr);
                c.Check = TypeCast.ToBool(dr["checked"]);
                retval.Add(c);
            }

            dr.Close();
            sql.reset();

            return retval;
        }

        public int Competences_Partner_createAssociation(int partnerId, int competenceId) {
            sql.commandText = "Competence_createAssociation";
            sql.parameters.AddWithValue("@partnerId", partnerId);
            sql.parameters.AddWithValue("@competenceId", competenceId);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            int intStatus = -1;
            if (dr.Read()) {
                intStatus = TypeCast.ToInt(dr["Status"]);
            }
            dr.Close();
            sql.reset();

            return intStatus;
        }

        public void Competences_Partner_deleteAssociation(int partnerId, object competenceId) {
            sql.commandText = "Competences_deleteAssociation";
            sql.parameters.AddWithValue("@partnerId", partnerId);
            sql.parameters.AddWithValue("@competenceId", TypeCast.ToIntOrDBNull(competenceId));
            sql.execute();
            sql.reset();
        }

        public List<Competence> Competences_getAll(string SortOrder, string sortAsc) {
            List<Competence> retval = new List<Competence>();
            sql.commandText = "competences_getAll";
            sql.parameters.AddWithValue("@SortOrder", SortOrder);
            sql.parameters.AddWithValue("@sortAsc", sortAsc);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new Competence(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public List<Competence> competences_getFromParent(string SortOrder, string sortAsc, int ParentId) {
            List<Competence> retval = new List<Competence>();
            sql.commandText = "competences_getFromParent";
            sql.parameters.AddWithValue("@SortOrder", SortOrder);
            sql.parameters.AddWithValue("@sortAsc", sortAsc);
            if (ParentId > 0) sql.parameters.AddWithValue("@ParentId", ParentId);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new Competence(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public int Competences_CreateUpdate(User U, Competence c) {
            sql.commandText = "Competences_createCompetence";
            sql.parameters.AddWithValue("@competenceId", TypeCast.ToIntOrNull(c.CompetenceId));
            sql.parameters.AddWithValue("@Competence", TypeCast.ToStringOrDBNull(c.CompetenceString));
            sql.parameters.AddWithValue("@Description", TypeCast.ToStringOrDBNull(c.Description));
            if (c.ParentId > 0) sql.parameters.AddWithValue("@ParentId", c.ParentId);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            int competenceId = 0;
            if (dr.Read()) {
                competenceId = TypeCast.ToInt(dr["Status"]);
            }

            c.CompetenceId = competenceId;

            dr.Close();
            sql.reset();

            Events_addToEventLog(U.OrganisationId, U.Id, c.CompetenceId == competenceId ? "UPDATE" : "CREATE", "COMPETENCES", competenceId.ToString(), c);


            return competenceId;
        }

        public void Competences_deleteCompetence(User U, int id) {
            sql.commandText = "Competences_delete";
            sql.parameters.AddWithValue("@competenceId", TypeCast.ToIntOrNull(id));
            sql.execute();
            sql.reset();

            Events_addToEventLog(U.OrganisationId, U.Id, "DELETE", "COMPETENCES", id.ToString(), null);
        }

        public Competence Competences_getCompetence(int id) {
            Competence retval = null;
            sql.commandText = "Competences_getCompetence";
            sql.parameters.AddWithValue("@competenceId", id);
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = new Competence(ref dr);
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        #endregion

        #region TravelUsage

        public List<int> TravelUsage_getAvailableYears(int OrganisationId) {
            List<int> retval = new List<int>();
            sql.commandText = "TravelUsage_getAvailableYears";
            sql.parameters.AddWithValue("@OrganisationId", OrganisationId);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(TypeCast.ToInt(dr["year"]));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public TravelUsage travelUsage_getTravel(User U, int Id) {
            TravelUsage retval = null;
            sql.commandText = "travelUsage_getTravel";
            sql.parameters.AddWithValue("@OrganisationId", U.OrganisationId);
            sql.parameters.AddWithValue("@id", Id);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = new TravelUsage(ref dr);
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public List<TravelUsage> travelUsage_getUserTravels(string UserId, int OrganisationId, int year, string sortOrder, string sortAsc) {
            List<TravelUsage> retval = new List<TravelUsage>();
            sql.commandText = "travelUsage_getUserTravels";
            sql.parameters.AddWithValue("@OrganisationId", OrganisationId);
            sql.parameters.AddWithValue("@userId", UserId);
            if (year > 0) sql.parameters.AddWithValue("@year", year);
            if (sortOrder != "" && sortOrder != null) sql.parameters.AddWithValue("@sortOrder", sortOrder);
            if (sortAsc != "" && sortAsc != null) sql.parameters.AddWithValue("@sortAsc", sortAsc);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new TravelUsage(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public int travelUsage_updateTravelUsage(User U, TravelUsage t) {
            int retval = 0;

            sql.commandText = "travelUsage_updateTravelUsage";

            if (t.Id > 0) sql.parameters.AddWithValue("@Id", t.Id);
            sql.parameters.AddWithValue("@OrganisationId", t.OrganisationId);
            sql.parameters.AddWithValue("@userId", t.userId);
            sql.parameters.AddWithValue("@PrimaryProjectTypeId", t.PrimaryProjectTypeId);
            sql.parameters.AddWithValue("@PrimaryProjectTypeName", t.PrimaryProjectTypeName);
            sql.parameters.AddWithValue("@SecondaryProjectTypeId", t.SecondaryProjectTypeId);
            sql.parameters.AddWithValue("@SecondaryProjectTypeName", t.SecondaryProjectTypeName);
            sql.parameters.AddWithValue("@SecondaryProjectTypeSerialNo", t.SecondaryProjectTypeSerialNo);
            sql.parameters.AddWithValue("@CaseNumberId", t.CaseNumberId);
            sql.parameters.AddWithValue("@CaseNumberRelationId", t.CaseNumberRelationId);
            sql.parameters.AddWithValue("@CaseNumber", t.CaseNumber);
            sql.parameters.AddWithValue("@CaseNumberName", t.CaseNumberName);
            sql.parameters.AddWithValue("@Description", t.Description);
            sql.parameters.AddWithValue("@DepartureDate", t.DepartureDate);
            sql.parameters.AddWithValue("@ReturnDate", t.ReturnDate);
            sql.parameters.AddWithValue("@DepartureAddress", t.DepartureAddress);
            sql.parameters.AddWithValue("@DepartureZipcode", t.DepartureZipcode);
            sql.parameters.AddWithValue("@DepartureCity", t.DepartureCity);
            sql.parameters.AddWithValue("@DepartureCountry", t.DepartureCountry);
            sql.parameters.AddWithValue("@ArrivalAddress", t.ArrivalAddress);
            sql.parameters.AddWithValue("@ArrivalZipcode", t.ArrivalZipcode);
            sql.parameters.AddWithValue("@ArrivalCity", t.ArrivalCity);
            sql.parameters.AddWithValue("@ArrivalCountry", t.ArrivalCountry);
            sql.parameters.AddWithValue("@ReturnAddress", t.ReturnAddress);
            sql.parameters.AddWithValue("@ReturnZipcode", t.ReturnZipcode);
            sql.parameters.AddWithValue("@ReturnCity", t.ReturnCity);
            sql.parameters.AddWithValue("@ReturnCountry", t.ReturnCountry);
            sql.parameters.AddWithValue("@pendingApproval", TypeCast.ToDateTimeOrDBNull(t.pendingApproval));
            sql.parameters.AddWithValue("@approved", TypeCast.ToDateTimeOrDBNull(t.approved));
            sql.parameters.AddWithValue("@approvedBy", TypeCast.ToStringOrDBNull(t.approvedBy));
            sql.parameters.AddWithValue("@notApproved", TypeCast.ToDateTimeOrDBNull(t.notApproved));
            sql.parameters.AddWithValue("@notApprovedBy", TypeCast.ToStringOrDBNull(t.notApprovedBy));
            sql.parameters.AddWithValue("@DepartureTravelDistance", t.DepartureTravelDistance);
            sql.parameters.AddWithValue("@ReturnTravelDistance", t.ReturnTravelDistance);
            sql.parameters.AddWithValue("@LicensePlate", t.LicensePlate);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = TypeCast.ToInt(dr["id"]);
            }
            dr.Close();
            sql.reset();

            return retval;

        }

        public bool travelUsage_Delete(int id, User user) {
            bool retval = false;

            sql.commandText = "travelUsage_Delete";
            sql.parameters.AddWithValue("@userId", user.Id);
            sql.parameters.AddWithValue("@OrganisationId", user.OrganisationId);
            sql.parameters.AddWithValue("@id", id);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = TypeCast.ToInt(dr["status"]) > 0;
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public void travelUsage_SendToApprovalById(string userId, int organisationId, int id) {
            sql.commandText = "travelUsage_SendToApprovalById";
            sql.parameters.AddWithValue("@userId", userId);
            sql.parameters.AddWithValue("@organisationId", organisationId);
            sql.parameters.AddWithValue("@id", id);

            sql.execute();

            sql.reset();
        }

        public void travelUsage_SetApprovalById(User U, string adminId, int organisationId, int id, bool approved) {
            travelUsage_SetApprovalById(U, adminId, organisationId, id, approved, null);
        }
        public void travelUsage_SetApprovalById(User U, string adminId, int organisationId, int id, bool approved, string notApprovedReason) {
            sql.commandText = "travelUsage_SetApprovalById";
            sql.parameters.AddWithValue("@adminId", adminId);
            sql.parameters.AddWithValue("@organisationId", organisationId);
            sql.parameters.AddWithValue("@id", id);
            sql.parameters.AddWithValue("@approved", approved);
            if (notApprovedReason != "" && notApprovedReason != "" && !approved) sql.parameters.AddWithValue("@notApprovedReason", notApprovedReason);

            sql.execute();

            sql.reset();

            Events_addToEventLog(U.OrganisationId, U.Id, approved ? "APPROVED" : "NOTAPPROVED", "TRAVELUSAGE", id.ToString(), travelUsage_getTravel(U, id));
        }

        public List<TravelUsageAdmin> TravelUsage_getAdministrativeItems(int organisationId, int status, string userId, int year, string sortOrder, string SortAsc) {
            List<TravelUsageAdmin> retval = new List<TravelUsageAdmin>();
            sql.commandText = "TravelUsage_getAdministrativeItems";
            sql.parameters.AddWithValue("@OrganisationId", organisationId);
            if (userId != "" && userId != null) sql.parameters.AddWithValue("@userId", userId);
            if (status > -1) sql.parameters.AddWithValue("@status", status);
            if (year > 0) sql.parameters.AddWithValue("@year", year);
            sql.parameters.AddWithValue("@sortOrder", sortOrder);
            sql.parameters.AddWithValue("@SortAsc", SortAsc);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new TravelUsageAdmin(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public List<TravelUsageAdmin> TravelUsage_searchAdministrativeItems(int organisationId, string userId, int year, bool pending, bool approved, bool declined, string sortOrder, string SortAsc) {
            List<TravelUsageAdmin> retval = new List<TravelUsageAdmin>();
            sql.commandText = "TravelUsage_searchAdministrativeItems";
            sql.parameters.AddWithValue("@OrganisationId", organisationId);
            if (userId != "" && userId != null) sql.parameters.AddWithValue("@userId", userId);
            if (year > 0) sql.parameters.AddWithValue("@year", year);

            sql.parameters.AddWithValue("@pending", pending);
            sql.parameters.AddWithValue("@approved", approved);
            sql.parameters.AddWithValue("@declined", declined);

            sql.parameters.AddWithValue("@sortOrder", sortOrder);
            sql.parameters.AddWithValue("@SortAsc", SortAsc);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new TravelUsageAdmin(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }


        #endregion

        #region Export
        public List<UserEvaluationExport> Contacts_ExtractToUserLocalEvaluation(int OrganisationId, bool inDebug, bool extractAll, object DateStart, object DateEnd) {
            List<UserEvaluationExport> retval = new List<UserEvaluationExport>();
            sql.commandText = "z_Contacts_ExtractToUserLocalEvaluation_" + OrganisationId;
            sql.parameters.AddWithValue("@inDebug", inDebug);
            sql.parameters.AddWithValue("@extractAll", extractAll);
            if (DateStart != null) sql.parameters.AddWithValue("@DateStart", DateStart);
            if (DateEnd != null) sql.parameters.AddWithValue("@DateEnd", DateEnd);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new UserEvaluationExport(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public List<UserEvaluationExport> Contacts_ExtractToUserEvaluation(int OrganisationId, bool inDebug, bool extractAll, object DateStart, object DateEnd, string Type) {
            List<UserEvaluationExport> retval = new List<UserEvaluationExport>();
            sql.commandText = "z_Contacts_ExtractToUserEvaluation_" + OrganisationId;
            sql.parameters.AddWithValue("@inDebug", inDebug);
            sql.parameters.AddWithValue("@extractAll", extractAll);
            if (DateStart != null) sql.parameters.AddWithValue("@DateStart", DateStart);
            if (DateEnd != null) sql.parameters.AddWithValue("@DateEnd", DateEnd);
            if (Type != "all") { sql.parameters.AddWithValue("@type", Type); }

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new UserEvaluationExport(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }
        #endregion

        #region Statistics
        public List<Statistics_EvaluationOrganisationYear> Statistics_Evaluations_UserYear(int OrganisationId, string UserId, int Year) {
            List<Statistics_EvaluationOrganisationYear> retval = new List<Statistics_EvaluationOrganisationYear>();
            sql.commandText = "Statistics_Evaluations_UserYear";
            sql.parameters.AddWithValue("@OrganisationId", OrganisationId);
            sql.parameters.AddWithValue("@UserId", UserId);
            if (Year > 0) sql.parameters.AddWithValue("@Year", Year);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new Statistics_EvaluationOrganisationYear(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public List<Statistics_EvaluationOrganisationMonth> Statistics_Evaluations_UserMonthly(int OrganisationId, string UserId, int Year) {
            List<Statistics_EvaluationOrganisationMonth> retval = new List<Statistics_EvaluationOrganisationMonth>();
            sql.commandText = "Statistics_Evaluations_UserMonthly";
            sql.parameters.AddWithValue("@OrganisationId", OrganisationId);
            sql.parameters.AddWithValue("@UserId", UserId);
            sql.parameters.AddWithValue("@Year", Year);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new Statistics_EvaluationOrganisationMonth(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public List<Statistics_EvaluationOrganisationDay> Statistics_Evaluations_UserDaily(int OrganisationId, string UserId, int Year, int Month) {
            List<Statistics_EvaluationOrganisationDay> retval = new List<Statistics_EvaluationOrganisationDay>();
            sql.commandText = "Statistics_Evaluations_UserDaily";
            sql.parameters.AddWithValue("@OrganisationId", OrganisationId);
            sql.parameters.AddWithValue("@UserId", UserId);
            sql.parameters.AddWithValue("@Year", Year);
            sql.parameters.AddWithValue("@Month", Month);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new Statistics_EvaluationOrganisationDay(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public List<Statistics_EvaluationOrganisationYear> Statistics_Evaluations_OrganisationYear(int OrganisationId) {
            List<Statistics_EvaluationOrganisationYear> retval = new List<Statistics_EvaluationOrganisationYear>();
            sql.commandText = "Statistics_Evaluations_OrganisationYear";
            sql.parameters.AddWithValue("@OrganisationId", OrganisationId);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new Statistics_EvaluationOrganisationYear(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public List<Statistics_EvaluationOrganisationMonth> Statistics_Evaluations_OrganisationMonthly(int OrganisationId, int Year) {
            List<Statistics_EvaluationOrganisationMonth> retval = new List<Statistics_EvaluationOrganisationMonth>();
            sql.commandText = "Statistics_Evaluations_OrganisationMonthly";
            sql.parameters.AddWithValue("@OrganisationId", OrganisationId);
            sql.parameters.AddWithValue("@Year", Year);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new Statistics_EvaluationOrganisationMonth(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public List<Statistics_EvaluationOrganisationDay> Statistics_Evaluations_OrganisationDaily(int OrganisationId, int Year, int Month) {
            List<Statistics_EvaluationOrganisationDay> retval = new List<Statistics_EvaluationOrganisationDay>();
            sql.commandText = "Statistics_Evaluations_OrganisationDaily";
            sql.parameters.AddWithValue("@OrganisationId", OrganisationId);
            sql.parameters.AddWithValue("@Year", Year);
            sql.parameters.AddWithValue("@Month", Month);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new Statistics_EvaluationOrganisationDay(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public List<Statistics_EvaluationOrganisationYear> Statistics_LocalEvaluations_UserYear(int OrganisationId, string UserId) {
            List<Statistics_EvaluationOrganisationYear> retval = new List<Statistics_EvaluationOrganisationYear>();
            sql.commandText = "Statistics_LocalEvaluations_UserYear";
            sql.parameters.AddWithValue("@OrganisationId", OrganisationId);
            sql.parameters.AddWithValue("@UserId", UserId);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new Statistics_EvaluationOrganisationYear(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public List<Statistics_EvaluationOrganisationMonth> Statistics_LocalEvaluations_UserMonthly(int OrganisationId, string UserId, int Year) {
            List<Statistics_EvaluationOrganisationMonth> retval = new List<Statistics_EvaluationOrganisationMonth>();
            sql.commandText = "Statistics_LocalEvaluations_UserMonthly";
            sql.parameters.AddWithValue("@OrganisationId", OrganisationId);
            sql.parameters.AddWithValue("@UserId", UserId);
            sql.parameters.AddWithValue("@Year", Year);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new Statistics_EvaluationOrganisationMonth(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public List<Statistics_EvaluationOrganisationDay> Statistics_LocalEvaluations_UserDaily(int OrganisationId, string UserId, int Year, int Month) {
            List<Statistics_EvaluationOrganisationDay> retval = new List<Statistics_EvaluationOrganisationDay>();
            sql.commandText = "Statistics_LocalEvaluations_UserDaily";
            sql.parameters.AddWithValue("@OrganisationId", OrganisationId);
            sql.parameters.AddWithValue("@UserId", UserId);
            sql.parameters.AddWithValue("@Year", Year);
            sql.parameters.AddWithValue("@Month", Month);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new Statistics_EvaluationOrganisationDay(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public List<Statistics_EvaluationOrganisationYear> Statistics_LocalEvaluations_OrganisationYear(int OrganisationId) {
            List<Statistics_EvaluationOrganisationYear> retval = new List<Statistics_EvaluationOrganisationYear>();
            sql.commandText = "Statistics_LocalEvaluations_OrganisationYear";
            sql.parameters.AddWithValue("@OrganisationId", OrganisationId);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new Statistics_EvaluationOrganisationYear(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public List<Statistics_EvaluationOrganisationMonth> Statistics_LocalEvaluations_OrganisationMonthly(int OrganisationId, int Year) {
            List<Statistics_EvaluationOrganisationMonth> retval = new List<Statistics_EvaluationOrganisationMonth>();
            sql.commandText = "Statistics_LocalEvaluations_OrganisationMonthly";
            sql.parameters.AddWithValue("@OrganisationId", OrganisationId);
            sql.parameters.AddWithValue("@Year", Year);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new Statistics_EvaluationOrganisationMonth(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public List<Statistics_EvaluationOrganisationDay> Statistics_LocalEvaluations_OrganisationDaily(int OrganisationId, int Year, int Month) {
            List<Statistics_EvaluationOrganisationDay> retval = new List<Statistics_EvaluationOrganisationDay>();
            sql.commandText = "Statistics_LocalEvaluations_OrganisationDaily";
            sql.parameters.AddWithValue("@OrganisationId", OrganisationId);
            sql.parameters.AddWithValue("@Year", Year);
            sql.parameters.AddWithValue("@Month", Month);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new Statistics_EvaluationOrganisationDay(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public struct NavisionTimeExport {
            public DateTime Date;
            public string ProjectNumber;
            public string CaseNumber;
            public string Initials;
            public decimal Timespent;
            public long NavisionIndex;
            public string Description;

            public NavisionTimeExport(ref System.Data.SqlClient.SqlDataReader dr) {
                Date = TypeCast.ToDateTime(dr["date"]);
                ProjectNumber = TypeCast.ToString(dr["ProjectNumber"]);
                CaseNumber = TypeCast.ToString(dr["CaseNumber"]);
                Initials = TypeCast.ToString(dr["Initials"]);
                Timespent = TypeCast.ToDecimal(dr["Timespent"]);
                NavisionIndex = TypeCast.ToLong(dr["NavisionIndex"]);
                Description = TypeCast.ToString(dr["Description"]);
            }
        }

        public List<NavisionTimeExport> statistics_exportTimeUsage(User U, bool markAsExported, object DateStart, object DateEnd) {
            List<NavisionTimeExport> retval = new List<NavisionTimeExport>();
            sql.commandText = "statistics_exportTimeUsage";
            sql.parameters.AddWithValue("@OrganisationId", U.OrganisationId);
            sql.parameters.AddWithValue("@UserId", U.Id);
            sql.parameters.AddWithValue("@markAsExported", markAsExported);

            if (DateStart != null) sql.parameters.AddWithValue("@dateStart", DateStart);
            if (DateEnd != null) sql.parameters.AddWithValue("@dateEnd", DateEnd);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new NavisionTimeExport(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public List<List<TableColumnWithValue>> statistics_exportTravelUsage(User U, bool markAsExported, object DateStart, object DateEnd) {
            List<List<TableColumnWithValue>> retval = new List<List<TableColumnWithValue>>();
            sql.commandText = "statistics_exportTravelUSage";
            sql.parameters.AddWithValue("@OrganisationId", U.OrganisationId);
            sql.parameters.AddWithValue("@UserId", U.Id);
            sql.parameters.AddWithValue("@markAsExported", markAsExported);

            if (DateStart != null) sql.parameters.AddWithValue("@dateStart", DateStart);
            if (DateEnd != null) sql.parameters.AddWithValue("@dateEnd", DateEnd);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;

            while (dr.Read()) {
                List<TableColumnWithValue> l = new List<TableColumnWithValue>();

                for (int i = 0; i < dr.FieldCount; i++) {
                    l.Add(new TableColumnWithValue(dr.GetName(i), dr.GetDataTypeName(i), 0, dr[i]));
                }

                retval.Add(l);
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public List<MonthValue> statistics_reportAcivities_meetings(int Year, int OrganisationId, int BaseOrganisationId) {
            List<MonthValue> retval = new List<MonthValue>();
            sql.commandText = "statistics_reportAcivities_meetings";
            if (Year > 0) sql.parameters.AddWithValue("@Year", Year);
            if (OrganisationId > 0) sql.parameters.AddWithValue("@OrganisationId", OrganisationId);
            sql.parameters.AddWithValue("@BaseOrganisationId", BaseOrganisationId);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new MonthValue(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public List<MonthValue> statistics_reportAcivities_contacts(int Year, string type, int OrganisationId, int BaseOrganisationId) {
            List<MonthValue> retval = new List<MonthValue>();
            sql.commandText = "statistics_reportAcivities_contacts";
            if (Year > 0) sql.parameters.AddWithValue("@Year", Year);
            if (OrganisationId > 0) sql.parameters.AddWithValue("@OrganisationId", OrganisationId);
            if (type != "") sql.parameters.AddWithValue("@type", type);
            sql.parameters.AddWithValue("@BaseOrganisationId", BaseOrganisationId);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new MonthValue(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public List<MonthValue> statistics_reportAcivities_companies(int Year, string type, int OrganisationId, int BaseOrganisationId) {
            List<MonthValue> retval = new List<MonthValue>();
            sql.commandText = "statistics_reportAcivities_companies";
            if (Year > 0) sql.parameters.AddWithValue("@Year", Year);
            if (OrganisationId > 0) sql.parameters.AddWithValue("@OrganisationId", OrganisationId);
            if (type != "") sql.parameters.AddWithValue("@type", type);
            sql.parameters.AddWithValue("@BaseOrganisationId", BaseOrganisationId);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new MonthValue(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public List<RessourceValue> statistics_reportCounsellingHours(int Year, int OrganisationId, int type, int BaseOrganisationId) {
            List<RessourceValue> retval = new List<RessourceValue>();
            sql.commandText = "statistics_reportCounsellingHours";
            if (Year > 0) sql.parameters.AddWithValue("@Year", Year);
            if (OrganisationId > 0) sql.parameters.AddWithValue("@OrganisationId", OrganisationId);
            if (type > -1) sql.parameters.AddWithValue("@type", type);
            sql.parameters.AddWithValue("@BaseOrganisationId", BaseOrganisationId);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new RessourceValue(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public List<RessourceValue> statistics_reportCounsellingTypes(int Year, int OrganisationId, int type, int BaseOrganisationId) {
            List<RessourceValue> retval = new List<RessourceValue>();
            sql.commandText = "statistics_reportCounsellingTypes";
            if (Year > 0) sql.parameters.AddWithValue("@Year", Year);
            if (OrganisationId > 0) sql.parameters.AddWithValue("@OrganisationId", OrganisationId);
            if (type > -1) sql.parameters.AddWithValue("@type", type);
            sql.parameters.AddWithValue("@BaseOrganisationId", BaseOrganisationId);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new RessourceValue(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }


        public List<RessourceValue> statistics_reportRessources(int Year, int OrganisationId, int BaseOrganisationId) {
            List<RessourceValue> retval = new List<RessourceValue>();
            sql.commandText = "statistics_reportRessources";
            if (Year > 0) sql.parameters.AddWithValue("@Year", Year);
            if (OrganisationId > 0) sql.parameters.AddWithValue("@OrganisationId", OrganisationId);
            sql.parameters.AddWithValue("@BaseOrganisationId", BaseOrganisationId);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new RessourceValue(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public List<UserEvaluationStatistics> Statistics_GetUserEvaluationStatistics(int Year, User U) {
            List<UserEvaluationStatistics> retval = new List<UserEvaluationStatistics>();
            sql.commandText = "Statistics_GetUser" + (U.Organisation.Type == Organisation.OrganisationType.County ? "Local" : "") + "EvaluationStatistics";
            if (Year > 0) sql.parameters.AddWithValue("@Year", Year);
            if (U.OrganisationId > 0) sql.parameters.AddWithValue("@OrganisationId", U.OrganisationId);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new UserEvaluationStatistics(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public List<RessourceValue> statistics_reportContacts(int Year, int OrganisationId, string FieldName, int UserOrganisationId) {
            try {


                List<RessourceValue> retval = new List<RessourceValue>();

                string SQL = "select \n" +
                            "   count(*) as value,\n" +
                            "   convert(varchar(8000)," + FieldName + ") as [name]\n" +
                            "from\n" +
                            "   ContactsAndCompaniesList_" + UserOrganisationId + "()\n" +
                            "Where\n" +
                            "   (year(ContactDateStamp)=" + Year + " or 0 = " + Year + ") AND\n" +
                            (OrganisationId > 0 ?
                            "   (ContactOrganisationId=" + OrganisationId + ")\n" :
                            "   dbo.GetOrganisationAndChildrenIds(" + UserOrganisationId + ") like '%,' + convert(varchar(8000), ContactOrganisationId) + ',%'") +
                            "Group By\n" +
                            "   " + FieldName + "\n" +
                            "order by\n" +
                            "   " + FieldName + "\n";
                //"   convert(varchar(8000)," + FieldName + ")\n";

                sql.commandType = System.Data.CommandType.Text;
                sql.commandText = SQL;


                System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
                while (dr.Read()) {
                    retval.Add(new RessourceValue(ref dr));
                }
                dr.Close();
                sql.reset();

                return retval;
            } catch {
                sql.reset();
                return statistics_reportContactsVarchar(Year, OrganisationId, FieldName, UserOrganisationId);
            }
        }
        private List<RessourceValue> statistics_reportContactsVarchar(int Year, int OrganisationId, string FieldName, int UserOrganisationId) {
            List<RessourceValue> retval = new List<RessourceValue>();

            string SQL = "select \n" +
                        "   count(*) as value,\n" +
                        "   convert(varchar(8000)," + FieldName + ") as [name]\n" +
                        "from\n" +
                        "   ContactsAndCompaniesList_" + UserOrganisationId + "()\n" +
                        "Where\n" +
                        "   (year(ContactDateStamp)=" + Year + " or 0 = " + Year + ") AND\n" +
                        (OrganisationId > 0 ?
                        "   (ContactOrganisationId=" + OrganisationId + ")\n" :
                        "   dbo.GetOrganisationAndChildrenIds(" + UserOrganisationId + ") like '%,' + convert(varchar(8000), ContactOrganisationId) + ',%'") +
                        "Group By\n" +
                        "   convert(varchar(8000)," + FieldName + ")\n" +
                        "order by\n" +
                        "   convert(varchar(8000)," + FieldName + ")\n";
            //"   convert(varchar(8000)," + FieldName + ")\n";

            sql.commandType = System.Data.CommandType.Text;
            sql.commandText = SQL;


            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new RessourceValue(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public List<List<String>> statistics_runReports(int OrganisationId, string tables, string fields, string arguments) {
            List<List<String>> retval = new List<List<String>>();

            string SQL = "select \n" +
                        fields + "\n" +
                        "from \n" +
                        tables;

            sql.commandType = System.Data.CommandType.Text;
            sql.commandText = SQL;

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;

            bool firstLine = true;

            while (dr.Read()) {

                if (firstLine) {
                    List<string> strF = new List<string>();

                    for (int i = 0; i < dr.FieldCount; i++) {
                        strF.Add(TypeCast.ToString(dr.GetName(i)));
                    }
                    retval.Add(strF);

                    firstLine = false;
                }

                List<string> str = new List<string>();
                for (int i = 0; i < dr.FieldCount; i++) {
                    str.Add(TypeCast.ToString(dr[i]));
                }
                retval.Add(str);
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public List<List<TableColumnWithValue>> statistics_executeReportGenerator(string SQL) {
            List<List<TableColumnWithValue>> retval = new List<List<TableColumnWithValue>>();

            sql.commandType = System.Data.CommandType.Text;
            sql.commandText = SQL;

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;

            while (dr.Read()) {
                List<TableColumnWithValue> l = new List<TableColumnWithValue>();

                for (int i = 0; i < dr.FieldCount; i++) {
                    l.Add(new TableColumnWithValue(dr.GetName(i), dr.GetDataTypeName(i), 0, dr[i]));
                }

                retval.Add(l);
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public string statistics_executeReportGeneratorXML(string SQL) {
            string retval = "";

            sql.commandType = System.Data.CommandType.Text;
            sql.commandText = SQL;

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;

            while (dr.Read()) {
                retval += TypeCast.ToString(dr[0]);
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public void sqlExecute(string SQL) {
            sql.commandType = System.Data.CommandType.Text;
            sql.commandText = SQL;

            sql.execute();
            sql.reset();
        }

        public List<DynamicField> reportGenerator_getTableFields(User U, string table) {
            List<DynamicField> retval = new List<DynamicField>();
            sql.commandText = "reportGenerator_getTableFields";
            sql.parameters.AddWithValue("@table", table);
            sql.parameters.AddWithValue("@OrganisationId", U.OrganisationId);
            sql.parameters.AddWithValue("@userId", U.Id);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new DynamicField(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public void ReportGenerator_SaveReportShare(int ReportId, int SharedWithOrganisationId) {
            sql.commandText = "ReportGenerator_SaveReportShare";
            sql.parameters.AddWithValue("@ReportId", ReportId);
            sql.parameters.AddWithValue("@SharedWithOrganisationId", SharedWithOrganisationId);
            sql.execute();
            sql.reset();
        }

        public int ReportGenerator_SaveReport(int Id, string Name, string Description, string UserId, int SharingLevel, bool ShareWithAny, int VisibleTo, string IntegrationKey, string IntegrationReportDescription, string SQL) {
            int retval = 0;
            sql.commandText = "ReportGenerator_SaveReport";
            if (Id > 0) sql.parameters.AddWithValue("@Id", Id);
            sql.parameters.AddWithValue("@Name", Name);
            sql.parameters.AddWithValue("@Description", Description);
            sql.parameters.AddWithValue("@UserId", UserId);
            if (SharingLevel > -1) {
                sql.parameters.AddWithValue("@SharedToUserLevel", SharingLevel);
                sql.parameters.AddWithValue("@SharedToLocalUnit", ShareWithAny);
            }
            sql.parameters.AddWithValue("@VisibleTo", VisibleTo);
            sql.parameters.AddWithValue("@IntegrationKey", IntegrationKey);
            sql.parameters.AddWithValue("@IntegrationReportDescription", IntegrationReportDescription);
            sql.parameters.AddWithValue("@SQL", SQL);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = TypeCast.ToInt(dr["id"]);
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public void ReportGenerator_SaveTable(int ReportId, string TableName, int X, int Y, int Width, int Height) {

            sql.commandText = "ReportGenerator_SaveTable";
            sql.parameters.AddWithValue("@ReportId", ReportId);
            sql.parameters.AddWithValue("@TableName", TableName);

            sql.parameters.AddWithValue("@X", X);
            sql.parameters.AddWithValue("@Y", Y);
            sql.parameters.AddWithValue("@Width", Width);
            sql.parameters.AddWithValue("@Height", Height);

            sql.execute();

            sql.reset();
        }

        public void ReportGenerator_SaveField(int ReportId, string FieldId) {

            sql.commandText = "ReportGenerator_SaveField";
            sql.parameters.AddWithValue("@ReportId", ReportId);
            sql.parameters.AddWithValue("@FieldId", FieldId);

            sql.execute();

            sql.reset();
        }

        public void ReportGenerator_deleteReport(int Id, string UserId) {

            sql.commandText = "ReportGenerator_deleteReport";
            sql.parameters.AddWithValue("@Id", Id);
            sql.parameters.AddWithValue("@UserId", UserId);

            sql.execute();

            sql.reset();
        }

        public void ReportGenerator_SaveFilter(int ReportId, string FieldId, string FilterCondition, string FilterValue) {

            sql.commandText = "ReportGenerator_SaveFilter";
            sql.parameters.AddWithValue("@ReportId", ReportId);
            sql.parameters.AddWithValue("@FieldId", FieldId);
            sql.parameters.AddWithValue("@FilterCondition", FilterCondition);
            sql.parameters.AddWithValue("@FilterValue", FilterValue);

            sql.execute();

            sql.reset();
        }

        public void ReportGenerator_SetIntegrationApprovalById(User AdminUser, int ReportId, bool Approved, string NotApprovedReason) {
            sql.commandText = "ReportGenerator_SetIntegrationApprovalById";
            sql.parameters.AddWithValue("@adminId", AdminUser.Id);
            sql.parameters.AddWithValue("@organisationId", AdminUser.OrganisationId);
            sql.parameters.AddWithValue("@Id", ReportId);
            sql.parameters.AddWithValue("@approved", Approved);
            sql.parameters.AddWithValue("@notApprovedReason", NotApprovedReason);

            sql.execute();

            sql.reset();
        }

        public List<ReportGeneratorSavedTable> ReportGenerator_LoadTables(int ReportId) {
            List<ReportGeneratorSavedTable> retval = new List<ReportGeneratorSavedTable>();
            sql.commandText = "ReportGenerator_LoadTables";
            sql.parameters.AddWithValue("@ReportId", ReportId);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new ReportGeneratorSavedTable(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public List<string> ReportGenerator_LoadFields(int ReportId) {
            List<string> retval = new List<string>();
            sql.commandText = "ReportGenerator_LoadFields";
            sql.parameters.AddWithValue("@ReportId", ReportId);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(TypeCast.ToString(dr["fieldId"]));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public List<ReportGeneratorSavedReport> ReportGenerator_searchSavedReportsPendingIntegration(string Keyword, bool? Pending, bool? Approved, bool? Declined) {
            List<ReportGeneratorSavedReport> retval = new List<ReportGeneratorSavedReport>();
            sql.commandText = "ReportGenerator_searchSavedReportsPendingIntegration";

            if (Keyword != null && Keyword != "") sql.parameters.AddWithValue("@name", Keyword);
            if (Pending != null) sql.parameters.AddWithValue("@Pending", Pending);
            if (Approved != null) sql.parameters.AddWithValue("@Approved", Approved);
            if (Declined != null) sql.parameters.AddWithValue("@Declined", Declined);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new ReportGeneratorSavedReport(ref dr, false));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public List<ReportGeneratorSavedReport> ReportGenerator_getSavedReportsPendingIntegrationApproval() {
            List<ReportGeneratorSavedReport> retval = new List<ReportGeneratorSavedReport>();
            sql.commandText = "ReportGenerator_getSavedReportsPendingIntegrationApproval";


            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new ReportGeneratorSavedReport(ref dr, false));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public List<ReportGeneratorSavedReport> ReportGenerator_getSavedReports(string userId, bool sharingCenter) {
            List<ReportGeneratorSavedReport> retval = new List<ReportGeneratorSavedReport>();
            sql.commandText = "ReportGenerator_getSavedReports";
            sql.parameters.AddWithValue("@userId", userId);
            sql.parameters.AddWithValue("@sharingCenter", sharingCenter);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new ReportGeneratorSavedReport(ref dr, false));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public ReportGeneratorSavedReport ReportGenerator_getSavedReport(int ReportId) {
            ReportGeneratorSavedReport retval = null;
            sql.commandText = "ReportGenerator_getSavedReport";
            sql.parameters.AddWithValue("@ReportId", ReportId);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = new ReportGeneratorSavedReport(ref dr, true);
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public List<ReportGeneratorSavedFilter> ReportGenerator_LoadFilters(int ReportId) {
            List<ReportGeneratorSavedFilter> retval = new List<ReportGeneratorSavedFilter>();
            sql.commandText = "ReportGenerator_LoadFilters";
            sql.parameters.AddWithValue("@ReportId", ReportId);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new ReportGeneratorSavedFilter(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public List<string> ReportGenerator_LookUpOrganisationInfo(string organisationPart, int OrganisationId) {
            List<string> retval = new List<string>();
            sql.commandText = "ReportGenerator_LookUpOrganisationInfo";
            sql.parameters.AddWithValue("@organisationPart", organisationPart);
            sql.parameters.AddWithValue("@OrganisationId", OrganisationId);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(TypeCast.ToString(dr["Result"]));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public List<string> ReportGenerator_LookUpUserInfo(string userPart, int OrganisationId) {
            List<string> retval = new List<string>();
            sql.commandText = "ReportGenerator_LookUpUserInfo";
            sql.parameters.AddWithValue("@userPart", userPart);
            sql.parameters.AddWithValue("@OrganisationId", OrganisationId);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(TypeCast.ToString(dr["Result"]));
            }
            dr.Close();
            sql.reset();

            return retval;
        }
        #endregion

        #region Zipcode

        public string Zipcodes_Lookup(string zipcode) {
            string retval = "";
            sql.commandText = "Zipcodes_Lookup";
            sql.parameters.AddWithValue("@zipcode", zipcode);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = TypeCast.ToString(dr["city"]);
            }
            dr.Close();
            sql.reset();

            return retval;
        }


        public List<string> Zipcodes_LookupCounty(string zipcode) {
            List<string> retval = new List<string>();
            sql.commandText = "Zipcodes_LookupCounty";
            sql.parameters.AddWithValue("@zipcode", zipcode);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(TypeCast.ToString(dr["county"]));
            }
            dr.Close();
            sql.reset();

            return retval;
        }
        #endregion

        #region DeletedItems

        public List<MailgroupDeleted> DeletedItems_getMailGroups(int organisationId) {
            sql.commandText = "DeletedItems_getMailGroups";
            sql.parameters.AddWithValue("@organisationId", organisationId);

            List<MailgroupDeleted> listMailgroup = new List<MailgroupDeleted>();
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                listMailgroup.Add(new MailgroupDeleted(ref dr));
            }
            dr.Close();
            sql.reset();

            return listMailgroup;
        }

        public List<UserDeleted> DeletedItems_getUsers(int organisationId) {
            sql.commandText = "DeletedItems_getUsers";
            sql.parameters.AddWithValue("@organisationId", organisationId);

            List<UserDeleted> listMailgroup = new List<UserDeleted>();
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                listMailgroup.Add(new UserDeleted(ref dr));
            }
            dr.Close();
            sql.reset();

            return listMailgroup;
        }

        public List<FileFolderDeleted> DeletedItems_getFolders(int organisationId) {
            sql.commandText = "DeletedItems_getFolders";
            sql.parameters.AddWithValue("@organisationId", organisationId);

            List<FileFolderDeleted> listMailgroup = new List<FileFolderDeleted>();
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                listMailgroup.Add(new FileFolderDeleted(ref dr));
            }
            dr.Close();
            sql.reset();

            return listMailgroup;
        }

        public List<FileDeleted> DeletedItems_getFiles(int organisationId) {
            sql.commandText = "DeletedItems_getFiles";
            sql.parameters.AddWithValue("@organisationId", organisationId);

            List<FileDeleted> listMailgroup = new List<FileDeleted>();
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                listMailgroup.Add(new FileDeleted(ref dr));
            }
            dr.Close();
            sql.reset();

            return listMailgroup;
        }

        public List<PartnerCompanyDeleted> DeletedItems_getSAMCompanies(int organisationId) {
            sql.commandText = "DeletedItems_getSAMCompanies";
            sql.parameters.AddWithValue("@organisationId", organisationId);

            List<PartnerCompanyDeleted> listMailgroup = new List<PartnerCompanyDeleted>();
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                listMailgroup.Add(new PartnerCompanyDeleted(ref dr));
            }
            dr.Close();
            sql.reset();

            return listMailgroup;
        }

        public List<PartnerContactDeleted> DeletedItems_getSAMContacts(int organisationId) {
            sql.commandText = "DeletedItems_getSAMContacts";
            sql.parameters.AddWithValue("@organisationId", organisationId);

            List<PartnerContactDeleted> listMailgroup = new List<PartnerContactDeleted>();
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                listMailgroup.Add(new PartnerContactDeleted(ref dr));
            }
            dr.Close();
            sql.reset();

            return listMailgroup;
        }

        public List<CompanyDeleted> DeletedItems_GetDeletedCompanies(int organisationId) {
            sql.commandText = "z_Companies_GetDeletedCompanies_" + organisationId;

            List<CompanyDeleted> listMailgroup = new List<CompanyDeleted>();
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                listMailgroup.Add(new CompanyDeleted(ref dr));
            }
            dr.Close();
            sql.reset();

            return listMailgroup;
        }

        public List<ContactDeleted> DeletedItems_GetDeletedContacts(int organisationId) {
            sql.commandText = "z_Contacts_GetDeletedContacts_" + organisationId;

            List<ContactDeleted> listMailgroup = new List<ContactDeleted>();
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                listMailgroup.Add(new ContactDeleted(ref dr));
            }
            dr.Close();
            sql.reset();

            return listMailgroup;
        }

        public List<NoteDeleted> DeletedItems_GetDeletedNotes(int organisationId) {
            sql.commandText = "z_Contacts_GetDeletedNotes_" + organisationId;

            List<NoteDeleted> listMailgroup = new List<NoteDeleted>();
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                listMailgroup.Add(new NoteDeleted(ref dr));
            }
            dr.Close();
            sql.reset();

            return listMailgroup;
        }

        public void DeletedItems_UnDelete_File(int Id) {
            sql.commandText = "DeletedItems_UnDelete_File";
            sql.parameters.AddWithValue("@id", Id);
            sql.execute();
            sql.reset();
        }

        public void DeletedItems_UnDelete_FileFolder(int Id) {
            sql.commandText = "DeletedItems_UnDelete_FileFolder";
            sql.parameters.AddWithValue("@id", Id);
            sql.execute();
            sql.reset();
        }

        public void DeletedItems_UnDelete_MailGroup(int Id) {
            sql.commandText = "DeletedItems_UnDelete_MailGroup";
            sql.parameters.AddWithValue("@id", Id);
            sql.execute();
            sql.reset();
        }


        public void DeletedItems_UnDelete_SAMCompany(int Id) {
            sql.commandText = "DeletedItems_UnDelete_SAMCompany";
            sql.parameters.AddWithValue("@id", Id);
            sql.execute();
            sql.reset();
        }

        public void DeletedItems_UnDelete_SAMContact(int Id) {
            sql.commandText = "DeletedItems_UnDelete_SAMContact";
            sql.parameters.AddWithValue("@id", Id);
            sql.execute();
            sql.reset();
        }

        public void DeletedItems_UnDelete_User(string Id) {
            sql.commandText = "DeletedItems_UnDelete_User";
            sql.parameters.AddWithValue("@id", Id);
            sql.execute();
            sql.reset();
        }

        public void DeletedItems_UnDelete_Company(int Id) {
            sql.commandText = "DeletedItems_UnDelete_Company";
            sql.parameters.AddWithValue("@id", Id);
            sql.execute();
            sql.reset();
        }

        public void DeletedItems_UnDelete_Contact(int Id) {
            sql.commandText = "DeletedItems_UnDelete_Contact";
            sql.parameters.AddWithValue("@id", Id);
            sql.execute();
            sql.reset();
        }

        public void DeletedItems_UnDelete_Note(int Id) {
            sql.commandText = "DeletedItems_UnDelete_Note";
            sql.parameters.AddWithValue("@id", Id);
            sql.execute();
            sql.reset();
        }

        #endregion

        #region EarlyWarning

        public List<int> EarlyWarning_Companies_GetCompanyIdsFromCVR(string cvr) {
            List<int> retval = new List<int>();
            sql.commandText = "EarlyWarning_Companies_GetCompanyIdsFromCVR";
            sql.parameters.AddWithValue("@cvr", cvr);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add((int)dr["id"]);
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public EarlyWarningContact EarlyWarning_Companies_getContact(int Id) {
            EarlyWarningContact retval = null;
            sql.commandText = "EarlyWarning_Companies_getContact";
            sql.parameters.AddWithValue("@id", Id);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = new EarlyWarningContact(ref dr);
            }
            dr.Close();
            sql.reset();

            return retval;
        }
        public List<EarlyWarningContact> EarlyWarning_Companies_getContacts(int Id) {
            List<EarlyWarningContact> retval = new List<EarlyWarningContact>();
            sql.commandText = "EarlyWarning_Companies_getContacts";
            sql.parameters.AddWithValue("@id", Id);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new EarlyWarningContact(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }
        public List<EarlyWarningContact> EarlyWarning_Companies_getContactsFromEmail(string email) {
            List<EarlyWarningContact> retval = new List<EarlyWarningContact>();
            sql.commandText = "EarlyWarning_Companies_getContactsFromEmail";
            sql.parameters.AddWithValue("@email", email);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new EarlyWarningContact(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }
        public EarlyWarningCompany EarlyWarning_Companies_getCompany(int Id) {
            EarlyWarningCompany retval = null;
            sql.commandText = "EarlyWarning_Companies_getCompany";
            sql.parameters.AddWithValue("@id", Id);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = new EarlyWarningCompany(ref dr);
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public EarlyWarningFileWithData EarlyWarning_Companies_GetFile(int Id) {
            EarlyWarningFileWithData retval = null;
            sql.commandText = "EarlyWarning_Companies_GetFile";
            sql.parameters.AddWithValue("@id", Id);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = new EarlyWarningFileWithData(ref dr);
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public EarlyWarningFileWithData EarlyWarning_Volunteers_GetResume(int Id) {
            EarlyWarningFileWithData retval = null;
            sql.commandText = "EarlyWarning_Volunteers_GetResume";
            sql.parameters.AddWithValue("@id", Id);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = new EarlyWarningFileWithData(ref dr);
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public void EarlyWarning_Companies_AddPartner(int PartnerId, int CompanyId, string UserId) {
            sql.commandText = "EarlyWarning_Companies_AddPartner";
            sql.parameters.AddWithValue("@PartnerId", PartnerId);
            sql.parameters.AddWithValue("@CompanyId", CompanyId);
            sql.parameters.AddWithValue("@UserId", UserId);
            sql.execute();
            sql.reset();
        }
        public void EarlyWarning_Companies_RemovePartner(int PartnerId, int CompanyId) {
            sql.commandText = "EarlyWarning_Companies_RemovePartner";
            sql.parameters.AddWithValue("@PartnerId", PartnerId);
            sql.parameters.AddWithValue("@CompanyId", CompanyId);
            sql.execute();
            sql.reset();
        }


        public void EarlyWarning_Companies_deleteContact(int Id, string UserId) {
            sql.commandText = "EarlyWarning_Companies_deleteContact";
            sql.parameters.AddWithValue("@id", Id);
            sql.parameters.AddWithValue("@UserId", UserId);
            sql.execute();
            sql.reset();
        }

        public void EarlyWarning_Companies_unDeleteContact(int Id) {
            sql.commandText = "EarlyWarning_Companies_unDeleteContact";
            sql.parameters.AddWithValue("@id", Id);
            sql.execute();
            sql.reset();
        }

        public void EarlyWarning_Volunteers_Undelete(int Id) {
            sql.commandText = "EarlyWarning_Volunteers_Undelete";
            sql.parameters.AddWithValue("@id", Id);
            sql.execute();
            sql.reset();
        }


        public void EarlyWarning_Companies_DeleteFile(int Id, string UserId) {
            sql.commandText = "EarlyWarning_Companies_DeleteFile";
            sql.parameters.AddWithValue("@id", Id);
            sql.parameters.AddWithValue("@UserId", UserId);
            sql.execute();
            sql.reset();
        }

        public void EarlyWarning_Notes_DeleteNote(int id, string UserId) {
            sql.commandText = "EarlyWarning_Notes_DeleteNote";
            sql.parameters.AddWithValue("@id", id);
            sql.parameters.AddWithValue("@UserId", UserId);
            sql.execute();
            sql.reset();
        }

        public void EarlyWarning_Companies_AddVolunteer(int VolunteerId, int CompanyId, string UserId) {
            sql.commandText = "EarlyWarning_Companies_AddVolunteer";
            sql.parameters.AddWithValue("@CompanyId", CompanyId);
            sql.parameters.AddWithValue("@VolunteerId", VolunteerId);
            sql.parameters.AddWithValue("@UserId", UserId);
            sql.execute();
            sql.reset();
        }

        public int EarlyWarning_Companies_UploadFile(int CompanyId, string Filename, byte[] Data, string CreatedBy) {
            int retval = 0;

            sql.commandText = "EarlyWarning_Companies_UploadFile";
            sql.parameters.AddWithValue("@CompanyId", CompanyId);
            sql.parameters.AddWithValue("@Filename", Filename);
            sql.parameters.AddWithValue("@Data", Data);
            sql.parameters.AddWithValue("@CreatedBy", CreatedBy);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = TypeCast.ToInt(dr["id"]);
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public void EarlyWarning_Volunteers_DeleteResume(int Id, string UserId) {
            sql.commandText = "EarlyWarning_Volunteers_DeleteResume";
            sql.parameters.AddWithValue("@id", Id);
            sql.parameters.AddWithValue("@UserId", UserId);
            sql.execute();
            sql.reset();
        }

        public int EarlyWarning_Volunteers_AddResume(int VolunteerId, string Filename, byte[] Data, string CreatedBy) {
            int retval = 0;
            sql.commandText = "EarlyWarning_Volunteers_AddResume";
            sql.parameters.AddWithValue("@VolunteerId", VolunteerId);
            sql.parameters.AddWithValue("@Filename", Filename);
            sql.parameters.AddWithValue("@Data", Data);
            sql.parameters.AddWithValue("@CreatedBy", CreatedBy);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = TypeCast.ToInt(dr["id"]);
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public void EarlyWarning_Companies_DeleteVolunteerReleation(int volunteerId, int companyId, string UserId) {
            sql.commandText = "EarlyWarning_Companies_DeleteVolunteerReleation";
            sql.parameters.AddWithValue("@volunteerId", volunteerId);
            sql.parameters.AddWithValue("@companyId", companyId);
            sql.parameters.AddWithValue("@UserId", UserId);
            sql.execute();
            sql.reset();
        }

        public void EarlyWarning_Volunteers_DeleteVolunteer(int Id, string UserId) {
            sql.commandText = "EarlyWarning_Volunteers_DeleteVolunteer";
            sql.parameters.AddWithValue("@id", Id);
            sql.parameters.AddWithValue("@UserId", UserId);
            sql.execute();
            sql.reset();
        }

        public void EarlyWarning_Volunteers_addSectorCode(int SectorCodeId, int VolunteerId) {
            sql.commandText = "EarlyWarning_Volunteers_addSectorCode";
            sql.parameters.AddWithValue("@SectorCodeId", SectorCodeId);
            sql.parameters.AddWithValue("@VolunteerId", VolunteerId);
            sql.execute();
            sql.reset();
        }

        public void EarlyWarning_Volunteers_addCompetence(int CompetenceId, int VolunteerId) {
            sql.commandText = "EarlyWarning_Volunteers_addCompetence";
            sql.parameters.AddWithValue("@CompetenceId", CompetenceId);
            sql.parameters.AddWithValue("@VolunteerId", VolunteerId);
            sql.execute();
            sql.reset();
        }


        public List<EarlyWarningNote> EarlyWarning_Companies_GetNotes(int companyId, int ContactId) {
            List<EarlyWarningNote> retval = new List<EarlyWarningNote>();
            sql.commandText = "EarlyWarning_Companies_GetNotes";
            sql.parameters.AddWithValue("@companyId", companyId);
            sql.parameters.AddWithValue("@ContactId", ContactId);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new EarlyWarningNote(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }
        public List<EarlyWarningNote> EarlyWarning_Volunteers_GetNotes(int Id) {
            List<EarlyWarningNote> retval = new List<EarlyWarningNote>();
            sql.commandText = "EarlyWarning_Volunteers_GetNotes";
            sql.parameters.AddWithValue("@Id", Id);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new EarlyWarningNote(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public int EarlyWarning_Notes_UpdateNote(EarlyWarningNote note, User U) {
            int retval = 0;
            sql.commandText = "EarlyWarning_Notes_UpdateNote";
            if (note.Id > 0) sql.parameters.AddWithValue("@id", note.Id);
            if (note.CompanyId > 0) sql.parameters.AddWithValue("@CompanyId", note.CompanyId);
            if (note.ContactId > 0) sql.parameters.AddWithValue("@ContactId", note.ContactId);
            if (note.VolunteerId > 0) sql.parameters.AddWithValue("@VolunteerId", note.VolunteerId);
            sql.parameters.AddWithValue("@Title", note.Title);
            sql.parameters.AddWithValue("@Body", note.Body);
            sql.parameters.AddWithValue("@userId", U.Id);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = TypeCast.ToInt(dr["id"]);
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public EarlyWarningNote EarlyWarning_Notes_getNote(int id) {
            EarlyWarningNote retval = null;
            sql.commandText = "EarlyWarning_Notes_getNote";
            sql.parameters.AddWithValue("@id", id);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = new EarlyWarningNote(ref dr);
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public List<PartnerContactForList> EarlyWarning_Companies_getPartners(int companyId) {
            List<PartnerContactForList> retval = new List<PartnerContactForList>();
            sql.commandText = "EarlyWarning_Companies_getPartners";
            sql.parameters.AddWithValue("@companyId", companyId);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new PartnerContactForList(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }


        public List<EarlyWarningSectorCode> EarlyWarning_Volunteers_GetSectorCodes(int VolunteerId) {
            List<EarlyWarningSectorCode> retval = new List<EarlyWarningSectorCode>();
            sql.commandText = "EarlyWarning_Volunteers_GetSectorCodes";
            sql.parameters.AddWithValue("@VolunteerId", VolunteerId);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new EarlyWarningSectorCode(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public int EarlyWarning_Volunteers_updateVolunteer(EarlyWarningVolunteer ev, User U) {
            int retval = 0;
            sql.commandText = "EarlyWarning_Volunteers_updateVolunteer";

            if (ev.Id > 0) sql.parameters.AddWithValue("@Id", ev.Id);
            sql.parameters.AddWithValue("@UserId", U.Id);
            sql.parameters.AddWithValue("@OrganisationId", U.OrganisationId);
            sql.parameters.AddWithValue("@Firstname", ev.Firstname);
            sql.parameters.AddWithValue("@Lastname", ev.Lastname);
            sql.parameters.AddWithValue("@Phone", ev.Phone);
            sql.parameters.AddWithValue("@Mobile", ev.Mobile);
            sql.parameters.AddWithValue("@Email", ev.Email);
            sql.parameters.AddWithValue("@Address", ev.Address);
            sql.parameters.AddWithValue("@Zipcode", ev.Zipcode);
            sql.parameters.AddWithValue("@City", ev.City);
            sql.parameters.AddWithValue("@CV", ev.CV);
            sql.parameters.AddWithValue("@Memo", ev.Memo);
            sql.parameters.AddWithValue("@competence_Design", ev.Competence_Design);
            sql.parameters.AddWithValue("@competence_Export", ev.Competence_Export);
            sql.parameters.AddWithValue("@competence_HR", ev.Competence_HR);
            sql.parameters.AddWithValue("@competence_Purchase", ev.Competence_Purchase);
            sql.parameters.AddWithValue("@competence_IPR", ev.Competence_IPR);
            sql.parameters.AddWithValue("@competence_IT", ev.Competence_IT);
            sql.parameters.AddWithValue("@competence_CommunicationsAndPR", ev.Competence_CommunicationsAndPR);
            sql.parameters.AddWithValue("@competence_Logistics", ev.Competence_Logistics);
            sql.parameters.AddWithValue("@competence_Advertising", ev.Competence_Advertising);
            sql.parameters.AddWithValue("@competence_Production", ev.Competence_Production);
            sql.parameters.AddWithValue("@competence_Sales", ev.Competence_Sales);
            sql.parameters.AddWithValue("@competence_Strategy", ev.Competence_Strategy);
            sql.parameters.AddWithValue("@competence_ProductDevelopment", ev.Competence_ProductDevelopment);
            sql.parameters.AddWithValue("@competence_Finance", ev.Competence_Finance);

            sql.parameters.AddWithValue("@ContactToEWConsultant", ev.ContactToEWConsultant);
            sql.parameters.AddWithValue("@Active", ev.Active);
            sql.parameters.AddWithValue("@Radius_Hovedstaden", ev.Radius_Hovedstaden);
            sql.parameters.AddWithValue("@Radius_Midtjylland", ev.Radius_Midtjylland);
            sql.parameters.AddWithValue("@Radius_Syddanmark", ev.Radius_Syddanmark);
            sql.parameters.AddWithValue("@Radius_Sjaelland", ev.Radius_Sjaelland);
            sql.parameters.AddWithValue("@Radius_Nordjylland", ev.Radius_Nordjylland);



            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = TypeCast.ToInt(dr["id"]);
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public EarlyWarningVolunteer EarlyWarning_Volunteers_getVolunteer(int Id) {
            EarlyWarningVolunteer retval = null;
            sql.commandText = "EarlyWarning_Volunteers_getVolunteer";
            sql.parameters.AddWithValue("@id", Id);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = new EarlyWarningVolunteer(ref dr);
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public List<EarlyWarningCompanyAndContact> EarlyWarning_Companies_searchCompaniesAndContacts(int SearchIn, string Query, string UserId, int Status, string SortOrder, string SortAsc) {
            List<EarlyWarningCompanyAndContact> retval = new List<EarlyWarningCompanyAndContact>();
            sql.commandText = "EarlyWarning_Companies_searchCompaniesAndContacts";
            sql.parameters.AddWithValue("@SearchIn", SearchIn);
            sql.parameters.AddWithValue("@Query", Query);
            sql.parameters.AddWithValue("@UserId", UserId);
            if (Status > -1) sql.parameters.AddWithValue("@Status", Status);
            sql.parameters.AddWithValue("@sortOrder", SortOrder);
            sql.parameters.AddWithValue("@sortAsc", SortAsc);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new EarlyWarningCompanyAndContact(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public List<EarlyWarningCompanyAndContact> EarlyWarning_Companies_getRecentCompaniesAndContacts(string UserId) {
            List<EarlyWarningCompanyAndContact> retval = new List<EarlyWarningCompanyAndContact>();
            sql.commandText = "EarlyWarning_Companies_getRecentCompaniesAndContacts";
            sql.parameters.AddWithValue("@UserId", UserId);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new EarlyWarningCompanyAndContact(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public List<EarlyWarningCompanyRelation> EarlyWarning_Volunteers_GetCompaniesRelations(int VolunteerId) {
            List<EarlyWarningCompanyRelation> retval = new List<EarlyWarningCompanyRelation>();
            sql.commandText = "EarlyWarning_Volunteers_GetCompaniesRelations";
            sql.parameters.AddWithValue("@VolunteerId", VolunteerId);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new EarlyWarningCompanyRelation(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public List<EarlyWarningCompanyAndContact> EarlyWarning_Volunteers_GetCompanies(int VolunteerId) {
            List<EarlyWarningCompanyAndContact> retval = new List<EarlyWarningCompanyAndContact>();
            sql.commandText = "EarlyWarning_Volunteers_GetCompanies";
            sql.parameters.AddWithValue("@VolunteerId", VolunteerId);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new EarlyWarningCompanyAndContact(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public List<EarlyWarningVolunteer> EarlyWarning_Companies_GetVolunteers(int CompanyId) {
            List<EarlyWarningVolunteer> retval = new List<EarlyWarningVolunteer>();
            sql.commandText = "EarlyWarning_Companies_GetVolunteers";
            sql.parameters.AddWithValue("@CompanyId", CompanyId);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new EarlyWarningVolunteer(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public List<EarlyWarningVolunteer> EarlyWarning_Volunteers_searchVolunteers(int SearchIn, string Query, string UserId, string SortOrder, string SortAsc, string Competences, string SectorCodes,
            object Radius_Hovedstaden, object Radius_Midtjylland, object Radius_Syddanmark, object Radius_Sjaelland, object Radius_Nordjylland, bool? Active) {
            List<EarlyWarningVolunteer> retval = new List<EarlyWarningVolunteer>();
            sql.commandText = "EarlyWarning_Volunteers_searchVolunteers";
            sql.parameters.AddWithValue("@SearchIn", SearchIn);
            sql.parameters.AddWithValue("@Query", Query);
            sql.parameters.AddWithValue("@UserId", UserId);
            sql.parameters.AddWithValue("@sortOrder", SortOrder);
            sql.parameters.AddWithValue("@sortAsc", SortAsc);
            if (SectorCodes != "" && SectorCodes != null) sql.parameters.AddWithValue("@SectorCodes", SectorCodes);

            List<int> CompetencesSorted = new List<int>();
            foreach (string s in Competences.Split(',')) {
                if (s != "" && s != null)
                    CompetencesSorted.Add(TypeCast.ToInt(s));
            }

            CompetencesSorted.Sort();

            Competences = "";

            foreach (int i in CompetencesSorted) {
                Competences += i + ",";
            }

            if (Competences != "") sql.parameters.AddWithValue("@competences", "," + Competences);

            if (Radius_Hovedstaden != null) sql.parameters.AddWithValue("@Radius_Hovedstaden", (bool)Radius_Hovedstaden);
            if (Radius_Midtjylland != null) sql.parameters.AddWithValue("@Radius_Midtjylland", (bool)Radius_Midtjylland);
            if (Radius_Syddanmark != null) sql.parameters.AddWithValue("@Radius_Syddanmark", (bool)Radius_Syddanmark);
            if (Radius_Sjaelland != null) sql.parameters.AddWithValue("@Radius_Sjaelland", (bool)Radius_Sjaelland);
            if (Radius_Nordjylland != null) sql.parameters.AddWithValue("@Radius_Nordjylland", (bool)Radius_Nordjylland);

            if (Active != null)
                sql.parameters.AddWithValue("@Active", (bool)Active);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new EarlyWarningVolunteer(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public List<EarlyWarningVolunteer> EarlyWarning_Volunteers_getRecent(int organisationId, string userId) {
            List<EarlyWarningVolunteer> retval = new List<EarlyWarningVolunteer>();
            sql.commandText = "EarlyWarning_Volunteers_getRecent";
            sql.parameters.AddWithValue("@organisationId", organisationId);
            sql.parameters.AddWithValue("@userId", userId);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new EarlyWarningVolunteer(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public List<EarlyWarningCompanyAndContactDeleted> EarlyWarning_Companies_getDeleted(string SortOrder, string SortAsc) {
            List<EarlyWarningCompanyAndContactDeleted> retval = new List<EarlyWarningCompanyAndContactDeleted>();
            sql.commandText = "EarlyWarning_Companies_getDeleted";
            sql.parameters.AddWithValue("@sortOrder", SortOrder);
            sql.parameters.AddWithValue("@sortAsc", SortAsc);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new EarlyWarningCompanyAndContactDeleted(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public List<EarlyWarningVolunteerDeleted> EarlyWarning_Volunteers_GetDeleted(string SortOrder, string SortAsc) {
            List<EarlyWarningVolunteerDeleted> retval = new List<EarlyWarningVolunteerDeleted>();
            sql.commandText = "EarlyWarning_Volunteers_GetDeleted";
            sql.parameters.AddWithValue("@sortOrder", SortOrder);
            sql.parameters.AddWithValue("@sortAsc", SortAsc);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new EarlyWarningVolunteerDeleted(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public List<EarlyWarningFile> EarlyWarning_Volunteers_GetResumes(int Id) {
            List<EarlyWarningFile> retval = new List<EarlyWarningFile>();
            sql.commandText = "EarlyWarning_Volunteers_GetResumes";
            sql.parameters.AddWithValue("@Id", Id);


            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new EarlyWarningFile(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public List<EarlyWarningHistoryItem> EarlyWarning_GetCompanyHistory(int CompanyId) {
            List<EarlyWarningHistoryItem> retval = new List<EarlyWarningHistoryItem>();
            sql.commandText = "EarlyWarning_GetCompanyHistory";
            sql.parameters.AddWithValue("@CompanyId", CompanyId);


            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new EarlyWarningHistoryItem(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public List<EarlyWarningFile> EarlyWarning_Companies_GetFiles(int Id) {
            List<EarlyWarningFile> retval = new List<EarlyWarningFile>();
            sql.commandText = "EarlyWarning_Companies_GetFiles";
            sql.parameters.AddWithValue("@Id", Id);


            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new EarlyWarningFile(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public List<EarlyWarningUser> EarlyWarning_GetAvalableUsers(bool NoAdmins) {
            List<EarlyWarningUser> retval = new List<EarlyWarningUser>();
            sql.commandText = "EarlyWarning_GetAvalableUsers";
            sql.parameters.AddWithValue("@NoAdmins", NoAdmins);
            //sql.parameters.AddWithValue("@sortAsc", sortAsc);
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new EarlyWarningUser(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public int EarlyWarning_Companies_UpdateContact(EarlyWarningContact c, User U) {
            int retval = 0;
            sql.commandText = "EarlyWarning_Companies_UpdateContact";

            if (c.Id > 0) sql.parameters.AddWithValue("@Id", c.Id);
            sql.parameters.AddWithValue("@OrganisationId", U.OrganisationId);
            sql.parameters.AddWithValue("@CompanyId", c.CompanyId);
            sql.parameters.AddWithValue("@Firstname", c.Firstname);
            sql.parameters.AddWithValue("@Lastname", c.Lastname);
            sql.parameters.AddWithValue("@Phone", c.Phone);
            sql.parameters.AddWithValue("@MobilePhone", c.MobilePhone);
            sql.parameters.AddWithValue("@Email", c.Email);
            sql.parameters.AddWithValue("@JobPosition", c.JobPosition);
            sql.parameters.AddWithValue("@EWKnowledge", c.EWKnowledge);
            sql.parameters.AddWithValue("@CreatedBy", U.Id);
            sql.parameters.AddWithValue("@Sex", c.Sex);



            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = TypeCast.ToInt(dr["id"]);
            }

            dr.Close();
            sql.reset();

            return retval;

        }

        public int EarlyWarning_Companies_UpdateCompany(EarlyWarningCompany c, User U) {
            int retval = 0;
            //sql.commandText = "EarlyWarning_Companies_UpdateCompany";
            //** Kalder nu SPROC, ESCRM-136/137
            sql.commandText = "EarlyWarning_Companies_UpdateCompany_V2";

            if (c.Id > 0) sql.parameters.AddWithValue("@Id", c.Id);
            sql.parameters.AddWithValue("@OrganisationId", U.OrganisationId);
            sql.parameters.AddWithValue("@CVR", c.CVR);
            sql.parameters.AddWithValue("@CompanyName", c.CompanyName);
            sql.parameters.AddWithValue("@Address", c.Address);
            sql.parameters.AddWithValue("@Zipcode", c.Zipcode);
            sql.parameters.AddWithValue("@City", c.City);
            sql.parameters.AddWithValue("@County", c.County);
            sql.parameters.AddWithValue("@Phone", c.Phone);
            sql.parameters.AddWithValue("@Email", c.Email);
            
            //** Har tilføjet region, ESCRM-136/137
            sql.parameters.AddWithValue("@Region", c.Region);

            sql.parameters.AddWithValue("@Homepage", c.Homepage);
            sql.parameters.AddWithValue("@CompanyType", c.CompanyType);
            sql.parameters.AddWithValue("@EstablishmentYear", c.EstablishmentYear);
            sql.parameters.AddWithValue("@EmployeeCount", c.EmployeeCount);
            sql.parameters.AddWithValue("@SectorCodes", c.SectorCodes);
            sql.parameters.AddWithValue("@Status", c.Status);
            if (c.CurrentConsultant != "" && c.CurrentConsultant != null) sql.parameters.AddWithValue("@CurrentConsultant", c.CurrentConsultant);

            sql.parameters.AddWithValue("@Weaknesses_Management_LackOfCompetences", c.Weaknesses_Management_LackOfCompetences);
            sql.parameters.AddWithValue("@Weaknesses_Management_LackOfAdjustingToSales", c.Weaknesses_Management_LackOfAdjustingToSales);
            sql.parameters.AddWithValue("@Weaknesses_Management_PersonalProblems", c.Weaknesses_Management_PersonalProblems);
            sql.parameters.AddWithValue("@Weaknesses_Management_DisputesInOwnership", c.Weaknesses_Management_DisputesInOwnership);
            sql.parameters.AddWithValue("@Weaknesses_Management_Other", c.Weaknesses_Management_Other);
            sql.parameters.AddWithValue("@Weaknesses_Management_Other_Text", c.Weaknesses_Management_Other_Text);
            sql.parameters.AddWithValue("@Weaknesses_Marketing_LackOfStrategicFocus", c.Weaknesses_Marketing_LackOfStrategicFocus);
            sql.parameters.AddWithValue("@Weaknesses_Marketing_NoTrackingOfCustomers", c.Weaknesses_Marketing_NoTrackingOfCustomers);
            sql.parameters.AddWithValue("@Weaknesses_Marketing_TooDependentOfSingleCustomers", c.Weaknesses_Marketing_TooDependentOfSingleCustomers);
            sql.parameters.AddWithValue("@Weaknesses_Marketing_LackOfMonitoringMarket", c.Weaknesses_Marketing_LackOfMonitoringMarket);
            sql.parameters.AddWithValue("@Weaknesses_Marketing_LackOfSalesEffort", c.Weaknesses_Marketing_LackOfSalesEffort);
            sql.parameters.AddWithValue("@Weaknesses_Marketing_LackOfSearchingSales", c.Weaknesses_Marketing_LackOfSearchingSales);
            sql.parameters.AddWithValue("@Weaknesses_Marketing_Other", c.Weaknesses_Marketing_Other);
            sql.parameters.AddWithValue("@Weaknesses_Marketing_Other_Text", c.Weaknesses_Marketing_Other_Text);
            sql.parameters.AddWithValue("@Weaknesses_Production_LackOfOutsourcing", c.Weaknesses_Production_LackOfOutsourcing);
            sql.parameters.AddWithValue("@Weaknesses_Production_TroubleWithOutsourcing", c.Weaknesses_Production_TroubleWithOutsourcing);
            sql.parameters.AddWithValue("@Weaknesses_Production_Quality", c.Weaknesses_Production_Quality);
            sql.parameters.AddWithValue("@Weaknesses_Production_Planning", c.Weaknesses_Production_Planning);
            sql.parameters.AddWithValue("@Weaknesses_Production_StockManagementAndProduction", c.Weaknesses_Production_StockManagementAndProduction);
            sql.parameters.AddWithValue("@Weaknesses_Production_OldTechnology", c.Weaknesses_Production_OldTechnology);
            sql.parameters.AddWithValue("@Weaknesses_Production_Other", c.Weaknesses_Production_Other);
            sql.parameters.AddWithValue("@Weaknesses_Production_Other_Text", c.Weaknesses_Production_Other_Text);
            sql.parameters.AddWithValue("@Weaknesses_Finance_LackOfAccountingUnderstanding", c.Weaknesses_Finance_LackOfAccountingUnderstanding);
            sql.parameters.AddWithValue("@Weaknesses_Finance_ExpensiveFinancing", c.Weaknesses_Finance_ExpensiveFinancing);
            sql.parameters.AddWithValue("@Weaknesses_Finance_DebitorLoses", c.Weaknesses_Finance_DebitorLoses);
            sql.parameters.AddWithValue("@Weaknesses_Finance_AdministrativeMess", c.Weaknesses_Finance_AdministrativeMess);
            sql.parameters.AddWithValue("@Weaknesses_Finance_Other", c.Weaknesses_Finance_Other);
            sql.parameters.AddWithValue("@Weaknesses_Finance_Other_Text", c.Weaknesses_Finance_Other_Text);
            sql.parameters.AddWithValue("@Weaknesses_HasPositiveResult", c.Weaknesses_HasPositiveResult);

            sql.parameters.AddWithValue("@CreatedBy", U.Id);
            sql.parameters.AddWithValue("@NaceCode", c.NaceCode);
            sql.parameters.AddWithValue("@TimeUsage", c.TimeUsage);
            sql.parameters.AddWithValue("@DateStarted", c.DateStarted);
            sql.parameters.AddWithValue("@DateSentToVolunteer", c.DateSentToVolunteer);
            sql.parameters.AddWithValue("@DateEnded", c.DateEnded);
            sql.parameters.AddWithValue("@Recommendation", c.Recommendation);
            sql.parameters.AddWithValue("@CrisisCharacter", c.CrisisCharacter);
            sql.parameters.AddWithValue("@hasProfessionalBoard", c.hasProfessionalBoard);
            sql.parameters.AddWithValue("@Weakness_Management", c.Weakness_Management);
            sql.parameters.AddWithValue("@Weakness_Finance", c.Weakness_Finance);
            sql.parameters.AddWithValue("@Weakness_Production", c.Weakness_Production);
            sql.parameters.AddWithValue("@Weakness_Marketing", c.Weakness_Marketing);

            sql.parameters.AddWithValue("@CaseType", string.IsNullOrEmpty(c.CaseType) ? "Intet valgt" : c.CaseType);
            sql.parameters.AddWithValue("@mitErhvervshus", c.mitErhvervshus);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = TypeCast.ToInt(dr["id"]);
            }

            dr.Close();
            sql.reset();

            return retval;
        }

        #endregion

        #region EmailTemplates

        public List<EmailTemplate> EmailTemplates_GetAll(int OrganisationId) {
            List<EmailTemplate> retval = new List<EmailTemplate>();
            sql.commandText = "EmailTemplates_GetAll";
            sql.parameters.AddWithValue("@OrganisationId", OrganisationId);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new EmailTemplate(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        #endregion

        #region Categories

        public List<Category> categories_GetAll(User U, int type) {
            return categories_GetAll(U, 0, type);
        }

        public List<Category> categories_GetAll(User U, int FileId, int type) {
            List<Category> retval = new List<Category>();
            sql.commandText = "category_GetAll";
            sql.parameters.AddWithValue("@organisationId", U.OrganisationId);
            sql.parameters.AddWithValue("@type", type);
            if (FileId > 0) sql.parameters.AddWithValue("@FileId", FileId);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                retval.Add(new Category(ref dr));
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public Category categories_Get(User U, int Id) {
            Category retval = null;
            sql.commandText = "category_Get_v1";
            sql.parameters.AddWithValue("@categoryId", Id);
            sql.parameters.AddWithValue("@organisationId", U.OrganisationId);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = new Category(ref dr);
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        public int categories_Update(User U, Category n) {
            int retval = 0;
            sql.commandText = "category_Update";
            sql.parameters.AddWithValue("@categoryId", TypeCast.ToIntOrDBNull(n.Id));
            sql.parameters.AddWithValue("@userId", U.Id);
            sql.parameters.AddWithValue("@organisationId", U.OrganisationId);
            sql.parameters.AddWithValue("@categoryName", n.Name);
            sql.parameters.AddWithValue("@type", n.Type);
            sql.parameters.AddWithValue("@sortOrder", n.SortOrder);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = TypeCast.ToInt(dr["id"]);
            }
            dr.Close();
            sql.reset();

            Events_addToEventLog(U.OrganisationId, U.Id, n.Id == retval ? "UPDATE" : "CREATE", "CATEGORY", retval.ToString(), n);

            return retval;
        }

        public void categories_Delete(User U, int id) {
            sql.commandText = "category_Delete";
            sql.parameters.AddWithValue("@categoryId", id);
            sql.parameters.AddWithValue("@organisationId", U.OrganisationId);
            sql.execute();
            sql.reset();
        }

        #endregion

        #region CVR
        /// <summary>
        /// unprocessed cvr numbers
        /// </summary>
        /// <returns></returns>
        public List<CVR> CVRGetAll() {
            var cvr = new List<CVR>();
            sql.commandText = "CVR_UpdateRequest";
            using (var dr = sql.executeReader) {
                while (dr.Read()) {
                    cvr.Add(new CVR() {
                        Response = TypeCast.ToString(dr["Response"]),
                        SidstOpdateret = TypeCast.ToDateTime(dr["sidstOpdateret"]),
                        CVRNumber = TypeCast.ToInt(dr["CVR"]),
                        SammensatStatus = TypeCast.ToString(dr["sammensatStatus"]),
                        StiftelsesDato = TypeCast.ToNullableDateTime(dr["stiftelsesDato"]) == null ? null : TypeCast.ToDateTime(dr["stiftelsesDato"]).ToString("yyyy-MM-dd"),
                        VirkningsDato = TypeCast.ToNullableDateTime(dr["virkningsDato"]) == null ? null : TypeCast.ToDateTime(dr["virkningsDato"]).ToString("yyyy-MM-dd"),
                        KommuneKode = TypeCast.ToIntOrNull(dr["kommuneKode"]) == null ? null : TypeCast.ToIntLoose(dr["kommuneKode"]),
                        KommuneNavn = TypeCast.ToString(dr["kommuneNavn"]),
                        Reklamebeskyttet = TypeCast.ToBoolLoose(dr["reklamebeskyttet"]),
                        BrancheTekst = TypeCast.ToString(dr["brancheTekst"]),
                        BrancheKode = TypeCast.ToString(dr["brancheKode"]),
                        TelefaxNummer = TypeCast.ToString(dr["telefaxNummer"]),
                        TelefonNummer = TypeCast.ToString(dr["telefonNummer"]),
                        VirksomhedsformLang = TypeCast.ToString(dr["virksomhedsformLang"]),
                        VirksomhedsformKort = TypeCast.ToString(dr["virksomhedsformKort"]),
                        ElektroniskPost = TypeCast.ToString(dr["elektroniskPost"]),
                        Binavne = TypeCast.ToString(dr["binavne"]),
                        HjemmeSide = TypeCast.ToString(dr["hjemmeSide"]),
                    });
                }
            }
            sql.reset();
            return cvr;
        }

        /// <summary>
        /// adding and updating of cvr data
        /// </summary>
        /// <param name="CVR">cvr id</param>
        /// <param name="Response">json response</param>
        /// <param name="StartDate">start dato</param>
        /// <param name="Status">sammensat status</param>
        /// <returns></returns>
        public void CVR_AddEdit(int CVR, string Response, string virkningsDato,
                    string stiftelsesDato, string sammensatStatus, int? kommuneKode, string kommuneNavn, bool? reklamebeskyttet, string sidstOpdateret,
                    string brancheTekst, string brancheKode, string telefaxNummer, string telefonNummer, string virksomhedsformLang, string virksomhedsformKort,
                    string elektroniskPost, string binavne, string hjemmeside,
                    string pNummer) {

            //** Clear all parameters
            sql.parameters.Clear();

            var sqlmindate = new DateTime(1753, 1, 1);
            sql.commandText = "CVR_AddEdit";
            sql.parameters.AddWithValue("@CVR", TypeCast.ToIntOrDBNull(CVR));
            sql.parameters.AddWithValue("@Response", Response);
            if (!string.IsNullOrEmpty(virkningsDato)) {
                var virking = DateTime.ParseExact(virkningsDato, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
                if (virking < sqlmindate) { virkningsDato = string.Empty; }
                if (!string.IsNullOrEmpty(virkningsDato)) { sql.parameters.AddWithValue("@virkningsDato", DateTime.ParseExact(virkningsDato, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture)); }
            }
            if (!string.IsNullOrEmpty(stiftelsesDato)) {
                var stiftelses = DateTime.ParseExact(stiftelsesDato, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
                if (stiftelses < sqlmindate) { stiftelsesDato = string.Empty; }
                if (!string.IsNullOrEmpty(stiftelsesDato)) { sql.parameters.AddWithValue("@stiftelsesDato", DateTime.ParseExact(stiftelsesDato, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture)); }
            }
            if (!string.IsNullOrEmpty(sidstOpdateret)) {
                sidstOpdateret = sidstOpdateret.Replace("T", " ");
                var sidst = DateTime.ParseExact(sidstOpdateret.Substring(0, 19), "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                if (sidst < sqlmindate) { sidstOpdateret = string.Empty; }
                if (!string.IsNullOrEmpty(sidstOpdateret)) { sql.parameters.AddWithValue("@sidstOpdateret", DateTime.ParseExact(sidstOpdateret.Substring(0, 19), "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture)); }
            }
            if (!string.IsNullOrEmpty(sammensatStatus)) { sql.parameters.AddWithValue("@sammensatStatus", sammensatStatus); }
            if (!string.IsNullOrEmpty(kommuneNavn)) { sql.parameters.AddWithValue("@kommuneNavn", kommuneNavn); }
            sql.parameters.AddWithValue("@kommuneKode", kommuneKode);
            sql.parameters.AddWithValue("@reklamebeskyttet", reklamebeskyttet);

            if (!string.IsNullOrEmpty(brancheTekst)) { sql.parameters.AddWithValue("@brancheTekst", brancheTekst); }
            if (!string.IsNullOrEmpty(brancheKode)) { sql.parameters.AddWithValue("@brancheKode", brancheKode); }
            if (!string.IsNullOrEmpty(telefaxNummer)) { sql.parameters.AddWithValue("@telefaxNummer", telefaxNummer); }
            if (!string.IsNullOrEmpty(telefonNummer)) { sql.parameters.AddWithValue("@telefonNummer", telefonNummer); }
            if (!string.IsNullOrEmpty(virksomhedsformLang)) { sql.parameters.AddWithValue("@virksomhedsformLang", virksomhedsformLang); }
            if (!string.IsNullOrEmpty(virksomhedsformKort)) { sql.parameters.AddWithValue("@virksomhedsformKort", virksomhedsformKort); }
            if (!string.IsNullOrEmpty(elektroniskPost)) { sql.parameters.AddWithValue("@elektroniskPost", elektroniskPost); }
            if (!string.IsNullOrEmpty(binavne)) { sql.parameters.AddWithValue("@binavne", binavne); }
            if (!string.IsNullOrEmpty(hjemmeside)) { sql.parameters.AddWithValue("@hjemmeSide", hjemmeside); }

            if (!string.IsNullOrEmpty(pNummer)) { sql.parameters.AddWithValue("@pNummer", pNummer); }

            sql.execute();
            sql.reset();
            //Events_addToEventLog("UPDATE", "RUN_CVR", retval.ToString(), n);
        }

        /// <summary>
        /// CVR information update for company organisation 1
        /// </summary>
        public void CVR_UpdateCompany() {
            sql.commandText = "CVR_UpdateCompany";
            sql.execute();
            sql.reset();
        }

        /// <summary>
        /// CVR Updated newer
        /// </summary>
        /// <param name="cvr"></param>
        /// <param name="UpdateToDate"></param>
        /// <returns></returns>
        public bool CVR_Updated_Newer(string CVR, string UpdateToDate)
        {
            bool retval = false;

            sql.commandText = "CVR_Updated_Newer";
            sql.parameters.AddWithValue("@CVR", CVR);
            sql.parameters.AddWithValue("@UpdateToDate", UpdateToDate);
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read())
            {
                retval = TypeCast.ToBool(dr["Newer"]);
            }
            dr.Close();
            sql.reset();

            return retval;
        }
        #endregion

        #region PNR
        /// <summary>
        /// Function : Compare if CVR and PNR is in PNR table, ESCRM-190/191
        /// </summary>
        /// <param name="CVR"></param>
        /// <param name="PNR"></param>
        /// <returns></returns>
        public bool CompareCVRinPNR(string CVR, string PNR)
        {
            bool result = false;

            sql.commandText = "CompareCVRinPNR";
            sql.parameters.AddWithValue("@CVR", CVR);
            sql.parameters.AddWithValue("@PNR", PNR);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read())
            {
                int antal = TypeCast.ToInt(dr[0]);
                result = (antal > 0) ? true : false;
            }
            dr.Close();
            sql.reset();

            return result;
        }
        #endregion

        #region Favorite, ESCRM-13/97
        /// <summary>
        /// Function : Kalde stores procedure Company_changeFavorite
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="CVR"></param>
        /// <param name="Pnummer"></param>
        public string ChangeFavoriteForCompany(Guid userID, string companyId, string contactId)
        {
            string result = string.Empty;

            try
            {
                //** Sæt parametre op til sproc kald
                sql.commandText = "Company_changeFavorite";
                sql.parameters.AddWithValue("@userId", userID);
                sql.parameters.AddWithValue("@companyId", companyId);
                sql.parameters.AddWithValue("@contactId", contactId);

                //** Definer retur value
                string retval = "";

                //** Kald sql data reader
                System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
                if (dr.Read())
                {
                    //** Sæt retur value
                    retval = TypeCast.ToString(dr["Status"]);
                }

                //** Luk data reader
                dr.Close();

                //** Sæt svaret
                result = retval;
            }
            catch (Exception ex)
            {
                result = null;
            }

            return result;
        }

        /// <summary>
        /// Function : Kalde stored procedure GetCompanyFavorites, ESCRM-13/97
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public Boolean GetCompanyFavorite(string userId, string companyId)
        {
            Boolean result = false;

            try
            {
                sql.commandText = "Company_isFavorite";
                sql.parameters.AddWithValue("@userId", userId);
                sql.parameters.AddWithValue("@companyId", companyId);
                sql.parameters.AddWithValue("@contactId", string.Empty);

                System.Data.SqlClient.SqlDataReader dr = sql.executeReader;

                if (dr.Read())
                {
                    //** Sæt retur value
                    var retval = TypeCast.ToString(dr["Counter"]);
                    if (retval == "0")
                    {
                        result = false;
                    }
                    else
                    {
                        result = true;
                    }
                }

                //** Luk data reader
                dr.Close();

                //** Clear sql parameters
                sql.parameters.Clear();
            }
            catch (Exception ex)
            {
                string error = ex.Message;
            }

            return result;
        }

        /// <summary>
        /// Function : Kalde stored procedure GetCompanyFavorites, ESCRM-13/97
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="contactId"></param>
        /// <returns></returns>
        public Boolean GetCompanyFavoriteViaContact(string userId, string contactId)
        {
            Boolean result = false;

            try
            {
                sql.commandText = "Company_isFavorite";
                sql.parameters.AddWithValue("@userId", userId);
                sql.parameters.AddWithValue("@companyId", string.Empty);
                sql.parameters.AddWithValue("@contactId", contactId);

                System.Data.SqlClient.SqlDataReader dr = sql.executeReader;

                if (dr.Read())
                {
                    //** Sæt retur value
                    var retval = TypeCast.ToString(dr["Counter"]);
                    if (retval == "0")
                    {
                        result = false;
                    }
                    else
                    {
                        result = true;
                    }
                }

                //** Luk data reader
                dr.Close();

                //** Clear sql parameters
                sql.parameters.Clear();
            }
            catch (Exception ex)
            {
                string error = ex.Message;
            }

            return result;
        }
        #endregion

        #region Tilskudsinformation, ESCRM-98/99
        /// <summary>
        /// Add tilskudsinformation
        /// </summary>
        /// <param name="tilskudsinformation"></param>
        /// <returns></returns>
        public Boolean AddTilskudsinformation(Tilskudsinformation tilskudsinformation)
        {
            Boolean result = true;

            try
            {
                //** Opsæt store procedure og parametre
                sql.commandText = "Tilskudsinformation_add";
                sql.parameters.AddWithValue("@CVR", tilskudsinformation.CVR);
                sql.parameters.AddWithValue("@Kontakt", tilskudsinformation.Kontakt);
                sql.parameters.AddWithValue("@Program", tilskudsinformation.Program);
                sql.parameters.AddWithValue("@BevillingsDato", tilskudsinformation.BevillingsDato);
                //** Bevillingsnavn omdøbt, ESCRM176/177
                //sql.parameters.AddWithValue("@BevillingsNavn", tilskudsinformation.BevillingsNavn);
                sql.parameters.AddWithValue("@Titel", tilskudsinformation.Titel);

                sql.parameters.AddWithValue("@Form", tilskudsinformation.Form);
                sql.parameters.AddWithValue("@Tilskudsbeloeb", tilskudsinformation.TilskudsBeloeb);
                //**Emne omdøbt, ESCRM-176/177
                //sql.parameters.AddWithValue("@Emne", tilskudsinformation.Emne);
                sql.parameters.AddWithValue("@Overordnet_indsats", tilskudsinformation.Overordnet_indsats);

                sql.parameters.AddWithValue("@Status", tilskudsinformation.Status);
                sql.parameters.AddWithValue("@CreatedBy", tilskudsinformation.CreatedBy);
                sql.parameters.AddWithValue("@Ansvarlig", tilskudsinformation.Ansvarlig);
                //** SagsNummer omdøbt, ESCRM-176/177
                //sql.parameters.AddWithValue("@SagsNummer", tilskudsinformation.SagsNummer);
                sql.parameters.AddWithValue("@JournalNummer", tilskudsinformation.JournalNummer);

                sql.parameters.AddWithValue("@Beskrivelse", tilskudsinformation.Beskrivelse);
                sql.parameters.AddWithValue("@SystemAdgang", tilskudsinformation.SystemAdgang);
                //** Tilføjet for ESCRM-98/108
                sql.parameters.AddWithValue("@AfslutningsDato", tilskudsinformation.AfslutningsDato);
                sql.parameters.AddWithValue("@Informations_Hjemmeside", tilskudsinformation.Informations_Hjemmeside);

                //** Nyt felt, ESCRM-176/177
                sql.parameters.AddWithValue("@PNummer", tilskudsinformation.PNummer);
                //** Nye felter økonomi, ESCRM-176/177
                sql.parameters.AddWithValue("@Oko_AnsoegtBeloeb", tilskudsinformation.Oko_AnsoegtBeloeb);
                sql.parameters.AddWithValue("@Oko_TilsagnsBeloeb", tilskudsinformation.Oko_TilsagnsBeloeb);
                sql.parameters.AddWithValue("@Oko_EgenFinansiering", tilskudsinformation.Oko_EgenFinansiering);
                sql.parameters.AddWithValue("@Oko_Formaal", tilskudsinformation.Oko_Formaal);
                sql.parameters.AddWithValue("@Oko_Projektkonsulent", tilskudsinformation.Oko_Projektkonsulent);
                //** Nye felter ekstern rådgiver, ESCRM-176/177
                sql.parameters.AddWithValue("@Raadgiver_Firmanavn", tilskudsinformation.Raadgiver_Firmanavn);
                sql.parameters.AddWithValue("@Raadgiver_CVR", tilskudsinformation.Raadgiver_CVR);

                //** Eksekver stored procedure
                sql.execute();
                sql.reset();
            }
            catch (Exception ex)
            {
                result = false;
            }

            return result;
        }

        /// <summary>
        /// Tilskudsinformation double check, BRUGES IKKE LÆNGERE
        /// </summary>
        /// <param name="tilskudsinformation"></param>
        /// <returns></returns>
        public Boolean TilskudsinformationDubletCheck(Tilskudsinformation tilskudsinformation)
        {
            Boolean result = false;

            //** Opsæt store procedure og parametre
            sql.commandText = "Tilskudsinformation_check";
            sql.parameters.AddWithValue("@CVR", tilskudsinformation.CVR);
            sql.parameters.AddWithValue("@Program", tilskudsinformation.Program);
            sql.parameters.AddWithValue("@BevillingsDato", tilskudsinformation.BevillingsDato);
            //** BevillingsNavn omdøbt, ESCRM-176/177
            //sql.parameters.AddWithValue("@BevillingsNavn", tilskudsinformation.BevillingsNavn);
            sql.parameters.AddWithValue("@Titel", tilskudsinformation.Titel);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read())
            {
                //** Hen hvor mange der findes i forvejen
                int antal = int.Parse(dr["Antal"].ToString());

                //** Check om den fandt nogen
                if (antal >= 1)
                    result = true;
            }
            dr.Close();
            sql.reset();

            return result;
        }

        /// <summary>
        /// Can import tilskud information
        /// </summary>
        /// <param name="ProgramNavn"></param>
        /// <param name="ProgramGuid"></param>
        /// <returns></returns>
        public bool CanImportTilskudsinformation(string ProgramNavn, Guid ProgramGuid)
        {
            bool result = false;

            try
            {
                //** Opsæt store procedure og parametre
                sql.commandText = "TilskudsinformationImport_check";
                sql.parameters.AddWithValue("@ProgramNavn", ProgramNavn);
                sql.parameters.AddWithValue("@ProgramGuid", ProgramGuid);
                SqlDataReader dr = sql.executeReader;
                while (dr.Read())
                {
                    //** Hent hvor mange der findes i forvejen
                    int antal = int.Parse(dr["Antal"].ToString());

                    //** Check om den fandt nogen
                    if (antal >= 1)
                        result = true;
                }

                dr.Close();
                sql.reset();
            }
            catch (Exception ex)
            {
                sql.reset();
            }

            return result;
        }
        #endregion  

        #region Company Evaluation
        /// <summary>
        /// Function : Kalde stores procedure CanEvaluationsButtonsBeActive, ESCRM-111/112
        /// </summary>
        /// <param name="CVR"></param>
        /// <param name="Pnummer"></param>
        public string Company_allowEvaluation(string companyId, string contactId)
        {
            string result = string.Empty;
            SqlDataReader dr = null;

            try
            {
                //** Sæt parametre op til sproc kald
                sql.commandText = "Company_allowEvaluation";
                //sql.parameters.AddWithValue("@companyId", companyId);
                sql.parameters.AddWithValue("@contactId", contactId);

                //** Definer retur value
                string retval = "";

                //** Kald sql data reader
                dr = sql.executeReader;
                if (dr.Read())
                {
                    //** Sæt retur value
                    retval = dr[0].ToString();
                }

                //** Luk data reader
                dr.Close();

                //** Sæt svaret
                result = retval;
            }
            catch (Exception ex)
            {
                //** Luk data reader
                //dr.Close();

                result = null;
            }

            return result;
        }
        #endregion

        /// <summary>Company Company_Get
        /// Creating a log xml
        /// </summary>
        /// <returns></returns>
        public void AddLog(int logid, User u, string fieldtype = "", string fieldid = "", string fieldname = "") {
            if (u != null) {
                var log = new StringBuilder();
                log.Append("<Log>");
                log.Append("    <LogLevel>{LogLevel}</LogLevel>");
                log.Append("    <LogType>{LogType}</LogType>");
                log.Append("    <Username>{Username}</Username>");
                log.Append("    <UserId>{UserId}</UserId>");
                log.Append("    <OrganisationId>{OrganisationId}</OrganisationId>");
                log.Append("    <IPaddress>{IPaddress}</IPaddress>");
                log.Append("    <Title>{Title}</Title>");
                log.Append("    <Note>{Note}</Note>");
                log.Append("    <FieldType>{FieldType}</FieldType>");
                log.Append("    <FieldId>{FieldId}</FieldId>");
                log.Append("    <FieldName>{FieldName}</FieldName>");
                log.Append("    <ExpiryDate>{ExpiryDate}</ExpiryDate>");
                log.Append("    <DataObjects>{DataObjects}</DataObjects>");
                log.Append("    <LogDate>{LogDate}</LogDate>");
                log.Append("    <ExpiryDate>{ExpiryDate}</ExpiryDate>");
                log.Append("</Log>");

                var context = System.Web.HttpContext.Current;
                var ip = context.Request.UserHostAddress;
                var url = context.Request.Url.AbsolutePath.Replace(@"\", "").Replace(@"/", "");
                var logType = u.LogTypes.FirstOrDefault(w => w.Id == logid);

                if (logType != null) {
                    log.Replace("{LogLevel}", logType.LogLevel.ToString());
                    log.Replace("{LogType}", logType.Name);
                    log.Replace("{Title}", logType.Title.ToString());
                    log.Replace("{Note}", "Request raised from page: " + url);
                }

                log.Replace("{FieldType}", fieldtype);
                log.Replace("{FieldId}", fieldid);
                log.Replace("{FieldName}", fieldname);

                log.Replace("{Username}", u.Username);
                log.Replace("{UserId}", u.Id);
                log.Replace("{OrganisationId}", u.OrganisationId.ToString());
                log.Replace("{IPaddress}", ip);

                sql.commandText = "AddLog";
                sql.parameters.AddWithValue("@LogId", logid);
                sql.parameters.AddWithValue("@LogXml", log.ToString());
                sql.execute();
                sql.reset();
            }
        }

        /// <summary>Add to RunCVR log, ESCRM-196/200
        /// </summary>
        /// <returns></returns>
        public void Add_RunCVR_Log(string TypeName, string Titel, string DataObjects)
        {
            try
            {
                string[] lines = DataObjects.Split(new[] { '\r', '\n' });

                StringBuilder sb = new StringBuilder();
                sb.Append("<Objects>");
                sb.Append("<Object>");
                foreach (string line in lines)
                {
                    if (line != string.Empty)
                        sb.Append("<line>" + line.Replace("<h2>", "").Replace("</h2>", "").Replace("<p>", "").Replace("</p>", "").Replace("<b style='color:red;'>", "").Replace("</b>", "").Replace("<h3>", "").Replace("</h3>", "").Replace("&", "") + "</line>");
                }
                sb.Append("</Object>");
                sb.Append("</Objects>");

                //XmlDocument doc = new XmlDocument();
                //doc.LoadXml(sb.ToString());

                sql.commandText = "Add_RunCVR_Log";
                sql.parameters.AddWithValue("@TypeName", TypeName);
                sql.parameters.AddWithValue("@Titel", Titel);
                sql.parameters.AddWithValue("@DataObjects", sb.ToString());
                sql.execute();
                sql.reset();
            }
            catch (Exception ex)
            {
                string error = ex.Message;
            }
        }

        /// <summary>
        /// Get log types
        /// </summary>
        /// <returns></returns>
        public List<LogType> GetLogType() {
            var record = new List<LogType>();
            sql.commandText = "LogType_Get";
            using (SqlDataReader dr = sql.executeReader) {
                if (dr.HasRows) {
                    while (dr.Read()) {
                        record.Add(new LogType() {
                            Id = TypeCast.ToInt(dr["Id"]),
                            Name = TypeCast.ToString(dr["Name"]),
                            ExpiryDatePart = TypeCast.ToString(dr["ExpiryDatePart"]),
                            ExpiryValue = TypeCast.ToInt(dr["ExpiryValue"]),
                            LogLevel = TypeCast.ToInt(dr["LogLevel"]),
                            PageName = TypeCast.ToString(dr["PageName"]),
                            Title = TypeCast.ToString(dr["Title"])
                        });
                    }
                }
                if (!dr.IsClosed) { dr.Close(); dr.Dispose(); }
            }
            sql.reset();
            return record;
        }

        /// <summary>
        /// Searching companies - API request
        /// </summary>
        /// <returns></returns>
        public DataTable APICompanySearch(int organisationId, string userId, string query) {
            var sqlConnection = new SqlConnection(connectionString);
            using (var sqladp = new SqlDataAdapter("API_CompanySearch", sqlConnection)) {
                sqladp.SelectCommand.Parameters.AddWithValue("@OrganisationId", organisationId);
                sqladp.SelectCommand.Parameters.AddWithValue("@UserId", userId);
                if (!string.IsNullOrEmpty(query.Trim())) { sqladp.SelectCommand.Parameters.AddWithValue("@Query", query.Trim()); }
                sqladp.SelectCommand.CommandType = CommandType.StoredProcedure;
                if (sqlConnection.State == ConnectionState.Closed) { sqlConnection.Open(); }
                var ds = new DataSet();
                sqladp.Fill(ds);
                if (ds.Tables[0] != null) {
                    return ds.Tables[0];
                }
            }
            return null;
        }

        /// <summary>
        /// Searching MEETING for companies - API request
        /// </summary>
        /// <returns></returns>
        public DataTable APIGetMeeting(int organisationId, string companyIds) {
            var sqlConnection = new SqlConnection(connectionString);
            using (var sqladp = new SqlDataAdapter("API_GetMeetings", sqlConnection)) {
                sqladp.SelectCommand.Parameters.AddWithValue("@OrganisationId", organisationId);
                sqladp.SelectCommand.Parameters.AddWithValue("@CompanyIds", companyIds);
                sqladp.SelectCommand.CommandType = CommandType.StoredProcedure;
                if (sqlConnection.State == ConnectionState.Closed) { sqlConnection.Open(); }
                var ds = new DataSet();
                sqladp.Fill(ds);
                if (ds.Tables[0] != null) {
                    return ds.Tables[0];
                }
            }
            return null;
        }

        /// <summary>
        /// Searching AVN for Company or Contact - API request
        /// </summary>
        /// <returns></returns>
        public List<DataTable> APIGetAVNs(bool isCompany, string userId, int id, string xml) {
            var result = new List<DataTable>();
            var sqlConnection = new SqlConnection(connectionString);
            using (var sqladp = new SqlDataAdapter("API_AVN_Get", sqlConnection)) {
                sqladp.SelectCommand.Parameters.AddWithValue("@UserId", userId);
                if (isCompany) { sqladp.SelectCommand.Parameters.AddWithValue("@SMVCompanyId", id); } else { sqladp.SelectCommand.Parameters.AddWithValue("@SMVContactId", id); }
                if (!string.IsNullOrEmpty(xml)) { sqladp.SelectCommand.Parameters.AddWithValue("@AVN", xml); }
                sqladp.SelectCommand.CommandType = CommandType.StoredProcedure;
                if (sqlConnection.State == ConnectionState.Closed) { sqlConnection.Open(); }
                var ds = new DataSet();
                sqladp.Fill(ds);
                if (ds.Tables[0] != null) {
                    foreach (DataRow row in ds.Tables[0].Rows) {
                        var avnTypeId = Convert.ToString(row["AvnId"]);
                        var entityId = Convert.ToInt32(row["EntityId"]);
                        var avnDetail = APIGetAVNDetail(avnTypeId, entityId);
                        if (avnDetail != null) {
                            result.Add(avnDetail);
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Get AVN Detail for specific AVNId - API request
        /// </summary>
        /// <returns></returns>
        public DataTable APIGetAVNDetail(string avnId, int entityId) {
            var sqlConnection = new SqlConnection(connectionString);
            using (var sqladp = new SqlDataAdapter("z_avn_GetAVN_" + avnId, sqlConnection)) {
                sqladp.SelectCommand.Parameters.AddWithValue("@Id", entityId);
                sqladp.SelectCommand.CommandType = CommandType.StoredProcedure;
                if (sqlConnection.State == ConnectionState.Closed) { sqlConnection.Open(); }
                var ds = new DataSet();
                sqladp.Fill(ds);
                if (ds.Tables[0] != null) {

                    ds.Tables[0].Columns.Add("AVNTypeId", typeof(System.String));
                    ds.Tables[0].Columns.Add("EntityId", typeof(System.String));

                    foreach (DataRow row in ds.Tables[0].Rows) {
                        row["AVNTypeId"] = avnId;
                        row["EntityId"] = Convert.ToString(entityId);
                    }

                    return ds.Tables[0];
                }
            }
            return null;
        }

        /// <summary>
        /// Get File bytes - API request
        /// </summary>
        /// <returns></returns>
        public DataTable APIGetFile(User user, int fileId, bool obfuscate) {
            var sqlConnection = new SqlConnection(connectionString);
            using (var sqladp = new SqlDataAdapter("API_GetFile", sqlConnection)) {
                sqladp.SelectCommand.Parameters.AddWithValue("@UserId", user.Id);
                sqladp.SelectCommand.Parameters.AddWithValue("@OrganisationId", user.OrganisationId);
                sqladp.SelectCommand.Parameters.AddWithValue("@FileId", fileId);
                sqladp.SelectCommand.Parameters.AddWithValue("@Obfuscate", obfuscate);
                if (System.Web.HttpContext.Current != null && System.Web.HttpContext.Current.Request != null) {
                    var ip = System.Web.HttpContext.Current.Request.UserHostAddress;
                    sqladp.SelectCommand.Parameters.AddWithValue("@IPaddress", ip);
                }
                sqladp.SelectCommand.CommandType = CommandType.StoredProcedure;
                if (sqlConnection.State == ConnectionState.Closed) { sqlConnection.Open(); }
                var ds = new DataSet();
                sqladp.Fill(ds);
                if (ds.Tables[0] != null) {
                    return ds.Tables[0];
                }
            }
            return null;
        }

        /// <summary>
        /// Notes for Company or Contact - API request
        /// </summary>
        /// <returns></returns>
        public DataTable APIGetNote(bool isCompany, int organisationId, int value) {
            var sqlConnection = new SqlConnection(connectionString);
            using (var sqladp = new SqlDataAdapter("API_GetNotes", sqlConnection)) {
                sqladp.SelectCommand.Parameters.AddWithValue("@OrganisationId", organisationId);
                if (isCompany) { sqladp.SelectCommand.Parameters.AddWithValue("@CompanyId", value); } else { sqladp.SelectCommand.Parameters.AddWithValue("@ContactId", value); }
                sqladp.SelectCommand.CommandType = CommandType.StoredProcedure;
                if (sqlConnection.State == ConnectionState.Closed) { sqlConnection.Open(); }
                var ds = new DataSet();
                sqladp.Fill(ds);
                if (ds.Tables[0] != null) {
                    return ds.Tables[0];
                }
            }
            return null;
        }

        /// <summary>
        /// Document for Contact - API request
        /// </summary>
        /// <returns></returns>
        public DataTable APIGetDocument(int organisationId, int contactId) {
            var sqlConnection = new SqlConnection(connectionString);
            using (var sqladp = new SqlDataAdapter("API_GetDocuments", sqlConnection)) {
                sqladp.SelectCommand.Parameters.AddWithValue("@OrganisationId", organisationId);
                sqladp.SelectCommand.Parameters.AddWithValue("@ContactId", contactId);
                sqladp.SelectCommand.CommandType = CommandType.StoredProcedure;
                if (sqlConnection.State == ConnectionState.Closed) { sqlConnection.Open(); }
                var ds = new DataSet();
                sqladp.Fill(ds);
                if (ds.Tables[0] != null) {
                    return ds.Tables[0];
                }
            }
            return null;
        }

        /// <summary>
        /// Document for Contact - API request
        /// </summary>
        /// <returns></returns>
        public bool APIShareFile(int fileId, int contactId, string organisationIds) {
            sql.commandText = "API_ShareFile";
            sql.parameters.AddWithValue("@FileId", fileId);
            sql.parameters.AddWithValue("@ContactId", contactId);
            sql.parameters.AddWithValue("@OrganisationIds", organisationIds);
            sql.execute();
            sql.reset();
            return true;
        }

        /// <summary>
        /// Get Company Id for contact - API request
        /// </summary>
        /// <param name="contactId"></param>
        /// <returns></returns>
        public DataTable APIGetCompanyIdForContact(int contactId) {
            var sqlConnection = new SqlConnection(connectionString);
            using (var sqladp = new SqlDataAdapter("API_GetCompanyIdForContact", sqlConnection)) {
                sqladp.SelectCommand.Parameters.AddWithValue("@ContactId", contactId);
                sqladp.SelectCommand.CommandType = CommandType.StoredProcedure;
                if (sqlConnection.State == ConnectionState.Closed) { sqlConnection.Open(); }
                var ds = new DataSet();
                sqladp.Fill(ds);
                if (ds.Tables[0] != null) {
                    return ds.Tables[0];
                }
            }
            return null;
        }

        /// <summary>
        /// Get CVR setting - RUN CVR
        /// </summary>
        /// <returns></returns>
        public DataTable GetCVRSetting() {
            var sqlConnection = new SqlConnection(connectionString);
            using (var sqladp = new SqlDataAdapter("CVR_Setting_Get", sqlConnection)) {
                sqladp.SelectCommand.CommandType = CommandType.StoredProcedure;
                if (sqlConnection.State == ConnectionState.Closed) { sqlConnection.Open(); }
                var ds = new DataSet();
                sqladp.Fill(ds);
                if (ds.Tables[0] != null) {
                    return ds.Tables[0];
                }
            }
            return null;
        }

        /// <summary>
        /// Set CVR setting - RUN CVR
        /// </summary>
        /// <param name="updatedTo"></param>
        /// <returns></returns>
        public bool SetCVRSetting(DateTime updatedTo) {
            try {
                var sqlConnection = new SqlConnection(connectionString);
                using (var cmd = new SqlCommand("CVR_Setting_Set", sqlConnection)) {
                    cmd.Parameters.AddWithValue("@UpdateToDate", updatedTo.AddDays(1));
                    cmd.CommandType = CommandType.StoredProcedure;
                    if (sqlConnection.State == ConnectionState.Closed) { sqlConnection.Open(); }
                    cmd.ExecuteNonQuery();
                }
                return true;
            } catch (Exception) {
                return false;
            }
        }
        
        /// <summary>
        /// Get tilskudsinformation data, ESCRM-9/41
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public DataTable GetTilskudsinformation(int Id)
        {
            var sqlConnection = new SqlConnection(connectionString);
            using (var sqladp = new SqlDataAdapter("Tilskudsinformation_Get", sqlConnection))
            {
                sqladp.SelectCommand.Parameters.AddWithValue("@Id", Id);
                sqladp.SelectCommand.CommandType = CommandType.StoredProcedure;
                if (sqlConnection.State == ConnectionState.Closed) { sqlConnection.Open(); }
                var ds = new DataSet();
                sqladp.Fill(ds);
                if (ds.Tables[0] != null)
                {
                    return ds.Tables[0];
                }
            }
            return null;
        }

        /// <summary>
        /// Get Contact to evaluation
        /// </summary>
        /// <returns></returns>
        public ContactToEvaluation GetAdminContactsToEvaluation(int id) {
            var data = new ContactToEvaluation();
            var sqlConnection = new SqlConnection(connectionString);
            using (var sqladp = new SqlDataAdapter("AdminContactsToEvaluation_Get", sqlConnection)) {
                sqladp.SelectCommand.Parameters.AddWithValue("@Id", id);
                sqladp.SelectCommand.CommandType = CommandType.StoredProcedure;
                if (sqlConnection.State == ConnectionState.Closed) { sqlConnection.Open(); }
                var ds = new DataSet();
                sqladp.Fill(ds);

                if (ds.Tables[0] != null) {
                    if (ds.Tables[0].Rows.Count > 0) {
                        foreach (DataRow row in ds.Tables[0].Rows) {
                            data.Id = Convert.ToInt32(row["id"]);
                            data.OrgId = Convert.ToInt32(row["organisationId"]);
                            data.ContactId = Convert.ToInt32(row["contactId"]);
                            data.CompanyId = Convert.ToInt32(row["companyId"]);
                            data.UserId = Convert.ToString(row["userId"]);
                            data.Datestamp = Convert.ToDateTime(row["dateStamp"]).ToString("dd-MM-yyyy HH:mm");

                            if (!(row["exported"] is DBNull)) {
                                data.Exported = Convert.ToDateTime(row["exported"]).ToString("dd-MM-yyyy HH:mm");
                            }
                            data.Type = Convert.ToString(row["type"]);
                        }
                    }
                }

                if (ds.Tables[1] != null) {
                    if (ds.Tables[1].Rows.Count > 0) {
                        foreach (DataRow row in ds.Tables[1].Rows) {
                            var arcdata = new ContactToEvaluationArc() {
                                Id = Convert.ToInt32(row["id"]),
                                IdOrgId = Convert.ToInt32(row["idOrg"]),
                                OrgId = Convert.ToInt32(row["organisationId"]),
                                ContactId = Convert.ToInt32(row["contactId"]),
                                CompanyId = Convert.ToInt32(row["companyId"]),
                                UserId = Convert.ToString(row["userId"]),
                                Datestamp = Convert.ToDateTime(row["dateStamp"]).ToString("dd-MM-yyyy HH:mm"),
                                Exported = Convert.ToDateTime(row["exported"]).ToString("dd-MM-yyyy HH:mm"),
                                Reason = Convert.ToString(row["Reason"]),
                                Type = Convert.ToString(row["type"])
                            };
                            data.ContactToEvaluationArc.Add(arcdata);
                        }
                    }
                }
            }
            return data;
        }

        /// <summary>
        /// Update/Delete Contact to evaluation
        /// </summary>
        /// <param name="id"></param>
        /// <param name="contactid"></param>
        /// <param name="companyid"></param>
        /// <param name="reason"></param>
        /// <param name="deleted"></param>
        /// <returns></returns>
        public Dictionary<int, string> UpdateAdminContactsToEvaluation(int id, int contactid, int companyid, string reason, string type, bool deleted = false) {
            var data = new Dictionary<int, string>();
            var sqlConnection = new SqlConnection(connectionString);
            using (var sqladp = new SqlDataAdapter("AdminContactsToEvaluation_Update", sqlConnection)) {
                sqladp.SelectCommand.Parameters.AddWithValue("@Id", id);
                sqladp.SelectCommand.Parameters.AddWithValue("@ContactId", contactid);
                sqladp.SelectCommand.Parameters.AddWithValue("@CompanyId", companyid);
                sqladp.SelectCommand.Parameters.AddWithValue("@Delete", deleted);
                sqladp.SelectCommand.Parameters.AddWithValue("@Reason", reason);
                sqladp.SelectCommand.Parameters.AddWithValue("@Type", type);
                sqladp.SelectCommand.CommandType = CommandType.StoredProcedure;
                if (sqlConnection.State == ConnectionState.Closed) { sqlConnection.Open(); }
                var ds = new DataSet();
                sqladp.Fill(ds);

                if (ds.Tables.Count > 0 && ds.Tables[0] != null) {
                    if (ds.Tables[0].Rows.Count > 0) {
                        foreach (DataRow row in ds.Tables[0].Rows) {
                            var errorcode = Convert.ToInt32(row["ErrorCode"]);
                            var message = Convert.ToString(row["StatusMsg"]);
                            data.Add(errorcode, message);
                        }
                    }
                }
            }
            return data;
        }

        /// <summary>
        /// Get DataClassification By FieldType
        /// </summary>
        /// <returns></returns>
        public List<DataClassificationFieldType> GetDataClassificationByFieldType() {
            var record = new List<DataClassificationFieldType>();
            sql.commandText = "FieldTypeDataClassification_Get";
            using (SqlDataReader dr = sql.executeReader) {
                if (dr.HasRows) {
                    while (dr.Read()) {
                        record.Add(new DataClassificationFieldType() {
                            Id = TypeCast.ToInt(dr["Id"]),
                            Name = TypeCast.ToString(dr["Name"]),
                            FieldType = TypeCast.ToString(dr["FieldType"]),
                            EnforceAnonymization = TypeCast.ToBool(dr["EnforceAnonymization"]),
                        });
                    }
                }
                if (!dr.IsClosed) { dr.Close(); dr.Dispose(); }
            }
            sql.reset();
            return record;
        }

        /// <summary>
        /// Get DataClassification By FieldType
        /// </summary>
        /// <returns></returns>
        public List<AnonymizationFieldType> GetAnonymizationByFieldType() {
            var record = new List<AnonymizationFieldType>();
            sql.commandText = "FieldTypeAnonymization_Get";
            using (SqlDataReader dr = sql.executeReader) {
                if (dr.HasRows) {
                    while (dr.Read()) {
                        record.Add(new AnonymizationFieldType() {
                            Id = TypeCast.ToInt(dr["Id"]),
                            Name = TypeCast.ToString(dr["Name"]),
                            FieldType = TypeCast.ToString(dr["FieldType"])
                        });
                    }
                }
                if (!dr.IsClosed) { dr.Close(); dr.Dispose(); }
            }
            sql.reset();
            return record;
        }

        /// <summary>
        /// Get DataClassification By FieldType
        /// </summary>
        /// <returns></returns>
        public List<DataClassification> GetDataClassifications() {
            var record = new List<DataClassification>();
            sql.commandText = "DataClassification_Get";
            using (SqlDataReader dr = sql.executeReader) {
                if (dr.HasRows) {
                    while (dr.Read()) {
                        record.Add(new DataClassification() {
                            Id = TypeCast.ToInt(dr["Id"]),
                            Name = TypeCast.ToString(dr["Name"]),
                            Description = TypeCast.ToString(dr["Description"]),
                            ExportMask = TypeCast.ToString(dr["ExportMask"]),
                            LogAccess = TypeCast.ToBool(dr["LogAccess"]),
                            EnforceAnonymization = TypeCast.ToBool(dr["EnforceAnonymization"]),
                            LockInGUI = TypeCast.ToBool(dr["LockInGUI"]),
                        });
                    }
                }
                if (!dr.IsClosed) { dr.Close(); dr.Dispose(); }
            }
            sql.reset();
            return record;
        }

        /// <summary>
        /// Get Flag Type And Data Object settings
        /// </summary>
        /// <returns></returns>
        public FlagAndDataObject GetFlagTypeAndDataObjectSettings() {
            var fdb = new FlagAndDataObject();
            var sqlConnection = new SqlConnection(connectionString);
            using (var sqladp = new SqlDataAdapter("FlagTypeAndDataObject_Get", sqlConnection)) {
                sqladp.SelectCommand.CommandType = CommandType.StoredProcedure;
                if (sqlConnection.State == ConnectionState.Closed) { sqlConnection.Open(); }
                var ds = new DataSet();
                sqladp.Fill(ds);
                if (ds.Tables[0] != null) {
                    if (ds.Tables[0].Rows.Count > 0) {
                        foreach (DataRow row in ds.Tables[0].Rows) {
                            fdb.flagtype.Add(new FlagType() {
                                Id = Convert.ToInt32(row["Id"]),
                                Name = Convert.ToString(row["Name"]),
                                Description = Convert.ToString(row["Description"]),
                                DefaultValue = Convert.ToString(row["DefaultValue"])
                            });
                        }
                    }
                }

                if (ds.Tables[1] != null) {
                    if (ds.Tables[1].Rows.Count > 0) {
                        foreach (DataRow row in ds.Tables[1].Rows) {
                            fdb.dataobject.Add(new DataObject() {
                                Id = Convert.ToInt32(row["Id"]),
                                Name = Convert.ToString(row["Name"]),
                                ValidFlagTypes = Convert.ToString(row["ValidFlagTypes"]),
                                ContactValidFlagTypes = Convert.ToString(row["ContactValidFlagTypes"]),
                                FriendlyName = Convert.ToString(row["FriendlyName"]),
                            });
                        }
                    }
                }
            }
            return fdb;
        }

        /// <summary>
        /// Get Flag Type And Data Object settings
        /// </summary>
        /// <returns></returns>
        public List<Flag> GetFlag(string id = "", int objectType = 0, int objectId = 0, User user = null, int contactId = 0, int avnId = 0, int importantFieldId = 0) {
            var flags = new List<Flag>();
            var sqlConnection = new SqlConnection(connectionString);
            using (var sqladp = new SqlDataAdapter("Flag_Get", sqlConnection)) {
                sqladp.SelectCommand.Parameters.AddWithValue("@UserId", user.Id);
                if (!string.IsNullOrEmpty(id)) { sqladp.SelectCommand.Parameters.AddWithValue("@Id", id); }
                if (objectType > 0) { sqladp.SelectCommand.Parameters.AddWithValue("@ObjectType", objectType); }
                if (objectId > 0) { sqladp.SelectCommand.Parameters.AddWithValue("@ObjectIdInt", objectId); }
                if (contactId > 0) { sqladp.SelectCommand.Parameters.AddWithValue("@ContactId", contactId); }
                if (avnId > 0) { sqladp.SelectCommand.Parameters.AddWithValue("@AVNId", avnId); }
                if (importantFieldId > 0) { sqladp.SelectCommand.Parameters.AddWithValue("@ImportantFieldId", importantFieldId); }
                sqladp.SelectCommand.CommandType = CommandType.StoredProcedure;
                if (sqlConnection.State == ConnectionState.Closed) { sqlConnection.Open(); }
                var ds = new DataSet();
                sqladp.Fill(ds);
                if (ds.Tables[0] != null) {
                    if (ds.Tables[0].Rows.Count > 0) {
                        foreach (DataRow row in ds.Tables[0].Rows) {
                            var flag = new Flag();

                            flag.Id = TypeCast.ToString(row["Id"]);
                            flag.Type = TypeCast.ToInt(row["Type"]);
                            flag.Date = Convert.ToDateTime(row["Date"]);

                            flag.ObjectType = TypeCast.ToInt(row["ObjectType"]);
                            flag.ObjectIdInt = TypeCast.ToInt(row["ObjectIdInt"]);
                            flag.ObjectIdGuid = TypeCast.ToString(row["ObjectIdGuid"]);

                            flag.ContactId = TypeCast.ToInt(row["ContactId"]);

                            flag.CreatedDate = Convert.ToDateTime(row["CreatedDate"]);
                            flag.CreatedBy = TypeCast.ToString(row["CreatedBy"]);

                            flag.ApprovedDate = TypeCast.ToDateTimeLoose(row["ApprovedDate"]);
                            flag.ApprovedBy = TypeCast.ToString(row["ApprovedBy"]);

                            flag.FlagAttributes = TypeCast.ToString(row["FlagAttributes"]);

                            flag.ReasonType = TypeCast.ToInt(row["ReasonType"]);
                            flag.Reason = TypeCast.ToString(row["Reason"]);

                            if (row.Table.Columns.Contains("AVNId")) {
                                flag.AVNId = TypeCast.ToInt(row["AVNId"]);
                            }
                            if (row.Table.Columns.Contains("DateAddInterval")) {
                                flag.DateAddInterval = TypeCast.ToInt(row["DateAddInterval"]);
                            }
                            if (row.Table.Columns.Contains("DateAddPart")) {
                                flag.DateAddPart = TypeCast.ToString(row["DateAddPart"]);
                            }
                            if (row.Table.Columns.Contains("ImportantFieldId")) {
                                flag.ImportantFieldId = TypeCast.ToInt(row["ImportantFieldId"]);
                            }
                            flags.Add(flag);
                        }
                    }
                }
            }
            return flags;
        }

        /// <summary>
        /// Save/Update Flag of data object
        /// </summary>
        /// <returns></returns>
        public string AddEditFlag(Flag f) {
            string retval = "";
            sql.commandText = "Flag_AddEdit";
            if (!string.IsNullOrEmpty(f.Id)) {
                sql.parameters.AddWithValue("@Id", f.Id);
            }
            sql.parameters.AddWithValue("@Type", f.Type);
            sql.parameters.AddWithValue("@Date", f.Date);
            sql.parameters.AddWithValue("@ObjectType", f.ObjectType);
            sql.parameters.AddWithValue("@ObjectIdInt", f.ObjectIdInt);
            if (!string.IsNullOrEmpty(f.ObjectIdGuid)) {
                sql.parameters.AddWithValue("@ObjectIdGuid", f.ObjectIdGuid);
            }
            if (f.ContactId > 0) {
                sql.parameters.AddWithValue("@ContactId", f.ContactId);
            }
            sql.parameters.AddWithValue("@CreatedDate", f.CreatedDate);
            sql.parameters.AddWithValue("@CreatedBy", f.CreatedBy);

            if (f.ApprovedDate == null) {
                sql.parameters.AddWithValue("@ApprovedDate", f.ApprovedDate);
            }
            if (!string.IsNullOrEmpty(f.ApprovedBy)) {
                sql.parameters.AddWithValue("@ApprovedBy", f.ApprovedBy);
            }
            sql.parameters.AddWithValue("@FlagAttributes", f.FlagAttributes);

            if (f.ReasonType > 0) {
                sql.parameters.AddWithValue("@ReasonType", f.ReasonType);
            }
            sql.parameters.AddWithValue("@Reason", f.Reason);

            if (f.AVNId > 0) {
                sql.parameters.AddWithValue("@AVNId", f.AVNId);
            }
            if (f.DateAddInterval > 0) {
                sql.parameters.AddWithValue("@DateAddInterval", f.DateAddInterval);
            }
            if (!string.IsNullOrEmpty(f.DateAddPart)) {
                sql.parameters.AddWithValue("@DateAddPart", f.DateAddPart);
            }
            if (f.ImportantFieldId > 0) {
                sql.parameters.AddWithValue("@ImportantFieldId", f.ImportantFieldId);
            }
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = TypeCast.ToString(dr["Id"]);
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        /// <summary>
        /// Delete Flag of data object of important fields
        /// </summary>
        /// <returns></returns>
        public void DeleteFlagForImportant(int objectType, int objectIdInt, string positiveFieldIds) {
            sql.commandText = "ImportantFieldFlag_Delete";
            sql.parameters.AddWithValue("@ObjectType", objectType);
            sql.parameters.AddWithValue("@ObjectIdInt", objectIdInt);
            sql.parameters.AddWithValue("@ImportantFieldIds", positiveFieldIds);
            sql.execute();
            sql.reset();
        }

        /// <summary>
        /// Delete Flag of data object of important fields
        /// </summary>
        /// <returns></returns>
        public void UpdateImportantFieldFlag(User u, int fieldId = 0) {
            UpdateImportantFieldFlag(u, u.Organisation, u.Organisation, fieldId);
        }

        /// <summary>
        /// Delete Flag of data object of important fields
        /// </summary>
        /// <returns></returns>
        private void UpdateImportantFieldFlag(User u, Organisation CurrentOrg, Organisation ParentOrg, int fieldId = 0) {
            sql.commandText = "ImportantFieldFlag_Update";
            sql.parameters.AddWithValue("@OrganisationId", CurrentOrg.Id);
            sql.parameters.AddWithValue("@UserId", u.Id);
            if (fieldId > 0) {
                sql.parameters.AddWithValue("@ParentOrganisationId", ParentOrg.Id);
                sql.parameters.AddWithValue("@FieldId", fieldId);
            }
            sql.execute();
            sql.reset();

            foreach (var newCurrentOrg in Organisations_getOrganisations(CurrentOrg.Id)) {
                UpdateImportantFieldFlag(u, newCurrentOrg, ParentOrg, fieldId);
            }
        }

        /// <summary>
        /// Store Flag into session table
        /// </summary>
        /// <returns></returns>
        public void DeleteFlag(string flagids) {
            sql.commandText = "Flag_Delete";
            sql.parameters.AddWithValue("@FlagIds", flagids);
            sql.execute();
            sql.reset();
        }

        /// <summary>
        /// Get Flag Summary
        /// </summary>
        /// <returns></returns>
        public string GetFlagSummary(int objectType = 0, int objectId = 0, User user = null, int avnid = 0) {
            var summary = string.Empty;
            var sqlConnection = new SqlConnection(connectionString);
            using (var sqladp = new SqlDataAdapter("FlagSummary_Get", sqlConnection)) {
                sqladp.SelectCommand.Parameters.AddWithValue("@UserId", user.Id);
                if (objectType > 0) { sqladp.SelectCommand.Parameters.AddWithValue("@ObjectType", objectType); }
                if (objectId > 0) { sqladp.SelectCommand.Parameters.AddWithValue("@ObjectIdInt", objectId); }
                if (avnid > 0) { sqladp.SelectCommand.Parameters.AddWithValue("@AVNId", avnid); }
                sqladp.SelectCommand.CommandType = CommandType.StoredProcedure;
                if (sqlConnection.State == ConnectionState.Closed) { sqlConnection.Open(); }
                var ds = new DataSet();
                sqladp.Fill(ds);
                if (ds.Tables[0] != null) {
                    if (ds.Tables[0].Rows.Count > 0) {
                        foreach (DataRow row in ds.Tables[0].Rows) {
                            summary = TypeCast.ToString(row["Summary"]);
                        }
                    }
                }
            }
            return summary;
        }

        /// <summary>
        /// Get Flag Type And Data Object settings
        /// </summary>
        /// <returns></returns>
        public string GetFlagSession(string screenkey) {
            var session = "";
            var sqlConnection = new SqlConnection(connectionString);
            using (var sqladp = new SqlDataAdapter("ScreenSession_Get", sqlConnection)) {
                sqladp.SelectCommand.Parameters.AddWithValue("@ScreenRefKey", screenkey);
                sqladp.SelectCommand.CommandType = CommandType.StoredProcedure;
                if (sqlConnection.State == ConnectionState.Closed) { sqlConnection.Open(); }
                var ds = new DataSet();
                sqladp.Fill(ds);
                if (ds.Tables[0] != null) {
                    if (ds.Tables[0].Rows.Count > 0) {
                        foreach (DataRow row in ds.Tables[0].Rows) {
                            session = TypeCast.ToString(row["Session"]);
                        }
                    }
                }
            }
            return session;
        }

        /// <summary>
        /// Store Flag into session table
        /// </summary>
        /// <returns></returns>
        public void AddEditFlagSession(string key, string session) {
            sql.commandText = "ScreenSession_AddEdit";
            sql.parameters.AddWithValue("@ScreenRefKey", key);
            sql.parameters.AddWithValue("@Session", session);
            sql.execute();
            sql.reset();
        }

        /// <summary>
        /// Get positive list
        /// </summary>
        /// <param name="dynamicFieldId"></param>
        /// <returns></returns>
        public string GetDynamicFieldPositiveList(int dynamicFieldId) {
            var positivCommaList = string.Empty;
            sql.commandText = "DynamicFieldsPositive_Get";
            sql.parameters.AddWithValue("@DynamicFieldId", dynamicFieldId);
            SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                positivCommaList = TypeCast.ToString(dr["Positiv"]);
            }
            dr.Close();
            sql.reset();

            return positivCommaList;
        }

        /// <summary>
        /// Get Contact allow anonymization
        /// </summary>
        /// <returns></returns>
        public List<Flag> GetContactAllowAnonymization(User user, int contactId) {
            var flags = new List<Flag>();
            var sqlConnection = new SqlConnection(connectionString);
            using (var sqladp = new SqlDataAdapter("Contact_AllowAnonymization", sqlConnection)) {
                sqladp.SelectCommand.Parameters.AddWithValue("@UserId", user.Id);
                sqladp.SelectCommand.Parameters.AddWithValue("@OrganisationId", user.OrganisationId);
                sqladp.SelectCommand.Parameters.AddWithValue("@ContactId", contactId);
                sqladp.SelectCommand.CommandType = CommandType.StoredProcedure;
                if (sqlConnection.State == ConnectionState.Closed) { sqlConnection.Open(); }
                var ds = new DataSet();
                sqladp.Fill(ds);
                if (ds.Tables[0] != null) {
                    if (ds.Tables[0].Rows.Count > 0) {
                        foreach (DataRow row in ds.Tables[0].Rows) {
                            var flag = new Flag();
                            flag.Id = TypeCast.ToString(row["Id"]);
                            flag.Type = TypeCast.ToInt(row["Type"]);
                            flag.Date = Convert.ToDateTime(row["Date"]);
                            flag.ObjectType = TypeCast.ToInt(row["ObjectType"]);
                            flag.ObjectIdInt = TypeCast.ToInt(row["ObjectIdInt"]);
                            flag.ObjectIdGuid = TypeCast.ToString(row["ObjectIdGuid"]);
                            flag.ContactId = TypeCast.ToInt(row["ContactId"]);
                            flag.CreatedDate = Convert.ToDateTime(row["CreatedDate"]);
                            flag.CreatedBy = TypeCast.ToString(row["CreatedBy"]);
                            flag.ApprovedDate = TypeCast.ToDateTimeLoose(row["ApprovedDate"]);
                            flag.ApprovedBy = TypeCast.ToString(row["ApprovedBy"]);
                            flag.FlagAttributes = TypeCast.ToString(row["FlagAttributes"]);
                            flag.ReasonType = TypeCast.ToInt(row["ReasonType"]);
                            flag.Reason = TypeCast.ToString(row["Reason"]);

                            flag.ReasonText = TypeCast.ToString(row["ReasonText"]);
                            flag.OrganisationName = TypeCast.ToString(row["OrganisationName"]);
                            flag.FullName = TypeCast.ToString(row["FullName"]);
                            flag.Email = TypeCast.ToString(row["Email"]);

                            flags.Add(flag);
                        }
                    }
                }
            }
            return flags;
        }

        /// <summary>
        /// Get Company allow anonymization
        /// </summary>
        /// <returns></returns>
        public List<Flag> GetCompanyAllowAnonymization(User user, int id) {
            var flags = new List<Flag>();
            var sqlConnection = new SqlConnection(connectionString);

            // TODO: Create the "Company_AllowAnonymization" stored procedure
            using (var sqladp = new SqlDataAdapter("Company_AllowAnonymization", sqlConnection)) {
                sqladp.SelectCommand.Parameters.AddWithValue("@UserId", user.Id);
                sqladp.SelectCommand.Parameters.AddWithValue("@OrganisationId", user.OrganisationId);
                sqladp.SelectCommand.Parameters.AddWithValue("@ContactId", id);
                sqladp.SelectCommand.CommandType = CommandType.StoredProcedure;
                if (sqlConnection.State == ConnectionState.Closed) { sqlConnection.Open(); }
                var ds = new DataSet();
                sqladp.Fill(ds);
                if (ds.Tables[0] != null) {
                    if (ds.Tables[0].Rows.Count > 0) {
                        foreach (DataRow row in ds.Tables[0].Rows) {
                            var flag = new Flag();
                            flag.Id = TypeCast.ToString(row["Id"]);
                            flag.Type = TypeCast.ToInt(row["Type"]);
                            flag.Date = Convert.ToDateTime(row["Date"]);
                            flag.ObjectType = TypeCast.ToInt(row["ObjectType"]);
                            flag.ObjectIdInt = TypeCast.ToInt(row["ObjectIdInt"]);
                            flag.ObjectIdGuid = TypeCast.ToString(row["ObjectIdGuid"]);
                            flag.ContactId = TypeCast.ToInt(row["ContactId"]);
                            flag.CreatedDate = Convert.ToDateTime(row["CreatedDate"]);
                            flag.CreatedBy = TypeCast.ToString(row["CreatedBy"]);
                            flag.ApprovedDate = TypeCast.ToDateTimeLoose(row["ApprovedDate"]);
                            flag.ApprovedBy = TypeCast.ToString(row["ApprovedBy"]);
                            flag.FlagAttributes = TypeCast.ToString(row["FlagAttributes"]);
                            flag.ReasonType = TypeCast.ToInt(row["ReasonType"]);
                            flag.Reason = TypeCast.ToString(row["Reason"]);

                            flag.ReasonText = TypeCast.ToString(row["ReasonText"]);
                            flag.OrganisationName = TypeCast.ToString(row["OrganisationName"]);
                            flag.FullName = TypeCast.ToString(row["FullName"]);
                            flag.Email = TypeCast.ToString(row["Email"]);

                            flags.Add(flag);
                        }
                    }
                }
            }
            return flags;
        }

        /// <summary>
        /// Save expiry for the AVN Entities
        /// </summary>
        /// <param name="avnID">AVN ID</param>
        /// <param name="contact">Expiry for Contact</param>
        public void AddEditAVNEntitiesExpiry(int avnID, bool contact) {
            sql.commandText = "AVNEntitiesExpiry_All";
            sql.parameters.AddWithValue("@AVNId", avnID);
            sql.parameters.AddWithValue("@Contact", contact);
            sql.execute();
            sql.reset();
        }

        /// <summary>
        /// Get AVN entity
        /// </summary>
        /// <returns></returns>
        public string[] GetAVNEntity(int avnId, int entityId) {
            string[] result = new string[2];
            sql.commandText = "GetAVNEntity_Get";
            sql.parameters.AddWithValue("@AVNId", avnId);
            sql.parameters.AddWithValue("@EntityId", entityId);
            SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                result[0] = TypeCast.ToString(dr["Id"]);
                result[1] = TypeCast.ToString(dr["SMVCompanyId"]);
            }
            dr.Close();
            sql.reset();
            return result;
        }

        /// <summary>
        /// Get Contact and Flag count for Company
        /// </summary>
        /// <returns></returns>
        public string[] GetContactAndFlagCountByCompany(int companyId) {
            string[] result = new string[2];
            sql.commandText = "Company_ContactAndFlagCount";
            sql.parameters.AddWithValue("@CompanyId", companyId);
            SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                result[0] = TypeCast.ToString(dr["ContactCount"]);
                result[1] = TypeCast.ToString(dr["FlagCount"]);
            }
            dr.Close();
            sql.reset();
            return result;
        }

        /// <summary>
        /// Get Agreements
        /// </summary>
        /// <returns></returns>
        public List<Agreement> GetAgreements(User user) {
            var record = new List<Agreement>();
            sql.commandText = "Agreements_Get";
            sql.parameters.AddWithValue("@OrganisationId", user.OrganisationId);
            using (SqlDataReader dr = sql.executeReader) {
                if (dr.HasRows) {
                    while (dr.Read()) {
                        record.Add(new Agreement() {
                            Id = TypeCast.ToInt(dr["Id"]),
                            Name = TypeCast.ToString(dr["Name"]),
                            Expiry = TypeCast.ToInt(dr["Expiry"]),
                            ExpiryDatePart = TypeCast.ToString(dr["ExpiryDatePart"]),
                            ExpiryDateFixed = TypeCast.ToDateTimeLoose(dr["ExpiryDateFixed"]),
                            FollowUp = TypeCast.ToInt(dr["FollowUp"]),
                            Description = TypeCast.ToString(dr["Description"]),
                            TemplateText = TypeCast.ToString(dr["TemplateText"]),
                            TemplateSubject = TypeCast.ToString(dr["TemplateSubject"]),
                            Type = TypeCast.ToString(dr["Type"]),
                            RequireDocumentation = TypeCast.ToBool(dr["RequireDocumentation"]),
                        });
                    }
                }
                if (!dr.IsClosed) { dr.Close(); dr.Dispose(); }
            }
            sql.reset();
            return record;
        }

        /// <summary>
        /// Save/Update Agreement contact
        /// </summary>
        /// <returns></returns>
        public string AddEditContactAgreement(AgreementContact agreement) {
            string retval = "";
            sql.commandText = "AgreementContact_AddEdit";
            sql.parameters.AddWithValue("@Id", agreement.Id);
            sql.parameters.AddWithValue("@ContractTypeId", agreement.AgreementTypeId);
            sql.parameters.AddWithValue("@ContactId", agreement.ContactId);
            if (agreement.Binary != null) { sql.parameters.AddWithValue("@Binary", agreement.Binary); }
            if (!string.IsNullOrEmpty(agreement.Comment)) { sql.parameters.AddWithValue("@Comment", agreement.Comment); }
            sql.parameters.AddWithValue("@ExpiryDate", agreement.ExpiryDate);
            sql.parameters.AddWithValue("@FollowUp", agreement.FollowUp);
            if (!string.IsNullOrEmpty(agreement.AgreementXml)) { sql.parameters.AddWithValue("@AgreementContent", agreement.AgreementXml); }
            if (agreement.Type != "email") { sql.parameters.AddWithValue("@Active", agreement.Active); }
            sql.parameters.AddWithValue("@CreatedById", agreement.CreatedById);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = TypeCast.ToString(dr["Id"]);
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        /// <summary>
        /// Does Contact's Agreement exist, ESCRM-273/274
        /// </summary>
        /// <returns></returns>
        public bool ContactAgreementExists(int contactId, int agreementId, int organisationId, out int fAgreementId)
        {
            bool retval = false;
            fAgreementId = 0;

            List<AgreementContact> records = new List<AgreementContact>();
            sql.commandText = "AgreementContact_Exists";
            sql.parameters.AddWithValue("@AgreementId", agreementId);
            sql.parameters.AddWithValue("@ContactId", contactId);
            sql.parameters.AddWithValue("@OrganisationId", organisationId);
            using (SqlDataReader dr = sql.executeReader)
            {
                if (dr.HasRows)
                {
                    dr.Read();
                    fAgreementId = TypeCast.ToInt(dr[0]);

                    retval = true;
                }
                if (!dr.IsClosed) { dr.Close(); dr.Dispose(); }
            }
            sql.reset();
            
            return retval;
        }

        /// <summary>
        /// Save/Update Document binary Agreement contact
        /// </summary>
        /// <returns></returns>
        public string UploadDocumentContactAgreement(AgreementContact agreement, byte[] document) {
            string retval = "";
            sql.commandText = "AgreementContact_AddEdit";
            sql.parameters.AddWithValue("@Id", agreement.Id);
            sql.parameters.AddWithValue("@ContractTypeId", agreement.AgreementTypeId);
            sql.parameters.AddWithValue("@ContactId", agreement.ContactId);
            if (document != null) {
                sql.parameters.AddWithValue("@FileName", agreement.FileName);
                sql.parameters.AddWithValue("@Binary", document);
            }
            sql.parameters.AddWithValue("@Active", (document != null ? true : false));

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = TypeCast.ToString(dr["Id"]);
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        /// <summary>
        /// Get Contact's Agreement, ESCRM-273/274
        /// </summary>
        /// <returns></returns>
        public List<Tuple<string, string, string, string, string, string, string>> ContactAgreementContactsById(int agreementId) //** Skal hedde ContactAgreementContactsById, fra GetAllContactAgreement
        {
            List<Tuple<string, string, string, string, string, string, string>> records = new List<Tuple<string, string, string, string, string, string, string>>();
            sql.commandText = "ContactAgreementContactsById"; //** Skal hedde ContactAgreementContactsById, fra AgreementContact_GetAll
            sql.parameters.AddWithValue("@AgreementId", agreementId);

            using (SqlDataReader dr = sql.executeReader)
            {
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        string contactId = TypeCast.ToString(dr[0]);
                        string fornavn = TypeCast.ToString(dr[1]);
                        string efternavn = TypeCast.ToString(dr[2]);
                        string email = TypeCast.ToString(dr[3]);
                        string navn = TypeCast.ToString(dr[4]);
                        string kontaktNavn = TypeCast.ToString(dr[5]);
                        string oprettet = TypeCast.ToString(dr[6]);

                        Tuple<string, string, string, string, string, string, string> tpl = new Tuple<string, string, string, string, string, string, string>(contactId, fornavn, efternavn, email, navn, kontaktNavn , oprettet);
                        
                        records.Add(tpl);
                    }
                }
                if (!dr.IsClosed) { dr.Close(); dr.Dispose(); }
            }
            sql.reset();
            return records;
        }

        /// <summary>
        /// Get Contact's Agreement
        /// </summary>
        /// <returns></returns>
        public List<AgreementContact> GetContactAgreement(User user, int contactid = 0, int id = 0) {
            List<AgreementContact> records = new List<AgreementContact>();
            sql.commandText = "AgreementContact_Get";
            sql.parameters.AddWithValue("@OrganisationId", user.OrganisationId);
            sql.parameters.AddWithValue("@Id", id);
            sql.parameters.AddWithValue("@ContactId", contactid);
            using (SqlDataReader dr = sql.executeReader) {
                if (dr.HasRows) {
                    while (dr.Read()) {
                        records.Add(new AgreementContact() {
                            Id = TypeCast.ToInt(dr["Id"]),

                            AgreementTypeId = TypeCast.ToInt(dr["AgreementId"]),
                            AgreementName = TypeCast.ToString(dr["AgreementName"]),

                            ContactId = TypeCast.ToInt(dr["ContactId"]),
                            ContactName = TypeCast.ToString(dr["ContactName"]),

                            FileName = TypeCast.ToString(dr["FileName"]),
                            Binary = (dr["Binary"] == System.DBNull.Value ? null : (byte[])dr["Binary"]),
                            AgreementXml = TypeCast.ToString(dr["AgreementContent"]),
                            Comment = TypeCast.ToString(dr["Comment"]),
                            ExpiryDate = TypeCast.ToDateTime(dr["ExpiryDate"]),
                            FollowUp = TypeCast.ToDateTime(dr["FollowUp"]),
                            CreatedDate = TypeCast.ToDateTime(dr["CreatedDate"]),
                            Active = TypeCast.ToBool(dr["Active"]),

                            Description = TypeCast.ToString(dr["Description"]),
                            Type = TypeCast.ToString(dr["Type"]),
                            RequireDocumentation = TypeCast.ToBool(dr["RequireDocumentation"]),

                            CreatedBy = TypeCast.ToString(dr["CreatedBy"]),
                        });
                    }
                }
                if (!dr.IsClosed) { dr.Close(); dr.Dispose(); }
            }
            sql.reset();
            return records;
        }

        /// <summary>
        /// Save/Update Agreement
        /// </summary>
        /// <returns></returns>
        public string AddEditAgreement(User user, Agreement agreement) {
            string retval = "";
            sql.commandText = "Agreement_AddEdit";

            sql.parameters.AddWithValue("@Id", agreement.Id);
            sql.parameters.AddWithValue("@Name", agreement.Name);
            sql.parameters.AddWithValue("@OrganisationId", user.OrganisationId);
            if (agreement.Expiry > 0) {
                sql.parameters.AddWithValue("@Expiry", agreement.Expiry);
                sql.parameters.AddWithValue("@ExpiryDatePart", agreement.ExpiryDatePart);
            } else {
                if (agreement.ExpiryDateFixed > DateTime.Now) {
                    sql.parameters.AddWithValue("@ExpiryDateFixed", agreement.ExpiryDateFixed);
                }
            }
            sql.parameters.AddWithValue("@FollowUp", agreement.FollowUp);
            sql.parameters.AddWithValue("@Description", agreement.Description);
            sql.parameters.AddWithValue("@TemplateText", agreement.TemplateText);
            sql.parameters.AddWithValue("@TemplateSubject", agreement.TemplateSubject);
            sql.parameters.AddWithValue("@Type", agreement.Type);
            sql.parameters.AddWithValue("@Active", DateTime.Now);
            sql.parameters.AddWithValue("@RequireDocumentation", agreement.RequireDocumentation);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = TypeCast.ToString(dr["Id"]);
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        /// <summary>
        /// Delete Agreement
        /// </summary>
        public void Agreement_Delete(User U, int id) {
            sql.commandText = "Agreement_Delete";
            sql.parameters.AddWithValue("@Id", id);
            sql.parameters.AddWithValue("@OrganisationId", U.OrganisationId);
            sql.execute();
            sql.reset();
        }

        /// <summary>
        /// Delete Contact Agreement
        /// </summary>
        public void ContactAgreement_Delete(User U, int id) {
            sql.commandText = "ContactAgreement_Delete";
            sql.parameters.AddWithValue("@Id", id);
            sql.parameters.AddWithValue("@OrganisationId", U.OrganisationId);
            sql.execute();
            sql.reset();
        }

        /// <summary>
        /// Get All Calendar Events
        /// </summary>
        /// <returns></returns>
        public List<CalEvent> GetCalendarEvents(User user, string requesttype) {
            var record = new List<CalEvent>();
            sql.commandText = "CalendarEvents_Get";
            sql.parameters.AddWithValue("@UserId", user.Id);
            sql.parameters.AddWithValue("@OrganisationId", user.OrganisationId);
            sql.parameters.AddWithValue("@type", requesttype);
            using (SqlDataReader dr = sql.executeReader) {
                if (dr.HasRows) {
                    while (dr.Read()) {
                        record.Add(new CalEvent() {
                            Id = TypeCast.ToInt(dr["Id"]),
                            UserId = TypeCast.ToString(dr["UserId"]),
                            OrganisationId = TypeCast.ToInt(dr["OrganisationId"]),

                            TypeId = TypeCast.ToInt(dr["TypeId"]),
                            Type = TypeCast.ToString(dr["Type"]),

                            Title = TypeCast.ToString(dr["Titel"]),
                            StartDate = TypeCast.ToDateTimeLoose(dr["StartTime"]),
                            EndDate = TypeCast.ToDateTimeLoose(dr["EndTime"]),
                            Place = TypeCast.ToString(dr["Location"]),
                            Municipality = TypeCast.ToInt(dr["Municipality"]),
                            Color = TypeCast.ToString(dr["Color"]),

                            ShortDescription = TypeCast.ToString(dr["Description"]),
                            Link = TypeCast.ToString(dr["URL"])
                        });
                    }
                }
                if (!dr.IsClosed) { dr.Close(); dr.Dispose(); }
            }
            sql.reset();
            return record;
        }

        /// <summary>
        /// Get Calendar Event
        /// </summary>
        /// <returns></returns>
        public CalEvent GetCalendarEvent(User user, int eventId) {
            var record = new CalEvent();
            sql.commandText = "CalendarEvent_Get";
            sql.parameters.AddWithValue("@Id", eventId);
            sql.parameters.AddWithValue("@UserId", user.Id);
            sql.parameters.AddWithValue("@OrganisationId", user.OrganisationId);
            using (SqlDataReader dr = sql.executeReader) {
                if (dr.HasRows) {
                    while (dr.Read()) {
                        record.Id = TypeCast.ToInt(dr["Id"]);
                        record.UserId = TypeCast.ToString(dr["UserId"]);
                        record.OrganisationId = TypeCast.ToInt(dr["OrganisationId"]);

                        record.TypeId = TypeCast.ToInt(dr["TypeId"]);

                        record.Title = TypeCast.ToString(dr["Titel"]);
                        record.StartDate = TypeCast.ToDateTimeLoose(dr["StartTime"]);
                        record.EndDate = TypeCast.ToDateTimeLoose(dr["EndTime"]);
                        record.Place = TypeCast.ToString(dr["Location"]);
                        record.Municipality = TypeCast.ToInt(dr["Municipality"]);

                        record.ShortDescription = TypeCast.ToString(dr["Description"]);
                        record.Link = TypeCast.ToString(dr["URL"]);
                        record.CompanyId = TypeCast.ToInt(dr["CompanyId"]);
                        record.ContactId = TypeCast.ToInt(dr["ContactId"]);
                    }
                }
                if (!dr.IsClosed) { dr.Close(); dr.Dispose(); }
            }
            sql.reset();
            return record;
        }

        /// <summary>
        /// Get Widget Calendar Event
        /// </summary>
        /// <returns></returns>
        public List<CalEvent> GetCalendarWidgetEvent(User user) {
            var record = new List<CalEvent>();
            sql.commandText = "CalendarWidgetEvent_Get";
            sql.parameters.AddWithValue("@UserId", user.Id);
            sql.parameters.AddWithValue("@OrganisationId", user.OrganisationId);
            using (SqlDataReader dr = sql.executeReader) {
                if (dr.HasRows) {
                    while (dr.Read()) {
                        record.Add(new CalEvent() {
                            Id = TypeCast.ToInt(dr["Id"]),
                            UserId = TypeCast.ToString(dr["UserId"]),
                            OrganisationId = TypeCast.ToInt(dr["OrganisationId"]),
                            OrganisationName = TypeCast.ToString(dr["OrganisationName"]),

                            TypeId = TypeCast.ToInt(dr["TypeId"]),
                            Type = TypeCast.ToString(dr["Type"]),

                            Title = TypeCast.ToString(dr["Titel"]),
                            StartDate = TypeCast.ToDateTimeLoose(dr["StartTime"]),
                            EndDate = TypeCast.ToDateTimeLoose(dr["EndTime"]),
                            Place = TypeCast.ToString(dr["Location"]),
                            MunicipalityName = TypeCast.ToString(dr["Municipality"]),
                            RegionName = TypeCast.ToString(dr["Region"]),
                            Color = TypeCast.ToString(dr["Color"]),

                            ShortDescription = TypeCast.ToString(dr["Description"]),
                            Link = TypeCast.ToString(dr["URL"]),

                            CompanyId = TypeCast.ToInt(dr["CompanyId"]),
                            CompanyName = TypeCast.ToString(dr["CompanyName"]),
                        });
                    }
                }
                if (!dr.IsClosed) { dr.Close(); dr.Dispose(); }
            }
            sql.reset();
            return record;
        }

        /// <summary>
        /// Save/Update Calendar Event
        /// </summary>
        /// <returns></returns>
        public string AddEditCalendarEvent(User user, CalEvent cal) {

            string retval = "";
            sql.commandText = "CalendarEvent_AddEdit";
            sql.parameters.AddWithValue("@Id", cal.Id);
            sql.parameters.AddWithValue("@UserId", user.Id);
            sql.parameters.AddWithValue("@OrganisationId", user.OrganisationId);
            sql.parameters.AddWithValue("@Type", cal.Type);
            sql.parameters.AddWithValue("@StartTime", cal.StartDate);
            if (cal.EndDate != null) {
                sql.parameters.AddWithValue("@EndTime", cal.EndDate);
            }
            sql.parameters.AddWithValue("@Location", cal.Place);
            sql.parameters.AddWithValue("@Municipality", cal.Municipality);
            sql.parameters.AddWithValue("@Titel", cal.Title);
            sql.parameters.AddWithValue("@Description", cal.ShortDescription);
            sql.parameters.AddWithValue("@URL", cal.Link);
            sql.parameters.AddWithValue("@CompanyId", cal.CompanyId);
            sql.parameters.AddWithValue("@ContactId", cal.ContactId);
            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            if (dr.Read()) {
                retval = TypeCast.ToString(dr["Id"]);
            }
            dr.Close();
            sql.reset();

            return retval;
        }

        /// <summary>
        /// Get Event types
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public Dictionary<string, string> GetEventType() {
            var record = new Dictionary<string, string>();
            sql.commandText = "CalendarEventType_Get";
            using (SqlDataReader dr = sql.executeReader) {
                if (dr.HasRows) {
                    while (dr.Read()) {
                        record.Add(TypeCast.ToString(dr["Name"]), TypeCast.ToString(dr["Id"]));
                    }
                }
                if (!dr.IsClosed) { dr.Close(); dr.Dispose(); }
            }
            sql.reset();
            return record;
        }

        /// <summary>
        /// Get County and Region
        /// </summary>
        public List<DropdownData> CountyAndRegion(User user) {

            var records = new List<DropdownData>();
            sql.commandText = "CountyAndRegion_Get";
            sql.parameters.AddWithValue("@UserId", user.Id);
            sql.parameters.AddWithValue("@OrganisationId", user.OrganisationId);
            using (SqlDataReader dr = sql.executeReader) {
                if (dr.HasRows) {
                    while (dr.Read()) {
                        records.Add(new DropdownData() {
                            Id = TypeCast.ToString(dr["Id"]),
                            Name = TypeCast.ToString(dr["Name"]),
                            Group = TypeCast.ToString(dr["Group"]),
                        });
                    }
                }
                //dr.NextResult();
                //if (dr.HasRows) {
                //    while (dr.Read()) {
                //        region.Add(TypeCast.ToString(dr["Name"]), TypeCast.ToString(dr["Id"]));
                //    }
                //}
                if (!dr.IsClosed) { dr.Close(); dr.Dispose(); }
            }
            sql.reset();
            return records;
        }

        /// <summary>
        /// Get Event companies
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public Dictionary<string, string> GetCalendarEventCompanies(User user) {
            var record = new Dictionary<string, string>();
            sql.commandText = "CalendarEventCompanies_Get";
            sql.parameters.AddWithValue("@UserId", user.Id);
            sql.parameters.AddWithValue("@OrganisationId", user.OrganisationId);
            using (SqlDataReader dr = sql.executeReader) {
                if (dr.HasRows) {
                    while (dr.Read()) {
                        record.Add(TypeCast.ToString(dr["CompanyId"]), TypeCast.ToString(dr["CompanyName"]));
                    }
                }
                if (!dr.IsClosed) { dr.Close(); dr.Dispose(); }
            }
            sql.reset();
            return record;
        }

        /// <summary>
        /// Delete Contact Agreement
        /// </summary>
        public void CalendarEvent_Delete(User U, int id) {
            sql.commandText = "CalendarEvent_Delete";
            sql.parameters.AddWithValue("@Id", id);
            sql.parameters.AddWithValue("@OrganisationId", U.OrganisationId);
            sql.execute();
            sql.reset();
        }

        /// <summary>
        /// Logging for report generator                                                                                                                
        /// </summary>
        /// <param name="user"></param>
        /// <param name="logTable"></param>
        /// <param name="fieldname"></param>
        /// <param name="objectXml"></param>
        public void ReportGenerator_AddLogs(User user, string logTable, string fieldname, string objectXml) {
            sql.commandText = "ReportGenerator_AddLogs";
            sql.parameters.AddWithValue("@UserId", user.Id);
            sql.parameters.AddWithValue("@OrganisationId", user.OrganisationId);
            sql.parameters.AddWithValue("@Type", logTable);
            sql.parameters.AddWithValue("@FieldName", fieldname);
            sql.parameters.AddWithValue("@ObjectXml", objectXml);
            if (System.Web.HttpContext.Current != null && System.Web.HttpContext.Current.Request != null) {
                var ip = System.Web.HttpContext.Current.Request.UserHostAddress;
                sql.parameters.AddWithValue("@IPaddress", ip);
            }
            sql.execute();
            sql.reset();
        }

        /// <summary>
        /// Sharing of AVN Entity
        /// </summary>
        /// <param name="avnid"></param>
        /// <param name="orgid"></param>
        /// <param name="entityid"></param>
        /// <param name="type"></param>
        /// <param name="id"></param>
        /// <param name="accesstype"></param>
        /// <returns></returns>
        public Dictionary<int, string> AddAdminAVNEntityShare(int avnid, int orgid, string entityid, string type, int id, int accesstype) {
            var data = new Dictionary<int, string>();
            var sqlConnection = new SqlConnection(connectionString);
            using (var sqladp = new SqlDataAdapter("AdminAVNEntityShare", sqlConnection)) {
                sqladp.SelectCommand.Parameters.AddWithValue("@AVNId", avnid);
                if (orgid > 0) { sqladp.SelectCommand.Parameters.AddWithValue("@CreatorOrganisationId", orgid); }
                if (!string.IsNullOrEmpty(entityid)) { sqladp.SelectCommand.Parameters.AddWithValue("@AVNEntityList", entityid); }
                if (type.ToLower() == "organization") {
                    sqladp.SelectCommand.Parameters.AddWithValue("@OrganisationId", id);
                } else {
                    sqladp.SelectCommand.Parameters.AddWithValue("@UserGroupId", id);
                }
                sqladp.SelectCommand.Parameters.AddWithValue("@ReadWriteState", accesstype);

                sqladp.SelectCommand.CommandType = CommandType.StoredProcedure;
                if (sqlConnection.State == ConnectionState.Closed) { sqlConnection.Open(); }
                var ds = new DataSet();
                sqladp.Fill(ds);

                if (ds.Tables.Count > 0 && ds.Tables[0] != null) {
                    if (ds.Tables[0].Rows.Count > 0) {
                        foreach (DataRow row in ds.Tables[0].Rows) {
                            var status = Convert.ToInt32(row["Status"]);
                            var message = Convert.ToString(row["Message"]);
                            data.Add(status, message);
                        }
                    }
                }
            }
            return data;
        }

        /// <summary>
        /// Get Company And Contact List By Postivefield
        /// </summary>
        /// <returns></returns>
        public List<CompanyContactByPositiveField> GetCompanyAndContactListByPostivefield(User user, int fieldid, string query, string qtype) {

            var record = new List<CompanyContactByPositiveField>();
            sql.commandText = "CompanyAndContactListByPostivefield_Get";
            sql.parameters.AddWithValue("@UserId", user.Id);
            sql.parameters.AddWithValue("@OrganisationId", user.OrganisationId);
            sql.parameters.AddWithValue("@FieldId", fieldid);
            if (!string.IsNullOrEmpty(query)) { sql.parameters.AddWithValue("@Query", query); }
            if (!string.IsNullOrEmpty(qtype)) { sql.parameters.AddWithValue("@Qtype", TypeCast.ToInt(qtype)); }

            using (SqlDataReader dr = sql.executeReader) {
                if (dr.HasRows) {
                    while (dr.Read()) {
                        record.Add(new CompanyContactByPositiveField() {

                            // Company fields
                            CompanyId = TypeCast.ToInt(dr["companyid"]),
                            CompanyName = TypeCast.ToString(dr["z_companies_1_Firmanavn_1"]),
                            CompanyCVRNumber = TypeCast.ToString(dr["z_companies_1_CVR-nummer_1"]),
                            CompanyKommune = TypeCast.ToString(dr["z_companies_1_Kommune_1"]),
                            PositiveFieldValue = TypeCast.ToString(dr["PositiveFieldValue"]),

                            // Contact fields
                            ContactId = TypeCast.ToInt(dr["contactId"]),
                            ContactFirstname = TypeCast.ToString(dr["z_contacts_1_Fornavn_1"]),
                            ContactLastname = TypeCast.ToString(dr["z_contacts_1_Efternavn_1"]),
                            ContactEmail = TypeCast.ToString(dr["z_contacts_1_Email_1"]),

                            Flag = (TypeCast.ToInt(dr["Flag"]) == 0 ? false : true),
                        });
                    }
                }
                if (!dr.IsClosed) { dr.Close(); dr.Dispose(); }
            }
            sql.reset();
            return record;
        }

        /// <summary>
        /// Get Flag from Companyd and Contact Id
        /// </summary>
        /// <returns></returns>
        public Flag GetFlagByCompanyAndContact(int companyId, int contactId, int importantFieldId) {
            var flag = new Flag();
            var sqlConnection = new SqlConnection(connectionString);
            using (var sqladp = new SqlDataAdapter("FlagByCompanyAndContact_Get", sqlConnection)) {
                sqladp.SelectCommand.Parameters.AddWithValue("@CompanyId", companyId);
                sqladp.SelectCommand.Parameters.AddWithValue("@ContactId", contactId);
                sqladp.SelectCommand.Parameters.AddWithValue("@FieldId", importantFieldId);
                sqladp.SelectCommand.CommandType = CommandType.StoredProcedure;
                if (sqlConnection.State == ConnectionState.Closed) { sqlConnection.Open(); }
                var ds = new DataSet();
                sqladp.Fill(ds);
                if (ds.Tables[0] != null) {
                    if (ds.Tables[0].Rows.Count > 0) {
                        foreach (DataRow row in ds.Tables[0].Rows) {

                            flag.Id = TypeCast.ToString(row["Id"]);
                            flag.Type = TypeCast.ToInt(row["Type"]);
                            flag.Date = Convert.ToDateTime(row["Date"]);

                            flag.ObjectType = TypeCast.ToInt(row["ObjectType"]);
                            flag.ObjectIdInt = TypeCast.ToInt(row["ObjectIdInt"]);
                            flag.ObjectIdGuid = TypeCast.ToString(row["ObjectIdGuid"]);

                            flag.ContactId = TypeCast.ToInt(row["ContactId"]);

                            flag.CreatedDate = Convert.ToDateTime(row["CreatedDate"]);
                            flag.CreatedBy = TypeCast.ToString(row["CreatedBy"]);

                            flag.ApprovedDate = TypeCast.ToDateTimeLoose(row["ApprovedDate"]);
                            flag.ApprovedBy = TypeCast.ToString(row["ApprovedBy"]);

                            flag.FlagAttributes = TypeCast.ToString(row["FlagAttributes"]);

                            flag.ReasonType = TypeCast.ToInt(row["ReasonType"]);
                            flag.Reason = TypeCast.ToString(row["Reason"]);

                            flag.ImportantFieldId = TypeCast.ToInt(row["ImportantFieldId"]);
                        }
                    }
                }
            }
            return flag;
        }

        /// <summary>
        /// Get list of columns name for table using INFORMATION_SCHEMA.COLUMNS
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public List<string> TableColumns(string tableName) {
            var records = new List<string>();
            sql.commandText = "Select * from INFORMATION_SCHEMA.COLUMNS Where TABLE_NAME='" + tableName + "'";
            sql.commandType = CommandType.Text;
            using (SqlDataReader dr = sql.executeReader) {
                if (dr.HasRows) {
                    while (dr.Read()) {
                        records.Add(TypeCast.ToString(dr["COLUMN_NAME"]));
                    }
                }
                if (!dr.IsClosed) {
                    dr.Close();
                    dr.Dispose();
                }
            }
            sql.reset();
            return records;
        }

        /// <summary>
        /// Get API AVN Entities
        /// </summary>
        /// <returns></returns>
        public string API_GetAVNEntities(string key, int avnId, string ip) {
            var entitiesXml = new StringBuilder();
            entitiesXml.Append("<?xml version=\"1.0\"?>");
            entitiesXml.Append("<AVN xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">");

            var sqlConnection = new SqlConnection(connectionString);
            using (var sqladp = new SqlDataAdapter("API_AVNEntities_Get", sqlConnection)) {
                sqladp.SelectCommand.Parameters.AddWithValue("@UserId", key);
                sqladp.SelectCommand.Parameters.AddWithValue("@AVNTypeId", avnId);
                sqladp.SelectCommand.Parameters.AddWithValue("@IPaddress", ip);
                sqladp.SelectCommand.CommandType = CommandType.StoredProcedure;
                if (sqlConnection.State == ConnectionState.Closed) { sqlConnection.Open(); }
                var ds = new DataSet();
                sqladp.Fill(ds);

                var columns = new Columns();
                entitiesXml.Append("<Columns>");
                if (ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0) {
                    foreach (DataRow row in ds.Tables[0].Rows) {

                        columns.Fields.Add(new IDS.EBSTCRM.Base.Classes._AVNField() {
                            Name = TypeCast.ToString(row["Column"]).Replace("[", string.Empty).Replace("]", string.Empty),
                            FriendlyName = TypeCast.ToString(row["FriendlyColumnName"])
                        });

                        entitiesXml.Append("<Field>");
                        entitiesXml.Append(string.Format("<Name>{0}</Name>", SecurityElement.Escape(TypeCast.ToString(row["FriendlyColumnName"]))));
                        entitiesXml.Append(string.Format("<Datatype>{0}</Datatype>", SecurityElement.Escape(TypeCast.ToString(row["DataType"]))));
                        entitiesXml.Append("</Field>");
                    }
                }
                entitiesXml.Append("</Columns>");
                if (ds.Tables[1] != null && ds.Tables[1].Rows.Count > 0) {
                    foreach (DataRow row in ds.Tables[1].Rows) {
                        entitiesXml.Append("<Rows>");
                        foreach (var column in columns.Fields) {
                            entitiesXml.Append(string.Format("<{0}>{1}</{0}>", column.FriendlyName.Replace(" ", "_"), SecurityElement.Escape(TypeCast.ToString(row[column.Name]))));
                        }
                        entitiesXml.Append("</Rows>");
                    }
                }
            }
            entitiesXml.Append("</AVN>");

            // Timestamp: 
            sql.commandText = "UPDATE [Users] SET [API_AVNRead_TimeStamp]=getdate() WHERE LOWER(Id)='" + key + "'";
            sql.commandType = CommandType.Text;
            sql.execute();

            return entitiesXml.ToString();
        }

        /// <summary>
        /// Validate user key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool API_ValidateUserKey(string key) {
            sql.commandText = "UserKey_Validate";
            sql.parameters.AddWithValue("@key", key);
            using (System.Data.SqlClient.SqlDataReader dr = sql.executeReader) {
                if (dr.Read()) {
                    var count = TypeCast.ToInt(dr["c"]);
                    if (count >= 1) {
                        return true;
                    }
                }
                sql.reset();
            }
            return false;
        }

        /// <summary>
        /// Insert/Update AVN file
        /// </summary>
        /// <param name="user"></param>
        /// <param name="f"></param>
        /// <returns></returns>
        public int AddEditAVNFile(User user, AVNFile f) {

            int retval = 0;
            sql.commandText = "AVNFile_AddEdit";

            sql.parameters.AddWithValue("@userId", TypeCast.ToString(user.Id));
            sql.parameters.AddWithValue("@organisationId", TypeCast.ToInt(user.OrganisationId));
            sql.parameters.AddWithValue("@id", TypeCast.ToIntOrDBNull(f.id));
            if (TypeCast.ToInt(f.avnId) > 0) { sql.parameters.AddWithValue("@avnid", TypeCast.ToInt(f.avnId)); }
            sql.parameters.AddWithValue("@name", TypeCast.ToStringOrDBNull(f.name));
            sql.parameters.AddWithValue("@description", TypeCast.ToString(f.description));
            sql.parameters.AddWithValue("@filename", TypeCast.ToString(f.fileName));
            sql.parameters.AddWithValue("@contenttype", TypeCast.ToString(f.contentType));
            sql.parameters.AddWithValue("@filedata", f.fileData);
            sql.parameters.AddWithValue("@contentlength", TypeCast.ToInt(f.contentLength));
            sql.parameters.AddWithValue("@sort", TypeCast.ToInt(f.sort));

            using (SqlDataReader dr = sql.executeReader) {
                if (dr.Read()) { retval = TypeCast.ToInt(dr["status"]); }
                dr.Close();
                sql.reset();
            }

            Events_addToEventLog(user.OrganisationId, user.Id, f.id == retval ? "UPDATE" : "CREATE", "AVNFILES", retval.ToString(), f);
            return retval;
        }

        /// <summary>
        /// Insert/Update AVN file
        /// </summary>
        /// <param name="user"></param>
        /// <param name="f"></param>
        /// <returns></returns>
        public void UpdateAVNFileSortOrder(User user, int id, int sort) {
            sql.commandText = "AVNFile_AddEdit";
            sql.parameters.AddWithValue("@id", id);
            sql.parameters.AddWithValue("@sort", sort);
            sql.execute();
            sql.reset();
        }

        /// <summary>
        /// Get files for AVN
        /// </summary>
        /// <returns></returns>
        public List<AVNFile> GetAVNFiles(int avnid, bool includedocbytes = true) {
            var data = new List<AVNFile>();
            var sqlConnection = new SqlConnection(connectionString);
            using (var sqladp = new SqlDataAdapter("AVNFiles_Get", sqlConnection)) {
                sqladp.SelectCommand.Parameters.AddWithValue("@AVNId", avnid);
                sqladp.SelectCommand.CommandType = CommandType.StoredProcedure;
                if (sqlConnection.State == ConnectionState.Closed) { sqlConnection.Open(); }
                var ds = new DataSet();
                sqladp.Fill(ds);

                if (ds.Tables[0] != null) {
                    if (ds.Tables[0].Rows.Count > 0) {
                        foreach (DataRow row in ds.Tables[0].Rows) {
                            data.Add(new AVNFile() {
                                id = TypeCast.ToInt(row["id"]),
                                avnId = TypeCast.ToInt(row["avnId"]),
                                name = TypeCast.ToString(row["name"]),
                                description = TypeCast.ToString(row["description"]),
                                fileName = TypeCast.ToString(row["filename"]),
                                sort = TypeCast.ToInt(row["sort"]),
                                contentType = TypeCast.ToString(row["contenttype"]),
                                fileData = (includedocbytes ? (byte[])(row["filedata"]) : null),
                                userId = TypeCast.ToString(row["userId"]),
                                contentLength = TypeCast.ToInt(row["contentlength"]),
                                organisationId = TypeCast.ToInt(row["organisationId"]),
                                dateCreated = TypeCast.ToDateTime(row["dateCreated"]).ToString("dd-MM-yyyy HH:mm")
                            });
                        }
                    }
                }
            }
            return data;
        }

        /// <summary>
        /// Get AVN file detail by fileid
        /// </summary>
        /// <param name="id">file id</param>
        /// <returns></returns>
        public AVNFile GetAVNFile(int id) {
            var data = new AVNFile();
            var sqlConnection = new SqlConnection(connectionString);
            using (var sqladp = new SqlDataAdapter("AVNFile_Get", sqlConnection)) {
                sqladp.SelectCommand.Parameters.AddWithValue("@Id", id);
                sqladp.SelectCommand.CommandType = CommandType.StoredProcedure;
                if (sqlConnection.State == ConnectionState.Closed) { sqlConnection.Open(); }
                var ds = new DataSet();
                sqladp.Fill(ds);

                if (ds.Tables[0] != null) {
                    if (ds.Tables[0].Rows.Count > 0) {
                        foreach (DataRow row in ds.Tables[0].Rows) {
                            data.id = Convert.ToInt32(row["id"]);
                            data.avnId = Convert.ToInt32(row["avnid"]);
                            data.name = Convert.ToString(row["name"]);
                            data.description = Convert.ToString(row["description"]);
                            data.fileName = Convert.ToString(row["filename"]);
                            data.contentType = Convert.ToString(row["contenttype"]);
                            data.fileData = (byte[])(row["filedata"]);
                            data.contentLength = Convert.ToInt32(row["contentlength"]);
                        }
                    }
                }
            }
            return data;
        }

        /// <summary>
        /// Delete AVN file by fileid
        /// </summary>
        /// <param name="user">user detail</param>
        /// <param name="id">file id</param>
        public void DeleteAVNFile(User user, int id) {
            sql.commandText = "AVNFile_Delete";
            sql.parameters.AddWithValue("@userid", user.Id);
            sql.parameters.AddWithValue("@id", id);
            sql.execute();
            sql.reset();
        }

        /// <summary>
        /// Get AVN information
        /// </summary>
        /// <param name="user"></param>
        /// <param name="AvnId"></param>
        /// <param name="Id"></param>
        /// <param name="obfuscate"></param>
        /// <returns></returns>
        public DataTable GetAVNEntityById(User user, int AvnId, int Id, bool obfuscate = true) {
            var sqlConnection = new SqlConnection(connectionString);
            using (var sqladp = new SqlDataAdapter("AVN_Get", sqlConnection)) {
                sqladp.SelectCommand.Parameters.AddWithValue("@UserId", user.Id);
                sqladp.SelectCommand.Parameters.AddWithValue("@Id", Id);
                sqladp.SelectCommand.Parameters.AddWithValue("@AVNTypeId", AvnId);
                sqladp.SelectCommand.Parameters.AddWithValue("@OrganisationId", user.OrganisationId);
                sqladp.SelectCommand.Parameters.AddWithValue("@Obfuscate", obfuscate);
                if (System.Web.HttpContext.Current != null && System.Web.HttpContext.Current.Request != null) {
                    var ip = System.Web.HttpContext.Current.Request.UserHostAddress;
                    sqladp.SelectCommand.Parameters.AddWithValue("@IPaddress", ip);
                }

                sqladp.SelectCommand.CommandType = CommandType.StoredProcedure;
                if (sqlConnection.State == ConnectionState.Closed) { sqlConnection.Open(); }
                var ds = new DataSet();
                sqladp.Fill(ds);
                return ds.Tables[0];
            }
        }

        /// <summary>
        /// Returns the list of CVR
        /// </summary>
        /// <returns></returns>
        public List<SearchCVR> SearchCVR(string keyword = "", string pnummer = "", bool skip = false) {
            var data = new List<SearchCVR>();
            var sqlConnection = new SqlConnection(connectionString);
            //** Check om vi allerede har data, ESCRM-190/191
            if(HasAlreadyRun)
            {
                data = GemteData;
                //HasAlreadyRun = false;
                GemteData = null;
                return data;
            }

            using (var sqladp = new SqlDataAdapter("SearchCVR_Get", sqlConnection)) {

                if (!string.IsNullOrEmpty(keyword)) { sqladp.SelectCommand.Parameters.AddWithValue("@q", keyword); }
                if (!string.IsNullOrEmpty(pnummer)) { sqladp.SelectCommand.Parameters.AddWithValue("@id", pnummer); }

                sqladp.SelectCommand.CommandType = CommandType.StoredProcedure;
                if (sqlConnection.State == ConnectionState.Closed) { sqlConnection.Open(); }
                var ds = new DataSet();
                sqladp.Fill(ds);
                
                if (ds.Tables[0] != null) {
                    if (ds.Tables[0].Rows.Count > 0) {
                        foreach (DataRow row in ds.Tables[0].Rows) {
                            data.Add(new SearchCVR() {
                                ID = TypeCast.ToInt(row["Id"]),
                                CompanyName = TypeCast.ToString(row["CompanyName"]),
                                CVR = TypeCast.ToString(row["CVR"]),
                                PNummer = TypeCast.ToString(row["PNummer"]),
                                Street = TypeCast.ToString(row["Street"]),
                                Zipcode = TypeCast.ToString(row["Zipcode"]),
                                City = TypeCast.ToString(row["City"]),
                                Municipality = TypeCast.ToString(row["Municipality"]),
                                Region = TypeCast.ToString(row["Region"]),
                            });
                        }
                        HasAlreadyRun = true;
                        GemteData = data;
                    }
                }
            }
            return data;
        }

        /// <summary>
        /// Search for company and contact list
        /// </summary>
        /// <param name="O"></param>
        /// <param name="UserId"></param>
        /// <param name="query"></param>
        /// <param name="type"></param>
        /// <param name="SortOrder"></param>
        /// <param name="sortAsc"></param>
        /// <param name="dyfields"></param>
        /// <param name="dataClassfications"></param>
        /// <returns></returns>
        public List<Contact> Search(Organisation O, string UserId, string query, int type, string SortOrder, string sortAsc, List<DynamicField> dyfields, List<DataClassification> dataClassfications) {
            var data = new List<Contact>();
            sql.commandText = "Search";
            sql.parameters.AddWithValue("@organisationId", O.Id);
            sql.parameters.AddWithValue("@userId", UserId);
            if (type > -1) { sql.parameters.AddWithValue("@type", type); }
            sql.parameters.AddWithValue("@q", query);
            if (!string.IsNullOrEmpty(SortOrder)) { sql.parameters.AddWithValue("@SortOrder", SortOrder); }
            if (!string.IsNullOrEmpty(sortAsc)) { sql.parameters.AddWithValue("@sortAsc", sortAsc); }

            // ShowInList Fields
            string sqlColumns = "";
            var columns = new List<SQLColumnItem>();
            dyfields = dyfields.OrderBy(o => o.ListviewIndex).ToList();
            foreach (var f in dyfields) {
                if (!f.NoInherit(O.Id) || f.BaseOrganisationId == f.OrganisationId) {
                    if (TypeCast.ToInt(f.ShowInListview) > 0 && f.OrganisationId == O.Id) {
                        string col = "";
                        if (f.DataSource.IndexOf('_') > 0) {
                            col = f.DataSource + "_" + f.DataSource.Split('_')[2];
                        } else {
                            col = f.DatabaseTable + "_" + f.DatabaseColumn + "_" + f.BaseOrganisationId;
                        }

                        var classification = dataClassfications.FirstOrDefault(o => o.Id == f.DataClassificationId);
                        if (classification == null) {
                            sqlColumns += "[" + col + "],";
                        } else if (classification != null) {
                            if (string.IsNullOrEmpty(classification.ExportMask)) {
                                sqlColumns += "[" + col + "],";
                            } else {
                                sqlColumns += "dbo.Obfuscate([" + col + "],'" + classification.ExportMask + "',CONVERT(CHAR(1),1)) as [" + col + "],";
                            }
                        }
                    }
                }
            }
            if (!string.IsNullOrEmpty(sqlColumns)) {
                sqlColumns = sqlColumns.Substring(0, sqlColumns.Length - 1);
                sql.parameters.AddWithValue("@ShowInListColumns", sqlColumns);
            }

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                data.Add(new Contact(ref dr));
            }
            dr.Close();
            sql.reset();
            return data;
        }

        /// <summary>
        /// Return the list of email for user group 
        /// </summary>
        /// <param name="groupid">user group id</param>
        /// <returns></returns>
        public string GetUserGroupEmailList(int groupid) {
            var emailList = "";
            sql.commandText = "SELECT dbo.GetUserGroupEmailList(" + groupid + ") as EmailList";
            sql.commandType = System.Data.CommandType.Text;
            using (var dr = sql.executeReader) {
                if (dr.HasRows) {
                    dr.Read();
                    emailList = TypeCast.ToString(dr["EmailList"]);
                }
            }
            sql.reset();
            return emailList;
        }

        /// <summary>
        /// List of Api Access IP Address
        /// </summary>
        /// <param name="methodName">Api method name</param>
        /// <returns></returns>
        public string GetApiAccessIp(string methodName) {
            var ipAddress = string.Empty;
            sql.commandText = "APIAccessIP_Get";
            sql.parameters.AddWithValue("@MethodName", methodName);
            SqlDataReader dr = sql.executeReader;
            if (dr.Read()) { ipAddress = TypeCast.ToString(dr["Ip"]); }
            dr.Close();
            sql.reset();
            return ipAddress;
        }

        /// <summary>
        /// Delete meeting reporting
        /// </summary>
        /// <param name="user">user detail</param>
        /// <param name="id">file id</param>
        public void DeleteMeetingReporting(User user, int id) {
            sql.commandText = "MeetingReporting_Delete";
            sql.parameters.AddWithValue("@userid", user.Id);
            sql.parameters.AddWithValue("@id", id);
            sql.execute();
            sql.reset();
        }

        /// <summary>
        /// List of columns from the Company table for specific organisation id
        /// </summary>
        /// <param name="orgid">file id</param>
        public List<Field> CompanyFields(int orgid) {
            var data = new List<Field>();
            var sqlConnection = new SqlConnection(connectionString);
            using (var sqladp = new SqlDataAdapter("CompanyFields_Get", sqlConnection)) {
                sqladp.SelectCommand.Parameters.AddWithValue("@OrganisationId", orgid);
                sqladp.SelectCommand.CommandType = CommandType.StoredProcedure;
                if (sqlConnection.State == ConnectionState.Closed) { sqlConnection.Open(); }
                var ds = new DataSet();
                sqladp.Fill(ds);

                if (ds.Tables[0] != null) {
                    if (ds.Tables[0].Rows.Count > 0) {
                        foreach (DataRow row in ds.Tables[0].Rows) {
                            data.Add(new Field() {
                                RowID = TypeCast.ToInt(row["RowID"]),
                                ColumnName = "[" + TypeCast.ToString(row["Column"]) + "]",
                                DataType = TypeCast.ToString(row["DataType"]),
                                DataLength = TypeCast.ToString(row["DataLength"])
                            });
                        }
                    }
                }
            }
            return data;
        }

        /// <summary>
        /// List of columns from the Contact table for specific organisation id
        /// </summary>
        /// <param name="orgid">file id</param>
        public List<Field> ContactFields(int orgid, bool showall = false) {
            var data = new List<Field>();
            var sqlConnection = new SqlConnection(connectionString);
            using (var sqladp = new SqlDataAdapter("ContactFields_Get", sqlConnection)) {
                sqladp.SelectCommand.Parameters.AddWithValue("@OrganisationId", orgid);
                sqladp.SelectCommand.Parameters.AddWithValue("@ShowAllColumns", showall);
                sqladp.SelectCommand.CommandType = CommandType.StoredProcedure;
                if (sqlConnection.State == ConnectionState.Closed) { sqlConnection.Open(); }
                var ds = new DataSet();
                sqladp.Fill(ds);

                if (ds.Tables[0] != null) {
                    if (ds.Tables[0].Rows.Count > 0) {
                        foreach (DataRow row in ds.Tables[0].Rows) {
                            data.Add(new Field() {
                                RowID = TypeCast.ToInt(row["RowID"]),
                                ColumnName = "[" + TypeCast.ToString(row["Column"]) + "]",
                                DataType = TypeCast.ToString(row["DataType"]),
                                DataLength = TypeCast.ToString(row["DataLength"])
                            });
                        }
                    }
                }
            }
            return data;
        }

        public List<User> GetUsersByRoles(User user, string roles) {
            sql.commandText = "UsersByRoles_Get";
            sql.parameters.AddWithValue("@Roles", roles);
            sql.parameters.AddWithValue("@organisationId", user.OrganisationId);
            var users = new List<User>();
            using (SqlDataReader dr = sql.executeReader) {
                while (dr.Read()) {
                    users.Add(new User() {
                        Id = TypeCast.ToString(dr["Id"]),
                        Firstname = TypeCast.ToString(dr["Firstname"]),
                        Lastname = TypeCast.ToString(dr["Lastname"]),
                        Username = TypeCast.ToString(dr["Username"]),
                        Email = TypeCast.ToString(dr["Email"]),
                    });
                }
            }
            sql.reset();
            return users;
        }

        /// <summary>
        /// Get All Calendar Events
        /// </summary>
        /// <returns></returns>
        public List<HourlyTimeUsage> GetHourlyTimesheet(User user, int status, string ownedby, string empuserid = null,
                                                        int? week = null, int? year = null,
                                                        int? secondaryprojecttypeid = null) {

            var record = new List<HourlyTimeUsage>();
            sql.commandText = "HourlyTimesheet_Get";

            sql.parameters.AddWithValue("@organisationId", user.OrganisationId);
            sql.parameters.AddWithValue("@userid", user.Id);
            sql.parameters.AddWithValue("@status", status);

            if (!string.IsNullOrEmpty(empuserid)) {
                sql.parameters.AddWithValue("@empuserid", empuserid);
            }
            sql.parameters.AddWithValue("@week", week);
            sql.parameters.AddWithValue("@year", year);
            if (secondaryprojecttypeid.GetValueOrDefault(0) > 0) {
                sql.parameters.AddWithValue("@secondaryprojecttypeid", secondaryprojecttypeid);
            }
            sql.parameters.AddWithValue("@ownedby", ownedby);

            using (SqlDataReader dr = sql.executeReader) {
                if (dr.HasRows) {
                    while (dr.Read()) {
                        record.Add(new HourlyTimeUsage() {

                            Id = TypeCast.ToInt(dr["Id"]),
                            WeekDate = TypeCast.ToDateTime(dr["WeekDate"]),

                            SecondaryProjectTypeId = TypeCast.ToInt(dr["SecondaryProjectTypeId"]),
                            SecondaryProjectTypeName = TypeCast.ToString(dr["SecondaryProjectTypeName"]),

                            Varenavn = TypeCast.ToString(dr["Varenavn"]),
                            //TimeSpent = TypeCast.ToInt(dr["TimeSpent"]),
                            //** Har ændret det til decimal, ESCRM-155/156
                            TimeSpent = TypeCast.ToDecimal(dr["TimeSpent"]),
                            Description = TypeCast.ToString(dr["Description"]),
                            Kommentar = TypeCast.ToString(dr["RejectedDescription"]),
                            OriginalDescription = TypeCast.ToString(dr["OriginalDescription"]),

                            Status = TypeCast.ToString(dr["Status"]),
                            Employee = TypeCast.ToString(dr["Employee"]),
                        });
                    }
                }
                if (!dr.IsClosed) { dr.Close(); dr.Dispose(); }
            }
            sql.reset();
            return record;
        }

        public List<ProjectTypeSecondary> GetSecondaryProjectTypesByOrgId(User user) {
            var projects = new List<ProjectTypeSecondary>();
            sql.commandText = "ProjectTypes_Secondary_get";
            sql.parameters.AddWithValue("@userid", user.Id);
            sql.parameters.AddWithValue("@organisationId", user.OrganisationId);

            System.Data.SqlClient.SqlDataReader dr = sql.executeReader;
            while (dr.Read()) {
                projects.Add(new ProjectTypeSecondary(ref dr));
            }
            dr.Close();
            sql.reset();

            return projects;
        }

        public List<HourlyEmployee> GetEmployeesByControllerAndLeader(User user, int? sptypeid = null) {
            var employees = new List<HourlyEmployee>();
            sql.commandText = "EmployeesByControllerAndLeader_Get";
            sql.parameters.AddWithValue("@userid", user.Id);
            sql.parameters.AddWithValue("@organisationId", user.OrganisationId);
            if (sptypeid != null && sptypeid.GetValueOrDefault() > 0) {
                sql.parameters.AddWithValue("@secondaryprojecttypeid", sptypeid);
            }
            using (SqlDataReader dr = sql.executeReader) {
                while (dr.Read()) {
                    employees.Add(new HourlyEmployee() {
                        Id = TypeCast.ToString(dr["id"]),
                        Name = TypeCast.ToString(dr["name"]),
                        OwnedBy = TypeCast.ToString(dr["ownedby"])
                    });
                }
                dr.Close();
                sql.reset();
            }
            return employees;
        }

        public List<HourlyTimeUsage> HourTimesheetStatus_Update(User user, string ids, int status, string role, string statusmessage) {

            var record = new List<HourlyTimeUsage>();
            sql.commandText = "HourlyTimesheetStatus_Update";
            sql.parameters.AddWithValue("@userid", user.Id);
            sql.parameters.AddWithValue("@organisationId", user.OrganisationId);
            sql.parameters.AddWithValue("@ids", ids);
            sql.parameters.AddWithValue("@status", status);
            sql.parameters.AddWithValue("@role", role);
            if (!string.IsNullOrEmpty(statusmessage)) {
                sql.parameters.AddWithValue("@message", statusmessage);
            }
            using (SqlDataReader dr = sql.executeReader) {
                if (dr.HasRows) {
                    while (dr.Read()) {
                        record.Add(new HourlyTimeUsage() {
                            WeekDate = TypeCast.ToDateTime(dr["WeekDate"]),
                            TimeSpent = TypeCast.ToInt(dr["TimeSpent"]),
                            Kommentar = TypeCast.ToString(dr["Kommentar"]),
                            Employee = TypeCast.ToString(dr["EmployeeName"]),
                            Email = TypeCast.ToString(dr["Email"]),
                        });
                    }
                }
                if (!dr.IsClosed) { dr.Close(); dr.Dispose(); }
            }
            sql.reset();
            return record;
        }

        /// <summary>
        /// This function will return the available weeks for hourly timesheet
        /// </summary>
        /// <param name="user"></param>
        /// <param name="status"></param>
        /// <param name="ownedby"></param>
        /// <param name="secondaryprojecttypeid"></param>
        /// <returns></returns>
        public List<string> GetHourlyWeeks(User user, int status, string ownedby, int? secondaryprojecttypeid = null) {
            var availableWeeks = new List<string>();
            sql.commandText = "HourlyWeeks_Get";
            sql.parameters.AddWithValue("@organisationId", user.OrganisationId);
            sql.parameters.AddWithValue("@userid", user.Id);
            sql.parameters.AddWithValue("@status", status);
            if (secondaryprojecttypeid.GetValueOrDefault(0) > 0) { sql.parameters.AddWithValue("@secondaryprojecttypeid", secondaryprojecttypeid); }
            sql.parameters.AddWithValue("@ownedby", ownedby);
            using (SqlDataReader dr = sql.executeReader) {
                while (dr.Read()) {
                    availableWeeks.Add(TypeCast.ToString(dr["availableweek"]));
                }
                dr.Close();
                sql.reset();
            }
            return availableWeeks;
        }

        #region DUMMY

        public void HOUSTON_RaiseError() {
            sql.commandText = "HOUSTON_RaiseError";
            try {
                sql.execute();
            } catch (SqlException sqle) { }
            sql.reset();
        }

        #endregion
    }
}