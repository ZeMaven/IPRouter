using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MomoSwitch.Actions;
using MomoSwitch.Models.Contracts.Specials.Router;

namespace MomoSwitch.Controllers.Inward.NibssExtras
{
    [Route("api/[controller]")]
    [ApiController]
    public class AmountUnblockController : ControllerBase
    {
        private readonly IInward Processor;
        public AmountUnblockController(IInward processor)
        {
            Processor = processor;
        }

        [HttpPost(Name = "InAmountUnBlock")]
        public async Task<AmountUnblockPxResponse> AccountUnBlock(AmountUnblockPxRequest Req)
        {
            return await Processor.AmountUnBlock(Req);
        }
    }
}
