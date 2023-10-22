using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MomoSwitch.Models;

namespace MomoSwitch.Controllers
{
    [Route("api/settings/[controller]")]
    [ApiController]
    public class PokeController : ControllerBase
    {
        [HttpGet("[action]")]
        public void Poke()
        {
           //To be called when setting is changed on the portal. To reset the cache.


        }
    }
}
