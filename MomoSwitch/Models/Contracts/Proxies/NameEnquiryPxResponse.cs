namespace MomoSwitch.Models.Contracts.Proxy
{
    public class NameEnquiryPxResponse
    {
        public string TransactionId { get; set; }
        public string Sessionid { get; set; }
        public string DestinationBankCode { get; set; }
        public string SourceBankCode { get; set; }//remove. but momo is to be set up on switch, proxy will handle this
        public string Accountnumber { get; set; }

    }
}
