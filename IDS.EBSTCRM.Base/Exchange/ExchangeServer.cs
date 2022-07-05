using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using Independentsoft;
using Independentsoft.Webdav.Exchange;
using Independentsoft.Webdav.Exchange.ContentClass;
using Independentsoft.Webdav.Exchange.Properties;
using Independentsoft.Webdav.Exchange.Sql;
using System.Security.Cryptography.X509Certificates;

namespace IDS.EBSTCRM.Base
{
    /// <summary>
    /// Wrapper class to handle Exchange Server Connections
    /// Uses IndependenSoft API
    /// </summary>
    public class ExchangeServer
    {
        #region Declarations

        NetworkCredential credential;
        WebdavSession session;
        public Resource ExchangeResource;
        public Independentsoft.Exchange.Service Exchange2010Service;
        public User User;
        public bool IsOwnerOfCalendar;
        public bool IsExchange2010 = false;

        #endregion

        #region Test and Version Control
        
        /// <summary>
        /// Test connection to Exchange server
        /// </summary>
        /// <param name="url"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="domain"></param>
        /// <param name="useNT"></param>
        /// <returns>True if connection was successful</returns>
        public static bool TestConnection(string url, string username, string password, string domain, bool useNT)
        {
            //Check if the url, matches Exchange 2010 webservice type, else use standard Exchange 2007 connection model
            if (url.ToLower().EndsWith("?wdsl") || url.ToLower().EndsWith(".asmx") || url.ToLower().EndsWith(".wsdl"))
            {
                return _TestConnection2010(url, username, password, domain);
            }
            else
            {
                return _TestConnection(url, username, password, domain, useNT);
            }
        }

        /// <summary>
        /// Test for Exchange 2010 connection
        /// </summary>
        /// <param name="url"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="domain"></param>
        /// <returns>returns true, when successfull</returns>
        private static bool _TestConnection2010(string url, string username, string password, string domain)
        {
            //Create credentials for Exchange Server
            NetworkCredential credential = new NetworkCredential(username, password, domain);

            //Create service
            Independentsoft.Exchange.Service service = new Independentsoft.Exchange.Service(url, credential);

            //Attempt to read todays appointments as a connection test.
            try
            {
                Independentsoft.Exchange.IsGreaterThanOrEqualTo restriction1 = new Independentsoft.Exchange.IsGreaterThanOrEqualTo(Independentsoft.Exchange.AppointmentPropertyPath.StartTime, DateTime.Today);
                Independentsoft.Exchange.IsLessThanOrEqualTo restriction2 = new Independentsoft.Exchange.IsLessThanOrEqualTo(Independentsoft.Exchange.AppointmentPropertyPath.EndTime, DateTime.Today.AddDays(1));
                Independentsoft.Exchange.And restriction3 = new Independentsoft.Exchange.And(restriction1, restriction2);

                Independentsoft.Exchange.FindItemResponse response = service.FindItem(Independentsoft.Exchange.StandardFolder.Calendar, Independentsoft.Exchange.AppointmentPropertyPath.AllPropertyPaths, restriction3);

                for (int i = 0; i < response.Items.Count; i++)
                {
                    if (response.Items[i] is Independentsoft.Exchange.Appointment)
                    {
                        Independentsoft.Exchange.Appointment appointment = (Independentsoft.Exchange.Appointment)response.Items[i];
                    }
                }

                return true;
            }
            catch (Exception exp)
            {
                return false;
            }
            finally
            {
                service = null;
            }

            return false;
        }

