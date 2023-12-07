using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MomoSwitch.Actions;
using MomoSwitch.Models.Contracts.Specials.Router;

namespace MomoSwitch.Controllers.Inward.NibssExtras
{
    [Route("api/inward/[controller]")]
    [ApiController]
    public class AccountBlockController : ControllerBase
    {

        private readonly IInward Processor;
        public AccountBlockController(IInward processor)
        {
            Processor = processor;
        }

        [HttpPost(Name = "InAccountBlock")]
        public async Task<AccountBlockPxResponse> AccountBlock(AccountBlockPxRequest Req)
        {
            return await Processor.AccountBlock(Req);
        }
    }
}
