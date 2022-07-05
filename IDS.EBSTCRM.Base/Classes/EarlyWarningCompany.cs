using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Data.SqlClient;

namespace IDS.EBSTCRM.Base
{
    /// <summary>
    /// Early Warning Company
    /// Contains all Early Warning Company Data
    /// </summary>
    public class EarlyWarningCompany
    {
        public int Id;
        public int OrganisationId;
        public string CVR;
        public string CompanyName;
        public string Address;
        public string Zipcode;
        public string City;
        public string County;
        public string Phone;
        public string Email;
        //** Tilføjet region, ESCRM-136/137
        public string Region;
        public string Homepage;
        public string CompanyType;
        public string EstablishmentYear;
        public int EmployeeCount;
        public string SectorCodes;
        public string Status;
        public string CurrentConsultant;

        public bool Weaknesses_Management_LackOfCompetences;
        public bool Weaknesses_Management_LackOfAdjustingToSales;
        public bool Weaknesses_Management_PersonalProblems;
        public bool Weaknesses_Management_DisputesInOwnership;
        public bool Weaknesses_Management_Other;
        public string Weaknesses_Management_Other_Text;
        public bool Weaknesses_Marketing_LackOfStrategicFocus;
        public bool Weaknesses_Marketing_NoTrackingOfCustomers;
        public bool Weaknesses_Marketing_TooDependentOfSingleCustomers;
        public bool Weaknesses_Marketing_LackOfMonitoringMarket;
        public bool Weaknesses_Marketing_LackOfSalesEffort;
        public bool Weaknesses_Marketing_LackOfSearchingSales;
        public bool Weaknesses_Marketing_Other;
        public string Weaknesses_Marketing_Other_Text;
        public bool Weaknesses_Production_LackOfOutsourcing;
        public bool Weaknesses_Production_TroubleWithOutsourcing;
        public bool Weaknesses_Production_Quality;
        public bool Weaknesses_Production_Planning;
        public bool Weaknesses_Production_StockManagementAndProduction;
        public bool Weaknesses_Production_OldTechnology;
        public bool Weaknesses_Production_Other;
        public string Weaknesses_Production_Other_Text;
        public bool Weaknesses_Finance_LackOfAccountingUnderstanding;
        public bool Weaknesses_Finance_ExpensiveFinancing;
        public bool Weaknesses_Finance_DebitorLoses;
        public bool Weaknesses_Finance_AdministrativeMess;
        public bool Weaknesses_Finance_Other;
        public string Weaknesses_Finance_Other_Text;
        public bool Weaknesses_HasPositiveResult;

        public DateTime DateCreated;
        public string CreatedBy;
        public object LastUpdated;
        public string LastUpdatedBy;
        public object DateDeleted;
        public string DeletedBy;
        public string NaceCode;
        public decimal TimeUsage;
        public object DateStarted;
        public object DateSentToVolunteer;
        public object DateEnded;
        public string Recommendation;
        public string CrisisCharacter;
        public bool hasProfessionalBoard;
        public bool Weakness_Management;
        public bool Weakness_Finance;
        public bool Weakness_Production;
        public bool Weakness_Marketing;


        public string CaseType { get; set; }
        public bool mitErhvervshus { get; set; }

        public EarlyWarningCompany()
        {

        }

