using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MomoSwitch.Actions;
using MomoSwitch.Models.Contracts.Specials.Router;

namespace MomoSwitch.Controllers.Inward.NibssExtras
{
    [Route("api/inward/[controller]")]
    [ApiController]
    public class DirectCreditAdviceController : ControllerBase
    {
        private readonly IInward Processor;
        public DirectCreditAdviceController(IInward processor)
        {
            Processor = processor;
        }

        [HttpPost(Name = "InDirectCreditAdvice")]
        public async Task<DirectCreditAdvicePxResponse> AccountUnBlock(DirectCreditAdvicePxRequest Req)
        {
            return await Processor.DirectCreditAdvice(Req);
        }
    }
}
