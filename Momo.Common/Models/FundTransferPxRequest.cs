namespace Momo.Common.Models
{
    public class FundTransferPxRequest
    {
        public string TransactionId { get; set; }
        public decimal Amount { get; set; }
        public string BenefAccountName { get; set; }
        public string BenefAccountNumber { get; set; }
        public string BenefBvn { get; set; }
        public int BenefKycLevel { get; set; }
        public string DestinationBankCode { get; set; }
        public string Narration { get; set; }
        public string SourceBankCode { get; set; }
        public string SourceAccountName { get; set; }
        public string SourceAccountNumber { get; set; }
        public string NameEnquiryRef { get; set; }
        public int ChannelCode { get; set; }

        public string InitiatorKYCLevel { get; set; }
        public string InitiatorBankVerificationNumber { get; set; }
        public string TransactionLocation { get; set; }
    }
}
