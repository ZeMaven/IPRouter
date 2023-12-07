using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Momo.Common.Models;
using NipOutwardProxy.Actions;

namespace NipOutwardProxy.Controllers.Outward
{
    [Route("api/outward/[controller]")]
    [ApiController]
    public class TransferController : ControllerBase
    {
        private readonly INipOutward Nip;
        public TransferController(INipOutward nip)
        {
            Nip = nip;
        }

        [HttpPost(Name = "OutTransfer")]
        public async Task<FundTransferPxResponse> Transfer(FundTransferPxRequest Req)
        {
            return await Nip.Transfer(Req);
        }
    }
}