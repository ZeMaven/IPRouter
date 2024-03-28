using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace MomoSwitch.Models.Contracts.Momo
{
    public class FundTransferRequest
    {
        
        public decimal amount { get; set; }
        public string beneficiaryAccountName { get; set; }
        [Required]
        [MinLength(10)]
        [MaxLength(10)]
        [RegularExpression("^[0-9]*$", ErrorMessage = "beneficiaryAccountNumber should be 10 number characters")]

        public string beneficiaryAccountNumber { get; set; }

        public string beneficiaryBankVerificationNumber { get; set; }
        public object beneficiaryKYCLevel { get; set; }
        public string beneficiaryNarration { get; set; }
        public string billerId { get; set; }
        public int channelCode { get; set; }

        [Required]
        [MinLength(6)]
        [MaxLength(6)]
        [RegularExpression("^[0-9]*$", ErrorMessage = "destinationInstitutionCode should be 6 number characters")]

        public string destinationInstitutionCode { get; set; }
        public string mandateReferenceNumber { get; set; }
        public string nameEnquiryRef { get; set; }
        public string initiatorAccountName { get; set; }
        [Required]
        [MinLength(10)]
        [MaxLength(10)]
        [RegularExpression("^[0-9]*$", ErrorMessage = "initiatorAccountNumber should be 10 number characters")]

        public string initiatorAccountNumber { get; set; } //cust acc

        public string initiatorBankVerificationNumber { get; set; }
        public int InitiatorKYCLevel { get; set; }
        public int originatorKYCLevel { get; set; }
        public string originatorNarration { get; set; }
        [RegularExpression("^[0-9]*$", ErrorMessage = "paymentReference should be number characters")]
        public string paymentReference { get; set; }

        [Required]
        [MinLength(6)]
        [MaxLength(6)]
        [RegularExpression("^[0-9]*$")]
        public string sourceInstitutionCode { get; set; }
        [Required]
        [RegularExpression("^[0-9]*$", ErrorMessage = "TransactionId should be number characters")]
        public string transactionId { get; set; }
        public string transactionLocation { get; set; }


        public string originatorAccountName { get; set; }
        [Required]
        [MinLength(10)]
        [MaxLength(10)]
        [RegularExpression("^[0-9]*$", ErrorMessage = "originatorAccountNumber should be 10 number characters")]

        public string originatorAccountNumber { get; set; } //pull acc

        public string originatorBankVerificationNumber { get; set; }
    }
}