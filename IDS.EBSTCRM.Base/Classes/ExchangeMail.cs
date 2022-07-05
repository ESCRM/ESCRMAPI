using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;

namespace IDS.EBSTCRM.Base
{
    /// <summary>
    /// Exchange Email Reference
    /// OBSOLETE
    /// </summary>
    public class ExchangeMail
    {
        public int Id;
        public int OrganisationId;
        public string EmailId;
        public string UserId;
        public string FromName;
        public string FromEmail;
        public string CC;
        public string Subject;
        public string ContentType;
        public DateTime Date;
        public DateTime DateReceived;
        public string InReplyTo;
        public string PermanentUrl;
        public bool HasAttachment;
        public string Path;
        public bool Read;
        public string ReplyBy;
        public object ReplyByDate;
        public string ReplyTo;

        public ExchangeMail()
        {

        }

        public ExchangeMail(ref SqlDataReader dr)
        {
            Id = TypeCast.ToInt(dr["Id"]);
            OrganisationId = TypeCast.ToInt(dr["OrganisationId"]);
            EmailId = TypeCast.ToString(dr["EmailId"]);
            UserId = TypeCast.ToString(dr["UserId"]);
            FromName = TypeCast.ToString(dr["FromName"]);
            FromEmail = TypeCast.ToString(dr["FromEmail"]);
            CC = TypeCast.ToString(dr["CC"]);
            Subject = TypeCast.ToString(dr["Subject"]);
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

        }
    }

    public class ExchangeMailItem : ExchangeMail
    {
        public string Body;
        public string HtmlDescription;

        public ExchangeMailItem()
            : base()
        {

        }

        public ExchangeMailItem(ref SqlDataReader dr)
            : base(ref dr)
        {
            Body = TypeCast.ToString(dr["Body"]);
            HtmlDescription = TypeCast.ToString(dr["HtmlDescription"]);
        }

    }


}

