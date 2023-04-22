using System;
using System.Collections.Generic;

namespace BHP.Models.DB
{
    public partial class Staff
    {
        public int StaffId { get; set; }
        public string StaffElpsId { get; set; }
        public int FieldOfficeId { get; set; }
        public int RoleId { get; set; }
        public int? LocationId { get; set; }
        public string StaffEmail { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Theme { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool ActiveStatus { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool DeleteStatus { get; set; }
        public int? DeletedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public string SignaturePath { get; set; }
        public string SignatureName { get; set; }
    }
}
