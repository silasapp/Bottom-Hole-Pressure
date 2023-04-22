using System;
using System.Collections.Generic;

namespace BHP.Models.DB
{
    public partial class Schdules
    {
        public int SchduleId { get; set; }
        public int RequestId { get; set; }
        public int SchduleBy { get; set; }
        public string SchduleType { get; set; }
        public string SchduleLocation { get; set; }
        public DateTime SchduleDate { get; set; }
        public int? Supervisor { get; set; }
        public int? SupervisorApprove { get; set; }
        public int? CustomerAccept { get; set; }
        public string Comment { get; set; }
        public string CustomerComment { get; set; }
        public string SupervisorComment { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? DeletedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
        public bool? DeletedStatus { get; set; }
        public bool? IsDone { get; set; }
    }
}
