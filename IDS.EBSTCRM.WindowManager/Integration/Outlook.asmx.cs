using System;
using System.Data;
using System.Web;
using System.Collections;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.ComponentModel;
using IDS.EBSTCRM.Base;

namespace IDS.EBSTCRM.WindowManager.Integration
{
    /// <summary>
    /// Webservice for outlook integration, allowing storing emails as documents on SMV/POT contacts
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [ToolboxItem(false)]
    public class Outlook : System.Web.Services.WebService
    {
        /// <summary>
        /// Sharing levels
        /// </summary>
        public enum FileSharingLevel
        {
            All = 0,
            MyOrganisation = 1,
            OnlMe = 2
        }

        /// <summary>
        /// Logs user in
        /// </summary>
        /// <param name="Username"></param>
        /// <param name="Password"></param>
        /// <returns></returns>
        [WebMethod]
        public string Login(string Username, string Password)
        {
            SQLDB sql = new SQLDB();

            User u = sql.Users_Login(Username, Password);

            sql.Dispose();
            sql = null;

            if (u != null)
                return u.Id;
            else
                return null;
        }


        [WebMethod]
        public OutlookUserInfo GetUserInfo(string Id)
        {
            SQLDB sql = new SQLDB();

            User u = sql.Organisations_getUser(Id);

            sql.Dispose();
            sql = null;

            if (u != null)
                return new OutlookUserInfo(u);
            else
                return null;
        }

        /// <summary>
        /// Search for contacts having a specific emailaddress
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="email"></param>
        /// <returns></returns>
        [WebMethod]
        public Outlook_SMVPOTContact[] GetContactsMatchingEmail(string userId, string email)
        {
            SQLDB sql = new SQLDB();

            User u = sql.Organisations_getUser(userId);

            if (u == null)
                throw new Exception("Brugeren findes ikke i CRM Systemet!");

            sql.Dispose();
            sql = null;

            return Outlook_FindContactsFromEmail(userId, email).ToArray();
        }

        /// <summary>
        /// Save emailmessage to early warning contact
        /// </summary>
        /// <param name="UserId"></param>
        /// <param name="ContactId"></param>
        /// <param name="Subject"></param>
        /// <param name="Data"></param>
        [WebMethod]
        public void SaveEmailMessageToEarlyWarningContact(string UserId, int ContactId, string Subject, byte[] Data)
        {
            SQLDB sql = new SQLDB();

            User u = sql.Organisations_getUser(UserId);

            if (u == null)
                throw new Exception("Brugeren findes ikke i CRM Systemet!");

            if(!u.EarlyWarningUser)
                throw new Exception("Brugeren har ikke adgang til Early Warning!");

            sql.EarlyWarning_Companies_UploadFile(ContactId, Subject + ".msg", Data, UserId);

            sql.Dispose();
            sql = null;
        }

        /// <summary>
        /// Save emailmessage to a specific SMV/POT contact
        /// </summary>
        /// <param name="UserId"></param>
        /// <param name="ContactId"></param>
        /// <param name="Subject"></param>
        /// <param name="Data"></param>
        [WebMethod]
        public void SaveEmailMessageToContact(string UserId, int ContactId, string Subject, byte[] Data)
        {
            SQLDB sql = new SQLDB();

            User u = sql.Organisations_getUser(UserId);

            if (u == null)
                throw new Exception("Brugeren findes ikke i CRM Systemet!");

            IDS.EBSTCRM.Base.File F = new IDS.EBSTCRM.Base.File();
            IDS.EBSTCRM.Base.Binary B = new IDS.EBSTCRM.Base.Binary();

            //Save binary data
            B.Data = Data;
            B.Id = sql.filearchive_insertBinary(B);

            //Set file
            F.binary = B.Id;
            F.filename = Subject + ".msg";
            F.contenttype = "Outlook/Email";
            F.description = null;
            F.organisationId = u.OrganisationId;
            F.contactId = ContactId;

            F.contentgroup = "";
            F.contentlength = Data.Length;
            F.folderType = 1;
            F.userId = u.Id;

            F.Id = sql.filearchive_insertFile(u, F);
            sql.Dispose();
            sql = null;
        }

