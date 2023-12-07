namespace MomoSwitch.Models.Contracts.Specials.Router
{
    public class NameEnquiryPxResponse
    {
        public string ProxySessionId { get; set; }
        public string TransactionId { get; set; }
        public string SessionId { get; set; }
        public string DestinationBankCode { get; set; }
        public string SourceBankCode { get; set; }//remove. but momo is to be set up on switch, proxy will handle this
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public string KycLevel { get; set; }
        public string ResponseCode { get; set; }
        public string ResponseMessage { get; set; }
        public string Bvn { get; set; }
    }
}
