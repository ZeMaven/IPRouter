using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Momo.Common.Models;
using MomoSwitch.Actions;
using MomoSwitch.Models.Contracts.Momo;

//From momo to processor
namespace MomoSwitch.Controllers.Outward
{
    [Route("api/outward/[controller]")]
    [ApiController]
    //  [Authorize]
    public class FundTransferController : ControllerBase
    {
        private readonly IOutward Processor;
        public FundTransferController(IOutward processor)
        {
            Processor = processor;
        }

        [HttpPost(Name = "OutFundTransfer")]
        public async Task<FundTransferResponse> FundTransfer(FundTransferRequest Req)
        {
            return await Processor.Transfer(Req);
        }
    }
}