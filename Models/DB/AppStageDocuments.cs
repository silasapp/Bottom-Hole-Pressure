using System;
using System.Collections.Generic;

namespace BHP.Models.DB
{
    public partial class AppStageDocuments
    {
        public int StageDocId { get; set; }
        public int AppStageId { get; set; }
        public int AppDocId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool DeleteStatus { get; set; }
        public int? DeletedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
