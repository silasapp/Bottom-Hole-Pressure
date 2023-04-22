using System;
using System.Collections.Generic;

namespace BHP.Models.DB
{
    public partial class Transactions
    {
        public int TransactionId { get; set; }
        public int? ElpsTransId { get; set; }
        public string TransRef { get; set; }
        public int RequestId { get; set; }
        public string TransactionType { get; set; }
        public string TransactionStatus { get; set; }
        public int? AmtPaid { get; set; }
        public int? ServiceCharge { get; set; }
        public int? TotalAmt { get; set; }
        public DateTime TransactionDate { get; set; }
        public string Description { get; set; }
        public string Rrr { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
