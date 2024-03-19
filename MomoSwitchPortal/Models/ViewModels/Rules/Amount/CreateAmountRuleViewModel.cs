using System.ComponentModel.DataAnnotations;

namespace MomoSwitchPortal.Models.ViewModels.Rules.Amount
{
    public class CreateAmountRuleViewModel
    {
        public int Id { get; set; }
        [Required]
        public string Processor { get; set; }
        public decimal AmountA { get; set; }
        public decimal AmountZ { get; set; }
    }
}
