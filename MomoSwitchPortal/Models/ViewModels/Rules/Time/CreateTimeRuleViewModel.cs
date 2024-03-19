using System.ComponentModel.DataAnnotations;

namespace MomoSwitchPortal.Models.ViewModels.Rules.Time
{
    public class CreateTimeRuleViewModel
    {        
        public int Id { get; set; }
        [Required]
        public string TimeA { get; set; }
        [Required]
        public string TimeZ { get; set; }
        [Required]
        public string Processor { get; set; }
    }
}
