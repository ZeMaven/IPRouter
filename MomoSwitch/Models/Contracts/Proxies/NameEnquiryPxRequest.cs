namespace MomoSwitch.Models.Contracts.Proxy
{
    public class NameEnquiryPxRequest
    {
        public string TransactionId { get; set; }
        public string DestinationBankCode { get; set; }
        public string AccountId { get; set; }
    }
}
