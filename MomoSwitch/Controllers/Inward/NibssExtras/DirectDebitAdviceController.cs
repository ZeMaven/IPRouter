using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MomoSwitch.Actions;
using MomoSwitch.Models.Contracts.Specials.Router;

namespace MomoSwitch.Controllers.Inward.NibssExtras
{
    [Route("api/inward/[controller]")]
    [ApiController]
    public class DirectDebitAdviceController : ControllerBase
    {
        private readonly IInward Processor;
        public DirectDebitAdviceController(IInward processor)
        {
            Processor = processor;
        }

        [HttpPost(Name = "InDirectDebitAdvice")]
        public async Task<DirectDebitAdvicePxResponse> DirectDebitAdvice(DirectDebitAdvicePxRequest Req)
        {
            return await Processor.DirectDebitAdvice(Req);
        }
    }
}
