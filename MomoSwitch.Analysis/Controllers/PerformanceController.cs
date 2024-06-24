using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MomoSwitch.Analysis.Actions;
using MomoSwitch.Analysis.Models;

namespace MomoSwitch.Analysis.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PerformanceController : ControllerBase
    {
        private readonly IAnalysis _analysis;
        public PerformanceController(IAnalysis analysis)
        {
          _analysis = analysis;
        }

        [HttpGet(Name = "Performance")]
        public List<Perfomance> Performance() => _analysis.GetPerfomance();
    }
}