        /// <summary>
        /// Test for Exchange 2007 connection
        /// </summary>
        /// <param name="url"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="domain"></param>
        /// <param name="useNT"></param>
        /// <returns>returns true, when successfull</returns>
        private static bool _TestConnection(string url, string username, string password, string domain, bool useNT)
        {
            try
            {
                NetworkCredential cred;
                WebdavSession sess;
                Resource res;

                //Create Exchange Credentials
                if (domain != null && domain != "")
                    cred = new NetworkCredential(username, password, domain);
                else
                    cred = new NetworkCredential(username, password);

                sess = new WebdavSession(url, cred);

                //Ignore SSL Certificate warnings
                ServicePointManager.CertificatePolicy = new AcceptAllCertificatePolicy();
                res = new Resource(sess);

                //Attempt to login
                if (!useNT) res.PerformFormsBasedAuthentication(url);

                //Properties to select
                PropertyName[] propertyName = new PropertyName[8];
                propertyName[0] = AppointmentProperty.ContentClass;
                propertyName[1] = AppointmentProperty.StartDate;
                propertyName[2] = AppointmentProperty.EndDate;
                propertyName[3] = AppointmentProperty.Subject;
                propertyName[4] = AppointmentProperty.Body;
                propertyName[5] = AppointmentProperty.Location;
                propertyName[6] = AppointmentProperty.RecurrenceID;
                propertyName[7] = AppointmentProperty.HRef;

                //Build select, to extract todays appointments as at test
                Select select = new Select(propertyName);
                From from = new From(res.Mailbox.Calendar);
                Where where = new Where();
                OrderBy orderby = new OrderBy();

                Condition condition1 = new Condition(AppointmentProperty.ContentClass, Operator.Equals, ContentClassType.Appointment);
                Condition condition2 = new Condition(AppointmentProperty.StartDate, Operator.GreatThenOrEquals, DateTime.Now);
                Condition condition3 = new Condition(AppointmentProperty.EndDate, Operator.LessThen, DateTime.Now.AddDays(1));

                where.Add(condition1);
                where.Add(LogicalOperator.AND);
                where.Add(condition2);
                where.Add(LogicalOperator.AND);
                where.Add(condition3);

                orderby.Add(AppointmentProperty.StartDate, Order.ASC);

                SqlQuery sqlQuery = new SqlQuery(select, from, where, orderby);
                MultiStatus multiStatus = res.Search(sqlQuery);

                //Perform search
                SearchResult searchResult = new SearchResult(multiStatus, propertyName);

                //Return success
                return true;
            }
            catch
            {
                //This is bad
                return false;
            }
        }

        #endregion

        #region Constructor

        //Create a new Exchange Server class
        public ExchangeServer(User U, bool isOwnerOfCalendar)
        {
            this.User = U;
            //TODO: Remove this, when approved by LLR
            this.IsOwnerOfCalendar = true; // isOwnerOfCalendar;

            //Create credentials
            if (U.ExchangeDomain != null && U.ExchangeDomain != "")
                credential = new NetworkCredential(U.ExchangeUsername, U.ExchangePassword, U.ExchangeDomain);
            else
                credential = new NetworkCredential(U.ExchangeUsername, U.ExchangePassword);

            //Decide which codebase to use - Exchange 2010 or 2007 / 2003
            if (U.ExchangeURL.ToLower().EndsWith("?wdsl") || U.ExchangeURL.ToLower().EndsWith(".asmx") || U.ExchangeURL.ToLower().EndsWith(".wsdl"))
            {
                this.IsExchange2010 = true;
                Exchange2010Service = new Independentsoft.Exchange.Service(U.ExchangeURL, credential);
            }
            else
            {
                this.IsExchange2010 = false;
                session = new WebdavSession(U.ExchangeURL, credential);

                ServicePointManager.CertificatePolicy = new AcceptAllCertificatePolicy();
                ExchangeResource = new Resource(session);
                if (U.ExchangeFormbasedLogin == true)
                    ExchangeResource.PerformFormsBasedAuthentication(U.ExchangeURL);
            }

        }

        #endregion

        #region Emails

        /// <summary>
        /// Obsolete
        /// </summary>
        /// <returns></returns>
        public Message[] GetInboxEmails()
        {
            return ExchangeResource.GetMessages("/", true);
        }

        /// <summary>
        /// Obsolete
        /// </summary>
        /// <returns></returns>
        public Message GetMessage(string URL)
        {
            return ExchangeResource.GetMessage(URL);
        }

        #endregion

        #region Exchange 2010

        /// <summary>
        /// Gets appointsments for a given month
        /// </summary>
        /// <param name="month"></param>
        /// <param name="year"></param>
        /// <returns></returns>
        public List<Independentsoft.Exchange.Appointment> GetAppointmentsFromMonth2010(int month, int year)
        {
            DateTime ds = new DateTime(year, month, 1);
            DateTime de = new DateTime(year, month, 1);
            de.AddMonths(1);
            de.AddDays(-1);

            return GetAppointmentsFromDateRange2010(ds, de);
        }

