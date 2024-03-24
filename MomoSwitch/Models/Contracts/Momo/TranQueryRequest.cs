using System.ComponentModel.DataAnnotations;

namespace MomoSwitch.Models.Contracts.Momo
{
    public class TranQueryRequest
    {
        [Required]
        [MinLength(30)]
        [MaxLength(30)]
        [RegularExpression("^[0-9]*$", ErrorMessage = "TransactionId should be 30 characters")]

        public string transactionId { get; set; }
    }
}
