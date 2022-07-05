using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;

namespace IDS.EBSTCRM.Base
{
    /// <summary>
    /// Transferred company between organisations
    /// Keeps log of transfer history
    /// </summary>
    [Serializable()]
    public class CompanyTransferred
    {
        public string SenderOrganisationName;
		public string senderFirstname;
		public string senderLastname;
		public string senderEmail;
		public string senderUsername;

		public string ReceiverOrganisationName;
		public string ReceiverFirstname;
		public string ReceiverLastname;
		public string ReceiverEmail;
		public string ReceiverUsername;

		public DateTime TransferDate;
		public object AcceptedDate;

        public CompanyTransferred()
        { 
        }

        public CompanyTransferred(ref SqlDataReader dr)
        {
            SenderOrganisationName = TypeCast.ToString(dr["SenderOrganisationName"]);
            senderFirstname = TypeCast.ToString(dr["senderFirstname"]);
            senderLastname = TypeCast.ToString(dr["senderLastname"]);
            senderEmail = TypeCast.ToString(dr["senderEmail"]);
            senderUsername = TypeCast.ToString(dr["senderUsername"]);

            ReceiverOrganisationName = TypeCast.ToString(dr["ReceiverOrganisationName"]);
            ReceiverFirstname = TypeCast.ToString(dr["ReceiverFirstname"]);
            ReceiverLastname = TypeCast.ToString(dr["ReceiverLastname"]);
            ReceiverEmail = TypeCast.ToString(dr["ReceiverEmail"]);
            ReceiverUsername = TypeCast.ToString(dr["ReceiverUsername"]);

            TransferDate = TypeCast.ToDateTime(dr["transferDate"]);
            AcceptedDate = TypeCast.ToDateTimeOrNull(dr["acceptedDate"]);
        }
    }

    public class CompanyTransferredDetailed : CompanyTransferred
    {
        public string CompanyName;
        public string Reason;
        public int CompanyId;

        public CompanyTransferredDetailed() : base()
        { 
        }

        public CompanyTransferredDetailed(ref SqlDataReader dr) : base(ref dr)
        {
            CompanyName = TypeCast.ToString(dr["CompanyName"]);
            Reason = TypeCast.ToString(dr["Reason"]);
            CompanyId = TypeCast.ToInt(dr["CompanyId"]);

        }
    }
}
