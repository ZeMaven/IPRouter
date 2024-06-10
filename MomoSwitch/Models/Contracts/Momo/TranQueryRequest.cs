using System.ComponentModel.DataAnnotations;

namespace MomoSwitch.Models.Contracts.Momo
{
    public class TranQueryRequest
    {
        [Required]     
        [RegularExpression("^[0-9]*$", ErrorMessage = "TransactionId should be number characters")]

        //public string sessionId { get; set; }

        public string transactionId { get; set; }
    }
}
;