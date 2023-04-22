using System;
using System.Collections.Generic;

namespace BHP.Models.DB
{
    public partial class Permits
    {
        public int PermitId { get; set; }
        public int? PermitElpsId { get; set; }
        public int RequestId { get; set; }
        public string PermitNo { get; set; }
        public DateTime IssuedDate { get; set; }
        public DateTime ExpireDate { get; set; }
        public bool Printed { get; set; }
        public bool? IsRenewed { get; set; }
        public int ApprovedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public int PermitSequence { get; set; }
    }
}
