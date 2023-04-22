using System;
using System.Collections.Generic;

namespace BHP.Models.DB
{
    public partial class NominatedStaff
    {
        public int NominateId { get; set; }
        public int RequestId { get; set; }
        public int StaffId { get; set; }
        public string Designation { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public bool? HasSubmitted { get; set; }
        public string DocSource { get; set; }
        public int? ElpsDocId { get; set; }
        public int? AppDocId { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool? IsActive { get; set; }
        public string Title { get; set; }
        public string Contents { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public string RespondStatus { get; set; }
        public string RespondComment { get; set; }
    }
}