        /// <summary>
        /// Save emailmessage to a specific SMV/POT contact with a sharing level set
        /// </summary>
        /// <param name="UserId"></param>
        /// <param name="ContactId"></param>
        /// <param name="Subject"></param>
        /// <param name="SharingLevel"></param>
        /// <param name="SharedWithOrganisations"></param>
        /// <param name="Data"></param>
        [WebMethod]
        public void SaveEmailMessageToContactWithSharingLevel(string UserId, int ContactId, string Subject, FileSharingLevel SharingLevel, int[] SharedWithOrganisations, byte[] Data)
        {
            SQLDB sql = new SQLDB();

            User u = sql.Organisations_getUser(UserId);

            if (u == null)
                throw new Exception("Brugeren findes ikke i CRM Systemet!");

            IDS.EBSTCRM.Base.File F = new IDS.EBSTCRM.Base.File();
            IDS.EBSTCRM.Base.Binary B = new IDS.EBSTCRM.Base.Binary();

            //Save binary data
            B.Data = Data;
            B.Id = sql.filearchive_insertBinary(B);

            //Set file
            F.binary = B.Id;
            F.filename = Subject + ".msg";
            F.contenttype = "Outlook/Email";
            F.description = null;
            F.organisationId = u.OrganisationId;
            F.contactId = ContactId;

            F.contentgroup = "";
            F.contentlength = Data.Length;

            //Reverse the folder type (bug from WebService)
            if (SharingLevel == FileSharingLevel.MyOrganisation)
                F.folderType = 1;
            else if (SharingLevel == FileSharingLevel.OnlMe)
                F.folderType = 2;
            else
                F.folderType = 0;


                

            F.userId = u.Id;

            F.Id = sql.filearchive_insertFile(u, F);

            if (TypeCast.ToInt(F.folderType) == 0)
            {
                sql.filearchive_updateFileShare(F.Id, u.OrganisationId);
                if (SharedWithOrganisations != null)
                {
                    bool sharedWithOwnOrganisation = false;

                    foreach (int orgId in SharedWithOrganisations)
                    {
                        if (orgId == u.OrganisationId)
                            sharedWithOwnOrganisation = true;
                        sql.filearchive_updateFileShare(F.Id, orgId);
                    }

                    if (!sharedWithOwnOrganisation)
                        sql.filearchive_updateFileShare(F.Id, u.OrganisationId);

                }
            }

            sql.Dispose();
            sql = null;
        }

        /// <summary>
        /// Get all available organisations for a specific user
        /// </summary>
        /// <param name="UserId"></param>
        /// <returns></returns>
        [WebMethod]
        public System.Collections.Generic.List<Outlook_Organisation> GetOrganisations(string UserId)
        {

            System.Collections.Generic.List<Outlook_Organisation> retval = new System.Collections.Generic.List<Outlook_Organisation>();

            SQLDB sql = new SQLDB();

            User u = sql.Organisations_getUser(UserId);

            if (u == null)
                throw new Exception("Brugeren findes ikke i CRM Systemet!");

            foreach(Organisation o in sql.Organisations_getOrganisations(1))
            {
                Outlook_Organisation oo = new Outlook_Organisation(o.Id, TypeCast.ToInt(o.ParentId), o.Name);
                
                System.Collections.Generic.List<Outlook_Organisation> retval2 = new System.Collections.Generic.List<Outlook_Organisation>();

                foreach (Organisation o2 in sql.Organisations_getOrganisations(o.Id))
                {
                    retval2.Add(new Outlook_Organisation(o2.Id, TypeCast.ToInt(o2.ParentId), o2.Name));
                }

                if(retval2.Count>0)
                    oo.Children = retval2.ToArray();

                retval.Add(oo);
            }

            sql.Dispose();
            sql = null;

            return retval;
        }




        #region Specialized SQL Queries

        /// <summary>
        /// Searches for all contacts having a specific email address
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        private System.Collections.Generic.List<Outlook_SMVPOTContact> Outlook_FindContactsFromEmail(string UserId, string email)
        {
            System.Collections.Generic.List<Outlook_SMVPOTContact> retval = new System.Collections.Generic.List<Outlook_SMVPOTContact>();

            SQLBase sql = new SQLBase(System.Configuration.ConfigurationManager.AppSettings["connectionString"]);
            SQLDB sql2 = new SQLDB();
            IDS.EBSTCRM.Base.User u = sql2.Organisations_getUser(UserId);
            sql2.Dispose();
            sql2 = null;
            System.Data.SqlClient.SqlDataReader dr = null;


            //Dont search within ew, unless the user has access
            if (u.EarlyWarningUser)
            {
                sql.commandText = "EarlyWarning_Companies_getContactsFromEmail";
                sql.parameters.AddWithValue("@email", email);

                dr = sql.executeReader;
                while (dr.Read())
                {
                    retval.Add(new Outlook_SMVPOTContact(ref dr, true));
                }
                dr.Close();
                sql.reset();
            }



            sql.commandText = "Outlook2CRMSearchForContact";

            string[] qs = email.Split(' ');
            int c = 1;
            foreach (string s in qs)
            {
                string val = s;
                if (val != "" && val != " ")
                {
                    if (c <= 10) sql.parameters.AddWithValue("@q" + c, val.Replace("*", "%").Replace("+", " "));
                    c++;
                }
            }
            
            sql.parameters.AddWithValue("@organisationId", "1");
            dr = sql.executeReader;
            while (dr.Read())
            {
                retval.Add(new Outlook_SMVPOTContact(ref dr, false));
            }
            dr.Close();
            sql.reset();

            sql.Dispose();
            sql = null;

            return retval;
        }

        #endregion
    }
}
