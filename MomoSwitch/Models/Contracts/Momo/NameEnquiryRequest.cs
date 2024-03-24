using System.ComponentModel.DataAnnotations;

namespace MomoSwitch.Models.Contracts.Momo
{
    public class NameEnquiryRequest
    {
        [Required]
        [MaxLength(10)]
        [MinLength(10)]
        [RegularExpression("^[0-9]*$", ErrorMessage = "Account number should be 10 characters")]
        public string accountNumber { get; set; }
        public int channelCode { get; set; }
        [Required]
        [MaxLength(6)]
        [MinLength(6)]
        [RegularExpression("^[0-9]*$", ErrorMessage = "Bank code should be 6 characters")]
        public string destinationInstitutionCode { get; set; }
        [Required]    
     
        [RegularExpression("^[0-9]*$", ErrorMessage = "TransactionId should be number characters")]
        public string transactionId { get; set; }
    }
}
