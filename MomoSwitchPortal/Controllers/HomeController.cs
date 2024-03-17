using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Momo.Common.Actions;
using Momo.Common.Models.Tables;
using MomoSwitchPortal.Actions;
using MomoSwitchPortal.Models.Database;
using MomoSwitchPortal.Models.ViewModels.Home;
using System.Globalization;

namespace MomoSwitchPortal.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private ILog Log;
        private readonly IHome homeManager;
        private readonly IConfiguration configuration;

        public HomeController(ILog log, IConfiguration configuration, IHome homeManager)
        {
            Log = log;
            this.configuration = configuration;
            this.homeManager = homeManager;
        }

        public async Task<IActionResult> Index()
        {
            try
            {              
                var institutionCode = configuration.GetValue<string>("MomoInstitutionCode");
                var db = new MomoSwitchDbContext();

                var incomingTransactions = await db.TransactionTb.Where(x => x.BenefBankCode == institutionCode).ToListAsync();
                var outgoingTransactions = await db.TransactionTb.Where(x => x.SourceBankCode == institutionCode).ToListAsync();
               

                var result = homeManager.GetDashboardData(incomingTransactions, outgoingTransactions);

                if(result.ResponseHeader.ResponseCode != "00")
                {
                    return View("Error");
                }

                result.DashboardData.RecentTransactions = db.TransactionTb.OrderByDescending(x => x.Date).Take(15).ToList();
                result.DashboardData.RecentUsers = db.PortalUserTb.OrderByDescending(x => x.EntryDate).Take(10).ToList();
                result.DashboardData.TotalUsers = db.PortalUserTb.ToList().Count;
                result.DashboardData.TotalSwitches = db.SwitchTb.ToList().Count;
                return View(result.DashboardData);
            }
            catch (Exception ex)
            {
                Log.Write("HomeController:Index", $"eRR: {ex.Message}");
                return View("Error");
            }          
        }

        [HttpGet("accessdenied")]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }


}
