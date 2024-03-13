using System.ComponentModel.DataAnnotations;

namespace SwitchPortal.Models.ViewModels.Account
{
    public class CreateUserRequest
    {
        [Required]
        [EmailAddress]
        public string Username { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string Role { get; set; }
        public string LoggedInUser { get; set; }
    }
}