        public EarlyWarningCompany(HttpRequest Request)
        {
            Id = TypeCast.ToInt(Request.Form["Id"]);
            if (Id == 0) Id = TypeCast.ToInt(Request.Form["txtId"]);

            OrganisationId = TypeCast.ToInt(Request.Form["OrganisationId"]);
            CVR = TypeCast.ToString(Request.Form["CVR"]);
            CompanyName = TypeCast.ToString(Request.Form["CompanyName"]);
            Address = TypeCast.ToString(Request.Form["Address"]);
            Zipcode = TypeCast.ToString(Request.Form["Zipcode"]);
            City = TypeCast.ToString(Request.Form["City"]);
            County = TypeCast.ToString(Request.Form["County"]);
            Phone = TypeCast.ToString(Request.Form["Phone"]);
            Email = TypeCast.ToString(Request.Form["Email"]);

            //** Tilføjet region, ESCRM-136/137
            Region = TypeCast.ToString(Request.Form["Region"]);

            Homepage = TypeCast.ToString(Request.Form["Homepage"]);
            CompanyType = TypeCast.ToString(Request.Form["CompanyType"]);
            EstablishmentYear = TypeCast.ToString(Request.Form["EstablishmentYear"]);
            EmployeeCount = TypeCast.ToInt(Request.Form["EmployeeCount"]);
            SectorCodes = TypeCast.ToString(Request.Form["SectorCodes"]);
            Status = TypeCast.ToString(Request.Form["Status"]);
            CurrentConsultant = TypeCast.ToString(Request.Form["CurrentConsultant"]);


            Weaknesses_Management_LackOfCompetences = TypeCast.ToBool(Request.Form["Weaknesses_Management_LackOfCompetences"]);
            Weaknesses_Management_LackOfAdjustingToSales = TypeCast.ToBool(Request.Form["Weaknesses_Management_LackOfAdjustingToSales"]);
            Weaknesses_Management_PersonalProblems = TypeCast.ToBool(Request.Form["Weaknesses_Management_PersonalProblems"]);
            Weaknesses_Management_DisputesInOwnership = TypeCast.ToBool(Request.Form["Weaknesses_Management_DisputesInOwnership"]);
            Weaknesses_Management_Other = TypeCast.ToBool(Request.Form["Weaknesses_Management_Other"]);
            Weaknesses_Management_Other_Text = TypeCast.ToString(Request.Form["Weaknesses_Management_Other_Text"]);
            Weaknesses_Marketing_LackOfStrategicFocus = TypeCast.ToBool(Request.Form["Weaknesses_Marketing_LackOfStrategicFocus"]);
            Weaknesses_Marketing_NoTrackingOfCustomers = TypeCast.ToBool(Request.Form["Weaknesses_Marketing_NoTrackingOfCustomers"]);
            Weaknesses_Marketing_TooDependentOfSingleCustomers = TypeCast.ToBool(Request.Form["Weaknesses_Marketing_TooDependentOfSingleCustomers"]);
            Weaknesses_Marketing_LackOfMonitoringMarket = TypeCast.ToBool(Request.Form["Weaknesses_Marketing_LackOfMonitoringMarket"]);
            Weaknesses_Marketing_LackOfSalesEffort = TypeCast.ToBool(Request.Form["Weaknesses_Marketing_LackOfSalesEffort"]);
            Weaknesses_Marketing_LackOfSearchingSales = TypeCast.ToBool(Request.Form["Weaknesses_Marketing_LackOfSearchingSales"]);
            Weaknesses_Marketing_Other = TypeCast.ToBool(Request.Form["Weaknesses_Marketing_Other"]);
            Weaknesses_Marketing_Other_Text = TypeCast.ToString(Request.Form["Weaknesses_Marketing_Other_Text"]);
            Weaknesses_Production_LackOfOutsourcing = TypeCast.ToBool(Request.Form["Weaknesses_Production_LackOfOutsourcing"]);
            Weaknesses_Production_TroubleWithOutsourcing = TypeCast.ToBool(Request.Form["Weaknesses_Production_TroubleWithOutsourcing"]);
            Weaknesses_Production_Quality = TypeCast.ToBool(Request.Form["Weaknesses_Production_Quality"]);
            Weaknesses_Production_Planning = TypeCast.ToBool(Request.Form["Weaknesses_Production_Planning"]);
            Weaknesses_Production_StockManagementAndProduction = TypeCast.ToBool(Request.Form["Weaknesses_Production_StockManagementAndProduction"]);
            Weaknesses_Production_OldTechnology = TypeCast.ToBool(Request.Form["Weaknesses_Production_OldTechnology"]);
            Weaknesses_Production_Other = TypeCast.ToBool(Request.Form["Weaknesses_Production_Other"]);
            Weaknesses_Production_Other_Text = TypeCast.ToString(Request.Form["Weaknesses_Production_Other_Text"]);
            Weaknesses_Finance_LackOfAccountingUnderstanding = TypeCast.ToBool(Request.Form["Weaknesses_Finance_LackOfAccountingUnderstanding"]);
            Weaknesses_Finance_ExpensiveFinancing = TypeCast.ToBool(Request.Form["Weaknesses_Finance_ExpensiveFinancing"]);
            Weaknesses_Finance_DebitorLoses = TypeCast.ToBool(Request.Form["Weaknesses_Finance_DebitorLoses"]);
            Weaknesses_Finance_AdministrativeMess = TypeCast.ToBool(Request.Form["Weaknesses_Finance_AdministrativeMess"]);
            Weaknesses_Finance_Other = TypeCast.ToBool(Request.Form["Weaknesses_Finance_Other"]);
            Weaknesses_Finance_Other_Text = TypeCast.ToString(Request.Form["Weaknesses_Finance_Other_Text"]);
            Weaknesses_HasPositiveResult = TypeCast.ToBool(Request.Form["Weaknesses_HasPositiveResult"]);

            DateCreated = TypeCast.ToDateTime(Request.Form["DateCreated"]);
            CreatedBy = TypeCast.ToString(Request.Form["CreatedBy"]);
            LastUpdated = TypeCast.ToDateTimeOrNull(Request.Form["LastUpdated"]);
            LastUpdatedBy = TypeCast.ToString(Request.Form["LastUpdatedBy"]);
            DateDeleted = TypeCast.ToDateTimeOrNull(Request.Form["DateDeleted"]);
            DeletedBy = TypeCast.ToString(Request.Form["DeletedBy"]);
            NaceCode = TypeCast.ToString(Request.Form["NaceCode"]);
            TimeUsage = TypeCast.ToDecimal(Request.Form["TimeUsage"]);
            DateStarted = TypeCast.ToDateTimeOrNull(Request.Form["DateStarted"]);
            DateSentToVolunteer = TypeCast.ToDateTimeOrNull(Request.Form["DateSentToVolunteer"]);
            DateEnded = TypeCast.ToDateTimeOrNull(Request.Form["DateEnded"]);
            Recommendation = TypeCast.ToString(Request.Form["Recommendation"]);
            CrisisCharacter = TypeCast.ToString(Request.Form["CrisisCharacter"]);
            hasProfessionalBoard = TypeCast.ToBool(Request.Form["hasProfessionalBoard"]);
            Weakness_Management = TypeCast.ToBool(Request.Form["Weakness_Management"]);
            Weakness_Finance = TypeCast.ToBool(Request.Form["Weakness_Finance"]);
            Weakness_Production = TypeCast.ToBool(Request.Form["Weakness_Production"]);
            Weakness_Marketing = TypeCast.ToBool(Request.Form["Weakness_Marketing"]);
        }


