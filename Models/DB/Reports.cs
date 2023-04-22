using System;
using System.Collections.Generic;

namespace BHP.Models.DB
{
    public partial class Reports
    {
        public int ReportId { get; set; }
        public int RequestId { get; set; }
        public int StaffId { get; set; }
        public string Comment { get; set; }
        public string Subject { get; set; }
        public int? ElpsDocId { get; set; }
        public int? AppDocId { get; set; }
        public string DocSource { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? DeletedBy { get; set; }
        public bool? DeletedStatus { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
