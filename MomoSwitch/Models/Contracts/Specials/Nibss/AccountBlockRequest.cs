using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MomoSwitch.Models.Contracts.Specials.Nibss
{
    public class AccountBlockRequest
    {
        public string SessionID { get; set; }
        public string DestinationInstitutionCode { get; set; }
        public int ChannelCode { get; set; }
        public string ReferenceCode { get; set; }
        public string TargetAccountName { get; set; }
        public string TargetBankVerificationNumber { get; set; }
        public string TargetAccountNumber { get; set; }
        public string ReasonCode { get; set; }
        public string Narration { get; set; }
    }
}