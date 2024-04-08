namespace MomoSwitchPortal.Models.ViewModels.Transaction
{
    public class TransactionFilterRequest
    {
        public string Processor { get; set; }
        public string TransactionId { get; set; }
        public string BenefAccountNumber { get; set; }
        public string SourceAccountNumber { get; set; }
        public decimal Amount { get; set; }
        public DateTime? StartDate { get; set; }
        public string TranType { get; set; }
        public DateTime? EndDate { get; set; }
        public string ResponseCode { get; set; }

    }
}
