using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Momo.Common.Models;
using MomoSwitch.Actions;

namespace MomoSwitch.Controllers.Inward.NibssExtras
{
    [Route("api/inward/[controller]")]
    [ApiController]
    public class DirectCreditController : ControllerBase
    {
        private readonly IInward Processor;
        public DirectCreditController(IInward processor)
        {
            Processor = processor;
        }

        [HttpPost(Name = "InDirectCredit")]
        public async Task<FundTransferPxResponse> DirectCredit(FundTransferPxRequest Req)
        {
            return await Processor.DirectCredit(Req);
        }
    }
}
