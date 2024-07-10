namespace RemitaProxy.Models.Auth
{
    public class AuthResponse
    {
    
        public string status { get; set; }
        public string message { get; set; }
        public List<Datum> data { get; set; }
    }

    public class Datum
    {
        public string accessToken { get; set; }
        public int expiresIn { get; set; }
    }

}
