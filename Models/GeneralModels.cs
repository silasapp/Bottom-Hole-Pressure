
using BHP.Models.DB;
using System;
using System.Collections.Generic;

namespace BHP.Models
{
   
    public class UserLogins
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string HostName { get; set; }
        public string MacAddress { get; set; }
        public string LocalIp { get; set; }
        public string RemoteIp { get; set; }
        public string UserAgent { get; set; }
        public string Status { get; set; }
        public DateTime LogInTime { get; set; }
        public DateTime? LogOutTime { get; set; }
        public string FieldOffice { get; set; }
    }


    public class RequestModal
    {
        public string RequestId { get; set; }
        public int CompanyId { get; set; }
        public string RequestRefNo { get; set; }
        public string CompanyName { get; set; }
        public string CompanyAddress { get; set; }
        public string CompanyEmail { get; set; }
        public string ProposalYear { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string EmailSent { get; set; }
        public string CreatedAt { get; set; }
        public string UpdatedAt { get; set; }
        public string DeletedAt { get;  set; }
        public bool DeletedStatus { get;  set; }
        public int Year { get;  set; }
        public bool ActiveStatus { get;  set; }
        public string Acknowledge { get;  set; }
        public string AcknowledgeAt { get;  set; }
        public string GeneratedRef { get;  set; }
    }


    public class StaffPushApps
    {
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string Email { get; set; }
        public int DeskCount { get; set; }
        public int FieldOffice { get; set; }
        public int StaffId { get;  set; }
    }


    public class ProposalRequestLetterModel
    {
        public string CompanyName { get;  set; }
        public string CompanyAddress { get;  set; }
        public string CompanyState { get;  set; }
        public int Year { get;  set; }
        public string CompanyCity { get;  set; }
        public DateTime StartDate { get;  set; }
        public DateTime EndDate { get;  set; }
        public string ProposalRefNo { get;  set; }
        public DateTime? ReportEndDate { get;  set; }
        public int PreviousYear { get;  set; }
        public string GeneratedNo { get;  set; }
        public string ContactName { get;  set; }
    }


    public class ProposalRequestLetter
    {
        public List<ProposalRequestLetterModel> letterModels { get; set; }
    }


    public class AppDocuument
    {
        public int LocalDocID { get; set; }
        public string DocName { get; set; }
        public int EplsDocTypeID { get; set; }
        public int CompanyDocID { get; set; }
        public string DocType { get; set; }
        public string DocSource { get; set; }
    }

    public class History
    {
        public string StaffEmail { get; set; }
        public string Status { get; set; }
        public string Comment { get; set; }
        public string HistoryDate { get; set; }
        public string ActionFrom { get;  set; }
        public string ActionTo { get;  set; }
    }

    public class NominationRequests
    {
        public string Staff { get; set; }
        public string hasDone { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ReminderedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string Comment { get; set; }
    }

    public class AppReport
    {
        public int StaffID { get; set; }
        public int ReportID { get; set; }
        public string Staff { get; set; }
        public string Comment { get; set; }
        public string CreatedAt { get; set; }
        public string UpdatedAt { get; set; }
        public string Title { get;  set; }
    }


    public class AppSchdule
    {
        public int SchduleID { get; set; }
        public string SchduleType { get; set; }
        public string SchduleLocation { get; set; }
        public string SchduleDate { get; set; }
        public string SchduleExpired { get; set; }
        public string cResponse { get; set; }
        public string SchduleComment { get; set; }
        public string CreatedAt { get; set; }
        public string UpdatedAt { get; set; }
        public int SchduleByID { get; set; }
        public string SchduleByEmail { get; set; }
        public string CustomerComment { get; set; }
        public string sResponse { get; set; }
        public string SupervisorComment { get; set; }
    }


    public class CurrentDesk
    {
        public string Staff { get; set; }
    }


    public class RequestApplication
    {
        public int DeskId { get; set; }
        public string RefNo { get; set; }
        public int RequestId { get; set; }
        public string CompanyName { get; set; }
        public string CompanyAddress { get; set; }
        public string CompanyEmail { get; set; }
        public int? TotalAmount { get; set; }
        public int Year { get; set; }
        public string Status { get;  set; }
        public string RRR { get;  set; }
        public string TransType { get;  set; }
        public int? AmountPaid { get;  set; }
        public int? ServiceCharge { get;  set; }
        public DateTime? TransDate { get;  set; }
        public string TransDescription { get;  set; }
        public string TransStatus { get;  set; }
        public DateTime? DateApplied { get;  set; }
        public string Comment { get;  set; }
        public DateTime ReportDate { get;  set; }
        public string Staff { get;  set; }
        public string GeneratedNo { get;  set; }
        public string ReportApproved { get;  set; }
        public string ProposalApproved { get;  set; }
        public string CompanyElpsId { get;  set; }
        public object CompanyElpsID { get; internal set; }
        public string Subject { get; internal set; }
        public string DocSource { get; internal set; }
        public DateTime? UpdatedAt { get; internal set; }
        public int NominationID { get; internal set; }
        public string Titile { get; internal set; }
        public string Content { get; internal set; }
    }


    public class RequestApplicationModel
    {
        public List<History> histories { get; set; }
        public List<RequestApplication> requestApplications { get; set; }
        public List<CurrentDesk> currentDesks { get; set; }
        public List<AppSchdule> appSchdules { get; set; }
        public List<AppReport> appReports { get; set; }
        public List<ApplicationProccess> applicationProccesses { get; set; }
        public List<Application> applications { get; set; }
        public List<SurveyReport> surveyReports { get; set; }
        public List<AppDocuument> appDocuuments { get; set; }
        public List<Nomination> nominations { get; set; }
        public List<StaffNomination> staffs { get; set; }
        public List<BothDocuments> bothDocuments { get;  set; }
        public List<NominationRequests> NominationRequest { get; internal set; }
    }



    public class Nomination
    {
        public int NominationID { get; set; }
        public string StaffName { get; set; }
        public string UserRoles { get; set; }
        public string StaffEmail { get;  set; }
        public int? CreatedBy { get;  set; }
        public string Location { get;  set; }
        public string Designation { get;  set; }
        public string PhoneNumber { get;  set; }
        public bool? hasSubmitted { get; internal set; }
    }

    public class ReminderModel
    {
        public string RefNo { get; set; }
        public int Year { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }
        public DateTime SentAt { get; set; }
        public bool IsSent { get;  set; }
    }


    public class HistoryInformation
    {
        public List<RequestApplication> requestApplications { get; set; }
        public List<History> histories { get; set; }
    }


    public class AppMessage
    {
        public object Subject { get; set; }
        public object Content { get; set; }
        public object RefNo { get; set; }
       
        public object Status { get; set; }
        public object TotalAmount { get; set; }
        public object Seen { get; set; }
        public object CompanyDetails { get; set; }
        public string GeneratedNo { get;  set; }
    }


    public class MyPermit
    {
        public int PermitID { get; set; }
        public string PermitNo { get; set; }
        public string RefNo { get; set; }
        public DateTime IssuedDate { get; set; }
        public DateTime ExpireDate { get; set; }
        public bool isPrinted { get; set; }
        public string CompanyName { get; set; }
        public int CompanyID { get; set; }
        public string CompanyEmail { get;  set; }
        public string CompanyAddress { get;  set; }
    }

    public class PermitViewModel
    {
        public List<PermitModel> permitModels { get; set; }
        public List<wells> wells { get; set; }

        public List<string> infos { get; set; }
        public List<Nomination> nominations { get; set; }

        public int totalWell { get; set; }
    }




    public class TransactionDetails
    {
        public string RefNo { get; set; }
        public string RRR { get; set; }
        public string CompanyName { get; set; }
        public int? Amount { get; set; }
        public int? TotalAmount { get; set; }
        public int? ServiceCharge { get; set; }
        public DateTime TransDate { get; set; }
        public string TransStatus { get; set; }
        public string TransType { get; set; }
        public string TransRef { get; set; }
        public string Description { get; set; }
    }



    public class CriteriaModel
    {
        public List<Application> applications { get; set; }
        public List<Criteria> criterias { get; set; }
        public List<SurveyCriteria>  surveyCriterias { get; set; }
        public List<SurveyReport>  surveyReports { get; set; }
    }



    public class Criteria
    {
        public string Reservoir { get; set; }
        public int NoOfSurvey { get; set; }
        public int ExpectedNoOfSurvey { get; set; }
        public string SurveyFrequecy { get; set; }
        public int YearsOfProduction { get; set; }
        public decimal InitialPressure { get; set; }
        public decimal CurrentPressure { get; set; }
        public decimal? PreviousPressure { get; set; }
        public decimal BubblePointPressure { get; set; }
        public DateTime? DateApplied { get; set; }
        public DateTime ProductionYear { get; set; }
        public string ResultDescription { get; set; }
        public string ReservoirState { get; set; }
        public string PressureDecline { get; set; }
    }



    public class SurveyCriteria
    {
        public decimal Oil { get; set; }
        public decimal InitialPressure { get; set; }
        public decimal SBHP_MP { get; set; }
        public decimal RemainingReservers { get; set; }
        public decimal PressureDecline { get; set; }
        public decimal FractionalRecovery { get; set; }
        public decimal AltimateRecovery { get; set; }
        public string Reservoir  { get; set; }
        public string Recommendation  { get; set; }
    }


    public class SchdulesList
    {
        public Schdules Schedule { get; set; }
        public RequestDuration Duration { get; set; }
        public RequestProposal Request { get; set; }
    }


    public class PermitModel
    {
        public int PermitID { get; set; }
        public int RequestID { get; set; }
        public string RefNo { get; set; }
        public string PermitNO { get; set; }
        public DateTime? DateApplied { get; set; }
        public string IssuedDate { get; set; }
        public string ExpiaryDate { get; set; }
        public int CompanyID { get; set; }
        public string CompanyName { get; set; }
        public string CompanyAddress { get; set; }
        public string CompanyCity { get; set; }
        public string CompanyState { get; set; }
        public string Signature { get; set; }
        public byte[] QrCode { get; set; }
        public int TotalAmount { get; set; }
        public string PayDescriiption { get;  set; }
        public int TotalField { get;  set; }
        public int TotalReservior { get;  set; }
        public int TotalWell { get;  set; }
        public string MyCompanyDetails { get;  set; }
        public string GeneratedRef { get;  set; }
        public int Year { get;  set; }
        public string Status { get;  set; }
        public string RRR { get;  set; }
        public string CompanyCode { get;  set; }
        public string StaffName { get;  set; }
    }


    public class wells
    {
        public string Well { get; set; }
        public int Count { get; set; }
        public string FieldName { get;  set; }
    }


    public class PermitView
    {
        public string PermitNO { get; set; }
        public string ViewType { get; set; }
        public DateTime? PreviewedAt { get; set; }
        public DateTime? DownloadedAt { get; set; }
        public string UserDetails { get; set; }
    }


    public class MyApps
    {
         public string isDone { get; set; }

        public int RequestId { get; set; }
        public string RefNo { get; set; }
        public string CompanyName { get; set; }
        public string CompanyAddress { get; set; }
        public string CompanyEmail { get; set; }
        public int CompanyID { get; set; }
        public string Status { get; set; }
        public string Rate { get; set; }
        public string DateApplied { get; set; }
        public string isSubmitted { get; set; }
        public bool? DeletedStatus { get; set; }
        public string HasAck { get; set; }
        public int Year { get;  set; }
        public string ScheduleType { get;  set; }
        public string ScheduleLocation { get;  set; }
        public DateTime ScheduleDate { get;  set; }
        public string Comment { get;  set; }
        public int CustomerAccept { get;  set; }
        public string CustomerComment { get;  set; }
        public string StaffName { get;  set; }
        public int StaffID { get;  set; }
        public DateTime CreatedAt { get;  set; }
        public DateTime? UpdateAt { get;  set; }
        public int AppID { get;  set; }
        public int DeskID { get;  set; }
        public bool HasWorked { get;  set; }
        public string Staff { get;  set; }
        public string Email { get;  set; }
        public string isProposalApproved { get;  set; }
        public string isReportApproved { get;  set; }
        public string isReportSubmitted { get;  set; }
    }


    public class MySchdule
    {
        public int ScheduleID { get; set; }
        public string ScheduleDate { get; set; }
        public string ScheduleBy { get; set; }
        public string CompanyName { get; set; }
        public int CompanyID { get; set; }
        public int staffID { get; set; }
        public int? CustomerResponse { get; set; }
        public string StaffComment { get; set; }
        public string SupervisorComment { get; set; }
        public string CustomerComment { get; set; }
        public string ScheduleType { get; set; }
        public string ScheduleLocation { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? Supervisor { get; set; }
        public int? SupervisorApproved { get; set; }
        public int ProposedYear { get; set; }
    }


    public class StaffDesk
    {
        public string StaffName { get; set; }
        public string StaffEmail { get; set; }
        public string FieldOffice { get; set; }
        public int StaffID { get; set; }
        public string StaffRole { get; set; }
        public int AppCount { get; set; }
        public string ActiveStatus { get; set; }
        public string DeletedStatus { get; set; }
        public int AllAppCount { get; set; }
    }



    public class NominationRequestList
    {
        public string StaffName { get; internal set; }
        public string Email { get; internal set; }
        public string Role { get; internal set; }
        public string RefNo { get; internal set; }
        public int AppId { get; internal set; }
        public int StaffId { get; internal set; }
        public bool isDone { get; internal set; }
        public string Comment { get; internal set; }
        public int RequestId { get; internal set; }
        public int ZonalOffice { get; internal set; }
        public int FieldOffice { get; internal set; }
        public int NominationRequestId { get; internal set; }
    }



    public class ReportViewModel
    {
        public string RefNo { get; set; }
        public string CompanyName { get; set; }
        public string CompanyAddress { get; set; }
        public string CompanyEmail { get; set; }
        public string Stage { get; set; }
        public string Status { get; set; }
        public DateTime DateApplied { get; set; }
        public DateTime? DateSubmitted { get; set; }
        public string Staff { get; set; }
        public DateTime ReportDate { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string Subject { get; set; }
        public string Comment { get; set; }
        public string DocSource { get; set; }
        public string WellName { get; set; }
    }



    public class NominatedList
    {
        public string AppRef { get; internal set; }
        public bool? hasSubmitted { get; internal set; }
        public DateTime CreatedAt { get; internal set; }
        public bool? isActive { get; internal set; }
        public string CompanyName { get; internal set; }
        public string StaffName { get; internal set; }
        public int RequestId { get; internal set; }
        public int NominationID { get; internal set; }
        public int Duration { get; internal set; }
        public int? PermitId { get; internal set; }
    }



    public class StaffNomination
    {
        public string FullName { get; set; }
        public int StaffId { get; set; }
        public string StaffEmail { get; set; }
        public string FieldOffice { get; set; }
        public string Division { get; set; }
        public string Phone { get; set; }
        public string RoleName { get; internal set; }
        public string ZonalOffice { get; internal set; }
    }


    public class PresentDocuments
    {
        public int SubmitDocID { get; set; }
        public bool Present { get; set; }
        public string FileName { get; set; }
        public string Source { get; set; }
        public int CompElpsDocID { get; set; }
        public int DocTypeID { get; set; }
        public int LocalDocID { get; set; }
        public string DocType { get; set; }
        public string TypeName { get; set; }
    }

    public class OtherDocuments
    {
        public int LocalDocID { get; set; }
        public string DocName { get; set; }
    }


    public class NominationReport
    {
        public List<BothDocuments> bothDocuments { get; set; }
        public List<RequestApplication> requestApplications { get; set; }


    }


    public class MissingDocument
    {
        public int SubmitDocID { get; set; }
        public bool Present { get; set; }
        public string FileName { get; set; }
        public string Source { get; set; }
        public int CompElpsDocID { get; set; }
        public int DocTypeID { get; set; }
        public int LocalDocID { get; set; }
        public string DocType { get; set; }
        public string TypeName { get; set; }
    }


    public class BothDocuments
    {
        public List<PresentDocuments> presentDocuments { get; set; }
        public List<MissingDocument> missingDocuments { get; set; }
    }


    public class SubmitDoc
    {
        public int LocalDocID { get; set; }
        public int CompElpsDocID { get; set; }
        public string DocSource { get; set; }
    }


}
