using System;
using System.Collections.Generic;

namespace BHP.Models.DB
{
    public partial class AppDeskHistory
    {
        public int HistoryId { get; set; }
        public int RequestId { get; set; }
        public string ActionFrom { get; set; }
        public string ActionTo { get; set; }
        public string Status { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
