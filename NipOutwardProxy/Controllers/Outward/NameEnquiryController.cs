using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Momo.Common.Models;
using NipOutwardProxy.Actions;

namespace NipOutwardProxy.Controllers.Outward
{
    [Route("api/outward/[controller]")]
    [ApiController]
    public class NameEnquiryController : ControllerBase
    {
        private readonly INipOutward Nip;
        public NameEnquiryController(INipOutward nip)
        {
             Nip= nip;
        }

        [HttpPost(Name = "OutNameEnquiry")]
        public async Task<NameEnquiryPxResponse> NameEnquiry(NameEnquiryPxRequest Req)
        {
            return await Nip.NameEnquiry(Req);
        }
    }
}
