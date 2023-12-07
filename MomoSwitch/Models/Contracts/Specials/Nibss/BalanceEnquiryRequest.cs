using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MomoSwitch.Models.Contracts.Specials.Nibss
{
    public class BalanceEnquiryRequest
    {
        public string SessionID { get; set; }
        public string DestinationInstitutionCode { get; set; }
        public string ChannelCode { get; set; }
        public string AuthorizationCode { get; set; }
        public string TargetAccountName { get; set; }
        public string TargetBankVerificationNumber { get; set; }
        public string TargetAccountNumber { get; set; }

    }
}