        /// <summary>
        /// Get appointments for a given daterange
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public List<Independentsoft.Exchange.Appointment> GetAppointmentsFromDateRange2010(DateTime startDate, DateTime endDate)
        {
            //Create result array
            List<Independentsoft.Exchange.Appointment> retval = new List<Independentsoft.Exchange.Appointment>();

            //Create search select
            try
            {
                Independentsoft.Exchange.IsGreaterThanOrEqualTo restriction1 = new Independentsoft.Exchange.IsGreaterThanOrEqualTo(Independentsoft.Exchange.AppointmentPropertyPath.StartTime, startDate);
                Independentsoft.Exchange.IsLessThanOrEqualTo restriction2 = new Independentsoft.Exchange.IsLessThanOrEqualTo(Independentsoft.Exchange.AppointmentPropertyPath.EndTime, endDate);
                Independentsoft.Exchange.And restriction3 = new Independentsoft.Exchange.And(restriction1, restriction2);

                Independentsoft.Exchange.FindItemResponse response = this.Exchange2010Service.FindItem(Independentsoft.Exchange.StandardFolder.Calendar, Independentsoft.Exchange.AppointmentPropertyPath.AllPropertyPaths, restriction3);

                //Add search results to result array
                for (int i = 0; i < response.Items.Count; i++)
                {
                    if (response.Items[i] is Independentsoft.Exchange.Appointment)
                    {
                        retval.Add( (Independentsoft.Exchange.Appointment)response.Items[i]);
                    }
                }

                return retval;
            }
            catch (Exception exp)
            {
                return retval;
            }
        }

        /// <summary>
        /// Get singular appointment
        /// </summary>
        /// <param name="PermanentUrl"></param>
        /// <returns></returns>
        public Independentsoft.Exchange.Appointment GetAppointment2010(string PermanentUrl)
        {
            try
            {
                return Exchange2010Service.GetAppointment(PermanentUrl);
            }
            catch (Exception exp)
            {

            }

            return null;
        }

        /// <summary>
        /// Delete appointment
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public bool DeleteItem2010(string url)
        {
            Independentsoft.Exchange.Item item = Exchange2010Service.GetItem(url);
            if(item!=null)
            {
                return Exchange2010Service.DeleteItem(item.ItemId).ResponseCode == Independentsoft.Exchange.ResponseCode.NoError;
            }
            else
                return false;
        }

        /// <summary>
        /// Create an appointment with meetingrequests
        /// </summary>
        /// <param name="U"></param>
        /// <param name="A"></param>
        /// <param name="UserEmails"></param>
        /// <param name="ContactIds"></param>
        /// <returns></returns>
        public string CreateAppointmentWithMeetingRequests2010(ref User U, ref Independentsoft.Exchange.Appointment A, string[] UserEmails, string[] ContactIds)
        {
            //Setup basic properties
            A.IsResponseRequested = true;
            //A.ResponseStatus = Independentsoft.Exchange.ResponseStatus.Organized;

            //Set attendees
            if (UserEmails != null)
            {
                for (int i = 0; i < UserEmails.Length; i++)
                {
                    if (UserEmails[i] != "" && UserEmails[i].ToLower().Trim() != U.Email.ToLower().Trim())
                        A.RequiredAttendees.Add(new Independentsoft.Exchange.Attendee(UserEmails[i].Trim()));
                }
            }

            Independentsoft.Exchange.ItemId ItemId = A.ItemId;

            //Check if the appointment already exists
            if (A.ItemId != null)
            {
                //Update existing appointment
                //Independentsoft.Exchange.PropertyCollection properties = new Independentsoft.Exchange.PropertyCollection();

                //properties.Add(new Independentsoft.Exchange.Property(Independentsoft.Exchange.AppointmentPropertyPath.Subject, A.Subject));
                //properties.Add(new Independentsoft.Exchange.Property(Independentsoft.Exchange.AppointmentPropertyPath.Location, A.Location));
                //properties.Add(new Independentsoft.Exchange.Property(Independentsoft.Exchange.AppointmentPropertyPath.Body, A.Body));
                //properties.Add(new Independentsoft.Exchange.Property(Independentsoft.Exchange.AppointmentPropertyPath.CommonStartTime, A.CommonStartTime));
                //properties.Add(new Independentsoft.Exchange.Property(Independentsoft.Exchange.AppointmentPropertyPath.CommonEndTime, A.CommonEndTime));
                //properties.Add(new Independentsoft.Exchange.Property(Independentsoft.Exchange.AppointmentPropertyPath.StartTime, A.StartTime));
                //properties.Add(new Independentsoft.Exchange.Property(Independentsoft.Exchange.AppointmentPropertyPath.EndTime, A.EndTime));
                //properties.Add(new Independentsoft.Exchange.Property(Independentsoft.Exchange.AppointmentPropertyPath.Importance, A.Importance));
                //properties.Add(new Independentsoft.Exchange.Property(Independentsoft.Exchange.AppointmentPropertyPath.BusyStatus, A.BusyStatus));
                //properties.Add(new Independentsoft.Exchange.Property(Independentsoft.Exchange.AppointmentPropertyPath.ReminderIsSet, A.ReminderIsSet));
                //properties.Add(new Independentsoft.Exchange.Property(Independentsoft.Exchange.AppointmentPropertyPath.ReminderMinutesBeforeStart, A.ReminderMinutesBeforeStart));

                //Exchange2010Service.UpdateItem(A.ItemId, properties);

            }
            else
            {
                //Create new appointment
                ItemId = Exchange2010Service.CreateItem(A);

                if (A.RequiredAttendees.Count > 0 || A.OptionalAttendees.Count > 0)
                    Exchange2010Service.SendMeetingRequest(A);
            }

            //Return Id
            return ItemId.Id;
        }

