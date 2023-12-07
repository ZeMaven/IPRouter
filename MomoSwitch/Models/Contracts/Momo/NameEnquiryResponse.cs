namespace MomoSwitch.Models.Contracts.Momo
{
    public class NameEnquiryResponse
    {
        public string accountName { get; set; }
        public string accountNumber { get; set; }
        public string bankVerificationNumber { get; set; }
        public int channelCode { get; set; }
        public string destinationInstitutionCode { get; set; }
        public string kycLevel { get; set; }
        public string responseCode { get; set; }
        public string responseMessage { get; set; }// not in Nibss
        public string sessionID { get; set; }
        public string transactionId { get; set; }

    }
}
