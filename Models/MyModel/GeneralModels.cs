using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BHP.Models.MyModel
{
    public class GeneralModels
    {

    }
    public class PaymentDetailsSubmit
    {
        public int RequestID { get; set; }
        public string RefNo { get; set; }
        public string Status { get; set; }
        public string AppType { get; set; }
        public string AppStage { get; set; }
        public string CompanyName { get; set; }
        public int? Amount { get; set; }
        public int? ServiceCharge { get; set; }
        public int? TotalAmount { get; set; }
        public string ShortName { get; set; }
        public int FacID { get; set; }
        public DateTime DateApplied { get; set; }
        public string rrr { get; set; }
        public int SurChargeAmount { get; set; }
        public string Description { get; set; }
    }
}
