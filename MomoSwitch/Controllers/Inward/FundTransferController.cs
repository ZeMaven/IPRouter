using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Momo.Common.Models;
using MomoSwitch.Actions;
using MomoSwitch.Models.Contracts.Momo;
using MomoSwitch.Models.Contracts.Specials.Router;
using System.Diagnostics;

namespace MomoSwitch.Controllers.Inward
{
    [Route("api/inward/[controller]")]
    [ApiController]
    public class FundTransferController : ControllerBase
    {
        private readonly IInward Processor;
        public FundTransferController(IInward processor)
        {
            Processor = processor;
        }


        [HttpPost(Name = "InFundTransfer")]
        public async Task<FundTransferPxResponse> FundTransfer(FundTransferPxRequest Req)
        {
            return await Processor.FundTransfer(Req);
        }
    }
}