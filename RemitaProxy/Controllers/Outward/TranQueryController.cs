using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Momo.Common.Models;
using RemitaProxy.Actions;

namespace RemitaProxy.Controllers.Outward
{
    [Route("api/outward/[controller]")]
    [ApiController]
    public class TranQueryController : ControllerBase
    {
        private readonly IRemitaOutward Remita;
        public TranQueryController(IRemitaOutward remita)
        {
            Remita = remita;
        }
        [HttpPost(Name = "OutTranQuery")]
        public async Task<TranQueryPxResponse> TranQuery(TranQueryPxRequest Req)
        {
            return await Remita.TransactionQuery(Req);
        }
    }
}
