using System;
using System.Collections.Generic;

namespace BHP.Models.DB
{
    public partial class Applications
    {
        public int AppId { get; set; }
        public int FacilityId { get; set; }
        public int CompanyId { get; set; }
        public int AppStageId { get; set; }
        public string AppRefNo { get; set; }
        public string WellName { get; set; }
        public string Reserviour { get; set; }
        public string Field { get; set; }
        public string OplOml { get; set; }
        public DateTime ProposedStartDate { get; set; }
        public DateTime ProposedEndDate { get; set; }
        public string Contractor { get; set; }
        public string RigName { get; set; }
        public string RigType { get; set; }
        public string Status { get; set; }
        public bool IsProposedSubmitted { get; set; }
        public bool IsProposedApproved { get; set; }
        public bool IsReportSubmitted { get; set; }
        public bool IsReportApproved { get; set; }
        public DateTime DateApplied { get; set; }
        public DateTime? DateSubmitted { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? DeskId { get; set; }
        public int? CurrentDeskId { get; set; }
        public int? DeletedBy { get; set; }
        public bool? DeletedStatus { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string Comment { get; set; }
        public string Duration { get; set; }
    }
}
