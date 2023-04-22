using System;
using System.Collections.Generic;

namespace BHP.Models.DB
{
    public partial class Companies
    {
        public int CompanyId { get; set; }
        public int CompanyElpsId { get; set; }
        public int RoleId { get; set; }
        public int? LocationId { get; set; }
        public string CompanyName { get; set; }
        public string CompanyEmail { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string StateName { get; set; }
        public bool ActiveStatus { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool DeleteStatus { get; set; }
        public int? DeletedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
        public bool? IsFirstTime { get; set; }
        public string Avarta { get; set; }
        public string IdentificationCode { get; set; }
    }
}
