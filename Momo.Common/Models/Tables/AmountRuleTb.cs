using System.ComponentModel.DataAnnotations;

namespace Momo.Common.Models.Tables
{
    public class AmountRuleTb
    {
        public int Id { get; set; }
        [Range(1, 100000, ErrorMessage = "Please enter valid doubleNumber")]
        public decimal AmountA { get; set; }
        [Required]
        public decimal AmountZ { get; set; }
        [StringLength(50)]
        [Required, Display(Name = "Processor")]
        public string Processor { get; set; }
    }
}
