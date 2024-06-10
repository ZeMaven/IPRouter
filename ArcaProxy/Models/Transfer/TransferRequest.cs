namespace ArcaProxy.Models.Transfer
{
    public class TransferRequest
    {
  
        public string accountNumber { get; set; }
        public string accountName { get; set; }
        public string institutionCode { get; set; }
        public string narration { get; set; }
        public decimal amount { get; set; }
        public string currency { get; set; }
        public DateTime transactionDate { get; set; }
        public string requestId { get; set; }
        public string transferInitiatornName { get; set; }
        public string svaCode { get; set; }
    }

}
