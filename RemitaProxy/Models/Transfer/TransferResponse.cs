namespace RemitaProxy.Models.Transfer
{
    public class TransferResponse
    {
   
        public string? status { get; set; }
        public string? message { get; set; }
        public TransferData? data { get; set; }
    }

    public class TransferData
    {
        public decimal  amount { get; set; }
        public string? transactionRef { get; set; }
        public string? transactionDescription { get; set; }
        public string? authorizationId { get; set; }
        public string? transactionDate { get; set; }
        public string? paymentDate { get; set; }
    }

}
