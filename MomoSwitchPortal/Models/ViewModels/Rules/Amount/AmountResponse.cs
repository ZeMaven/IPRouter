namespace MomoSwitchPortal.Models.ViewModels.Rules.Amount
{
    public class AmountResponse
    {
        public ResponseHeader ResponseHeader { get; set; }
        public List<AmountDetails> AmountDetails { get; set; }
        public string Processor { get; set; }

    }
}
