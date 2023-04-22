using System;
using System.Collections.Generic;

namespace BHP.Models.DB
{
    public partial class AuditTrail
    {
        public int AuditLogId { get; set; }
        public string AuditAction { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UserId { get; set; }
    }
}
