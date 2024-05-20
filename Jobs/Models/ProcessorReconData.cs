namespace Jobs.Models
{
    public class ProcessorReconData
    {
        public decimal Amount { get; set; }
        public string Processor { get; set; }
        public string  Date { get; set; }

        public string SessionId { get; set; }
        public string PaymentRef { get; set; }
        private string _responseCode;
        public string ResponseCode
        {
            get { return _responseCode; }
            set
            {
                if (
                    value == "00" ||
                    value.Equals("Successful", StringComparison.OrdinalIgnoreCase) ||
                    value.Equals("Approved or Completed Successfully", StringComparison.OrdinalIgnoreCase))
                {
                    _responseCode = "00";
                }
                else
                {
                    _responseCode = "99";
                }
            }
        }
    }
}
