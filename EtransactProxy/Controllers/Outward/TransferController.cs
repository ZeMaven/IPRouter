using EtransactProxy.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Momo.Common.Models;

namespace EtransactProxy.Controllers.Outward
{
    [Route("api/outward/[controller]")]
    [ApiController]
    public class TransferController : ControllerBase
    {
        private readonly IEntranzactOutward Etranzact;
        public TransferController(IEntranzactOutward etranzact)
        {
            Etranzact = etranzact;
        }

        [HttpPost(Name = "OutTransfer")]
        public async Task<FundTransferPxResponse> Transfer(FundTransferPxRequest Req)
        {
            return await Etranzact.Transfer(Req);
        }
    }
}
