using MomoSwitch.Models.Contracts.Momo;

namespace MomoSwitch.Models.Internals
{
    public class GetTransactionResponse
    {
        public ResponseHeader ResponseHeader { get; set; }
        public FundTransferResponse Transaction   { get; set; }
    }
}
