using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Momo.Common.Models;
using RemitaProxy.Actions;

namespace RemitaProxy.Controllers.Outward
{
    [Route("api/outward/[controller]")]
    [ApiController]
    public class NameEnquiryController : ControllerBase
    {
        private readonly IRemitaOutward Remita;
        public NameEnquiryController(IRemitaOutward remita)
        {
           Remita = remita;
        }

        [HttpPost(Name = "OutNameEnquiry")]
        public async Task<NameEnquiryPxResponse> NameEnquiry(NameEnquiryPxRequest Req)
        {
            return await Remita.NameEnquiry(Req);
        }
    }
}
