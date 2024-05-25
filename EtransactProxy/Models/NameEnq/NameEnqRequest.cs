namespace EtransactProxy.Models.NameEnq
{
    public class NameEnqRequest
    {
        public string action { get; set; }
        public string terminalId { get; set; }
        public NameEnqTransaction transaction { get; set; }
    }
    public class NameEnqTransaction
    {
        public string pin { get; set; }
        public string bankCode { get; set; }
        public decimal amount { get; set; }
        public string description { get; set; }
        public string destination { get; set; }
        public string reference { get; set; }
        public string endPoint { get; set; }
    }
}
