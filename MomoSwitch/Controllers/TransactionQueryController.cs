using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MomoSwitch.Models.Contracts.Momo;

namespace MomoSwitch.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionQueryController : ControllerBase
    {
        [HttpPost(Name = "TransactionQuery")]
        public async Task<TranQueryPxResponse> TransactionQuery(TranQueryPxRequest Req)
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
