using System;
using System.Collections.Generic;

namespace BHP.Models.DB
{
    public partial class Location
    {
        public int LocationId { get; set; }
        public string LocationName { get; set; }
        public DateTime CreatedAt { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? UpdatedBy { get; set; }
        public bool DeleteStatus { get; set; }
        public DateTime? DeletedAt { get; set; }
        public int? DeletedBy { get; set; }
    }
}
