using SwitchPortal.Actions;
using System.Security.Claims;

namespace SwitchPortal.Models
{
    public class User
    {
        public string Username { get; set; }
        public bool IsActive { get; set; }

        public static User FromClaimsPrincipal(ClaimsPrincipal principal) => new()
        {
            Username = principal.FindFirstValue("username")
        };
    }
}
