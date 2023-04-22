using System;
using System.Collections.Generic;

namespace BHP.Models.DB
{
    public partial class Facilities
    {
        public int FacilityId { get; set; }
        public int? ElpsFacilityId { get; set; }
        public int CompanyId { get; set; }
        public string FacilityName { get; set; }
        public string FacilityAddress { get; set; }
        public int State { get; set; }
        public string Lga { get; set; }
        public string City { get; set; }
        public int? LandMeters { get; set; }
        public bool? IsPipeLine { get; set; }
        public bool? IsHighTention { get; set; }
        public bool? IsHighWay { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public int? DeletedBy { get; set; }
        public bool DeleteStatus { get; set; }
        public string ContactName { get; set; }
        public string ContactPhone { get; set; }
    }
}
