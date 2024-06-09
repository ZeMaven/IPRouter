using System;
using System.Text.Json.Serialization;

namespace Momo.Common.Models
{
    public class FundTransferPxResponse
    {
        public string TransactionId { get; set; }
        public string SessionId { get; set; }
        public decimal Amount { get; set; }
        public string BenefAccountName { get; set; }
        
        public string BenefAccountNumber { get; set; }
        public string BenfBankCode { get; set; }
    
        public string BenefBvn { get; set; }
        public int BenefKycLevel { get; set; }      
        public string Narration { get; set; }
        public string SourceBankCode { get; set; }
        public string SourceAccountName { get; set; }
        public string SourceAccountNumber { get; set; }
        public string NameEnquiryRef { get; set; }
        public string ResponseCode { get; set; }
        public string ResponseMessage { get; set; }
        public int ChannelCode { get;   set; }
        public DateTime TransactionDate { get; set; }
        public string ProcessorTranId { get; set; }
    }
}
