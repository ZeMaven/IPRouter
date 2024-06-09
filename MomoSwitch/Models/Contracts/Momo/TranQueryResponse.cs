namespace MomoSwitch.Models.Contracts.Momo
{
    public class TranQueryResponse
    {
        public string sourceInstitutionCode { get; set; }
        public string responseCode { get; set; }
        public string responseMessage { get; set; }// not in nibss
        public string code { get; set; }
        public int channelCode { get; set; }
        public string sessionID { get; set; }
      
        public string message { get; set; }
        public string transactionId { get; set; }
        public string processorTranId { get; internal set; }
    }
}
