using CipProxy.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Momo.Common.Models;

namespace CipProxy.Controllers.Outward
{
    [Route("api/outward/[controller]")]//From Momo to processor
    [ApiController]
    public class NameEnquiryController : ControllerBase
    {
        private readonly ICipOutward Cip;
        public NameEnquiryController(ICipOutward cip)
        {
            Cip = cip;
        }

        [HttpPost(Name = "OutNameEnquiry")]
        public async Task<NameEnquiryPxResponse> NameEnquiry(NameEnquiryPxRequest Req)
        {
            return await Cip.NameEnquiry(Req);
        }
    }
}