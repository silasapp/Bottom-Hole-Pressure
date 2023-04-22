using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BHP.Models.MyModel
{
    public partial class RequestModel
    {
        public int RequestId { get; set; }
        public int CompanyId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string RequestRefNo { get; set; }
        public int ProposalYear { get; set; }
        public bool EmailSent { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? DeletedBy { get; set; }
        public bool? DeletedStatus { get; set; }
        public bool? IsProposalApproved { get; set; }
        
        public DateTime? DeletedAt { get; set; }
        public string Comment { get; set; }
        public int DurationId { get; set; }
        public bool? HasAcknowledge { get; set; }
        public string Status { get; set; }
        public string companyname { get; set; }
        public string CompanyEmail { get; set; }
        public string Address { get; set; }
        public string StateName { get; set; }
        public string IdentificationCode { get; set; }
        
        public bool? IsSubmitted { get; set; }
        public int? DeskId { get; set; }
        public int? CurrentDeskId { get; set; }
        public bool? IsReportApproved { get; internal set; }
    }
}
