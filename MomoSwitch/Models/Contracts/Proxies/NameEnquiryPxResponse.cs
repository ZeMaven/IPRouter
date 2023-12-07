namespace MomoSwitch.Models.Contracts.Proxy
{
    public class NameEnquiryPxResponse
    {
        public string TransactionId { get; set; }
        public string SessionId { get; set; }
        public string DestinationBankCode { get; set; }
        public string SourceBankCode { get; set; }//remove. but momo is to be set up on switch, proxy will handle this
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public int KycLevel { get; set; }
        public string ResponseCode { get; set; }
        public string Bvn { get; set; }
    }
}
