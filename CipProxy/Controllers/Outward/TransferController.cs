using CipProxy.Actions;
using CipProxy.Models.Cip.Transfer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Momo.Common.Models;

namespace CipProxy.Controllers.Outward
{
    [Route("api/outward/[controller]")]//From Momo to processor
    [ApiController]
    public class TransferController : ControllerBase
    {
        private readonly ICipOutward Cip;
        public TransferController(ICipOutward cip)
        {
            Cip = cip;
        }

        [HttpPost(Name = "OutTransfer")]
        public async Task<FundTransferPxResponse> Transfer(FundTransferPxRequest Req)
        {
            return await Cip.Transfer(Req);
        }
    }
}
