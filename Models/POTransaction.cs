using System;

namespace FMS.Models
{
    public class POTransaction
    {
        public int Id { get; set; }
        public string transactionid { get; set; }
        public decimal amount { get; set; }
        public string currencycode { get; set; }
        public DateTime transactiondate { get; set; }
        public string status { get; set; }
    }
}