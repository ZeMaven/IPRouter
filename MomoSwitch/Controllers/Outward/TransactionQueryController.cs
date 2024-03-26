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
    [Authorize]
    public class TransactionQueryController : ControllerBase
    {
        private readonly IOutward Processor;
        public TransactionQueryController(IOutward processor)
        {
            Processor = processor;
        }


        [HttpPost(Name = "OutTransactionQuery")]
        public async Task<TranQueryResponse> TransactionQuery(TranQueryRequest Req)
        {
            return await Processor.GetTransaction(Req.transactionId);
        }
    }
}