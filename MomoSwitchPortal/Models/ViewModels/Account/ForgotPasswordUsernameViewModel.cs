using System.ComponentModel.DataAnnotations;

namespace MomoSwitchPortal.Models.ViewModels.Account
{
    public class ForgotPasswordUsernameViewModel
    {
        [EmailAddress]
        public string? Username { get; set; }
        public bool EmailSent { get; set; }
    }
}