        public EarlyWarningCompany(HttpRequest Request, User U)
        {
            Id = TypeCast.ToInt(Request.Form["txtId"]);

            OrganisationId = TypeCast.ToInt(Request.Form["OrganisationId"]);
            CVR = TypeCast.ToString(Request.Form["CVR"]);
            CompanyName = TypeCast.ToString(Request.Form["CompanyName"]);
            Address = TypeCast.ToString(Request.Form["Address"]);
            Zipcode = TypeCast.ToString(Request.Form["Zipcode"]);
            City = TypeCast.ToString(Request.Form["City"]);
            Phone = TypeCast.ToString(Request.Form["CompanyPhone"]);
            County = TypeCast.ToString(Request.Form["County"]);
            Email = TypeCast.ToString(Request.Form["CompanyEmail"]);
            
            //** Tilføjet region, ESCRM-136/137
            Region = TypeCast.ToString(Request.Form["CompanyRegion"]);

            Homepage = TypeCast.ToString(Request.Form["Homepage"]);
            CompanyType = TypeCast.ToString(Request.Form["CompanyType"]);
            EstablishmentYear = TypeCast.ToString(Request.Form["EstablishmentYear"]);
            EmployeeCount = TypeCast.ToInt(Request.Form["EmployeeCount"]);
            SectorCodes = TypeCast.ToString(Request.Form["SectorCodes"]);
            Status = TypeCast.ToString(Request.Form["Status"]);
            CurrentConsultant = TypeCast.ToString(Request.Form["CurrentConsultant"]);

            CaseType = TypeCast.ToString(Request.Form["CaseType"]); ;
            mitErhvervshus = TypeCast.ToBool(Request.Form["mitErhvervshus"]);

            Weaknesses_Management_LackOfCompetences = TypeCast.ToBool(Request.Form["Weaknesses_Management_LackOfCompetences"]);
            Weaknesses_Management_LackOfAdjustingToSales = TypeCast.ToBool(Request.Form["Weaknesses_Management_LackOfAdjustingToSales"]);
            Weaknesses_Management_PersonalProblems = TypeCast.ToBool(Request.Form["Weaknesses_Management_PersonalProblems"]);
            Weaknesses_Management_DisputesInOwnership = TypeCast.ToBool(Request.Form["Weaknesses_Management_DisputesInOwnership"]);
            Weaknesses_Management_Other = TypeCast.ToBool(Request.Form["Weaknesses_Management_Other"]);
            Weaknesses_Management_Other_Text = TypeCast.ToString(Request.Form["Weaknesses_Management_Other_Text"]);
            Weaknesses_Marketing_LackOfStrategicFocus = TypeCast.ToBool(Request.Form["Weaknesses_Marketing_LackOfStrategicFocus"]);
            Weaknesses_Marketing_NoTrackingOfCustomers = TypeCast.ToBool(Request.Form["Weaknesses_Marketing_NoTrackingOfCustomers"]);
            Weaknesses_Marketing_TooDependentOfSingleCustomers = TypeCast.ToBool(Request.Form["Weaknesses_Marketing_TooDependentOfSingleCustomers"]);
            Weaknesses_Marketing_LackOfMonitoringMarket = TypeCast.ToBool(Request.Form["Weaknesses_Marketing_LackOfMonitoringMarket"]);
            Weaknesses_Marketing_LackOfSalesEffort = TypeCast.ToBool(Request.Form["Weaknesses_Marketing_LackOfSalesEffort"]);
            Weaknesses_Marketing_LackOfSearchingSales = TypeCast.ToBool(Request.Form["Weaknesses_Marketing_LackOfSearchingSales"]);
            Weaknesses_Marketing_Other = TypeCast.ToBool(Request.Form["Weaknesses_Marketing_Other"]);
            Weaknesses_Marketing_Other_Text = TypeCast.ToString(Request.Form["Weaknesses_Marketing_Other_Text"]);
            Weaknesses_Production_LackOfOutsourcing = TypeCast.ToBool(Request.Form["Weaknesses_Production_LackOfOutsourcing"]);
            Weaknesses_Production_TroubleWithOutsourcing = TypeCast.ToBool(Request.Form["Weaknesses_Production_TroubleWithOutsourcing"]);
            Weaknesses_Production_Quality = TypeCast.ToBool(Request.Form["Weaknesses_Production_Quality"]);
            Weaknesses_Production_Planning = TypeCast.ToBool(Request.Form["Weaknesses_Production_Planning"]);
            Weaknesses_Production_StockManagementAndProduction = TypeCast.ToBool(Request.Form["Weaknesses_Production_StockManagementAndProduction"]);
            Weaknesses_Production_OldTechnology = TypeCast.ToBool(Request.Form["Weaknesses_Production_OldTechnology"]);
            Weaknesses_Production_Other = TypeCast.ToBool(Request.Form["Weaknesses_Production_Other"]);
            Weaknesses_Production_Other_Text = TypeCast.ToString(Request.Form["Weaknesses_Production_Other_Text"]);
            Weaknesses_Finance_LackOfAccountingUnderstanding = TypeCast.ToBool(Request.Form["Weaknesses_Finance_LackOfAccountingUnderstanding"]);
            Weaknesses_Finance_ExpensiveFinancing = TypeCast.ToBool(Request.Form["Weaknesses_Finance_ExpensiveFinancing"]);
            Weaknesses_Finance_DebitorLoses = TypeCast.ToBool(Request.Form["Weaknesses_Finance_DebitorLoses"]);
            Weaknesses_Finance_AdministrativeMess = TypeCast.ToBool(Request.Form["Weaknesses_Finance_AdministrativeMess"]);
            Weaknesses_Finance_Other = TypeCast.ToBool(Request.Form["Weaknesses_Finance_Other"]);
            Weaknesses_Finance_Other_Text = TypeCast.ToString(Request.Form["Weaknesses_Finance_Other_Text"]);
            Weaknesses_HasPositiveResult = TypeCast.ToBool(Request.Form["Weaknesses_HasPositiveResult"]);

            DateCreated = TypeCast.ToDateTime(Request.Form["DateCreated"]);
            CreatedBy = TypeCast.ToString(Request.Form["CreatedBy"]);
            LastUpdated = TypeCast.ToDateTimeOrNull(Request.Form["LastUpdated"]);
            LastUpdatedBy = TypeCast.ToString(Request.Form["LastUpdatedBy"]);
            DateDeleted = TypeCast.ToDateTimeOrNull(Request.Form["DateDeleted"]);
            DeletedBy = TypeCast.ToString(Request.Form["DeletedBy"]);
            NaceCode = TypeCast.ToString(Request.Form["NaceCode"]);
            TimeUsage = TypeCast.ToDecimal(Request.Form["TimeUsage"]);
            DateStarted = TypeCast.ToDateTimeOrNull(Request.Form["DateStarted"]);
            DateSentToVolunteer = TypeCast.ToDateTimeOrNull(Request.Form["DateSentToVolunteer"]);
            DateEnded = TypeCast.ToDateTimeOrNull(Request.Form["DateEnded"]);
            Recommendation = TypeCast.ToString(Request.Form["Recommendation"]);
            CrisisCharacter = TypeCast.ToString(Request.Form["CrisisCharacter"]);
            hasProfessionalBoard = TypeCast.ToBool(Request.Form["hasProfessionalBoard"]);
            Weakness_Management = TypeCast.ToBool(Request.Form["Weakness_Management"]);
            Weakness_Finance = TypeCast.ToBool(Request.Form["Weakness_Finance"]);
            Weakness_Production = TypeCast.ToBool(Request.Form["Weakness_Production"]);
            Weakness_Marketing = TypeCast.ToBool(Request.Form["Weakness_Marketing"]);
        }

