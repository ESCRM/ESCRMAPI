using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;

namespace IDS.EBSTCRM.Base
{
    /// <summary>
    /// Exchange Email Sync Reference
    /// OBSOLETE
    /// </summary>
    public class ExchangeMailSync
    {
        public int Id;
        public int OrganisationId;
        public string EmailId;
        public string UserId;
        public string FromName;
        public string FromEmail;
        public string CC;
        public string Subject;
        public string Body;
        public string HtmlDescription;
        public string ContentType;
        public DateTime Date;
        public DateTime DateReceived;
        public string InReplyTo;
        public string PermanentUrl;
        public bool HasAttachment;
        public string Path;
        public string FolderName;

        public bool Read;
        public string ReplyBy;
        public object ReplyByDate;
        public string ReplyTo;

        public int ContactId;
        public int VisibleTo;

        public ExchangeMailSync()
        {

        }

        public ExchangeMailSync(ref SqlDataReader dr)
        {
            Id = TypeCast.ToInt(dr["Id"]);
            OrganisationId = TypeCast.ToInt(dr["OrganisationId"]);
            EmailId = TypeCast.ToString(dr["EmailId"]);
            UserId = TypeCast.ToString(dr["UserId"]);
            FromName = TypeCast.ToString(dr["FromName"]);
            FromEmail = TypeCast.ToString(dr["FromEmail"]);
            CC = TypeCast.ToString(dr["CC"]);
            Subject = TypeCast.ToString(dr["Subject"]);
            Body = TypeCast.ToString(dr["Body"]);
            HtmlDescription = TypeCast.ToString(dr["HtmlDescription"]);
            ContentType = TypeCast.ToString(dr["ContentType"]);
            Date = TypeCast.ToDateTime(dr["Date"]);
            DateReceived = TypeCast.ToDateTime(dr["DateReceived"]);
            InReplyTo = TypeCast.ToString(dr["InReplyTo"]);
            PermanentUrl = TypeCast.ToString(dr["PermanentUrl"]);
            HasAttachment = TypeCast.ToBool(dr["HasAttachment"]);
            Path = TypeCast.ToString(dr["Path"]);
            Read = TypeCast.ToBool(dr["Read"]);
            ReplyBy = TypeCast.ToString(dr["ReplyBy"]);
            ReplyByDate = TypeCast.ToDateTimeOrNull(dr["ReplyByDate"]);
            ReplyTo = TypeCast.ToString(dr["ReplyTo"]);

            ContactId = TypeCast.ToInt(dr["ContactId"]);
            VisibleTo = TypeCast.ToInt(dr["VisibleTo"]);

        }

        public ExchangeMailSync(Independentsoft.Webdav.Exchange.ContentClass.Message m, User U)
        {
            this.OrganisationId = U.OrganisationId;
            this.EmailId = m.ID;
            this.UserId = U.Id;
            this.FromName = m.FromName;
            this.FromEmail = m.FromEmail;
            this.CC = m.Cc;
            this.Subject = m.Subject;
            this.Body = m.Body;
            this.HtmlDescription = m.HtmlDescription;
            this.ContentType = m.ContentType;
            this.Date = m.Date;
            this.DateReceived = m.DateReceived;
            this.InReplyTo = m.InReplyTo;
            this.PermanentUrl = m.PermanentUrl;
            this.HasAttachment = m.HasAttachment;
            this.ReplyTo = m.ReplyTo;

            this.Path = m.HRef.Substring(U.ExchangeURL.Length);
            this.Path = this.Path.Substring(0, this.Path.LastIndexOf("/"));

            this.Read = m.Read;
            this.ReplyBy = m.ReplyBy;

            System.Web.HttpContext ctx = System.Web.HttpContext.Current;
            if (ctx != null) this.Path = ctx.Server.UrlDecode(this.Path);

            if(this.Path.LastIndexOf("/")>-1)
                this.FolderName = this.Path.Substring(this.Path.LastIndexOf("/")+1);

            //m.Address
            //this.ReplyByDate = m.ReplyByDate != null ? (object)m.ReplyByDate : null;
            

        }
    }
}
