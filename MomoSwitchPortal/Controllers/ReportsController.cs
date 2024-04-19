using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Momo.Common.Actions;
using Momo.Common.Models.Tables;
using MomoSwitchPortal.Models.Database;
using MomoSwitchPortal.Models.ViewModels.Reports;
using OfficeOpenXml;

namespace MomoSwitchPortal.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
        private readonly IConfiguration configuration;
        private readonly ILog Log;

        public ReportsController(IConfiguration configuration, ILog log)
        {
            this.configuration = configuration;
            Log = log;
        }

        [HttpGet]
        public IActionResult SummaryReport()
        {
            ViewBag.tranTypes = new SelectList(new[] { "INCOMING", "OUTGOING" });

            return View();
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SummaryReport(SummaryReportFilterRequest model)
        {
            try
            {
                var db = new MomoSwitchDbContext();



                var institutionCode = configuration.GetValue<string>("MomoInstitutionCode");


                var Data = new List<TransactionTb>();
                var startDay = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd") + " 00:00:00");
                var endDay = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd") + " 23:59:59");

                if (model.EndDate == null && model.StartDate == null)
                {
                    Data = await db.TransactionTb
                                    .Where(t => t.Date >= startDay && t.Date <= endDay && t.SourceBankCode == institutionCode)
                                    .ToListAsync();
                }

                else
                {
                    db.TransactionTb.Where(x => (true || x.TransactionId == ""));

                    Data = await db.TransactionTb
                                      .Where(t => (!model.StartDate.HasValue || t.Date >= DateTime.Parse(Convert.ToDateTime(model.StartDate).ToString("yyyy-MM-dd") + " 00:00:00")) &&
                                                  (!model.EndDate.HasValue || t.Date <= DateTime.Parse(Convert.ToDateTime(model.EndDate).ToString("yyyy-MM-dd") + " 23:59:59")) &&
                                                   t.SourceBankCode == institutionCode)
                                     .ToListAsync();

                }

                var summaryReportList = new List<SummaryReport>();
                var dailyTransactions = Data.GroupBy(x => x.Date.Date).ToList();
                dailyTransactions = dailyTransactions.OrderByDescending(x => x.Key.Date).ToList();
                foreach (var transactions in dailyTransactions)
                {
                    var failedTransactions = transactions.Where(x => x.ResponseCode != "00" && x.ResponseCode != "09").ToList();
                    var successTransactions = transactions.Where(x => x.ResponseCode == "00").ToList();

                    var summaryReport = new SummaryReport
                    {
                        FailedPercentage = Convert.ToDouble(failedTransactions.Count) == 0 ? "0" : Math.Round(failedTransactions.Count / Convert.ToDouble(transactions.Count()) * 100, 2).ToString(),
                        Date = transactions.Key.Date.ToString("dd/MM/yyyy"),
                        FailedTransactions = failedTransactions.Count,
                        SuccessfulTransactions = successTransactions.Count,
                        SuccessPercentage = Convert.ToDouble(successTransactions.Count) == 0 ? "0" : Math.Round(successTransactions.Count / Convert.ToDouble(transactions.Count()) * 100, 2).ToString(),
                        TotalTransactions = transactions.Count()
                    };
                    summaryReportList.Add(summaryReport);
                }


                Log.Write("ReportsController.SummaryReport", $"Report count {Data.Count}");
                
                var sheet = $"MomoSwitchTransactions-{DateTime.Now}";
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using (ExcelPackage excelPackage = new ExcelPackage())
                {
                    var workSheet = excelPackage.Workbook.Worksheets.Add(sheet);
                    var SheetRange = workSheet.Cells["A1"].LoadFromCollection(summaryReportList, true);
                    var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    workSheet.Row(1).Style.Font.Bold = true;
                    workSheet.Row(1).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                    SheetRange.AutoFitColumns();
                    SheetRange.Style.Numberformat.Format = "_( #,##_);_( (#,##0.00);_(* \" 0 \"_);_(@_)";

                    using (MemoryStream stream = new MemoryStream())
                    {
                        excelPackage.SaveAs(stream);
                        var content = stream.ToArray();
                        return File(content, contentType, sheet + ".xlsx");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Write("ReportsController.SummaryReport", $"Err:  {ex.Message}");
                ViewBag.BadNews = "System Challenge";
                return View();
            }
        }
 
    }
    
}
