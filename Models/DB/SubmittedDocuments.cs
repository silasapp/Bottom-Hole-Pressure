using System;
using System.Collections.Generic;

namespace BHP.Models.DB
{
    public partial class SubmittedDocuments
    {
        public int SubDocId { get; set; }
        public int RequestId { get; set; }
        public int AppDocId { get; set; }
        public int? CompElpsDocId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool DeletedStatus { get; set; }
        public int? DeletedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string DocSource { get; set; }
    }
}
