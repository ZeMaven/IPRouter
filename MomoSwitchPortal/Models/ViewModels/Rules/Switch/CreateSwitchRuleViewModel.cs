using System.ComponentModel.DataAnnotations;

namespace MomoSwitchPortal.Models.ViewModels.Rules.Switch
{
    public class CreateSwitchRuleViewModel
    {
        public int Id { get; set; }
        [Required]
        public string Processor { get; set; }      
        public string NameEnquiryUrl { get; set; }       
        public string TransferUrl { get; set; }       
        public string TranQueryUrl { get; set; }
        public decimal DailyLimit { get; set; }
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }
    }
}
