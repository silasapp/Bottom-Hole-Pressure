using System;
using System.Collections.Generic;

namespace BHP.Models.DB
{
    public partial class MyDesk
    {
        public int DeskId { get; set; }
        public int ProcessId { get; set; }
        public int RequestId { get; set; }
        public int StaffId { get; set; }
        public int Sort { get; set; }
        public bool HasWork { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool HasPushed { get; set; }
        public string Comment { get; set; }
    }
}
