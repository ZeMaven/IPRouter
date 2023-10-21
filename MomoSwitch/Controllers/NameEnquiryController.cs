using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MomoSwitch.Models.Contracts.Momo;

namespace MomoSwitch.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NameEnquiryController : ControllerBase
    {
        [HttpPost(Name = "NameEnquiry")]
        public async Task<NameEnquiryPxResponse> NameEnquiry(NameEnquiryPxRequest Req)
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
