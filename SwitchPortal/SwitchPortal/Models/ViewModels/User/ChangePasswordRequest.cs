using System.ComponentModel.DataAnnotations;

namespace SwitchPortal.Models.ViewModels.User
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
