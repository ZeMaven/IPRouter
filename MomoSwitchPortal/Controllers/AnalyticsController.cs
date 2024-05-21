using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Momo.Common.Actions;
using Momo.Common.Models.Tables;
using MomoSwitchPortal.Actions;
using MomoSwitchPortal.Models.Database;
using MomoSwitchPortal.Models.ViewModels.Analytics;
using MomoSwitchPortal.Models.ViewModels.Transaction;
using OfficeOpenXml;
using System.Globalization;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MomoSwitchPortal.Controllers
{
    [Authorize]
    public class AnalyticsController : Controller
    {
        private readonly IConfiguration configuration;
        private readonly ILog Log;

        public AnalyticsController(IConfiguration configuration, ILog log)
        {
            this.configuration = configuration;
            Log = log;
        }

        [HttpGet]
        public async Task<IActionResult> SummaryReport(int page)
        {
            try
            {
                int pageSize = 30;
                int pageNumber = (page == 0 ? 1 : page);

                ViewBag.tranTypes = new SelectList(new[] { "INCOMING", "OUTGOING" });

                var institutionCode = configuration.GetValue<string>("MomoInstitutionCode");
                var summaryReportTableList = new List<SummaryReportTableViewModel>();

                var db = new MomoSwitchDbContext();

                var loggedInUser = HttpContext.GetLoggedInUser();

                var loggedInUserInDatabase = await db.PortalUserTb.SingleOrDefaultAsync(x => x.Username.ToLower() == loggedInUser.ToLower());

                if (loggedInUserInDatabase == null)
                {
                    Log.Write("AnalyticsController:SummaryReport", $"eRR: Logged in user not gotten");
                    return RedirectToAction("Logout", "Account");
                }

                if (!loggedInUserInDatabase.IsActive)
                {
                    //should they be logged out?
                    Log.Write("AnalyticsController:SummaryReport", $"eRR: User with username: {loggedInUserInDatabase.Username} is deactivated");
                    return RedirectToAction("Logout", "Account");
                }

                var startDay = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd") + " 00:00:00");
                var endDay = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd") + " 23:59:59");
                var Data = new List<SummaryReportTransactionsMiniViewModel>();

                if (page != 0 && TempData["SummaryFilterRequest"]?.ToString() != null)
                {
                    //  List<TransactionItem> Trans1 = JsonSerializer.Deserialize<List<TransactionItem>>(TempData["Tran"].ToString());

                    var FilterRequest = JsonSerializer.Deserialize<TransactionViewModel>(TempData["SummaryFilterRequest"].ToString());



                    if (FilterRequest.FilterRequest.EndDate == null && FilterRequest.FilterRequest.StartDate == null)
                    {
                        Data = await db.TransactionTb
                                        .Where(t => t.Date >= startDay && t.Date <= endDay && t.SourceBankCode == institutionCode && t.ResponseCode != "09")
                                           .Select(x => new SummaryReportTransactionsMiniViewModel
                                           {
                                               Date = x.Date,
                                               ResponseCode = x.ResponseCode
                                           })
                                        .ToListAsync();
                    }

                    else
                    {
                        Data = await db.TransactionTb
                                          .Where(t => (!FilterRequest.FilterRequest.StartDate.HasValue || t.Date >= DateTime.Parse(Convert.ToDateTime(FilterRequest.FilterRequest.StartDate).ToString("yyyy-MM-dd") + " 00:00:00")) &&
                                                      (!FilterRequest.FilterRequest.EndDate.HasValue || t.Date <= DateTime.Parse(Convert.ToDateTime(FilterRequest.FilterRequest.EndDate).ToString("yyyy-MM-dd") + " 23:59:59")) &&
                                                       t.SourceBankCode == institutionCode && t.ResponseCode != "09")
                                             .Select(x => new SummaryReportTransactionsMiniViewModel
                                             {
                                                 Date = x.Date,
                                                 ResponseCode = x.ResponseCode
                                             })
                                         .ToListAsync();
                    }


                    var dailyTransactions = Data.GroupBy(x => x.Date.Date).ToList();
                    dailyTransactions = dailyTransactions.OrderByDescending(x => x.Key.Date).ToList();
                    foreach (var transactions in dailyTransactions)
                    {
                        var failedTransactions = transactions.Where(x => x.ResponseCode != "00" && x.ResponseCode != "09").ToList();
                        var successTransactions = transactions.Where(x => x.ResponseCode == "00").ToList();

                        var summaryReport = new SummaryReportTableViewModel
                        {
                            FailedPercentage = Convert.ToDouble(failedTransactions.Count) == 0 ? "0" : Math.Round(failedTransactions.Count / Convert.ToDouble(transactions.Count()) * 100, 2).ToString(),
                            Date = transactions.Key.Date.ToString("dd/MM/yyyy"),
                            FailedTransactions = failedTransactions.Count,
                            SuccessfulTransactions = successTransactions.Count,
                            SuccessPercentage = Convert.ToDouble(successTransactions.Count) == 0 ? "0" : Math.Round(successTransactions.Count / Convert.ToDouble(transactions.Count()) * 100, 2).ToString(),
                            TotalTransactions = transactions.Count()
                        };
                        summaryReportTableList.Add(summaryReport);
                    }


                    int Count = summaryReportTableList.Count;
                    summaryReportTableList = summaryReportTableList
                   .OrderByDescending(x => x.Date)
                   .Skip((pageNumber - 1) * pageSize)
                   .Take(pageSize)
                   .ToList();

                    var startSerialNumber = (pageNumber - 1) * pageSize + 1;


                    var viewModel = new SummaryReportViewModel();
                    viewModel.Transactions = summaryReportTableList;
                    viewModel.PaginationMetaData = new(Count, pageNumber, pageSize);
                    viewModel.StartSerialNumber = startSerialNumber;

                    viewModel.FilterRequest = new SummaryReportFilterRequest
                    {
                        StartDate = FilterRequest.FilterRequest.StartDate,
                        EndDate = FilterRequest.FilterRequest.EndDate,
                        TranType = FilterRequest.FilterRequest.TranType,
                    };

                    TempData.Keep();

                    return View(viewModel);
                }

                TempData["SummaryFilterRequest"] = null;

                SummaryReportViewModel Trans = new();
                await Task.Run((() =>
                {
                    Data = db.TransactionTb
                                        .Where(t => t.Date >= startDay && t.Date <= endDay && t.SourceBankCode == institutionCode)
                                           .Select(x => new SummaryReportTransactionsMiniViewModel
                                           {
                                               Date = x.Date,
                                               ResponseCode = x.ResponseCode
                                           })
                                        .ToList();

                    var dailyTransactions = Data.GroupBy(x => x.Date.Date).ToList();
                    dailyTransactions = dailyTransactions.OrderByDescending(x => x.Key.Date).ToList();
                    foreach (var transactions in dailyTransactions)
                    {
                        var failedTransactions = transactions.Where(x => x.ResponseCode != "00" && x.ResponseCode != "09").ToList();
                        var successTransactions = transactions.Where(x => x.ResponseCode == "00").ToList();

                        var summaryReport = new SummaryReportTableViewModel
                        {
                            FailedPercentage = Convert.ToDouble(failedTransactions.Count) == 0 ? "0" : Math.Round(failedTransactions.Count / Convert.ToDouble(transactions.Count()) * 100, 2).ToString(),
                            Date = transactions.Key.Date.ToString("dd/MM/yyyy"),
                            FailedTransactions = failedTransactions.Count,
                            SuccessfulTransactions = successTransactions.Count,
                            SuccessPercentage = Convert.ToDouble(successTransactions.Count) == 0 ? "0" : Math.Round(successTransactions.Count / Convert.ToDouble(transactions.Count()) * 100, 2).ToString(),
                            TotalTransactions = transactions.Count()
                        };
                        summaryReportTableList.Add(summaryReport);
                    }

                    Trans = new SummaryReportViewModel()
                    {
                        Transactions = summaryReportTableList
                    };
                }));

                int Count2 = Trans.Transactions.Count;
                Trans.Transactions = Trans.Transactions
               .OrderByDescending(x => x.Date)
               .Skip((pageNumber - 1) * pageSize)
               .Take(pageSize)
               .ToList();

                var startSerialNumber2 = (pageNumber - 1) * pageSize + 1;


                Trans.PaginationMetaData = new(Count2, pageNumber, pageSize);
                Trans.StartSerialNumber = startSerialNumber2;

                return View(Trans);
            }
            catch (Exception ex)
            {
                Log.Write("AnalyticsController:SummaryReport", $"eRR: {ex.Message}");
                return View("Error");
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SummaryReport(SummaryReportViewModel model)
        {
            try
            {

                TempData["SummaryFilterRequest"] = null;
                int pageSize = 30;
                int pageNumber = 1;

                ViewBag.tranTypes = new SelectList(new[] { "INCOMING", "OUTGOING" });

                var db = new MomoSwitchDbContext();
                var institutionCode = configuration.GetValue<string>("MomoInstitutionCode");

                var filterRequest = new SummaryReportViewModel
                {
                    FilterRequest = new SummaryReportFilterRequest
                    {
                        EndDate = model.FilterRequest.EndDate,
                        StartDate = model.FilterRequest.StartDate,
                        TranType = model.FilterRequest.TranType
                    }
                };



                var Data = new List<SummaryReportTransactionsMiniViewModel>();

                var startDay = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd") + " 00:00:00");
                var endDay = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd") + " 23:59:59");

                if (model.FilterRequest.EndDate == null && model.FilterRequest.StartDate == null)
                {
                    Data = await db.TransactionTb
                                    .Where(t => t.Date >= startDay && t.Date <= endDay && t.SourceBankCode == institutionCode && t.ResponseCode != "09")
                                    .Select(x => new SummaryReportTransactionsMiniViewModel
                                    {
                                        Date = x.Date,
                                        ResponseCode = x.ResponseCode
                                    })
                                    .ToListAsync();
                }

                else
                {
                    db.TransactionTb.Where(x => (true || x.TransactionId == ""));

                    Data = await db.TransactionTb
                                      .Where(t => (!model.FilterRequest.StartDate.HasValue || t.Date >= DateTime.Parse(Convert.ToDateTime(model.FilterRequest.StartDate).ToString("yyyy-MM-dd") + " 00:00:00")) &&
                                                  (!model.FilterRequest.EndDate.HasValue || t.Date <= DateTime.Parse(Convert.ToDateTime(model.FilterRequest.EndDate).ToString("yyyy-MM-dd") + " 23:59:59")) &&
                                                   t.SourceBankCode == institutionCode && t.ResponseCode != "09")
                                      .Select(x => new SummaryReportTransactionsMiniViewModel
                                      {
                                          Date = x.Date,
                                          ResponseCode = x.ResponseCode
                                      })
                                     .ToListAsync();

                }


                TempData["SummaryFilterRequest"] = JsonSerializer.Serialize(filterRequest);
                TempData.Keep();



                var summaryReportList = new List<SummaryReportTableViewModel>();
                var dailyTransactions = Data.GroupBy(x => x.Date.Date).ToList();
                dailyTransactions = dailyTransactions.OrderByDescending(x => x.Key.Date).ToList();
                foreach (var transactions in dailyTransactions)
                {
                    var failedTransactions = transactions.Where(x => x.ResponseCode != "00" && x.ResponseCode != "09").ToList();
                    var successTransactions = transactions.Where(x => x.ResponseCode == "00").ToList();

                    var summaryReport = new SummaryReportTableViewModel
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


                int Count = summaryReportList.Count;
                summaryReportList = summaryReportList
               .OrderByDescending(x => x.Date)
               .Skip((pageNumber - 1) * pageSize)
               .Take(pageSize)
               .ToList();

                var startSerialNumber = (pageNumber - 1) * pageSize + 1;

                model.Transactions = summaryReportList;


                model.PaginationMetaData = new(Count, pageNumber, pageSize);
                model.StartSerialNumber = startSerialNumber;
                ViewBag.startDate = model.FilterRequest.StartDate;
                return View(model);
            }
            catch (Exception ex)
            {
                Log.Write("AnalyticsController:SummaryReport", $"eRR: {ex.Message}");
                return View("Error");
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DownloadSummaryReport(SummaryReportViewModel model)
        {
            try
            {
                var db = new MomoSwitchDbContext();



                var institutionCode = configuration.GetValue<string>("MomoInstitutionCode");


                var Data = new List<SummaryReportTransactionsMiniViewModel>();

                var startDay = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd") + " 00:00:00");
                var endDay = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd") + " 23:59:59");

                if (model.FilterRequest.EndDate == null && model.FilterRequest.StartDate == null)
                {
                    Data = await db.TransactionTb
                                    .Where(t => t.Date >= startDay && t.Date <= endDay && t.SourceBankCode == institutionCode && t.ResponseCode != "09")
                                    .Select(x => new SummaryReportTransactionsMiniViewModel
                                    {
                                        Date = x.Date,
                                        ResponseCode = x.ResponseCode
                                    })
                                    .ToListAsync();
                }

                else
                {
                    db.TransactionTb.Where(x => (true || x.TransactionId == ""));

                    Data = await db.TransactionTb
                                      .Where(t => (!model.FilterRequest.StartDate.HasValue || t.Date >= DateTime.Parse(Convert.ToDateTime(model.FilterRequest.StartDate).ToString("yyyy-MM-dd") + " 00:00:00")) &&
                                                  (!model.FilterRequest.EndDate.HasValue || t.Date <= DateTime.Parse(Convert.ToDateTime(model.FilterRequest.EndDate).ToString("yyyy-MM-dd") + " 23:59:59")) &&
                                                   t.SourceBankCode == institutionCode && t.ResponseCode != "09")
                                      .Select(x => new SummaryReportTransactionsMiniViewModel
                                      {
                                          Date = x.Date,
                                          ResponseCode = x.ResponseCode
                                      })
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


                Log.Write("AnalyticsController.DownloadSummaryReport", $"Report count {Data.Count}");

                var sheet = $"MomoSwitchTransactionsSummary-{DateTime.Now}";
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
                Log.Write("AnalyticsController.DownloadSummaryReport", $"Err:  {ex.Message}");
                ViewBag.BadNews = "System Challenge";
                return View();
            }
        }

        [HttpGet]
        public async Task<IActionResult> InstitutionPerformance(int page)
        {
            try
            {
                int pageSize = 30;
                int pageNumber = (page == 0 ? 1 : page);

                ViewBag.tranTypes = new SelectList(new[] { "INCOMING", "OUTGOING" });

                var institutionCode = configuration.GetValue<string>("MomoInstitutionCode");

                var db = new MomoSwitchDbContext();

                var loggedInUser = HttpContext.GetLoggedInUser();

                var loggedInUserInDatabase = await db.PortalUserTb.SingleOrDefaultAsync(x => x.Username.ToLower() == loggedInUser.ToLower());

                if (loggedInUserInDatabase == null)
                {
                    Log.Write("AnalyticsController:InstitutionPerformance", $"eRR: Logged in user not gotten");
                    return RedirectToAction("Logout", "Account");
                }

                if (!loggedInUserInDatabase.IsActive)
                {
                    //should they be logged out?
                    Log.Write("AnalyticsController:InstitutionPerformance", $"eRR: User with username: {loggedInUserInDatabase.Username} is deactivated");
                    return RedirectToAction("Logout", "Account");
                }


                var Data = new List<InstitutionPerformanceTableViewModel>();

                if (page != 0 && TempData["InstitutionPerformanceFilterRequest"]?.ToString() != null)
                {
                    //  List<TransactionItem> Trans1 = JsonSerializer.Deserialize<List<TransactionItem>>(TempData["Tran"].ToString());

                    var FilterRequest = JsonSerializer.Deserialize<InstitutionPerformanceViewModel>(TempData["InstitutionPerformanceFilterRequest"].ToString());



                    if (FilterRequest.FilterRequest.BankCode == null)
                    {
                        Data = await db.PerformanceTb.Select(x => new InstitutionPerformanceTableViewModel
                        {
                            BankCode = x.BankCode,
                            BankName = x.BankName,
                            Remark = x.Remark,
                            SuccessRate = x.Rate,
                            Time = x.Time
                        }).ToListAsync();
                    }

                    else
                    {
                        Data = await db.PerformanceTb.Where(t => (string.IsNullOrWhiteSpace(FilterRequest.FilterRequest.BankCode) || t.BankCode == FilterRequest.FilterRequest.BankCode))
                            .Select(x => new InstitutionPerformanceTableViewModel
                            {
                                BankCode = x.BankCode,
                                BankName = x.BankName,
                                Remark = x.Remark,
                                SuccessRate = x.Rate,
                                Time = x.Time
                            })
                            .ToListAsync();
                    }



                    int Count = Data.Count;
                    Data = Data
                   .OrderByDescending(d => d.SuccessRate)
                   .Skip((pageNumber - 1) * pageSize)
                   .Take(pageSize)
                   .ToList();

                    var startSerialNumber = (pageNumber - 1) * pageSize + 1;


                    var viewModel = new InstitutionPerformanceViewModel();
                    viewModel.Institutions = Data;
                    viewModel.PaginationMetaData = new(Count, pageNumber, pageSize);
                    viewModel.StartSerialNumber = startSerialNumber;

                    viewModel.FilterRequest = new InstitutionPerformanceFilterRequest
                    {
                        BankCode = FilterRequest.FilterRequest.BankCode
                    };

                    TempData.Keep();

                    return View(viewModel);
                }

                TempData["InstitutionPerformanceFilterRequest"] = null;

                InstitutionPerformanceViewModel institutions = new();
                await Task.Run((() =>
                {
                    Data = db.PerformanceTb.Select(x => new InstitutionPerformanceTableViewModel
                    {
                        BankCode = x.BankCode,
                        BankName = x.BankName,
                        Remark = x.Remark,
                        SuccessRate = x.Rate,
                        Time = x.Time
                    }).ToList();


                    institutions = new InstitutionPerformanceViewModel()
                    {
                        Institutions = Data
                    };
                }));

                int Count2 = institutions.Institutions.Count;
                institutions.Institutions = institutions.Institutions
               .OrderByDescending(d => d.SuccessRate)
               .Skip((pageNumber - 1) * pageSize)
               .Take(pageSize)
               .ToList();

                var startSerialNumber2 = (pageNumber - 1) * pageSize + 1;


                institutions.PaginationMetaData = new(Count2, pageNumber, pageSize);
                institutions.StartSerialNumber = startSerialNumber2;

                return View(institutions);
            }
            catch (Exception ex)
            {
                Log.Write("AnalyticsController.InstitutionPerformance", $"Err:  {ex.Message}");
                ViewBag.BadNews = "System Challenge";
                return View();
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InstitutionPerformance(InstitutionPerformanceViewModel model)
        {
            try
            {

                TempData["InstitutionPerformanceFilterRequest"] = null;
                int pageSize = 30;
                int pageNumber = 1;

                ViewBag.tranTypes = new SelectList(new[] { "INCOMING", "OUTGOING" });

                var db = new MomoSwitchDbContext();
                var institutionCode = configuration.GetValue<string>("MomoInstitutionCode");

                var filterRequest = new InstitutionPerformanceViewModel
                {
                    FilterRequest = new InstitutionPerformanceFilterRequest
                    {
                        BankCode = model.FilterRequest.BankCode
                    }
                };



                var Data = new List<InstitutionPerformanceTableViewModel>();

                if (model.FilterRequest.BankCode == null)
                {
                    Data = await db.PerformanceTb.Select(x => new InstitutionPerformanceTableViewModel
                    {
                        BankCode = x.BankCode,
                        BankName = x.BankName,
                        Remark = x.Remark,
                        SuccessRate = x.Rate,
                        Time = x.Time
                    }).ToListAsync();
                }

                else
                {
                    Data = await db.PerformanceTb.Where(t => (string.IsNullOrWhiteSpace(model.FilterRequest.BankCode) || t.BankCode == model.FilterRequest.BankCode))
                        .Select(x => new InstitutionPerformanceTableViewModel
                        {
                            BankCode = x.BankCode,
                            BankName = x.BankName,
                            Remark = x.Remark,
                            SuccessRate = x.Rate,
                            Time = x.Time
                        })
                        .ToListAsync();
                }


                TempData["InstitutionPerformanceFilterRequest"] = JsonSerializer.Serialize(filterRequest);
                TempData.Keep();




                int Count = Data.Count;
                Data = Data
               .OrderByDescending(x => x.SuccessRate)
               .Skip((pageNumber - 1) * pageSize)
               .Take(pageSize)
               .ToList();

                var startSerialNumber = (pageNumber - 1) * pageSize + 1;

                model.Institutions = Data;


                model.PaginationMetaData = new(Count, pageNumber, pageSize);
                model.StartSerialNumber = startSerialNumber;
                return View(model);
            }
            catch (Exception ex)
            {
                Log.Write("AnalyticsController:DownloadInstituionsPerformanceReport", $"eRR: {ex.Message}");
                return View("Error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DownloadInstituionsPerformanceReport(InstitutionPerformanceViewModel model)
        {
            try
            {
                var db = new MomoSwitchDbContext();


                var Data = new List<InstitutionPerformanceReport>();

                if (model.FilterRequest.BankCode == null)
                {
                    Data = await db.PerformanceTb.Select(x => new InstitutionPerformanceReport
                    {
                        BankCode = x.BankCode,
                        BankName = x.BankName,
                        Remark = x.Remark,
                        SuccessRate = x.Rate.ToString(x.Rate % 1 == 0 ? "#,0" : "#,0.00", CultureInfo.InvariantCulture),
                        Time = x.Time.ToString("dd/MM/yyyy HH:mm:ss.fff")
                    }).ToListAsync();
                }

                else
                {
                    Data = await db.PerformanceTb.Where(t => (string.IsNullOrWhiteSpace(model.FilterRequest.BankCode) || t.BankCode == model.FilterRequest.BankCode))
                        .Select(x => new InstitutionPerformanceReport
                        {
                            BankCode = x.BankCode,
                            BankName = x.BankName,
                            Remark = x.Remark,
                            SuccessRate = x.Rate.ToString(x.Rate % 1 == 0 ? "#,0" : "#,0.00", CultureInfo.InvariantCulture),
                            Time = x.Time.ToString("dd/MM/yyyy HH:mm:ss.fff")
                        })
                        .ToListAsync();
                }

                Data = Data.OrderByDescending(x => x.SuccessRate).ToList();
                Log.Write("AnalyticsController.DownloadInstituionsPerformanceReport", $"Report count {Data.Count}");

                var sheet = $"MomoSwitchInstitutionsPerformance-{DateTime.Now}";
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using (ExcelPackage excelPackage = new ExcelPackage())
                {
                    var workSheet = excelPackage.Workbook.Worksheets.Add(sheet);
                    var SheetRange = workSheet.Cells["A1"].LoadFromCollection(Data, true);
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
                Log.Write("AnalyticsController.DownloadInstituionsPerformanceReport", $"Err:  {ex.Message}");
                ViewBag.BadNews = "System Challenge";
                return View();
            }

        }

        [HttpGet]
        public async Task<IActionResult> DailyReconcilation(int page)
        {
            try
            {
                int pageSize = 30;
                int pageNumber = (page == 0 ? 1 : page);

                ViewBag.remarks = new SelectList(new[] { "OK", "Others" });

                var institutionCode = configuration.GetValue<string>("MomoInstitutionCode");
                var summaryReportTableList = new List<SummaryReportTableViewModel>();

                var db = new MomoSwitchDbContext();

                var loggedInUser = HttpContext.GetLoggedInUser();

                var loggedInUserInDatabase = await db.PortalUserTb.SingleOrDefaultAsync(x => x.Username.ToLower() == loggedInUser.ToLower());

                if (loggedInUserInDatabase == null)
                {
                    Log.Write("AnalyticsController:SummaryReport", $"eRR: Logged in user not gotten");
                    return RedirectToAction("Logout", "Account");
                }

                if (!loggedInUserInDatabase.IsActive)
                {
                    //should they be logged out?
                    Log.Write("AnalyticsController:SummaryReport", $"eRR: User with username: {loggedInUserInDatabase.Username} is deactivated");
                    return RedirectToAction("Logout", "Account");
                }

                var Data = new List<DailyReconcilationTableViewModel>();

                if (page != 0 && TempData["DailyReconcilationFilterRequest"]?.ToString() != null)
                {
                    var FilterRequest = JsonSerializer.Deserialize<DailyReconcilationViewModel>(TempData["DailyReconcilationFilterRequest"].ToString());



                    if (FilterRequest.FilterRequest.PaymentRef == null && FilterRequest.FilterRequest.Processor == null
                        && FilterRequest.FilterRequest.EwpSessionId == null && FilterRequest.FilterRequest.MsrSessionId == null && FilterRequest.FilterRequest.ProcessorSessionId == null
                        && FilterRequest.FilterRequest.Remarks == null && FilterRequest.FilterRequest.StartDate == null && FilterRequest.FilterRequest.EndDate == null)
                    {
                        Data = await db.DailyReconciliationTb.OrderByDescending(x => x.Date).Take(50).Select(x => new DailyReconcilationTableViewModel
                        {
                            Amount = x.Amount,
                            Date = x.Date,
                            Id = x.Id,
                            EwpResponseCode = x.EwpResponseCode,
                            EwpSessionId = x.EwpSessionId,
                            MsrResponseCode = x.MsrResponseCode,
                            MsrSessionId = x.MsrSessionId,
                            PaymentRef = x.PaymentRef,
                            Processor = x.Processor,
                            ProcessorResponseCode = x.ProcessorResponseCode,
                            ProcessorSessionId = x.ProcessorSessionId,
                            Remarks = x.Remarks
                        }).ToListAsync();
                    }

                    else
                    {
                        Data = await db.DailyReconciliationTb.Where(t => (!FilterRequest.FilterRequest.StartDate.HasValue || t.Date >= DateTime.Parse(Convert.ToDateTime(FilterRequest.FilterRequest.StartDate).ToString("yyyy-MM-dd") + " 00:00:00")) &&
                                                      (!FilterRequest.FilterRequest.EndDate.HasValue || t.Date <= DateTime.Parse(Convert.ToDateTime(FilterRequest.FilterRequest.EndDate).ToString("yyyy-MM-dd") + " 23:59:59")) &&
                                                        (string.IsNullOrEmpty(FilterRequest.FilterRequest.PaymentRef) || t.PaymentRef == FilterRequest.FilterRequest.PaymentRef.Trim()) &&
                                                        (string.IsNullOrEmpty(FilterRequest.FilterRequest.EwpSessionId) || t.EwpSessionId == FilterRequest.FilterRequest.EwpSessionId.Trim()) &&
                                                        (string.IsNullOrEmpty(FilterRequest.FilterRequest.ProcessorSessionId) || t.ProcessorSessionId == FilterRequest.FilterRequest.ProcessorSessionId.Trim()) &&
                                                        (string.IsNullOrEmpty(FilterRequest.FilterRequest.MsrSessionId) || t.MsrSessionId == FilterRequest.FilterRequest.MsrSessionId.Trim()) &&
                                                        (string.IsNullOrEmpty(FilterRequest.FilterRequest.Processor) || t.Processor == FilterRequest.FilterRequest.Processor.Trim()) &&
                                                        (string.IsNullOrEmpty(FilterRequest.FilterRequest.Remarks) || (FilterRequest.FilterRequest.Remarks.ToLower() == "ok" && t.Remarks.ToLower() == "ok") || (FilterRequest.FilterRequest.Remarks == "Others" && t.Remarks.ToLower() != "ok")))
                                                        .Select(x => new DailyReconcilationTableViewModel
                                                        {
                                                            Amount = x.Amount,
                                                            Date = x.Date,
                                                            Id = x.Id,
                                                            EwpResponseCode = x.EwpResponseCode,
                                                            EwpSessionId = x.EwpSessionId,
                                                            MsrResponseCode = x.MsrResponseCode,
                                                            MsrSessionId = x.MsrSessionId,
                                                            PaymentRef = x.PaymentRef,
                                                            Processor = x.Processor,
                                                            ProcessorResponseCode = x.ProcessorResponseCode,
                                                            ProcessorSessionId = x.ProcessorSessionId,
                                                            Remarks = x.Remarks
                                                        })
                                                        .ToListAsync();
                    }



                    int Count = Data.Count;
                    Data = Data
                   .OrderByDescending(d => d.Date)
                   .Skip((pageNumber - 1) * pageSize)
                   .Take(pageSize)
                   .ToList();

                    var startSerialNumber = (pageNumber - 1) * pageSize + 1;


                    var viewModel = new DailyReconcilationViewModel();
                    viewModel.DailyReconcilations = Data;
                    viewModel.PaginationMetaData = new(Count, pageNumber, pageSize);
                    viewModel.StartSerialNumber = startSerialNumber;

                    viewModel.FilterRequest = new DailyReconcilationFilterRequest
                    {
                        Processor = FilterRequest.FilterRequest.Processor,
                        EndDate = FilterRequest.FilterRequest.EndDate,
                        StartDate = FilterRequest.FilterRequest.StartDate,
                        MsrSessionId = FilterRequest.FilterRequest.MsrSessionId,
                        EwpSessionId = FilterRequest.FilterRequest.EwpSessionId,
                        PaymentRef = FilterRequest.FilterRequest.PaymentRef,
                        ProcessorSessionId = FilterRequest.FilterRequest.ProcessorSessionId,
                        Remarks = FilterRequest.FilterRequest.Remarks
                    };

                    TempData.Keep();

                    return View(viewModel);
                }

                TempData["DailyReconcilationFilterRequest"] = null;

                DailyReconcilationViewModel dailyRecons = new();
                await Task.Run((() =>
                {
                    Data = db.DailyReconciliationTb.OrderByDescending(x => x.Date).Take(50).Select(x => new DailyReconcilationTableViewModel
                    {

                        Amount = x.Amount,
                        Date = x.Date,
                        Id = x.Id,
                        EwpResponseCode = x.EwpResponseCode,
                        EwpSessionId = x.EwpSessionId,
                        MsrResponseCode = x.MsrResponseCode,
                        MsrSessionId = x.MsrSessionId,
                        PaymentRef = x.PaymentRef,
                        Processor = x.Processor,
                        ProcessorResponseCode = x.ProcessorResponseCode,
                        ProcessorSessionId = x.ProcessorSessionId,
                        Remarks = x.Remarks
                    }).ToList();


                    dailyRecons = new DailyReconcilationViewModel()
                    {
                        DailyReconcilations = Data
                    };
                }));

                int Count2 = dailyRecons.DailyReconcilations.Count;
                dailyRecons.DailyReconcilations = dailyRecons.DailyReconcilations
               .OrderByDescending(d => d.Date)
               .Skip((pageNumber - 1) * pageSize)
               .Take(pageSize)
               .ToList();

                var startSerialNumber2 = (pageNumber - 1) * pageSize + 1;


                dailyRecons.PaginationMetaData = new(Count2, pageNumber, pageSize);
                dailyRecons.StartSerialNumber = startSerialNumber2;

                return View(dailyRecons);

            }
            catch (Exception ex)
            {
                Log.Write("AnalyticsController.DailyReconcilation.Get", $"Err:  {ex.Message}");
                ViewBag.BadNews = "System Challenge";
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DailyReconcilation(DailyReconcilationViewModel model)
        {
            try
            {
                TempData["DailyReconcilationFilterRequest"] = null;
                int pageSize = 30;
                int pageNumber = 1;

                ViewBag.remarks = new SelectList(new[] { "OK", "Others" });


                var db = new MomoSwitchDbContext();
                var institutionCode = configuration.GetValue<string>("MomoInstitutionCode");

                var FilterRequest = new DailyReconcilationViewModel
                {
                    FilterRequest = new DailyReconcilationFilterRequest
                    {
                        Processor = model.FilterRequest.Processor,
                        EndDate = model.FilterRequest.EndDate,
                        StartDate = model.FilterRequest.StartDate,
                        MsrSessionId = model.FilterRequest.MsrSessionId,
                        EwpSessionId = model.FilterRequest.EwpSessionId,
                        PaymentRef = model.FilterRequest.PaymentRef,
                        ProcessorSessionId = model.FilterRequest.ProcessorSessionId,
                        Remarks = model.FilterRequest.Remarks
                    }
                };



                var Data = new List<DailyReconcilationTableViewModel>();

                var startDay = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd") + " 00:00:00");
                var endDay = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd") + " 23:59:59");

                if (FilterRequest.FilterRequest.PaymentRef == null && FilterRequest.FilterRequest.Processor == null
                      && FilterRequest.FilterRequest.EwpSessionId == null && FilterRequest.FilterRequest.MsrSessionId == null && FilterRequest.FilterRequest.ProcessorSessionId == null
                      && FilterRequest.FilterRequest.Remarks == null && FilterRequest.FilterRequest.StartDate == null && FilterRequest.FilterRequest.EndDate == null)
                {
                    Data = await db.DailyReconciliationTb.OrderByDescending(x => x.Date).Take(50).Select(x => new DailyReconcilationTableViewModel
                    {
                        Amount = x.Amount,
                        Date = x.Date,
                        EwpResponseCode = x.EwpResponseCode,
                        EwpSessionId = x.EwpSessionId,
                        MsrResponseCode = x.MsrResponseCode,
                        MsrSessionId = x.MsrSessionId,
                        PaymentRef = x.PaymentRef,
                        Processor = x.Processor,
                        Id = x.Id,
                        ProcessorResponseCode = x.ProcessorResponseCode,
                        ProcessorSessionId = x.ProcessorSessionId,
                        Remarks = x.Remarks
                    }).ToListAsync();
                }
                else
                {
                    db.TransactionTb.Where(x => (true || x.TransactionId == ""));


                    Data = await db.DailyReconciliationTb.Where(t => (!FilterRequest.FilterRequest.StartDate.HasValue || t.Date >= DateTime.Parse(Convert.ToDateTime(FilterRequest.FilterRequest.StartDate).ToString("yyyy-MM-dd") + " 00:00:00")) &&
                                                  (!FilterRequest.FilterRequest.EndDate.HasValue || t.Date <= DateTime.Parse(Convert.ToDateTime(FilterRequest.FilterRequest.EndDate).ToString("yyyy-MM-dd") + " 23:59:59")) &&
                                                    (string.IsNullOrEmpty(FilterRequest.FilterRequest.PaymentRef) || t.PaymentRef == FilterRequest.FilterRequest.PaymentRef.Trim()) &&                                                    
                                                    (string.IsNullOrEmpty(FilterRequest.FilterRequest.EwpSessionId) || t.EwpSessionId == FilterRequest.FilterRequest.EwpSessionId.Trim()) &&
                                                    (string.IsNullOrEmpty(FilterRequest.FilterRequest.ProcessorSessionId) || t.ProcessorSessionId == FilterRequest.FilterRequest.ProcessorSessionId.Trim()) &&
                                                    (string.IsNullOrEmpty(FilterRequest.FilterRequest.MsrSessionId) || t.MsrSessionId == FilterRequest.FilterRequest.MsrSessionId.Trim()) &&
                                                    (string.IsNullOrEmpty(FilterRequest.FilterRequest.Processor) || t.Processor == FilterRequest.FilterRequest.Processor.Trim()) &&
                                                    (string.IsNullOrEmpty(FilterRequest.FilterRequest.Remarks) || (FilterRequest.FilterRequest.Remarks.ToLower() == "ok" && t.Remarks.ToLower() == "ok") || (FilterRequest.FilterRequest.Remarks == "Others" && t.Remarks.ToLower() != "ok")))
                                                    .Select(x => new DailyReconcilationTableViewModel
                                                    {
                                                        Amount = x.Amount,
                                                        Id = x.Id,
                                                        Date = x.Date,
                                                        EwpResponseCode = x.EwpResponseCode,
                                                        EwpSessionId = x.EwpSessionId,
                                                        MsrResponseCode = x.MsrResponseCode,
                                                        MsrSessionId = x.MsrSessionId,
                                                        PaymentRef = x.PaymentRef,
                                                        Processor = x.Processor,
                                                        ProcessorResponseCode = x.ProcessorResponseCode,
                                                        ProcessorSessionId = x.ProcessorSessionId,
                                                        Remarks = x.Remarks
                                                    })
                                                    .ToListAsync();
                }


                TempData["DailyReconcilationFilterRequest"] = JsonSerializer.Serialize(FilterRequest);
                TempData.Keep();


                int Count = Data.Count;
                Data = Data
               .OrderByDescending(x => x.Date)
               .Skip((pageNumber - 1) * pageSize)
               .Take(pageSize)
               .ToList();

                var startSerialNumber = (pageNumber - 1) * pageSize + 1;

                model.DailyReconcilations = Data;


                model.PaginationMetaData = new(Count, pageNumber, pageSize);
                model.StartSerialNumber = startSerialNumber;
                return View(model);
            }
            catch (Exception ex)
            {
                Log.Write("AnalyticsController:DailyReconcilation.Post", $"eRR: {ex.Message}");
                return View("Error");
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DownloadDailyReconcilationReport(DailyReconcilationViewModel model)
        {
            try
            {
                var db = new MomoSwitchDbContext();


                var Data = new List<DailyReconcilationReport>();


                if (model.FilterRequest.PaymentRef == null && model.FilterRequest.Processor == null
                      && model.FilterRequest.EwpSessionId == null && model.FilterRequest.MsrSessionId == null && model.FilterRequest.ProcessorSessionId == null
                      && model.FilterRequest.Remarks == null && model.FilterRequest.StartDate == null && model.FilterRequest.EndDate == null)
                {
                    Data = await db.DailyReconciliationTb.OrderByDescending(x => x.Date).Take(50).Select(x => new DailyReconcilationReport
                    {
                        Amount = x.Amount,
                        Date = x.Date.ToString("dd/MM/yyyy HH:mm:ss.fff"),
                        EwpResponseCode = x.EwpResponseCode,
                        EwpSessionId = x.EwpSessionId,
                        MsrResponseCode = x.MsrResponseCode,
                        MsrSessionId = x.MsrSessionId,
                        PaymentRef = x.PaymentRef,
                        Processor = x.Processor,                        
                        ProcessorResponseCode = x.ProcessorResponseCode,
                        ProcessorSessionId = x.ProcessorSessionId,
                        Remarks = x.Remarks
                    }).ToListAsync();
                }

                else
                {
                    db.TransactionTb.Where(x => (true || x.TransactionId == ""));


                    Data = await db.DailyReconciliationTb.Where(t => (!model.FilterRequest.StartDate.HasValue || t.Date >= DateTime.Parse(Convert.ToDateTime(model.FilterRequest.StartDate).ToString("yyyy-MM-dd") + " 00:00:00")) &&
                                                  (!model.FilterRequest.EndDate.HasValue || t.Date <= DateTime.Parse(Convert.ToDateTime(model.FilterRequest.EndDate).ToString("yyyy-MM-dd") + " 23:59:59")) &&
                                                    (string.IsNullOrEmpty(model.FilterRequest.PaymentRef) || t.PaymentRef == model.FilterRequest.PaymentRef.Trim()) &&                                                    
                                                    (string.IsNullOrEmpty(model.FilterRequest.EwpSessionId) || t.EwpSessionId == model.FilterRequest.EwpSessionId.Trim()) &&
                                                    (string.IsNullOrEmpty(model.FilterRequest.ProcessorSessionId) || t.ProcessorSessionId == model.FilterRequest.ProcessorSessionId.Trim()) &&
                                                    (string.IsNullOrEmpty(model.FilterRequest.MsrSessionId) || t.MsrSessionId == model.FilterRequest.MsrSessionId.Trim()) &&
                                                    (string.IsNullOrEmpty(model.FilterRequest.Processor) || t.Processor == model.FilterRequest.Processor.Trim()) &&
                                                    (string.IsNullOrEmpty(model.FilterRequest.Remarks) || (model.FilterRequest.Remarks.ToLower() == "ok" && t.Remarks.ToLower() == "ok") || (model.FilterRequest.Remarks == "Others" && t.Remarks.ToLower() != "ok")))
                                                    .Select(x => new DailyReconcilationReport
                                                    {
                                                        Amount = x.Amount,                                                        
                                                        Date = x.Date.ToString("dd/MM/yyyy HH:mm:ss.fff"),
                                                        EwpResponseCode = x.EwpResponseCode,
                                                        EwpSessionId = x.EwpSessionId,
                                                        MsrResponseCode = x.MsrResponseCode,
                                                        MsrSessionId = x.MsrSessionId,
                                                        PaymentRef = x.PaymentRef,
                                                        Processor = x.Processor,
                                                        ProcessorResponseCode = x.ProcessorResponseCode,
                                                        ProcessorSessionId = x.ProcessorSessionId,
                                                        Remarks = x.Remarks
                                                    })
                                                    .ToListAsync();
                }

                Data = Data.OrderByDescending(x => x.Date).ToList();
                Log.Write("AnalyticsController.DownloadDailyReconcilationReport", $"Report count {Data.Count}");

                var sheet = $"MomoSwitchDailyReconcilation-{DateTime.Now}";
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using (ExcelPackage excelPackage = new ExcelPackage())
                {
                    var workSheet = excelPackage.Workbook.Worksheets.Add(sheet);
                    var SheetRange = workSheet.Cells["A1"].LoadFromCollection(Data, true);
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
                Log.Write("AnalyticsController.DownloadDailyReconcilationReport", $"Err:  {ex.Message}");
                ViewBag.BadNews = "System Challenge";
                return View();
            }

        }

    }
}
