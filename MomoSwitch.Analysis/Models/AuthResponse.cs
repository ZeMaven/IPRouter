namespace MomoSwitch.Analysis.Models
{
    public class AuthResponse
    {
        public string token_type { get; set; }
        public string access_token { get; set; }
        public int expires_in { get; set; }
        public int ext_expires_in { get; set; }

        public string status { get; set; }
    }
}
