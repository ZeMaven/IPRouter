using System.ComponentModel.DataAnnotations;

namespace MomoSwitchPortal.Models.ViewModels.Account
{
    public class ForgotPasswordViewModel
    {
        public string Key { get; set; }
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        [Compare(nameof(Password), ErrorMessage = "This must be the same as Password")]
        public string ConfirmPassword { get; set; }
    }
}
