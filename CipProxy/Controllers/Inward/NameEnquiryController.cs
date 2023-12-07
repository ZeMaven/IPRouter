using CipProxy.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Momo.Common.Models;

namespace CipProxy.Controllers.Inward
{
    [Route("api/inward/[controller]")]
    [ApiController]
    public class NameEnquiryController : ControllerBase
    {

        private readonly ICipInward Cip;
        public NameEnquiryController(ICipInward cip)
        {
            Cip = cip;
        }

        [HttpPost(Name = "InNameEnquiry")]
        public async Task<string> NameEnquiry(string Req)
        {
            return await Cip.NameEnquiry(Req);
        }
    }
}