        #endregion

        #region Exchange older than 2010

        /// <summary>
        /// Obsolete
        /// </summary>
        /// <param name="startDate"></param>
        /// <returns></returns>
        public SearchResultRecord[] GetEmailInboxSyncList(object startDate)
        {
            PropertyName[] propertyName = new PropertyName[1];
            propertyName[0] = MessageProperty.PermanentUrl;

            Select select = new Select(propertyName);
            From from = new From(new string[]{ ExchangeResource.Mailbox.Inbox, ExchangeResource.Mailbox.SentItems, ExchangeResource.Mailbox.Root }, Scope.Deep);
            Where where = new Where();
            OrderBy orderby = new OrderBy();

            Condition condition1 = new Condition(AppointmentProperty.ContentClass, Operator.Equals, ContentClassType.Message);
            Condition condition2 = null;
            
            if(startDate !=null)  condition2 = new Condition( MessageProperty.DateReceived, Operator.GreatThenOrEquals, TypeCast.ToDateTime(startDate));

            where.Add(condition1);

            if (condition2 != null)
            {
                where.Add(LogicalOperator.AND);
                where.Add(condition2);
            }

            orderby.Add(MessageProperty.DateReceived, Order.DESC);

            SqlQuery sqlQuery = new SqlQuery(select, from, where, orderby);
            MultiStatus multiStatus = ExchangeResource.Search(sqlQuery);

            SearchResult searchResult = new SearchResult(multiStatus, propertyName);
            return searchResult.Record;

        }

        /// <summary>
        /// Get appontments from a given month
        /// </summary>
        /// <param name="month"></param>
        /// <param name="year"></param>
        /// <returns></returns>
        public SearchResultRecord[] GetAppointmentFromMonth(int month, int year)
        {
            //Convert month into start and end date
            DateTime startDate = new DateTime(year, month, 1);
            DateTime EndDate = startDate.AddMonths(1);

            //Setup properties
            PropertyName[] propertyName = new PropertyName[1];
            propertyName[0] = AppointmentProperty.StartDate;

            //Create select
            Select select = new Select(propertyName);
            From from = new From(ExchangeResource.Mailbox.Calendar);
            Where where = new Where();
            OrderBy orderby = new OrderBy();

            Condition condition1 = new Condition(AppointmentProperty.ContentClass, Operator.Equals, ContentClassType.Appointment);
            Condition condition2 = new Condition(AppointmentProperty.StartDate, Operator.GreatThenOrEquals, startDate);
            Condition condition3 = new Condition(AppointmentProperty.EndDate, Operator.LessThen, EndDate);

            if (!this.IsOwnerOfCalendar)
            {
                where.Add(LogicalOperator.AND);
                where.Add(new Condition(AppointmentProperty.IsPrivate, Operator.Equals, false));
            }

            where.Add(condition1);
            where.Add(LogicalOperator.AND);
            where.Add(condition2);
            where.Add(LogicalOperator.AND);
            where.Add(condition3);

            orderby.Add(AppointmentProperty.StartDate, Order.ASC);

            //Create Query from properties
            SqlQuery sqlQuery = new SqlQuery(select, from, where, orderby);
            MultiStatus multiStatus = ExchangeResource.Search(sqlQuery);

            //Execute query and return results
            SearchResult searchResult = new SearchResult(multiStatus, propertyName);
            return searchResult.Record;


        }

