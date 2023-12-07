using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MomoSwitch.Actions;
using MomoSwitch.Models.Contracts.Specials.Router;

namespace MomoSwitch.Controllers.Inward.NibssExtras
{
    [Route("api/inward/[controller]")]
    [ApiController]
    public class BalanceEnquiryController : ControllerBase
    {
        private readonly IInward Processor;
        public BalanceEnquiryController(IInward processor)
        {
            Processor = processor;
        }

        [HttpPost(Name = "InBalanceEnquiry")]
        public async Task<BalanceEnquiryPxResponse> AccountUnBlock(BalanceEnquiryPxRequest Req)
        {
            return await Processor.BalanceEnquiry(Req);
        }
    }
}
