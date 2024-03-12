using SwitchPortal.Actions;
using System.Security.Claims;

namespace SwitchPortal.Models.Dtos
{
    public class UserDto
    {
        public string Username { get; set; }
        public bool IsActive { get; set; }

        public static UserDto FromClaimsPrincipal(ClaimsPrincipal principal) => new()
        {
            Username = principal.FindFirstValue("username")
        };
    }
}