        /// <summary>
        /// Get appointments from a given daterange
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="EndDate"></param>
        /// <returns></returns>
        public SearchResultRecord[] GetAppointmentHeadersFromDateRange(DateTime startDate, DateTime EndDate)
        {
            //Setup properties
            PropertyName[] propertyName = new PropertyName[6];
            propertyName[0] = AppointmentProperty.StartDate;
            propertyName[1] = AppointmentProperty.EndDate;
            propertyName[2] = AppointmentProperty.Subject;
            propertyName[3] = AppointmentProperty.Location;
            propertyName[4] = AppointmentProperty.RecurrenceID;
            propertyName[5] = AppointmentProperty.PermanentUrl;

            //Create select
            Select select = new Select(propertyName);
            From from = new From(ExchangeResource.Mailbox.Calendar);
            Where where = new Where();
            OrderBy orderby = new OrderBy();

            Condition condition1 = new Condition(AppointmentProperty.ContentClass, Operator.Equals, ContentClassType.Appointment);
            Condition condition2 = new Condition(AppointmentProperty.StartDate, Operator.GreatThenOrEquals, startDate);
            Condition condition3 = new Condition(AppointmentProperty.EndDate, Operator.LessThen, EndDate);

            where.Add(condition1);
            where.Add(LogicalOperator.AND);
            where.Add(condition2);
            where.Add(LogicalOperator.AND);
            where.Add(condition3);

            //Allow reading others calendars
            if(!this.IsOwnerOfCalendar)
            {
                where.Add(LogicalOperator.AND);
                where.Add(new Condition(AppointmentProperty.IsPrivate, Operator.Equals, false));
            }

            orderby.Add(AppointmentProperty.StartDate, Order.ASC);

            //Create SQL query
            SqlQuery sqlQuery = new SqlQuery(select, from, where, orderby);
            MultiStatus multiStatus = ExchangeResource.Search(sqlQuery);

            //Execute SQL query and return result
            SearchResult searchResult = new SearchResult(multiStatus, propertyName);
            return searchResult.Record;


        }

        /// <summary>
        /// Ignores any SSL Warnings (without this, most SSL warning will induce an Exception, thus disabling the integration
        /// </summary>
        internal class AcceptAllCertificatePolicy : ICertificatePolicy
        {
            public AcceptAllCertificatePolicy()
            {
            }

            public bool CheckValidationResult(ServicePoint sPoint, X509Certificate cert, WebRequest wRequest, int certProb)
            {
                // Always accept 
                return true;
            } 

        }

        /// <summary>
        /// Delete an Exchange 2007/2003 item - Appointment, Folder, email etc.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public bool DeleteItem(string url)
        {
            return ExchangeResource.Delete(url);
        }

        /// <summary>
        /// Create an appointment with meeting requests
        /// </summary>
        /// <param name="U"></param>
        /// <param name="A"></param>
        /// <param name="UserEmails"></param>
        /// <param name="ContactIds"></param>
        /// <returns></returns>
        public string CreateAppointmentWithMeetingRequests(ref User U,ref Appointment A, string[] UserEmails, string[] ContactIds)
        {
            //Setup default properties
            A.ResponseRequested = true;
            A.ResponseStatus = ResponseStatus.Organized;
            if (A.CalendarUID == null || A.CalendarUID == "") A.CalendarUID = Guid.NewGuid().ToString();

            string ue = "";

            //Add attendees
            if (UserEmails != null)
            {
                for (int i = 0; i < UserEmails.Length; i++)
                {
                    if(UserEmails[i]!="" && UserEmails[i].ToLower().Trim() != U.Email.ToLower().Trim())
                        ue += (ue == "" ? "" : ";") + UserEmails[i];
                }

                if(ue!="")
                    A.To = ue;
            }

            //Check if the appointment already exists
            if (A.PermanentUrl == "" || A.PermanentUrl == null)
            {
                MultiStatus M = null;

                string tmp = A.To;
                A.To = null;
                M = ExchangeResource.CreateItem(A);
                A = ExchangeResource.GetAppointment(M.Response[0].HRef);

                //Update existing appointment
                if (tmp != "" && tmp != null)
                {
                    A.To = tmp;
                    M = ExchangeResource.SendMeetingRequest(A, true);
                }

                return A.PermanentUrl;
            }
            else
            {
                //Create new appointment
                if (A.To != "" && A.To != null)
                {
                    try
                    {
                        ExchangeResource.SendMeetingRequest(A, true);
                    }
                    catch {}
                }
                else
                    ExchangeResource.SaveItem(A);
                return A.PermanentUrl;
            }

        }

        /// <summary>
        /// Get single Appoinment
        /// </summary>
        /// <param name="PermanentUrl"></param>
        /// <returns></returns>
        public Appointment GetAppointment(string PermanentUrl)
        {
            ExchangeResource.PerformFormsBasedAuthentication(this.User.ExchangeURL);
            return ExchangeResource.GetAppointment(PermanentUrl);
        }

        #endregion

    }
}
