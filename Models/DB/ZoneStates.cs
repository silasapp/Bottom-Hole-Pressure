using System;
using System.Collections.Generic;

namespace BHP.Models.DB
{
    public partial class ZoneStates
    {
        public int ZoneStatesId { get; set; }
        public int ZoneId { get; set; }
        public int StateId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool DeleteStatus { get; set; }
        public int? DeletedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
