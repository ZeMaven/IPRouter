using SwitchPortal.Models.ViewModels.Rules;

namespace SwitchPortal.Models.ViewModels
{
    public class TimeRuleResponse
    {
        public ResponseHeader ResponseHeader { get; set; }
        public List<TimeDetails> TimeDetails { get; set; }
    }
}
