using System.ComponentModel.DataAnnotations;

namespace SwitchPortal.Models.Dtos
{
    public class ForgotPasswordDto
    {        
        [EmailAddress]
        public string Username { get; set; }
        public string Key { get; set; }
        public string NewPassword { get; set; }       
        public string ConfirmPassword { get; set; }
    }
}
