using System.ComponentModel.DataAnnotations;

namespace MomoSwitchPortal.Models.ViewModels.Rules.BankSwitch
{
    public class CreateBankSwitchRuleViewModel
    {
        public int Id { get; set; }
        [Required]
        public string BankCode { get; set; }
        [Required]
        public string Processor { get; set; }
    }
}
