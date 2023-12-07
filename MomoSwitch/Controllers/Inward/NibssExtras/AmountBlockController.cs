using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MomoSwitch.Actions;
using MomoSwitch.Models.Contracts.Specials.Router;

namespace MomoSwitch.Controllers.Inward.NibssExtras
{
    [Route("api/inward/[controller]")]
    [ApiController]
    public class AmountBlockController : ControllerBase
    {

        private readonly IInward Processor;
        public AmountBlockController(IInward processor)
        {
            Processor = processor;
        }

        [HttpPost(Name = "InAmountBlock")]
        public async Task<AmountBlockPxResponse> AccountBlock(AmountBlockPxRequest Req)
        {
            return await Processor.AmountBlock(Req);
        }
    }
}
