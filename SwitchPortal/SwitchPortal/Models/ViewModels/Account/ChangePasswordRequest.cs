using System.ComponentModel.DataAnnotations;

namespace SwitchPortal.Models.ViewModels.Account
{
    public class ChangePasswordRequest
    {
        public string Username { get; set; }
        [Required]
        public string OldPassword { get; set; }
        [Required]
        public string NewPassword { get; set; }
    }
}
