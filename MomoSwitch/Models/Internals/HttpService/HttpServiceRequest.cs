namespace MomoSwitch.Models.Internals.HttpService
{
    public class HttpServiceRequest
    {
        public object RequestObject { get; set; }
        public string EndPoint { get; set; }
        public Method Method { get; set; }
        public bool SslOveride { get; internal set; }
    }
    public enum Method { Get = 0, Post = 1 }
}
