using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MomoSwitch.Models.Contracts.Momo;

namespace MomoSwitch.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FundTransferController : ControllerBase
    {
        [HttpPost(Name = "FundTransfer")]
        public async Task<FundTransferPxResponse> FundTransfer(FundTransferPxRequest Req)
        {
            try
            {


            }
            catch (Exception Ex)
            {

            }
        }
    }
}
