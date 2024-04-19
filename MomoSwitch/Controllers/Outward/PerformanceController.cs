using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MomoSwitch.Actions;
using MomoSwitch.Models.Contracts.Momo;

namespace MomoSwitch.Controllers.Outward
{
    [Route("api/[controller]")]
    [ApiController]
   // [Authorize]
    public class PerformanceController : ControllerBase
    {
        private readonly IOutward Outward;
        public PerformanceController(IOutward outward)
        {
            Outward = outward;
        }

        [HttpGet(Name = "Performance")]
        public List<Perfomance> Performance() => Outward.GetPerfomance();
    }
}