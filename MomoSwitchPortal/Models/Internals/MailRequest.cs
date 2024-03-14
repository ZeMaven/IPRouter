namespace MomoSwitchPortal.Models.Internals
{
    public class MailRequest
    {
        public string To { get; set; }
        public string ToName { get; set; }
        public string From { get; set; }
        public string FromName { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }
}
