namespace MomoSwitchPortal.Actions
{
    public static class Utilities
    {
        public static string GetLoggedInUser(this HttpContext httpContext)
        {
            return httpContext.User == null ? string.Empty : httpContext.User.Claims.Single(x => x.Type == "username").Value;
        }
    }
}
