using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Momo.Common.Models;
using MomoSwitch.Actions;
using System.Diagnostics;

namespace MomoSwitch.Controllers.Inward
{
    [Route("api/inward/[controller]")]
    [ApiController]
    public class NameEnquiryController : ControllerBase
    {
        private readonly IInward Processor;
        public NameEnquiryController(IInward processor)
        {
            Processor = processor;
        }






        [HttpPost(Name = "InNameEnquiry")]
        public async Task<NameEnquiryPxResponse> NameEnquiry(NameEnquiryPxRequest Req)
        {

             return await Processor.NameEnquiry(Req);
        }
    }
}
