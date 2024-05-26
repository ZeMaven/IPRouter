namespace EtransactProxy.Models.TranQuery
{
    public class TranQueryRequest
    {
        public string action { get; set; }
        public string terminalId { get; set; }
        public QTransaction transaction { get; set; }
    }
    public class QTransaction 
    {
        public string pin { get; set; }
        public string description { get; set; }
        public string reference { get; set; }
        public string lineType { get; set; }
    }
}
