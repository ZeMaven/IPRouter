using System.ComponentModel.DataAnnotations;

namespace MomoSwitchPortal.Models.ViewModels.User
{
    public class CreateUserViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Username")]
        public string Username { get; set; }
        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }
        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }        
        [Required]
        [Display(Name = "User Type")]

        public string Role { get; set; }
    }
}
