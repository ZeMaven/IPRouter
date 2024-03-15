using System.ComponentModel.DataAnnotations;

namespace MomoSwitchPortal.Models.ViewModels.User
{
    public class EditUserViewModel
    {
        [Display(Name = "Id")]
        public int Id { get; set; }
        [Required]
        [EmailAddress]
        public string Username { get; set; }
        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }
        [Required]
        [Display(Name = "Entry Date")]
        public string EntryDate { get; set; }
        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }
        public string Password { get; set; }
        [Required]
        [Display(Name = "User Type")]
        public string Role { get; set; }
        public bool IsActive { get; set; }
    }
}
