using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MomoSwitch.Actions;
using MomoSwitch.Models.Contracts;
using MomoSwitch.Models.Contracts.Momo;
using System.Diagnostics;

namespace MomoSwitch.Controllers.Outward
{
    [Route("api/outward/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IOutward Processor;
        public AuthController(IOutward processor)
        {
            Processor = processor;
        }

        [HttpPost(Name = "Auth")]
        public AuthResponse Auth([FromForm] AuthRequest request)
        {
            return Processor.Reset(request);
        }
    }
}
