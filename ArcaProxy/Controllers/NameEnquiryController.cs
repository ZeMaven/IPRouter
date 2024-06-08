using ArcaProxy.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Momo.Common.Models;

namespace ArcaProxy.Controllers
{
    [Route("api/outward/[controller]")]
    [ApiController]
    public class NameEnquiryController : ControllerBase
    {
        private readonly IArcaOutward ArcaOutward;

        public NameEnquiryController(IArcaOutward arcaOutward)
        {
            ArcaOutward = arcaOutward;
        }

        [HttpPost(Name = "OutNameEnquiry")]
        public async Task<NameEnquiryPxResponse> NameEnquiry(NameEnquiryPxRequest Req)
        {
            return await ArcaOutward.NameEnquiry(Req);
        }
    }
}
