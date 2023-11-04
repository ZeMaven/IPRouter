using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Momo.Common.Models;
using MomoSwitch.Actions;
using MomoSwitch.Models.Contracts.Momo;
using System.Runtime.CompilerServices;

namespace MomoSwitch.Controllers
{
    [Route("api/outward/[controller]")]
    [ApiController]
    public class NameEnquiryController : ControllerBase
    {
        private readonly IOutward Outward;
        public NameEnquiryController(IOutward  outward)
        {
             Outward = outward;
        }



        [HttpPost(Name = "NameEnquiry")]
        public async Task<NameEnquiryResponse> NameEnquiry(NameEnquiryResponse Req)
        {
            return await Outward.NameEnquiry(Req);
        }
    }
}
