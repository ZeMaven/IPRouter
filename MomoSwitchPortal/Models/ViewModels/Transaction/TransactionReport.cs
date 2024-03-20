using System.ComponentModel.DataAnnotations;

namespace MomoSwitchPortal.Models.ViewModels.Transaction
{
    public class TransactionReport
    {        
        public string Date { get; set; }
        [Display(Name = "Session Id")]
        public string SessionId { get; set; }
        [Display(Name = "Transaction Id")]
        public string TransactionId { get; set; }
        public decimal Amount { get; set; }
        public decimal Fee { get; set; }
        public string Processor { get; set; }
        [Display(Name = "Response Code")]
        public string ResponseCode { get; set; }
        [Display(Name = "Response Message")]
        public string ResponseMessage { get; set; }
        [Display(Name = "Payment Date")]
        public DateTime? PaymentDate { get; set; }
        [Display(Name = "Payment Date")]
        public DateTime? ValidateDate { get; set; }
        [Display(Name = "Payment Reference")]
        public string PaymentReference { get; set; }
        [Display(Name = "Source Bank Code")]
        public string SourceBankCode { get; set; }
        [Display(Name = "Source Account Name")]
        public string SourceAccountName { get; set; }
        [Display(Name = "Source Account Number")]
        public string SourceAccountNumber { get; set; }
        [Display(Name = "Source Bvn")]
        public string SourceBvn { get; set; }
        [Display(Name = "Source Kyc Level")]
        public string SourceKycLevel { get; set; }
        [Display(Name = "Beneficiary Bank Code")]
        public string BenefBankCode { get; set; }
        [Display(Name = "Beneficiary Account Name")]
        public string BenefAccountName { get; set; }
        [Display(Name = "Beneficiary Account Number")]
        public string BenefAccountNumber { get; set; }
        [Display(Name = "Beneficiary BVN")]
        public string BenefBvn { get; set; }
        [Display(Name = "Beneficiary Kyc Level")]
        public string BenefKycLevel { get; set; }
        public string Narration { get; set; }
        [Display(Name = "Channel Code")]
        public string ChannelCode { get; set; }
        [Display(Name = "Manadate Ref")]
        public string ManadateRef { get; set; }
        [Display(Name = "NameEnquiry Ref")]
        public string NameEnquiryRef { get; set; }











    }
}
