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

                DateTime firstDayOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

                DateTime lastDayOfMonth = DateTime.Parse(firstDayOfMonth.AddMonths(1).AddDays(-1).ToString("yyyy-MM-dd") + " 23:59:59");


                var startDay = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd") + " 00:00:00");
                var endDay = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd") + " 23:59:59");
                var allTransactions = await db.TransactionTb.Where(x => x.Date >= firstDayOfMonth && x.Date <= lastDayOfMonth).Select(x => new HomeMiniTransaction
                {
                    Amount = x.Amount,
                    TransactionId = x.TransactionId,
                    SourceBankName = x.SourceBankName,
                    ResponseCode = x.ResponseCode,
                    BenefBankName = x.BenefBankName,
                    BenefBankCode = x.BenefBankCode,
                    SourceBankCode = x.SourceBankCode,
                    Date = x.Date
                }).ToListAsync();
                var incomingTransactions = allTransactions.Where(x => x.BenefBankCode == institutionCode && x.ResponseCode != "09").ToList();

                var outgoingTransactions = allTransactions.Where(x => x.SourceBankCode == institutionCode && x.ResponseCode != "09").ToList();

                var todayTransactions = allTransactions.Where(x => x.Date >= startDay && x.Date <= endDay && x.ResponseCode != "09").Count();

                var result = homeManager.GetDashboardData(incomingTransactions, outgoingTransactions);

                if (result.ResponseHeader.ResponseCode != "00")
                {
                    return View("Error");
                }

                
                result.DashboardData.RecentTransactions = allTransactions.OrderByDescending(x => x.Date).Take(15).ToList();
                result.DashboardData.TotalUsers = db.PortalUserTb.Count();
                result.DashboardData.TotalSwitches = db.SwitchTb.Count();
                result.DashboardData.TotalTransactions = todayTransactions;
                result.DashboardData.TotalIncomingPercentage = result.DashboardData.TotalTransactions == 0 ? 0 : Math.Round((result.DashboardData.TotalIncomingCount / todayTransactions) * 100, 2);
                result.DashboardData.TotalOutGoingPercentage = result.DashboardData.TotalTransactions == 0 ? 0 : Math.Round((result.DashboardData.TotalOutGoingCount / todayTransactions) * 100,2);
                result.DashboardData.TotalSuccessfulPercentage = result.DashboardData.TotalTransactions == 0 ? 0 : Math.Round((result.DashboardData.TotalSuccessfulCount / result.DashboardData.TotalOutGoingCount) * 100,2);
                result.DashboardData.TotalFailedPercentage = result.DashboardData.TotalTransactions == 0 ? 0 : Math.Round((result.DashboardData.TotalFailedCount / result.DashboardData.TotalOutGoingCount) * 100, 2);

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
