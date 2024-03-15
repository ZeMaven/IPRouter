namespace MomoSwitchPortal.Models.ViewModels.Rules.BankSwitch
{
    public class BankSwitchResponse
    {
        public ResponseHeader ResponseHeader { get; set; }
        public List<BankSwitchDetails> BankSwitchDetails { get; set; }
        public string Processor { get; set; }
        public string BankCode { get; set; }
    }
}
