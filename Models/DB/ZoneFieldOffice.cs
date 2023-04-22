using System;
using System.Collections.Generic;

namespace BHP.Models.DB
{
    public partial class ZoneFieldOffice
    {
        public int ZoneFieldOfficeId { get; set; }
        public int ZoneId { get; set; }
        public int FieldOfficeId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool DeleteStatus { get; set; }
        public int? DeletedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
