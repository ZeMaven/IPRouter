using ArcaProxy.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Momo.Common.Models;

namespace ArcaProxy.Controllers
{
    [Route("api/outward/[controller]")]
    [ApiController]
    public class TranQueryController : ControllerBase
    {
        private readonly IArcaOutward ArcaOutward;
        public TranQueryController(IArcaOutward arcaOutward)
        {
            ArcaOutward = arcaOutward;
        }


        [HttpPost(Name = "OutTranQuery")]
        public async Task<TranQueryPxResponse> Transfer(TranQueryPxRequest Req)
        {
            return await ArcaOutward.TransactionQuery(Req);
        }
    }
}