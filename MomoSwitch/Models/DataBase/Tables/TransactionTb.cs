using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace MomoSwitch.Models.DataBase.Tables
{

    [Index(nameof(SessionId), IsUnique = true)]
    [Index(nameof(TransactionId), IsUnique = true)]
    [Index(nameof(Date))]
    public class TransactionTb
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public DateTime? PaymentDate { get; set; }
        public DateTime? ValidateDate { get; set; }
        public string Processor { get; set; }
        public string TransactionId { get; set; }
        public string SessionId { get; set; }
        public string PaymentReference { get; set; }
        public string BenefBankCode { get; set; }
        public string BenefAccountName { get; set; }
        public string BenefAccountNumber { get; set; }
        public string BenefBvn { get; set; }
        public string BenefKycLevel { get; set; }
        public string Narration { get; set; }
        public string SourceAccountName { get; set; }
        public string SourceAccountNumber { get; set; }
        public string SourceBvn { get; set; }
        public string SourceKycLevel { get; set; }
        public string SourceBankCode { get; set; }
        public string ChannelCode { get; set; }
        public string ManadateRef { get; set; }
        public string NameEnquiryRef { get; set; }
        public decimal Amount { get; set; }
        public decimal Fee { get; set; }
        public string ResponseCode { get; set; }
        public string ResponseMessage { get; set; }
    }
}
