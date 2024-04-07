using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MomoSwitch.Actions;
using MomoSwitch.Models.Contracts.Momo;

namespace MomoSwitch.Controllers.Outward
{
    [Route("api/[controller]")]
    [ApiController]
    public class SwitchQueryController : ControllerBase
    {
        private readonly IOutward Processor;
        private readonly IConfiguration Config;
        public SwitchQueryController(IOutward processor, IConfiguration config)
        {
            Processor = processor;
            Config = config;
        }


        [HttpPost(Name = "SwitchQuery")]
        public async Task<IActionResult> TransactionQuery([FromHeader] string Key, TranQueryRequest Req)
        {
            if (Key != Config.GetSection("SwitchQueryKey").Value)
            {
                return Unauthorized("Invalid Key");
            }
            return Ok(await Processor.GetTransaction(Req.transactionId));
        }
    }
}
