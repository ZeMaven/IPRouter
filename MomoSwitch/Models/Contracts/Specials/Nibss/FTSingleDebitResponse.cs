using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MomoSwitch.Models.Contracts.Specials.Nibss
{
    public class FTSingleDebitResponse
    {
        public string SessionId { get; set; }
        public string NameEnquiryRef { get; set; }
        public string DestinationInstitutionCode { get; set; }
        public int ChannelCode { get; set; }
        public string DebitAccountName { get; set; }
        public string DebitAccountNumber { get; set; }
        public string DebitBankVerificationNumber { get; set; }
        public string DebitKycLevel { get; set; }
        public string BeneficiaryAccountName { get; set; }
        public string BeneficiaryAccountNumber { get; set; }
        public string BeneficiaryBankVerificationNumber { get; set; }
        public long BeneficiaryKycLevel { get; set; }
        public string TransactionLocation { get; set; }
        public string Narration { get; set; }
        public string PaymentReference { get; set; }
        public string MandateReferenceNumber { get; set; }
        public string TransactionFee { get; set; }
        public decimal Amount { get; set; }
        public string ResponseCode { get; set; }

    }
}