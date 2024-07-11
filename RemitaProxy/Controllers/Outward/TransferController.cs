using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Momo.Common.Models;
using RemitaProxy.Actions;

namespace RemitaProxy.Controllers.Outward
{
    [Route("api/outward/[controller]")]
    [ApiController]
    public class TransferController : ControllerBase
    {
        private readonly IRemitaOutward Remita;
        public TransferController(IRemitaOutward remita)
        {
            Remita = remita;
        }
        [HttpPost(Name = "OutTransfer")]
        public async Task<FundTransferPxResponse> Transfer(FundTransferPxRequest Req)
        {
            return await Remita.Transfer(Req);
        }
    }
}
