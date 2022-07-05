using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;

namespace IDS.EBSTCRM.Base
{

    /// <summary>
    /// Transferred Contact between organisations
    /// </summary>
    [Serializable()]
    public class ContactTransferred : Contact
    {

        public int Id;
        public int ContactId;

        public string SenderUserId;
        public string SenderUserName;
        public int SenderOrganisationId;
        public string SenderOrganisationName;

        public string ReceiverUserId;
        public string ReceiverUserName;
        public int ReceiverOrganisationId;
        public string ReceiverOrganisationName;

        public string AcceptedByUserId;
        public string AcceptedByUserName;
        public object AcceptedDate;
        public string AcceptedReason;

        public string RejectedByUserId;
        public string RejectedByUserName;
        public object RejectedDate;
        public string RejectedReason;

        public DateTime TransferDate;
        public string TransferReason;

        public int ForwardedToId;

        public ContactTransferred()
        {

        }

        public ContactTransferred(ref SqlDataReader dr)
            : base(ref dr)
        {
            Id = TypeCast.ToInt(dr["Id"]);
            ContactId = TypeCast.ToInt(dr["ContactId"]);

            SenderUserId = TypeCast.ToString(dr["SenderUserId"]);
            SenderUserName = TypeCast.ToString(dr["SenderUserName"]);
            SenderOrganisationId = TypeCast.ToInt(dr["SenderOrganisationId"]);
            SenderOrganisationName = TypeCast.ToString(dr["SenderOrganisationName"]);

            ReceiverUserId = TypeCast.ToString(dr["ReceiverUserId"]);
            ReceiverUserName = TypeCast.ToString(dr["ReceiverUserName"]);
            ReceiverOrganisationId = TypeCast.ToInt(dr["ReceiverOrganisationId"]);
            ReceiverOrganisationName = TypeCast.ToString(dr["ReceiverOrganisationName"]);

            AcceptedByUserId = TypeCast.ToString(dr["AcceptedByUserId"]);
            AcceptedByUserName = TypeCast.ToString(dr["AcceptedByUserName"]);
            AcceptedDate = TypeCast.ToDateTimeOrNull(dr["AcceptedDate"]);
            AcceptedReason = TypeCast.ToString(dr["AcceptedReason"]);

            RejectedByUserId = TypeCast.ToString(dr["RejectedByUserId"]);
            RejectedByUserName = TypeCast.ToString(dr["RejectedByUserName"]);
            RejectedDate = TypeCast.ToDateTimeOrNull(dr["RejectedDate"]);
            RejectedReason = TypeCast.ToString(dr["RejectedReason"]);

            TransferDate = TypeCast.ToDateTime(dr["TransferDate"]);
            TransferReason = TypeCast.ToString(dr["TransferReason"]);

            ForwardedToId = TypeCast.ToInt(dr["ForwardedToId"]);
        }
    }
}
