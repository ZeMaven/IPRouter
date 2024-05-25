namespace EtransactProxy.Models.TranQuery
{
    public class TranQueryResponse
    {
        public string direction { get; set; }
        public string reference { get; set; }
        public decimal amount { get; set; }
        public int totalFailed { get; set; }
        public int totalSuccess { get; set; }
        public string error { get; set; }
        public string message { get; set; }
        public string action { get; set; }
        public string otherReference { get; set; }
        public int? records { get; set; }
        public decimal openingBalance { get; set; }
        public decimal closingBalance { get; set; }
    }
}
