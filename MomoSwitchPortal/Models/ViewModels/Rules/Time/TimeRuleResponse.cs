namespace MomoSwitchPortal.Models.ViewModels.Rules.Time
{
    public class TimeRuleResponse
    {
        public ResponseHeader ResponseHeader { get; set; }
        public List<TimeDetails> TimeDetails { get; set; }
        public string Processor { get; set; }
    }
}
