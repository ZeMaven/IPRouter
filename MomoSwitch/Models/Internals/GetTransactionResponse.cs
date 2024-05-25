using MomoSwitch.Models.Contracts.Momo;
using Momo.Common.Models;

namespace MomoSwitch.Models.Internals
{
    public class GetTransactionResponse
    {
        public ResponseHeader ResponseHeader { get; set; }
        public FundTransferResponse Transaction   { get; set; }
    }
}
