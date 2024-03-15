using System.ComponentModel.DataAnnotations;

namespace MomoSwitchPortal.Models.ViewModels.Profile
{
    public class EditProfileViewModel
    {
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }        
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
        [Compare(nameof(NewPassword), ErrorMessage = "Confirm Password must be the same as New Password")]
        public string ConfirmPassword { get; set; }
    }
}
