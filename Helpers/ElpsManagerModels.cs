using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace LpgLicense.Models
{
    public class ElpsResponse
    {
        public ElpsResponse() { }
        public string message { get; set; }
        public object value { get; set; }
    }

    

    public class ResponseWrapper
    {
        public ResponseWrapper() { }
        public bool status { get; set; }
        public string value { get; set; }
    }

    public class CompanyDetail
    {
        public string user_Id { get; set; }
        public string name { get; set; }
        public string business_Type { get; set; }
        public string registered_Address_Id { get; set; }
        public string operational_Address_Id { get; set; }
        public string affiliate { get; set; }
        public string nationality { get; set; }
        public string contact_FirstName { get; set; }
        public string contact_LastName { get; set; }
        public string contact_Phone { get; set; }
        public string year_Incorporated { get; set; }
        public string rC_Number { get; set; }
        public string tin_Number { get; set; }
        public string no_Staff { get; set; }
        public string no_Expatriate { get; set; }
        public string total_Asset { get; set; }
        public string yearly_Revenue { get; set; }
        public string accident { get; set; }
        public string accident_Report { get; set; }
        public string training_Program { get; set; }
        public string mission_Vision { get; set; }
        public string hse { get; set; }
        public string hseDoc { get; set; }
        public string date { get; set; }
        public string isCompleted { get; set; }
        public string elps_Id { get; set; }
        public int id { get; set; }
    }


    

    public class Document
    {
        public int id { get; set; }
        public int companyId { get; set; }
        public string fileName { get; set; }
        public string source { get; set; }
        public string document_type_id { get; set; }
        public string documentTypeName { get; set; }
        public string date_modified { get; set; }
        public string date_added { get; set; }
        public string status { get; set; }
        public string archived { get; set; }
        public string document_Name { get; set; }
        public string uniqueId { get; set; }
    }



    public class Lineitem
    {
        public string lineItemsId { get; set; }
        public string beneficiaryName { get; set; }
        public string beneficiaryAccount { get; set; }
        public string bankCode { get; set; }
        public string beneficiaryAmount { get; set; }
        public string deductFeeFrom { get; set; }
    }

    public class PaymentRequest
    {
        public string serviceTypeId { get; set; }
        public string categoryName { get; set; }
        public string totalAmount { get; set; }
        public string payerName { get; set; }
        public string payerEmail { get; set; }
        public string serviceCharge { get; set; }
        public string amountDue { get; set; }
        //public string payerPhone { get; set; }
        public string orderId { get; set; }
        public string returnSuccessUrl { get; set; }
        public string returnFailureUrl { get; set; }
        public string returnBankPaymentUrl { get; set; }
        public Lineitem[] lineItems { get; set; }
        public int[] documentTypes { get; set; }
        //public Applicationitem[] applicationItems { get; set; }

    }

    public class PaymentResponse
    {
        public object statusMessage { get; set; }
        public string appId { get; set; }
        public string status { get; set; }
        public string rrr { get; set; }
        public object transactiontime { get; set; }
        public string transactionId { get; set; }
        public List<object> requiredDocs { get; set; }
    }

    public class TransactionStatus
    {
        public string statusmessage { get; set; }
        public object merchantId { get; set; }
        public string status { get; set; }
        public string rrr { get; set; }
        public string transactiontime { get; set; }
        public string orderId { get; set; }
        public object statuscode { get; set; }
    }

    public class Branch
    {
        public string name { get; set; }
        public int stateId { get; set; }
        public string address { get; set; }
        public string stateName { get; set; }
        public string countryName { get; set; }
        public int countryId { get; set; }
        public bool isFieldOffice { get; set; }
        public bool selected { get; set; }
        public int id { get; set; }
    }

    public class Staff
    {
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string userId { get; set; }
        public string email { get; set; }
        public object phoneNo { get; set; }
        public int id { get; set; }
    }

    public class ChangePassword
    {
        public string oldPassword { get; set; }
        public string newPassword { get; set; }
        public string confirmPassword { get; set; }
    }

    public class ChangePasswordResponse
    {
        public int code { get; set; }
        public string msg { get; set; }
    }

    public class StatesInZone
    {
        public int stateId { get; set; }
        public int zoneId { get; set; }
        public string stateName { get; set; }
        public string zoneName { get; set; }
        public int id { get; set; }
    }


    public class TransactionStatusResponse
    {
        public string statusmessage { get; set; }
        public object merchantId { get; set; }
        public string status { get; set; }
        public string rrr { get; set; }
        public string transactiontime { get; set; }
        public string orderId { get; set; }
        public object statuscode { get; set; }
    }

    public class ZoneMapping
    {
        public string name { get; set; }
        public string branchName { get; set; }
        public string code { get; set; }
        public int branchId { get; set; }
        public int stateId { get; set; }
        public string address { get; set; }
        public string stateName { get; set; }
        public string countryName { get; set; }
        public int countryId { get; set; }
        public List<StatesInZone> coveredStates { get; set; }
        public List<Branch> coveredFieldOffices { get; set; }
        public int id { get; set; }
    }

    public class Zones
    {
        public string name { get; set; }
        public string branchName { get; set; }
        public string code { get; set; }
        public int branchId { get; set; }
        public int stateId { get; set; }
        public string address { get; set; }
        public string stateName { get; set; }
        public string countryName { get; set; }
        public int countryId { get; set; }
        public object coveredStates { get; set; }
        public object coveredFieldOffices { get; set; }
        public int id { get; set; }
    }

    public class PermitDTO
    {
        public string permit_No { get; set; }
        public string orderId { get; set; }
        public int company_Id { get; set; }
        public string date_Issued { get; set; }
        public string date_Expire { get; set; }
        public string categoryName { get; set; }
        public string is_Renewed { get; set; }
        public int licenseId { get; set; }
        public int id { get; set; }
    }

    public class PermitsDTO
    {
        public string permit_No { get; set; }
        public string orderId { get; set; }
        public string companyName { get; set; }
        public int company_Id { get; set; }
        public string date_Issued { get; set; }
        public string date_Expire { get; set; }
        public string categoryName { get; set; }
        public int licenseId { get; set; }
        public string licenseName { get; set; }
        public string licenseShortName { get; set; }
        public string business_Type { get; set; }
        public string rrr { get; set; }
        public string city { get; set; }
        public string stateName { get; set; }
        public string jS_Combined { get; set; }
        public int id { get; set; }
    }

    public class CompanyAddressDTO
    {
        public string address_1 { get; set; }
        public string address_2 { get; set; }
        public string city { get; set; }
        public string country_Id { get; set; }
        public string stateId { get; set; }
        public string countryName { get; set; }
        public string stateName { get; set; }
        public string postal_code { get; set; }
        public string type { get; set; }
        public int id { get; set; }
    }


    public class Addresss
    {
        public string address_1 { get; set; }
        public string address_2 { get; set; }
        public string city { get; set; }
        public string postal_Code { get; set; }
        public int stateId { get; set; }
        public int country_Id { get; set; }
        public string type { get; set; }
        public int id { get; set; }
        public string stateName { get; set; }
    }



    public class CompanyDetailsModel
    {
        public CompanyDetail compProfile { get; set; }
        public CompanyAddressDTO Address { get; set; }
        public Directors Directors { get; set; }
        public CompanyKeyStaffs KeyStaffs { get; set; }
    }

    
    public class Directors
    {
        public int company_Id { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public int address_Id { get; set; }
        public string telephone { get; set; }
        public int nationality { get; set; }
        public int id { get; set; }
        public Address address { get; set; }
    }



    public class Address
    {
        public string address_1 { get; set; }
        public string address_2 { get; set; }
        public string city { get; set; }
        public string postal_Code { get; set; }
        public int stateId { get; set; }
        public int country_Id { get; set; }
        public string type { get; set; }
        public int id { get; set; }
        public string stateName { get; set; }
    }


    public class CompanyInformationModel
    {
        public CompanyDetail company { get; set; }
        //public CompanyAddressDTO Address { get; set; }
        //public CompanyAddressDTO operationalAddress { get; set; }
        public List<Address> address { get; set; }
        //public List<Address> addresss { get; set; }
    }


    public class CompanyChangeModel
    {
        public string Name { get; set; }
        public string RC_Number { get; set; }
        public string Business_Type { get; set; }
        public bool emailChange { get; set; }
        public int CompanyId { get; set; }
        public string NewEmail { get; set; }
    }

    public class CompanyProfile
    {
        public string CName { get; set; }
        public string ContactPersonFirstName { get; set; }
        public string ContactPersonLastName { get; set; }
        public string YearIncoperated { get; set; }
        public string TIN { get; set; }
        public string TotalAssets { get; set; }
        public string BusinessType { get; set; }
        public string RegistrationNumber { get; set; }
        public string NoOfStaff { get; set; }
        public string YearlyRevenue { get; set; }
        public string CompanyEmail { get; set; }
        public string Affiliate { get; set; }
        public string ContactPersonTelephoneNo { get; set; }
        public string Nationality { get; set; }
        public string NoOfExpatriates { get; set; }

        public string CompanyNSTIFSection { get; set; }
        public string MedicalRetainship { get; set; }
        public string ProfessionalOrganisation { get; set; }
        public string RequestForExpatriatQuota { get; set; }
        public string TechnicalAgreement { get; set; }
        public string NoOfAccident { get; set; }
        public string AccidentReport { get; set; }
        public string TrainingProgram { get; set; }
        public string MissionAndVission { get; set; }

        public string HSEDocument { get; set; }
    }

    public class CompanyAddress
    {
        public string Address1 { get; set; }
        public string Country { get; set; }
        public string Address2 { get; set; }
        public string State { get; set; }
        public string City { get; set; }
    }

    public class CompanyDirectors
    {
        public List<String> DirectorNames { get; set; }
    }

    public class CompanyKeyStaffs
    {
        public List<String> KeyStaffNames { get; set; }
    }


    public class State
    {
        public int CountryId { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
    }


    public class Facility
    {
        public string Name { get; set; }
        public int CompanyId { get; set; }
        public DateTime DateAdded { get; set; }
        public string StreetAddress { get; set; }
        public string City { get; set; }
        public int StateId { get; set; }
        public int LGAId { get; set; }
        public string FacilityType { get; set; }
        public int Id { get; set; }
        public List<FacilityDocument> facilityDocuments { get; set; }
    }

    public class FacilityDocument
    {
        public int Company_Id { get; set; }
        public int Document_Type_Id { get; set; }
        public int FacilityId { get; set; }
        public string Name { get; set; }
        public string Source { get; set; }
        public DateTime Date_Modified { get; set; }
        public DateTime Date_Added { get; set; }
        public bool Status { get; set; }
        public bool Archived { get; set; }
        public string UniqueId { get; set; }
        public string Document_Name { get; set; }
        public DateTime Expiry_Date { get; set; }
        public int Id { get; set; }
    }


    public class Application
    {
        public string OrderId { get; set; }
        public string Status { get; set; }
        public string CategoryName { get; set; }
        public int CompanyId { get; set; }
        public int LicenseId { get; set; }
        public DateTime Date { get; set; }
        public string LicenseName { get; set; }
        public int Id { get; set; }
    }


    public class CompanyModelAPI
    {
        public Company Company { get; set; }
        public List<vCompanyMedical> CompanyMedicals { get; set; }
        public List<vCompanyExpatriateQuota> CompanyExpatriateQuotas { get; set; }
        public List<vCompanyNsitf> CompanyNsitfs { get; set; }
        public List<vCompanyProffessional> CompanyProffessionals { get; set; }
        public List<vCompanyTechnicalAgreement> CompanyTechnicalAgreements { get; set; }
    }


    public class Company
    {
        public DateTime? Date { get; set; }
        public bool? HSEDoc { get; set; }
        [Display(Name = "Health & Safety Environment (HSE)")]
        public string Hse { get; set; }
        [Display(Name = "Mission & Vision")]
        public string Mission_Vision { get; set; }
        [Display(Name = "Training Program")]
        public string Training_Program { get; set; }
        [Display(Name = "Accident/Incident Report")]
        public string Accident_Report { get; set; }
        [Display(Name = "No. of Accident/Incident")]
        [Range(0, 1000)]
        public int Accident { get; set; }
        [Display(Name = "Yearly Revenue")]
        [StringLength(50)]
        public string Yearly_Revenue { get; set; }
        [Display(Name = "Total Assets")]
        [StringLength(50)]
        public string Total_Asset { get; set; }
        [Display(Name = "No. of Expatriates")]
        [Range(0, 1000000)]
        public int? No_Expatriate { get; set; }
        [Display(Name = "No. of Staff")]
        [Range(0, 1000000)]
        public int? No_Staff { get; set; }
        public int? Elps_Id { get; set; }
        [Display(Name = "TIN")]
        [StringLength(50)]
        public string Tin_Number { get; set; }
        [Display(Name = "Year Incorporated")]
        public int? Year_Incorporated { get; set; }
        [Display(Name = "Contact Person's Telephone")]
        [Required]
        [StringLength(15)]
        public string Contact_Phone { get; set; }
        [Display(Name = "Contact Person's Last Name")]
        [StringLength(50)]
        public string Contact_LastName { get; set; }
        [Display(Name = "Contact Person's First Name")]
        [StringLength(50)]
        public string Contact_FirstName { get; set; }
        [StringLength(50)]
        public string Nationality { get; set; }
        [StringLength(100)]
        public string Affiliate { get; set; }
        [Display(Name = "Operational Address")]
        public int? Operational_Address_Id { get; set; }
        [Display(Name = "Registered Address")]
        public int? Registered_Address_Id { get; set; }
        [Display(Name = "Business Type")]
        [Required]
        [StringLength(25)]
        public string Business_Type { get; set; }
        [Display(Name = "Company Name")]
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        [Display(Name = "Company Email")]
        public string User_Id { get; set; }
        [Display(Name = "Registration Number")]
        [Required]
        [StringLength(50)]
        public string RC_Number { get; set; }
        public string Id { get; set; }
    }


    public class vCompanyMedical
    {
        public int Id { get; set; }
        public string Medical_Organisation { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public int Company_Id { get; set; }
        public DateTime Date_Issued { get; set; }
        public int? FileId { get; set; }
        public string FileName { get; set; }
        public string FileSource { get; set; }
    }

    public class vCompanyExpatriateQuota
    {
        public int Id { get; set; }
        public int Company_Id { get; set; }
        public string Name { get; set; }
        public int? FileId { get; set; }
        public string FileName { get; set; }
        public string FileSource { get; set; }
    }

    public class vCompanyNsitf
    {
        public int Id { get; set; }
        public int No_People_Covered { get; set; }
        public string Policy_No { get; set; }
        public int Company_Id { get; set; }
        public DateTime Date_Issued { get; set; }
        public int? FileId { get; set; }
        public string FileName { get; set; }
        public string FileSource { get; set; }
        [NotMapped]
        public int Elps_Id { get; set; }
    }

    public class vCompanyProffessional
    {
        public int Id { get; set; }
        public string Proffessional_Organisation { get; set; }
        public string Cert_No { get; set; }
        public int Company_Id { get; set; }
        public DateTime Date_Issued { get; set; }
        public int? FileId { get; set; }
        public string FileName { get; set; }
        public string FileSource { get; set; }
    }

    public class vCompanyTechnicalAgreement
    {
        public int Id { get; set; }
        public int Company_Id { get; set; }
        public string Name { get; set; }
        public int? FileId { get; set; }
        public string FileName { get; set; }
        public string FileSource { get; set; }
    }


    public class RemitaSplit
    {
        //public string merchantId { get; set; }
        public string serviceTypeId { get; set; }
        public string totalAmount { get; set; }
        public string hash { get; set; }
        public string payerName { get; set; }
        public string payerEmail { get; set; }
        public string payerPhone { get; set; }
        public string orderId { get; set; }
        public List<RPartner> lineItems { get; set; }
        public string ServiceCharge { get; set; }
        public string AmountDue { get; set; }
        public string ReturnSuccessUrl { get; set; }
        public string ReturnFailureUrl { get; set; }
        public string ReturnBankPaymentUrl { get; set; }
        public List<int> DocumentTypes { get; set; }
        public string CategoryName { get; set; }
        public List<ApplicationItem> ApplicationItems { get; set; }
    }



    public class RPartner
    {
        public string lineItemsId { get; set; }
        public string beneficiaryName { get; set; }
        public string beneficiaryAccount { get; set; }
        public string bankCode { get; set; }
        public string beneficiaryAmount { get; set; }
        public string deductFeeFrom { get; set; }
    }

    public class ApplicationItem
    {
        public string Group { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}