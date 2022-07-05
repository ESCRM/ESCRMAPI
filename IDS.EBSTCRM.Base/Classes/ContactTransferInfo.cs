using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;

namespace IDS.EBSTCRM.Base
{
    /// <summary>
    /// Contact Tranferred details (Anvisninger)
    /// Contains specific details about a single transfer of a Contact betweeen organisations
    /// </summary>
    public class ContactTransferInfo
    {
        public string SenderUserName;
        public string ReceiverUserName;
        public string TransferReason;
        public DateTime TransferDate;
        public int ContactId;

        public string AcceptedReason;
        public DateTime? AcceptedDate;

        public ContactTransferInfo() { }

        public ContactTransferInfo(ref SqlDataReader dr)
        {
            SenderUserName = TypeCast.ToString(dr["SenderUserName"]);
            ReceiverUserName = TypeCast.ToString(dr["ReceiverUserName"]);
            TransferReason = TypeCast.ToString(dr["TransferReason"]);
            TransferDate = TypeCast.ToDateTime(dr["TransferDate"]);
            ContactId = TypeCast.ToInt(dr["ContactId"]);

            AcceptedReason = TypeCast.ToString(dr["AcceptedReason"]);
            AcceptedDate = TypeCast.ToDateTimeLoose(dr["AcceptedDate"]);
        }
    }
}
