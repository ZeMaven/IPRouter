using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Momo.Common.Actions;
using Momo.Common.Models;
using MomoSwitch.Actions;
using MomoSwitch.Models.Contracts.Momo;

//From momo to processor
namespace MomoSwitch.Controllers.Outward
{
    [Route("api/outward/[controller]")]
    [ApiController]
    [Authorize]


 
    public class FundTransferController : ControllerBase
    {
      private readonly ILog _log;
        private readonly IOutward Processor;
        public FundTransferController(IOutward processor, ILog log)
        {
            Processor = processor;
             _log = log;
        }

        [HttpPost(Name = "OutFundTransfer")]
      //[ServiceFilter(typeof(ActionRequestLogger<FundTransferRequest>))]
        public async Task<FundTransferResponse> FundTransfer(FundTransferRequest Req)
        {
            return await Processor.Transfer(Req);
        }
    }
}