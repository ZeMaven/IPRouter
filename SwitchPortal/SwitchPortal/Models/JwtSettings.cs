namespace SwitchPortal.Models
{
    public class JwtSettings
    {
        public string Secret { get; set; }
        public TimeSpan TokenLifeTime { get; set; }
        public int RefreshExpiration { get; set; }
    }
}
