namespace MomoSwitchPortal.Models.ViewModels.Home
{
    public class HomeMiniTransaction
    {
        public DateTime Date { get; set; }
        public string TransactionId { get; set; }
        public string BenefBankName { get; set; }
        public string SourceBankName { get; set; }
        public decimal Amount { get; set; }
        public string ResponseCode { get; set; }
    }
}
