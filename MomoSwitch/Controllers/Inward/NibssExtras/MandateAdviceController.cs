using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MomoSwitch.Actions;
using MomoSwitch.Models.Contracts.Specials.Router;

namespace MomoSwitch.Controllers.Inward.NibssExtras
{
    [Route("api/inward/[controller]")]
    [ApiController]
    public class MandateAdviceController : ControllerBase
    {
        private readonly IInward Processor;
        public MandateAdviceController(IInward processor)
        {
            Processor = processor;
        }

        [HttpPost(Name = "InMandateAdvice")]
        public async Task<MandateAdvicePxResponse> DirectDebitAdvice(MandateAdvicePxRequest Req)
        {
            return await Processor.ManadateAdvice(Req);
        }
    }
}