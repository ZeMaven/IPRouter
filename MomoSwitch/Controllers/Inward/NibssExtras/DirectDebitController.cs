using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MomoSwitch.Actions;
using MomoSwitch.Models.Contracts.Specials.Router;

namespace MomoSwitch.Controllers.Inward.NibssExtras
{
    [Route("api/inward/[controller]")]
    [ApiController]
    public class DirectDebitController : ControllerBase
    {
        private readonly IInward Processor;
        public DirectDebitController(IInward processor)
        {
            Processor = processor;
        }

        [HttpPost(Name = "InDirectDebit")]
        public async Task<DirectDebitPxResponse> DirectDebitAdvice(DirectDebitPxRequest Req)
        {
            return await Processor.DirectDebit(Req);
        }
    }
}
