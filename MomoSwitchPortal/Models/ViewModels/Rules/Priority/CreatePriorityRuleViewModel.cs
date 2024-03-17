using System.ComponentModel.DataAnnotations;

namespace MomoSwitchPortal.Models.ViewModels.Rules.Priority
{
    public class CreatePriorityRuleViewModel
    {
        public int Id { get; set; }
        [Required]
        public int Priority { get; set; }      
        public string Rule { get; set; }       

    }
}
