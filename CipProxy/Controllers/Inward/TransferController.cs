using CipProxy.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CipProxy.Controllers.Inward
{
    [Route("api/inward/[controller]")]
    [ApiController]
    public class TransferController : ControllerBase
    {
        private readonly ICipInward Cip;
        public TransferController(ICipInward cip)
        {
            Cip = cip;
        }

        [HttpPost(Name = "InTransfer")]
        public async Task<string> Transfer(string Req)
        {
            return await Cip.Transfer(Req);
        }
    }
}
