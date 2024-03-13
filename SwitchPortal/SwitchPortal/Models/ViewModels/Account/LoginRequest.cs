using System.ComponentModel.DataAnnotations;

namespace SwitchPortal.Models.ViewModels.Account
{
    public class LoginRequest
    {
        [Required]
        [EmailAddress]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
