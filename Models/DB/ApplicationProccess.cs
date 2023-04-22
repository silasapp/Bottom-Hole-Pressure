using System;
using System.Collections.Generic;

namespace BHP.Models.DB
{
    public partial class ApplicationProccess
    {
        public int ProccessId { get; set; }
        public int RoleId { get; set; }
        public int Sort { get; set; }
        public int LocationId { get; set; }
        public bool CanPush { get; set; }
        public bool CanWork { get; set; }
        public bool CanInspect { get; set; }
        public bool CanSchdule { get; set; }
        public bool CanReport { get; set; }
        public bool CanAccept { get; set; }
        public bool CanReject { get; set; }
        public int OnAcceptRoleId { get; set; }
        public int OnRejectRoleId { get; set; }
        public string Process { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? UpdatedBy { get; set; }
        public bool DeleteStatus { get; set; }
        public int? DeletedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
        public bool? CanRequest { get; set; }
    }
}
