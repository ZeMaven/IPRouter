namespace MomoSwitchPortal.Models.ViewModels.Profile
{
    public class ProfileDetailsViewModel
    {
        public ResponseHeader ResponseHeader { get; set; }
        public ProfileDetail ProfileDetails { get; set; }
    }

    public class ProfileDetail
    {
        public string FullName { get; set; }
        public string Username { get; set; }
        public string Role { get; set; }
        public string Joined { get; set; }
    }
}
