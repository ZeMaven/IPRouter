using EtransactProxy.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Momo.Common.Models;

namespace EtransactProxy.Controllers.Outward
{
    [Route("api/outward/[controller]")]
    [ApiController]
    public class NameEnquiryController : ControllerBase
    {
        private readonly IEntranzactOutward Etranzact;
        public NameEnquiryController(IEntranzactOutward etranzact)
        {
            Etranzact = etranzact;
        }

        [HttpPost(Name = "OutNameEnquiry")]
        public async Task<NameEnquiryPxResponse> NameEnquiry(NameEnquiryPxRequest Req)
        {
            return await Etranzact.NameEnquiry(Req);
        }
    }
}
