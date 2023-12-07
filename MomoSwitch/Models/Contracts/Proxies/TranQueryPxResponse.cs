namespace MomoSwitch.Models.Contracts.Proxy
{
    public class TranQueryPxResponse
    {
        public string SessionId { get; set; }
        public string TransactionId { get; set; }

        public string SourceBankCode { get; set; }
             
        public int ChannelCode { get; set; }

        public string ResponseCode { get; set; }
        public string ResponseMessage { get; set; }
       
    }
}
