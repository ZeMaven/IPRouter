using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NipInwardProxy.Models.Nibss
{
    public class NESingleResponse
    {
        public string SessionID { get; set; }
        public string DestinationInstitutionCode { get; set; }
        public int ChannelCode { get; set; }
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public string BankVerificationNumber { get; set; }
        public string KYCLevel { get; set; }
        public string ResponseCode { get; set; }
    }
}