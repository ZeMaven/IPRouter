using System.ComponentModel.DataAnnotations;

namespace MomoSwitchPortal.Models.ViewModels.User
{
    public class EditUserViewModel
    {
        [Required]
        [EmailAddress]
        public string Username { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        public string Password { get; set; }
        [Required]
        public string Role { get; set; }
        public bool IsActive { get; set; }
    }
}
