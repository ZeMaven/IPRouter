namespace EtransactProxy.Models.Transfer
{
    public class TransferRequest 
    {
        public string action { get; set; }
        public string terminalId { get; set; }
        public Transaction transaction { get; set; }
       
    }
 

    public class Transaction
    {
        public string pin { get; set; }
        public string senderName { get; set; }
        public string bankCode { get; set; }
        public decimal amount { get; set; }
        public string description { get; set; }
        public string destination { get; set; }
        public string reference { get; set; }
        public string endPoint { get; set; }
    }
}
