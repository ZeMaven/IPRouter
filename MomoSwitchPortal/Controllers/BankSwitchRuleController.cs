using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MomoSwitchPortal.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class BankSwitchRuleController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