        public EarlyWarningCompany(ref SqlDataReader dr)
        {
            Id = TypeCast.ToInt(dr["Id"]);
            OrganisationId = TypeCast.ToInt(dr["OrganisationId"]);
            CVR = TypeCast.ToString(dr["CVR"]);
            CompanyName = TypeCast.ToString(dr["CompanyName"]);
            Address = TypeCast.ToString(dr["Address"]);
            Zipcode = TypeCast.ToString(dr["Zipcode"]);
            City = TypeCast.ToString(dr["City"]);
            County = TypeCast.ToString(dr["County"]);
            Phone = TypeCast.ToString(dr["Phone"]);
            Email = TypeCast.ToString(dr["Email"]);
            
            //** Tilføjet region, ESCRM-136/137
            Region = TypeCast.ToString(dr["Region"]);

            Homepage = TypeCast.ToString(dr["Homepage"]);
            CompanyType = TypeCast.ToString(dr["CompanyType"]);
            EstablishmentYear = TypeCast.ToString(dr["EstablishmentYear"]);
            EmployeeCount = TypeCast.ToInt(dr["EmployeeCount"]);
            SectorCodes = TypeCast.ToString(dr["SectorCodes"]);
            Status = TypeCast.ToString(dr["Status"]);
            CurrentConsultant = TypeCast.ToString(dr["CurrentConsultant"]);

            CaseType = TypeCast.ToString(dr["CaseType"]);
            mitErhvervshus = TypeCast.ToBool(dr["mitErhvervshus"]);

            Weaknesses_Management_LackOfCompetences = TypeCast.ToBool(dr["Weaknesses_Management_LackOfCompetences"]);
            Weaknesses_Management_LackOfAdjustingToSales = TypeCast.ToBool(dr["Weaknesses_Management_LackOfAdjustingToSales"]);
            Weaknesses_Management_PersonalProblems = TypeCast.ToBool(dr["Weaknesses_Management_PersonalProblems"]);
            Weaknesses_Management_DisputesInOwnership = TypeCast.ToBool(dr["Weaknesses_Management_DisputesInOwnership"]);
            Weaknesses_Management_Other = TypeCast.ToBool(dr["Weaknesses_Management_Other"]);
            Weaknesses_Management_Other_Text = TypeCast.ToString(dr["Weaknesses_Management_Other_Text"]);
            Weaknesses_Marketing_LackOfStrategicFocus = TypeCast.ToBool(dr["Weaknesses_Marketing_LackOfStrategicFocus"]);
            Weaknesses_Marketing_NoTrackingOfCustomers = TypeCast.ToBool(dr["Weaknesses_Marketing_NoTrackingOfCustomers"]);
            Weaknesses_Marketing_TooDependentOfSingleCustomers = TypeCast.ToBool(dr["Weaknesses_Marketing_TooDependentOfSingleCustomers"]);
            Weaknesses_Marketing_LackOfMonitoringMarket = TypeCast.ToBool(dr["Weaknesses_Marketing_LackOfMonitoringMarket"]);
            Weaknesses_Marketing_LackOfSalesEffort = TypeCast.ToBool(dr["Weaknesses_Marketing_LackOfSalesEffort"]);
            Weaknesses_Marketing_LackOfSearchingSales = TypeCast.ToBool(dr["Weaknesses_Marketing_LackOfSearchingSales"]);
            Weaknesses_Marketing_Other = TypeCast.ToBool(dr["Weaknesses_Marketing_Other"]);
            Weaknesses_Marketing_Other_Text = TypeCast.ToString(dr["Weaknesses_Marketing_Other_Text"]);
            Weaknesses_Production_LackOfOutsourcing = TypeCast.ToBool(dr["Weaknesses_Production_LackOfOutsourcing"]);
            Weaknesses_Production_TroubleWithOutsourcing = TypeCast.ToBool(dr["Weaknesses_Production_TroubleWithOutsourcing"]);
            Weaknesses_Production_Quality = TypeCast.ToBool(dr["Weaknesses_Production_Quality"]);
            Weaknesses_Production_Planning = TypeCast.ToBool(dr["Weaknesses_Production_Planning"]);
            Weaknesses_Production_StockManagementAndProduction = TypeCast.ToBool(dr["Weaknesses_Production_StockManagementAndProduction"]);
            Weaknesses_Production_OldTechnology = TypeCast.ToBool(dr["Weaknesses_Production_OldTechnology"]);
            Weaknesses_Production_Other = TypeCast.ToBool(dr["Weaknesses_Production_Other"]);
            Weaknesses_Production_Other_Text = TypeCast.ToString(dr["Weaknesses_Production_Other_Text"]);
            Weaknesses_Finance_LackOfAccountingUnderstanding = TypeCast.ToBool(dr["Weaknesses_Finance_LackOfAccountingUnderstanding"]);
            Weaknesses_Finance_ExpensiveFinancing = TypeCast.ToBool(dr["Weaknesses_Finance_ExpensiveFinancing"]);
            Weaknesses_Finance_DebitorLoses = TypeCast.ToBool(dr["Weaknesses_Finance_DebitorLoses"]);
            Weaknesses_Finance_AdministrativeMess = TypeCast.ToBool(dr["Weaknesses_Finance_AdministrativeMess"]);
            Weaknesses_Finance_Other = TypeCast.ToBool(dr["Weaknesses_Finance_Other"]);
            Weaknesses_Finance_Other_Text = TypeCast.ToString(dr["Weaknesses_Finance_Other_Text"]);
            Weaknesses_HasPositiveResult = TypeCast.ToBool(dr["Weaknesses_HasPositiveResult"]);

            DateCreated = TypeCast.ToDateTime(dr["DateCreated"]);
            CreatedBy = TypeCast.ToString(dr["CreatedBy"]);
            LastUpdated = TypeCast.ToDateTimeOrNull(dr["LastUpdated"]);
            LastUpdatedBy = TypeCast.ToString(dr["LastUpdatedBy"]);
            DateDeleted = TypeCast.ToDateTimeOrNull(dr["DateDeleted"]);
            DeletedBy = TypeCast.ToString(dr["DeletedBy"]);
            NaceCode = TypeCast.ToString(dr["NaceCode"]);
            TimeUsage = TypeCast.ToDecimal(dr["TimeUsage"]);
            DateStarted = TypeCast.ToDateTimeOrNull(dr["DateStarted"]);
            DateSentToVolunteer = TypeCast.ToDateTimeOrNull(dr["DateSentToVolunteer"]);
            DateEnded = TypeCast.ToDateTimeOrNull(dr["DateEnded"]);
            Recommendation = TypeCast.ToString(dr["Recommendation"]);
            CrisisCharacter = TypeCast.ToString(dr["CrisisCharacter"]);
            hasProfessionalBoard = TypeCast.ToBool(dr["hasProfessionalBoard"]);
            Weakness_Management = TypeCast.ToBool(dr["Weakness_Management"]);
            Weakness_Finance = TypeCast.ToBool(dr["Weakness_Finance"]);
            Weakness_Production = TypeCast.ToBool(dr["Weakness_Production"]);
            Weakness_Marketing = TypeCast.ToBool(dr["Weakness_Marketing"]);
        }
     }
}
