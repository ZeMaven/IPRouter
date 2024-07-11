namespace RemitaProxy.Models.TranQuery
{
    public class TranQueryResponse
    {

        public string? status { get; set; }
        public string? message { get; set; }
        public TranQueryData? data { get; set; }
    }

    public class TranQueryData
    {
        public string? authorizationId { get; set; }
        public string? transactionRef { get; set; }
        public decimal amount { get; set; }
        public decimal feeAmount { get; set; }
        public string? paymentStatus { get; set; }
        public string? transactionDescription { get; set; }
        public string? transactionDate { get; set; }
        public string? paymentDate { get; set; }
        public string? currency { get; set; }
        public string? destinationAccount { get; set; }
        public string? destinationBankCode { get; set; }
        public string? sourceAccount { get; set; }
        public string? sourceBankCode { get; set; }
    }

}
