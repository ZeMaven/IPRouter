using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MomoSwitch.Models.Contracts.Specials.Nibss
{
    public class MandateAdviceRequest
    {

        public string SessionID { get; set; }
        public string DestinationInstitutionCode { get; set; }
        public int ChannelCode { get; set; }
        public string MandateReferenceNumber { get; set; }
        public decimal Amount { get; set; }
        public string DebitAccountName { get; set; }
        public string DebitAccountNumber { get; set; }
        public string DebitBankVerificationNumber { get; set; }
        public int DebitKYCLevel { get; set; }
        public string BeneficiaryAccountName { get; set; }
        public string BeneficiaryAccountNumber { get; set; }
        public string BeneficiaryBankVerificationNumber { get; set; }
        public string BeneficiaryKYCLevel { get; set; }
    }
}