using System;
using System.Collections.Generic;

namespace BHP.Models.DB
{
    public partial class RequestReminders
    {
        public int ReminderId { get; set; }
        public int RequestId { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }
        public DateTime SentAt { get; set; }
        public bool IsSent { get; set; }
    }
}
