using System;
using System.Collections.Generic;

namespace BHP.Models.DB
{
    public partial class OutOfOffice
    {
        public int OutId { get; set; }
        public int StaffId { get; set; }
        public int ReliverId { get; set; }
        public string Comment { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public int? DeletedBy { get; set; }
        public string Status { get; set; }
        public bool? DeletedStatus { get; set; }
    }
}
