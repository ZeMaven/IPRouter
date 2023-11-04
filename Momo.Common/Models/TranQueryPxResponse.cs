using System;

namespace Momo.Common.Models
{
    public class TranQueryPxResponse
    {
        public string SessionId { get; set; }
        public string TransactionId { get; set; }

        public string SourceBankCode { get; set; }
        public string SourceAccountNumber { get; set; }
        public string SourceAccountName { get; set; }

        public string BenefAccountName { get; set; }
        public string BenefAccountNumber { get; set; }
        public string BenfBankCode { get; set; }
       
        public int ChannelCode { get; set; }

        public string ResponseCode { get; set; }
        public string ResponseMessage { get; set; }
        public decimal Amount { get; set; }

        public string Narration { get; set; }
        public DateTime Date { get; set; }
    }
}
