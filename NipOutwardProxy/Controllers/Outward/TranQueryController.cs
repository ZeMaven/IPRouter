using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Momo.Common.Models;
using NipOutwardProxy.Actions;

namespace NipOutwardProxy.Controllers.Outward
{
    [Route("api/outward/[controller]")]
    [ApiController]
    public class TranQueryController : ControllerBase
    {
        private readonly INipOutward Nip;
        public TranQueryController(INipOutward nip)
        {
            Nip = nip;
        }

        [HttpPost(Name = "OutTranQuery")]
        public async Task<TranQueryPxResponse> NameEnquiry(TranQueryPxRequest Req)
        {
            return await Nip.TransactionQuery(Req);
        }
    }
}