using System;
using System.Collections.Generic;

namespace BHP.Models.DB
{
    public partial class ApplicationStage
    {
        public int AppStageId { get; set; }
        public string StageName { get; set; }
        public string ShortName { get; set; }
        public int? Amount { get; set; }
        public int? ServiceCharge { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool DeleteStatus { get; set; }
        public int? DeletedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
