using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MomoSwitch.Actions;
using MomoSwitch.Models;

namespace MomoSwitch.Controllers
{
    [Route("api/settings/[controller]")]
    [ApiController]
    public class PokeController : ControllerBase
    {
        private readonly IUtilities _Utilities;
        public PokeController(IUtilities utilities)
        {
             _Utilities= utilities;

        }



        [HttpGet("[action]")]
        public void Poke()
        {

            _Utilities.RefreshCache();
           //To be called when setting is changed on the portal. To reset the cache.
        }
    }
}
