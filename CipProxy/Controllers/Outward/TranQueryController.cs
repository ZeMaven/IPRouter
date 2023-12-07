using CipProxy.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Momo.Common.Models;

namespace CipProxy.Controllers.Outward
{
    [Route("api/outward/[controller]")]//From Momo to processor
    [ApiController]
    public class TranQueryController : ControllerBase
    {
        private readonly ICipOutward Cip;
        public TranQueryController(ICipOutward cip)
        {
            Cip = cip;
        }

        [HttpPost(Name = "OutTranQuery")]
        public async Task<TranQueryPxResponse> TranQuery(TranQueryPxRequest Req)
        {
            return await Cip.TransactionQuery(Req);
        }
    }
}
