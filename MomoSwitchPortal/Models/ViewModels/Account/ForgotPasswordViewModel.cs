using System.ComponentModel.DataAnnotations;

namespace MomoSwitchPortal.Models.ViewModels.Account
{
    public class ForgotPasswordViewModel
    {
        public string Key { get; set; }
        [Required]
        public string Username { get; set; }
        [Required]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z\d]).{8,}$", ErrorMessage = "Password must contain at least one capital letter, one number, one special character")]
        public string Password { get; set; }
        [Required]
        [Compare(nameof(Password), ErrorMessage = "Confirm Password must be the same as Password")]
        public string ConfirmPassword { get; set; }
    }
}
