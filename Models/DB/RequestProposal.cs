using System;
using System.Collections.Generic;

namespace BHP.Models.DB
{
    public partial class RequestProposal
    {
        public int RequestId { get; set; }
        public int CompanyId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string RequestRefNo { get; set; }
        public bool EmailSent { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? DeletedBy { get; set; }
        public bool? DeletedStatus { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string Comment { get; set; }
        public int DurationId { get; set; }
        public bool? HasAcknowledge { get; set; }
        public string Status { get; set; }
        public bool? IsSubmitted { get; set; }
        public int? DeskId { get; set; }
        public int? CurrentDeskId { get; set; }
        public DateTime? DateApplied { get; set; }
        public DateTime? AcknowledgeAt { get; set; }
        public int? RequestSequence { get; set; }
        public string GeneratedNumber { get; set; }
        public bool? IsProposalApproved { get; set; }
        public bool? IsReportSubmitted { get; set; }
        public bool? IsReportApproved { get; set; }
    }
}
