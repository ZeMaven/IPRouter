namespace MomoSwitch.Models.Contracts.Momo
{
    public class FundTransferRequest
    {
        public decimal amount { get; set; }
        public string beneficiaryAccountName { get; set; }
        public string beneficiaryAccountNumber { get; set; }
        public string beneficiaryBankVerificationNumber { get; set; }
        public int beneficiaryKYCLevel { get; set; }
        public string beneficiaryNarration { get; set; }
        public string billerId { get; set; }
        public int channelCode { get; set; }
        public string destinationInstitutionCode { get; set; }
        public string mandateReferenceNumber { get; set; }
        public string nameEnquiryRef { get; set; }
        public string originatorAccountName { get; set; }
        public string originatorAccountNumber { get; set; }
        public string originatorBankVerificationNumber { get; set; }
        public int originatorKYCLevel { get; set; }
        public string originatorNarration { get; set; }
        public string paymentReference { get; set; }
        public string sourceInstitutionCode { get; set; }
        public string transactionId { get; set; }
        public string transactionLocation { get; set; }
    }
}
