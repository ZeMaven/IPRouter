using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Momo.Common.Models;
using MomoSwitch.Actions;
using MomoSwitch.Models.Contracts.Momo;
 

namespace MomoSwitch.Controllers
{
    [Route("api/outward/[controller]")]
    [ApiController]
    public class TransactionQueryController : ControllerBase
    {
        private readonly IOutward Processor;
        public TransactionQueryController(IOutward processor)
        {
            Processor = processor;
        }



        [HttpPost(Name = "NameEnquiry")]
        public async Task<TranQueryResponse> NameEnquiry(TranQueryRequest Req)
        {
            return await Processor.TranQuery(Req);
        }
    }
}
