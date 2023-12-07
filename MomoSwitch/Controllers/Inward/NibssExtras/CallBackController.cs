using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MomoSwitch.Actions;
using MomoSwitch.Models.Contracts.Specials.Router;

namespace MomoSwitch.Controllers.Inward.NibssExtras
{
    [Route("api/inward/[controller]")]
    [ApiController]
    public class CallBackController : ControllerBase
    {
        private readonly IInward Processor;
        public CallBackController(IInward processor)
        {
            Processor = processor;
        }

        [HttpPost(Name = "InCallBackEnquiry")]
        public async Task<string> AccountUnBlock(CallbackPxRequest Req)
        {
            return await Processor.CallBack(Req);
        }
    }
}
