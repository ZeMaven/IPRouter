using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MomoSwitch.Analysis.Actions;
using MomoSwitch.Analysis.Models;

namespace MomoSwitch.Analysis.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuth _auth;
        public AuthController(IAuth auth)
        {
           _auth=auth;
        }

        [HttpPost(Name = "Auth")]
        public AuthResponse Auth([FromForm] AuthRequest request)
        {
            return _auth.Reset(request);
        }
    }
}
