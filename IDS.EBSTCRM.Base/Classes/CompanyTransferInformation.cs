using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;


namespace IDS.EBSTCRM.Base
{
    /// <summary>
    /// Company Tranferred details (Anvisninger)
    /// Contains specific details about a single transfer of a company betweeen organisations
    /// </summary>
    public class CompanyTransferInformation
    {
        public int Id;
        public int CompanyId;

        public string SenderUserId;
        public string SenderUserUserName;
        public string SenderUserFirstname;
        public string SenderUserLastname;
        public string SenderUserEmail;

        public int SenderOrganisationId;
        public string SenderOrganisationName;

        public int ReceiverOrganisationId;
        public string ReceiverOrganisationName;

        public string ReceiverUserId;
        public string ReceiverUserUserName;
        public string ReceiverUserFirstname;
        public string ReceiverUserLastname;
        public string ReceiverUserEmail;

        public string AcceptedByUserId;
        public string AcceptedUserUserName;
        public string AcceptedUserFirstname;
        public string AcceptedUserLastname;
        public string AcceptedUserEmail;

        public string TransferReason;
        public DateTime TransferDate;
        public object AcceptedDate;

        public CompanyTransferInformation(ref SqlDataReader dr)
        {
            Id = TypeCast.ToInt(dr["Id"]);
            CompanyId = TypeCast.ToInt(dr["CompanyId"]);

            SenderUserId = TypeCast.ToString(dr["SenderUserId"]);
            SenderUserUserName = TypeCast.ToString(dr["SenderUserUserName"]);
            SenderUserFirstname = TypeCast.ToString(dr["SenderUserFirstname"]);
            SenderUserLastname = TypeCast.ToString(dr["SenderUserLastname"]);
            SenderUserEmail = TypeCast.ToString(dr["SenderUserEmail"]);

            SenderOrganisationId = TypeCast.ToInt(dr["SenderOrganisationId"]);
            SenderOrganisationName = TypeCast.ToString(dr["SenderOrganisationName"]);

            ReceiverOrganisationId = TypeCast.ToInt(dr["ReceiverOrganisationId"]);
            ReceiverOrganisationName = TypeCast.ToString(dr["ReceiverOrganisationName"]);

            ReceiverUserId = TypeCast.ToString(dr["ReceiverUserId"]);
            ReceiverUserUserName = TypeCast.ToString(dr["ReceiverUserUserName"]);
            ReceiverUserFirstname = TypeCast.ToString(dr["ReceiverUserFirstname"]);
            ReceiverUserLastname = TypeCast.ToString(dr["ReceiverUserLastname"]);
            ReceiverUserEmail = TypeCast.ToString(dr["ReceiverUserEmail"]);

            AcceptedByUserId = TypeCast.ToString(dr["AcceptedByUserId"]);
            AcceptedUserUserName = TypeCast.ToString(dr["AcceptedUserUserName"]);
            AcceptedUserFirstname = TypeCast.ToString(dr["AcceptedUserFirstname"]);
            AcceptedUserLastname = TypeCast.ToString(dr["AcceptedUserLastname"]);
            AcceptedUserEmail = TypeCast.ToString(dr["AcceptedUserEmail"]);

            TransferReason = TypeCast.ToString(dr["TransferReason"]);
            TransferDate = TypeCast.ToDateTime(dr["TransferDate"]);
            AcceptedDate = TypeCast.ToDateTimeOrNull(dr["AcceptedDate"]);
        }
    }
}
