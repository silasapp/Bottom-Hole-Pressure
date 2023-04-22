using System;
using System.Collections.Generic;

namespace BHP.Models.DB
{
    public partial class NominationRequest
    {
        public int RequestId { get; set; }
        public int AppId { get; set; }
        public int StaffId { get; set; }
        public string Comment { get; set; }
        public bool HasDone { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? ReminderDate { get; set; }
    }
}
