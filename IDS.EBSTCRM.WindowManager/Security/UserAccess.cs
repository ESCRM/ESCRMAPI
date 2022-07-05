using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using IDS.EBSTCRM.Base;
using System.Collections.Generic;
using IDS.EBSTCRM.Base.Classes;
using System.IO;

namespace IDS.EBSTCRM.WindowManager.Security {
    /// <summary>
    /// Automated User Access Control
    /// </summary>
    public static class UserAccess {
        /// <summary>
        /// Verify the has is granted access to the object (s)he is attempting to view
        /// </summary>
        /// <param name="U">Current User logged in</param>
        /// <param name="sender">Page Request in question</param>
        public static void ValidateUserAccess(IDS.EBSTCRM.Base.User U, object sender) {
            #region General Rights
            if (sender is IDS.EBSTCRM.WindowManager.Security.PageUserRight) {
                if (U != null) {
                    if ((int)U.UserRole < (int)((IDS.EBSTCRM.WindowManager.Security.PageUserRight)sender).MinimumUserRight)
                        HttpContext.Current.Response.Redirect("AccessDenied.aspx");
                } else
                    HttpContext.Current.Response.Redirect("AccessDenied.aspx");
            }
            #endregion

            //Test if page requires Early Warning
            #region Early Warning Required
            if (sender is IDS.EBSTCRM.WindowManager.Security.EarlyWarningUserRight) {
                if (U != null) {
                    if (!U.EarlyWarningUser && ((IDS.EBSTCRM.WindowManager.Security.EarlyWarningUserRight)sender).RequiresEarlyWarning && U.UserRole != UserRoles.SystemOwner)
                        HttpContext.Current.Response.Redirect("AccessDenied.aspx");
                } else
                    HttpContext.Current.Response.Redirect("AccessDenied.aspx");
            }
            #endregion

            //Test if page requires Login as Another User
            #region Impersonation
            if (sender is IDS.EBSTCRM.WindowManager.Security.ImpersonationRight) {
                if (U != null) {
                    if (!U.AllowImpersonation && ((IDS.EBSTCRM.WindowManager.Security.ImpersonationRight)sender).RequiresImpersonation && U.UserRole < UserRoles.GlobalAdministrator)
                        HttpContext.Current.Response.Redirect("AccessDenied.aspx");
                } else
                    HttpContext.Current.Response.Redirect("AccessDenied.aspx");
            }
            #endregion

            //Test if page requires Mass Registration
            #region Mass Registration
            if (sender is IDS.EBSTCRM.WindowManager.Security.MassRegistrationRight) {
                if (U != null) {
                    if (!U.AllowMassRegistrations && ((IDS.EBSTCRM.WindowManager.Security.ImpersonationRight)sender).RequiresImpersonation)
                        HttpContext.Current.Response.Redirect("AccessDenied.aspx");
                } else
                    HttpContext.Current.Response.Redirect("AccessDenied.aspx");
            }
            #endregion

            //Test if page requires Can Import Data
            #region Can Import Data
            if (sender is IDS.EBSTCRM.WindowManager.Security.CanImportDataRight) {
                if (U != null) {
                    if (!U.AllowImportData && ((IDS.EBSTCRM.WindowManager.Security.CanImportDataRight)sender).CanImportData)
                        HttpContext.Current.Response.Redirect("AccessDenied.aspx");
                } else
                    HttpContext.Current.Response.Redirect("AccessDenied.aspx");
            }
            #endregion

            //Access is denied, if the user is null
            if (U == null)
                HttpContext.Current.Response.Redirect("AccessDenied.aspx");
        }

        /// <summary>
        /// Gets the currenty user logged in within the active domain
        /// </summary>
        /// <returns>User object or null</returns>
        public static IDS.EBSTCRM.Base.User GetCurrentUser() {
            string sessionId = "user";

            //** For at ikke få fejlen. SKAL SLETTES IGEN
            if (HttpContext.Current == null)
                return null;

            string url = HttpContext.Current.Request.Url.ToString();
            url = url.Substring(url.IndexOf("//") + 2);
            if (url.IndexOf("/") > -1)
                url = url.Substring(0, url.IndexOf("/"));

            string[] up = url.Split('.');
            if (up[0].Length == 36) {
                sessionId = "user_" + up[0];
            }

            //HttpContext.Current.Response.Write("<h1>GetUserSession:" + sessionId + " = ");
            //HttpContext.Current.Response.Write((HttpContext.Current.Session[sessionId] != null ? "YES" :"NO") + "</h1>");

            if (HttpContext.Current.Session != null && HttpContext.Current.Session[sessionId] != null) {
                return (User)HttpContext.Current.Session[sessionId];
            } else {
                return null;
            }
        }

        /// <summary>
        /// Set the current user logged in
        /// </summary>
        /// <param name="U">User logged in</param>
        public static void SetCurrentUser(IDS.EBSTCRM.Base.User U) {
            string sessionId = "user";

            string url = HttpContext.Current.Request.Url.ToString();
            url = url.Substring(url.IndexOf("//") + 2);
            if (url.IndexOf("/") > -1)
                url = url.Substring(0, url.IndexOf("/"));

            string[] up = url.Split('.');
            if (up[0].Length == 36) {
                sessionId = "user_" + up[0];
            }

            //Keep browser data, if not applied with new user object
            IDS.EBSTCRM.Base.User current = GetCurrentUser();
            if (current != null && U != null && current.BrowserData != null && current.BrowserData != "" && (U.BrowserData == null || U.BrowserData == "")) {
                U.BrowserData = current.BrowserData;
            }

            HttpContext.Current.Session.Timeout = 60;
            HttpContext.Current.Session.Remove(sessionId);
            HttpContext.Current.Session.Add(sessionId, U);

            //HttpContext.Current.Response.Write("<h1>SetUserSession:" + sessionId + " = ");
            //HttpContext.Current.Response.Write((HttpContext.Current.Session[sessionId] != null ? "YES" : "NO") + "</h1>");
        }

        /// <summary>
        /// Gets or sets the current user logged in
        /// </summary>
        public static IDS.EBSTCRM.Base.User CurrentUser {
            get {
                return GetCurrentUser();
            }
            set {

                SetCurrentUser(value);
            }
        }

    }

    /// <summary>
    /// Interface to implement on pages, where rights are requied.
    /// </summary>
    interface PageUserRight {
        IDS.EBSTCRM.Base.UserRoles MinimumUserRight { get; }
    }

    /// <summary>
    /// Interface to implement on pages, where rights are requied.
    /// </summary>
    interface EarlyWarningUserRight {
        bool RequiresEarlyWarning { get; }
    }

    /// <summary>
    /// Interface to implement on pages, where rights are required.
    /// </summary>
    interface ImpersonationRight {
        bool RequiresImpersonation { get; }
    }

    /// <summary>
    /// Interface to implement on pages, where rights are required.
    /// </summary>
    interface MassRegistrationRight {
        bool MassRegistration { get; }
    }

    /// <summary>
    /// Interface to implement on pages, where rights are required.
    /// </summary>
    interface CanImportDataRight {
        bool CanImportData { get; }
    }
}