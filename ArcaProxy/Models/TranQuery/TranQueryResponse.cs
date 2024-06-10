namespace ArcaProxy.Models.TranQuery
{
    public class TranQueryResponse
    {
   
        public string requestId { get; set; }
        public string accountTransferId { get; set; }
        public string transactionId { get; set; }
        public object sessionId { get; set; }
        public string status { get; set; }
        public string statusReason { get; set; }
        public string responseMessage { get; set; }
    }

}
