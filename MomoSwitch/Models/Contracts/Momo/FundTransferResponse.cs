namespace MomoSwitch.Models.Contracts.Momo
{
    public class FundTransferResponse
    {
        public string responseCode { get; set; }
        public string sessionID { get; set; }
        public string transactionId { get; set; }
        public int channelCode { get; set; }
        public string nameEnquiryRef { get; set; }
        public string destinationInstitutionCode { get; set; }
        public string debitAccountName { get; set; }
        public string debitAccountNumber { get; set; }
        public string debitBankVerificationNumber { get; set; }
        public int debitKYCLevel { get; set; }
        public string beneficiaryAccountName { get; set; }
        public string beneficiaryAccountNumber { get; set; }
        public string beneficiaryBankVerificationNumber { get; set; }
        public object beneficiaryKYCLevel { get; set; }
        public object transactionLocation { get; set; }
        public string narration { get; set; }
        public string paymentReference { get; set; }
        public string mandateReferenceNumber { get; set; }
        public decimal transactionFee { get; set; }
        public decimal amount { get; set; }
    }
}
