using System;
using System.Collections.Generic;

namespace BHP.Models.DB
{
    public partial class PermitHistory
    {
        public int PermitHistoryId { get; set; }
        public int PermitId { get; set; }
        public string ViewType { get; set; }
        public string UserDetails { get; set; }
        public DateTime? PreviewedAt { get; set; }
        public DateTime? DownloadedAt { get; set; }
    }
}
