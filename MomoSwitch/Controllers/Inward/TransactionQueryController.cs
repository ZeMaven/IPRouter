using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Momo.Common.Models;
using MomoSwitch.Actions;
using System.Diagnostics;

namespace MomoSwitch.Controllers.Inward
{
    [Route("api/inward/[controller]")]
    [ApiController]
    public class TransactionQueryController : ControllerBase
    {
        private readonly IInward Processor;
        public TransactionQueryController(IInward processor)
        {
            Processor = processor;
        }

        [HttpPost(Name = "InTransactionQuery")]
        public async Task<TranQueryPxResponse> TransactionQuery(TranQueryPxRequest Req)
        {

             return await Processor.TransactionQuery(Req);
        }
    }
}