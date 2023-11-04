using CipProxy.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CipProxy.Controllers.Inward
{
    [Route("api/inward/[controller]")]
    [ApiController]
    public class TranQueryController : ControllerBase
    {

        private readonly ICipInward Cip;
        public TranQueryController(ICipInward cip)
        {
            Cip = cip;
        }

        [HttpPost(Name = "InTranQuery")]
        public async Task<string> TranQuery(string Req)
        {
            return await Cip.TransactionQuery(Req);
        }
    }
}
