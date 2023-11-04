namespace Momo.Common.Models
{
    public class NameEnquiryPxRequest
    {
        public string TransactionId { get; set; }
        public string DestinationBankCode { get; set; }
        public string SourceBankCode { get; set; }//Optional
        public string AccountId { get; set; }
    }
}
