using ArcaProxy.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Momo.Common.Models;

namespace ArcaProxy.Controllers
{
    [Route("api/outward/[controller]")]
    [ApiController]
    public class TransferController : ControllerBase
    {
        private readonly IArcaOutward ArcaOutward;

        public TransferController(IArcaOutward arcaOutward)
        {
               ArcaOutward = arcaOutward;
        }


        [HttpPost(Name = "OutTransfer")]
        public async Task<FundTransferPxResponse> Transfer(FundTransferPxRequest Req)
        {
            return await ArcaOutward.Transfer(Req);
        }
    }
}