namespace ArcaProxy.Models.Transfer
{
    public class TransferResponse
    {

        public string requestId { get; set; }
        public string accountTransferId { get; set; }
        public string transactionId { get; set; }
        public string sessionId { get; set; }
        public string responseCode { get; set; }
        public string responseCodeReason { get; set; }
        public string responseMessage { get; set; }
    }
}
