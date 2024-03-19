namespace MomoSwitchPortal.Models.ViewModels.User
{
    public class UserDetailsViewModel
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Role { get; set; }
        public bool IsActive { get; set; }
        public DateTime EntryDate { get; set; }

    }
}
