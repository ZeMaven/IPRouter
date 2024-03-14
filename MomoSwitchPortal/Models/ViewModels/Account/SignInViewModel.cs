using System.ComponentModel.DataAnnotations;

namespace MomoSwitchPortal.Models.ViewModels.Account
{
    public class SignInViewModel
    {
        [EmailAddress]
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
        public bool KeepLoggedIn { get; set; }
    }
}
