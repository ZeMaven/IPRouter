using EtransactProxy.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Momo.Common.Models;

namespace EtransactProxy.Controllers.Outward
{
    [Route("api/outward/[controller]")]
    [ApiController]
    public class TranQueryController : ControllerBase
    {
        private readonly IEntranzactOutward Etranzact;
        public TranQueryController(IEntranzactOutward etranzact)
        {
            Etranzact = etranzact;
        }

        [HttpPost(Name = "OutTranQuery")]
        public async Task<TranQueryPxResponse> TranQuery(TranQueryPxRequest Req)
        {
            return await Etranzact.TransactionQuery(Req);
        }
    }
}