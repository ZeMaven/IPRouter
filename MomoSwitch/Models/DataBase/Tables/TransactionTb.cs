using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
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
        [StringLength(50)]
        public string Processor { get; set; }
        [StringLength(50)]
        public string TransactionId { get; set; }
        [StringLength(50)]
        public string SessionId { get; set; }
        [StringLength(50)]
        public string PaymentReference { get; set; }
        [StringLength(50)]
        public string BenefBankCode { get; set; }
        [StringLength(50)]
        public string BenefAccountName { get; set; }
        [StringLength(50)]
        public string BenefAccountNumber { get; set; }
        [StringLength(50)]
        public string BenefBvn { get; set; }
        [StringLength(50)]
        public string BenefKycLevel { get; set; }
        [StringLength(50)]
        public string Narration { get; set; }
        [StringLength(50)]
        public string SourceAccountName { get; set; }
        [StringLength(50)]
        public string SourceAccountNumber { get; set; }
        [StringLength(50)]
        public string SourceBvn { get; set; }
        [StringLength(50)]
        public string SourceKycLevel { get; set; }
        [StringLength(50)]
        public string SourceBankCode { get; set; }
        [StringLength(50)]
        public string ChannelCode { get; set; }
        [StringLength(50)]
        public string ManadateRef { get; set; }
        [StringLength(50)]
        public string NameEnquiryRef { get; set; }
    
        public decimal Amount { get; set; }
        public decimal Fee { get; set; }
        public string ResponseCode { get; set; }
        public string ResponseMessage { get; set; }
    }
}
