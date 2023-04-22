using System;
using System.Collections.Generic;

namespace BHP.Models.DB
{
    public partial class RequestDuration
    {
        public int DurationId { get; set; }
        public DateTime RequestStartDate { get; set; }
        public DateTime RequestEndDate { get; set; }
        public int ProposalYear { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public bool? DeleteStatus { get; set; }
        public int? DeletedBy { get; set; }
        public DateTime? ReportEndDate { get; set; }
        public string ContactPerson { get; set; }
    }
}
