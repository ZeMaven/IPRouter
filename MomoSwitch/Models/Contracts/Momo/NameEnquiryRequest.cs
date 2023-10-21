namespace MomoSwitch.Models.Contracts.Momo
{
    public class NameEnquiryRequest
    {
        public string accountNumber { get; set; }
        public int channelCode { get; set; }
        public string destinationInstitutionCode { get; set; }
        public string transactionId { get; set; }
    }
}
