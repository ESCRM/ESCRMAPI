using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Collections;

namespace IDS.EBSTCRM.Base
{
    /// <summary>
    /// Administrative Users Travel usage
    /// </summary>
    public class TravelUsageAdmin : TravelUsage
    {
        public string Username;
        public string UserFirstname;
        public string UserLastname;

        public string notApprovedByUserName;
        public string approvedByUserName;
        public string Status;

        public TravelUsageAdmin()
            : base()
        {

        }

        public TravelUsageAdmin(ref SqlDataReader dr)
            : base(ref dr)
        {
            Username = TypeCast.ToString(dr["username"]);
            UserFirstname = TypeCast.ToString(dr["UserFirstname"]);
            UserLastname = TypeCast.ToString(dr["UserLastname"]);

            notApprovedByUserName = TypeCast.ToString(dr["notApprovedByUserName"]);
            approvedByUserName = TypeCast.ToString(dr["approvedByUserName"]);

            Status = TypeCast.ToString(dr["Status"]);
        }

    }

    /// <summary>
    /// Users Travel usage
    /// </summary>
    public class TravelUsage : EventLogBase
    {
        public int Id;
        public int OrganisationId;
        public string userId;
        public int PrimaryProjectTypeId;
        public string PrimaryProjectTypeName;
        public int SecondaryProjectTypeId;
        public string SecondaryProjectTypeName;
        public int SecondaryProjectTypeSerialNo;
        public int CaseNumberId;
        public int CaseNumberRelationId;
        public int CaseNumber;
        public string CaseNumberName;
        public string Description;
        public string LicensePlate;
        public DateTime DepartureDate;
        public DateTime ReturnDate;
        public string DepartureAddress;
        public string DepartureZipcode;
        public string DepartureCity;
        public string DepartureCountry;
        public string ArrivalAddress;
        public string ArrivalZipcode;
        public string ArrivalCity;
        public string ArrivalCountry;
        public string ReturnAddress;
        public string ReturnZipcode;
        public string ReturnCity;
        public string ReturnCountry;
        public Object pendingApproval;
        public Object approved;
        public string approvedBy;
        public Object notApproved;
        public string notApprovedBy;
        public string notApprovedReason;
        public decimal DepartureTravelDistance;
        public decimal ReturnTravelDistance;

        public decimal TotalDistance;

        public TravelUsage()
        {
            DepartureDate = DateTime.Now;
            ReturnDate = DepartureDate;
        }

        public TravelUsage(ref SqlDataReader dr)
        {
            Id = TypeCast.ToInt(dr["Id"]);
            OrganisationId = TypeCast.ToInt(dr["OrganisationId"]);
            userId = TypeCast.ToString(dr["userId"]);
            PrimaryProjectTypeId = TypeCast.ToInt(dr["PrimaryProjectTypeId"]);
            PrimaryProjectTypeName = TypeCast.ToString(dr["PrimaryProjectTypeName"]);
            SecondaryProjectTypeId = TypeCast.ToInt(dr["SecondaryProjectTypeId"]);
            SecondaryProjectTypeName = TypeCast.ToString(dr["SecondaryProjectTypeName"]);
            SecondaryProjectTypeSerialNo = TypeCast.ToInt(dr["SecondaryProjectTypeSerialNo"]);
            CaseNumberId = TypeCast.ToInt(dr["CaseNumberId"]);
            CaseNumberRelationId = TypeCast.ToInt(dr["CaseNumberRelationId"]);
            CaseNumber = TypeCast.ToInt(dr["CaseNumber"]);
            CaseNumberName = TypeCast.ToString(dr["CaseNumberName"]);
            Description = TypeCast.ToString(dr["Description"]);
            LicensePlate = TypeCast.ToString(dr["LicensePlate"]);
            DepartureDate = TypeCast.ToDateTime(dr["DepartureDate"]);
            ReturnDate = TypeCast.ToDateTime(dr["ReturnDate"]);
            DepartureAddress = TypeCast.ToString(dr["DepartureAddress"]);
            DepartureZipcode = TypeCast.ToString(dr["DepartureZipcode"]);
            DepartureCity = TypeCast.ToString(dr["DepartureCity"]);
            DepartureCountry = TypeCast.ToString(dr["DepartureCountry"]);
            ArrivalAddress = TypeCast.ToString(dr["ArrivalAddress"]);
            ArrivalZipcode = TypeCast.ToString(dr["ArrivalZipcode"]);
            ArrivalCity = TypeCast.ToString(dr["ArrivalCity"]);
            ArrivalCountry = TypeCast.ToString(dr["ArrivalCountry"]);
            ReturnAddress = TypeCast.ToString(dr["ReturnAddress"]);
            ReturnZipcode = TypeCast.ToString(dr["ReturnZipcode"]);
            ReturnCity = TypeCast.ToString(dr["ReturnCity"]);
            ReturnCountry = TypeCast.ToString(dr["ReturnCountry"]);
            pendingApproval = TypeCast.ToDateTimeOrNull(dr["pendingApproval"]);
            approved = TypeCast.ToDateTimeOrNull(dr["approved"]);
            approvedBy = TypeCast.ToString(dr["approvedBy"]);
            notApproved = TypeCast.ToDateTimeOrNull(dr["notApproved"]);
            notApprovedBy = TypeCast.ToString(dr["notApprovedBy"]);
            DepartureTravelDistance = TypeCast.ToDecimal(dr["DepartureTravelDistance"]);
            ReturnTravelDistance = TypeCast.ToDecimal(dr["ReturnTravelDistance"]);
            TotalDistance = TypeCast.ToDecimal(dr["TotalDistance"]);
            notApprovedReason = TypeCast.ToString(dr["notApprovedReason"]);
        }


        public override VisualItems GetVisualItemsForEventLog(string Event)
        {
            VisualItems retval = new VisualItems();

            retval.Text = eventToText(Event) + " rejseregnskabet d. " + DepartureDate.ToString("dd-MM-yyyy");
            retval.Icon = "images/listviewIcons/null.png";

            retval.JavaScript = (Event == "DELETE" ?
                                    "alert('Rejseregnskabet er blevet slettet');"
                                    :
                                    "top.frames['root'].frames['frameAdmin'].travelUsage_editUsage(" + Id + ");");



            return retval;
        }
    }
}
