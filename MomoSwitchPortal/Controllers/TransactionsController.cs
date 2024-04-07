using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Momo.Common.Actions;
using Momo.Common.Models.Tables;
using MomoSwitchPortal.Actions;
using MomoSwitchPortal.Models.Database;
using MomoSwitchPortal.Models.Internals;
using MomoSwitchPortal.Models.ViewModels.Transaction;
using OfficeOpenXml;
using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MomoSwitchPortal.Controllers
{
    //[Authorize]
    public class TransactionsController : Controller
    {
        private ILog Log;
        private readonly INotyfService ToastNotification;
        private readonly IConfiguration configuration;

        public TransactionsController(ILog log, INotyfService toastNotification, IConfiguration configuration)
        {
            Log = log;
            ToastNotification = toastNotification;
            this.configuration = configuration;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int page)
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
                    Log.Write("TransactionsController:Index", $"eRR: Logged in user not gotten");
                    return RedirectToAction("Logout", "Account");
                }

                if (!loggedInUserInDatabase.IsActive)
                {
                    //should they be logged out?
                    Log.Write("TransactionsController:Index", $"eRR: User with username: {loggedInUserInDatabase.Username} is deactivated");
                    return RedirectToAction("Logout", "Account");
                }


                if (page != 0 && TempData["TransactionFilterRequest"]?.ToString() != null)
                {
                    //  List<TransactionItem> Trans1 = JsonSerializer.Deserialize<List<TransactionItem>>(TempData["Tran"].ToString());

                    var FilterRequest = JsonSerializer.Deserialize<TransactionViewModel>(TempData["TransactionFilterRequest"].ToString());

                    var Data = new List<TransactionTableViewModel>();


                    if (FilterRequest.FilterRequest.EndDate == null && FilterRequest.FilterRequest.StartDate == null
                        && string.IsNullOrEmpty(FilterRequest.FilterRequest.TranType) && string.IsNullOrEmpty(FilterRequest.FilterRequest.ResponseCode) && string.IsNullOrEmpty(FilterRequest.FilterRequest.Processor)
                        && string.IsNullOrEmpty(FilterRequest.FilterRequest.TransactionId))
                    {
                        Data = await db.TransactionTb.OrderByDescending(x => x.Date).Take(50).Select(x => new TransactionTableViewModel
                        {
                            Amount = x.Amount,
                            Date = x.Date,
                            BenefBankCode = x.BenefBankCode,
                            Id = x.Id,
                            Processor = x.Processor,
                            ResponseCode = x.ResponseCode,
                            ResponseMessage = x.ResponseMessage,
                            SourceBankCode = x.SourceBankCode,
                            TransactionId = x.TransactionId,
                            BenefAccountNumber = x.BenefAccountNumber,
                            SourceAccountNumber = x.SourceAccountNumber,
                            BenefBankName = x.BenefBankName,
                            SourceBankName = x.SourceBankName,
                        }).ToListAsync();
                    }

                    else
                    {

                        Data = await db.TransactionTb
                                          .Where(t => (!FilterRequest.FilterRequest.StartDate.HasValue || t.Date >= DateTime.Parse(Convert.ToDateTime(FilterRequest.FilterRequest.StartDate).ToString("yyyy-MM-dd") + " 00:00:00")) &&
                                                      (!FilterRequest.FilterRequest.EndDate.HasValue || t.Date <= DateTime.Parse(Convert.ToDateTime(FilterRequest.FilterRequest.EndDate).ToString("yyyy-MM-dd") + " 23:59:59")) &&
                                                      (string.IsNullOrEmpty(FilterRequest.FilterRequest.TransactionId) || t.TransactionId == FilterRequest.FilterRequest.TransactionId.Trim()) &&
                                                      (string.IsNullOrEmpty(FilterRequest.FilterRequest.Processor) || t.Processor.ToLower().Contains(FilterRequest.FilterRequest.Processor.Trim().ToLower())) &&
                                                      (string.IsNullOrEmpty(FilterRequest.FilterRequest.ResponseCode) || t.ResponseCode == FilterRequest.FilterRequest.ResponseCode.Trim().ToLower()) &&
                                                      (string.IsNullOrEmpty(FilterRequest.FilterRequest.TranType) || (FilterRequest.FilterRequest.TranType == "INCOMING" && t.BenefBankCode == institutionCode)
                                                      || (FilterRequest.FilterRequest.TranType == "OUTGOING" && t.SourceBankCode == institutionCode)))
                                          .Select(x => new TransactionTableViewModel
                                          {
                                              Amount = x.Amount,
                                              Date = x.Date,
                                              BenefBankCode = x.BenefBankCode,
                                              Id = x.Id,
                                              Processor = x.Processor,
                                              ResponseCode = x.ResponseCode,
                                              ResponseMessage = x.ResponseMessage,
                                              SourceBankCode = x.SourceBankCode,
                                              TransactionId = x.TransactionId,
                                              BenefAccountNumber = x.BenefAccountNumber,
                                              SourceAccountNumber = x.SourceAccountNumber,
                                              BenefBankName = x.BenefBankName,
                                              SourceBankName = x.SourceBankName,
                                          }).ToListAsync();


                    }



                    int Count = Data.Count;
                    Data = Data
                   .OrderByDescending(x => x.Date)
                   .Skip((pageNumber - 1) * pageSize)
                   .Take(pageSize)
                   .ToList();

                    var startSerialNumber = (pageNumber - 1) * pageSize + 1;


                    var viewModel = new TransactionViewModel();
                    viewModel.Transactions = Data;
                    viewModel.PaginationMetaData = new(Count, pageNumber, pageSize);
                    viewModel.StartSerialNumber = startSerialNumber;

                    viewModel.FilterRequest = new TransactionFilterRequest
                    {
                        StartDate = FilterRequest.FilterRequest.StartDate,
                        EndDate = FilterRequest.FilterRequest.EndDate,
                        TransactionId = FilterRequest.FilterRequest.TransactionId,
                        TranType = FilterRequest.FilterRequest.TranType,
                        ResponseCode = FilterRequest.FilterRequest.ResponseCode,
                        Processor = FilterRequest.FilterRequest.Processor
                    };

                    TempData.Keep();

                    return View(viewModel);
                }

                TempData["TransactionFilterRequest"] = null;

                TransactionViewModel Trans = new();
                await Task.Run((() =>
                {
                    Trans = new TransactionViewModel()
                    {
                        Transactions = db.TransactionTb.OrderByDescending(x => x.Date).Take(50).Select(x => new TransactionTableViewModel
                        {
                            Amount = x.Amount,
                            BenefBankCode = x.BenefBankCode,
                            Date = x.Date,
                            Id = x.Id,
                            ResponseCode = x.ResponseCode,
                            Processor = x.Processor,
                            ResponseMessage = x.ResponseMessage,
                            SourceBankCode = x.SourceBankCode,
                            TransactionId = x.TransactionId,
                            BenefAccountNumber = x.BenefAccountNumber,
                            SourceAccountNumber = x.SourceAccountNumber,
                            BenefBankName = x.BenefBankName,
                            SourceBankName = x.SourceBankName,
                        }).ToList()
                    };
                }));

                int Count2 = Trans.Transactions.Count;
                Trans.Transactions = Trans.Transactions
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
                Log.Write("TransactionsController:Index", $"eRR: {ex.Message}");
                return View("Error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(TransactionViewModel model)
        {
            try
            {

                TempData["TransactionFilterRequest"] = null;
                int pageSize = 30;
                int pageNumber = 1;

                ViewBag.tranTypes = new SelectList(new[] { "INCOMING", "OUTGOING" });

                var db = new MomoSwitchDbContext();
                var institutionCode = configuration.GetValue<string>("MomoInstitutionCode");

                var filterRequest = new TransactionViewModel
                {
                    FilterRequest = new TransactionFilterRequest
                    {
                        ResponseCode = model.FilterRequest.ResponseCode,
                        Amount = model.FilterRequest.Amount,
                        BenefBankCode = model.FilterRequest.BenefBankCode,
                        EndDate = model.FilterRequest.EndDate,
                        Processor = model.FilterRequest.Processor,
                        SourceBankCode = model.FilterRequest.SourceBankCode,
                        StartDate = model.FilterRequest.StartDate,
                        TransactionId = model.FilterRequest.TransactionId,
                        TranType = model.FilterRequest.TranType
                    },
                };



                var Data = new List<TransactionTableViewModel>();


                if (model.FilterRequest.EndDate == null && model.FilterRequest.StartDate == null
                    && string.IsNullOrEmpty(model.FilterRequest.TranType) && string.IsNullOrEmpty(model.FilterRequest.ResponseCode) && string.IsNullOrEmpty(model.FilterRequest.Processor)
                    && string.IsNullOrEmpty(model.FilterRequest.TransactionId))
                {
                    Data = await db.TransactionTb.OrderByDescending(x => x.Date).Take(50).Select(x => new TransactionTableViewModel
                    {
                        Amount = x.Amount,
                        Date = x.Date,
                        BenefBankCode = x.BenefBankCode,
                        Id = x.Id,
                        Processor = x.Processor,
                        ResponseCode = x.ResponseCode,
                        ResponseMessage = x.ResponseMessage,
                        SourceBankCode = x.SourceBankCode,
                        TransactionId = x.TransactionId,
                        BenefAccountNumber = x.BenefAccountNumber,
                        SourceAccountNumber = x.SourceAccountNumber,
                        BenefBankName = x.BenefBankName,
                        SourceBankName = x.SourceBankName,
                    }).ToListAsync();
                }

                else
                {

                    Data = await db.TransactionTb
                                      .Where(t => (!model.FilterRequest.StartDate.HasValue || t.Date >= DateTime.Parse(Convert.ToDateTime(model.FilterRequest.StartDate).ToString("yyyy-MM-dd") + " 00:00:00")) &&
                                                  (!model.FilterRequest.EndDate.HasValue || t.Date <= DateTime.Parse(Convert.ToDateTime(model.FilterRequest.EndDate).ToString("yyyy-MM-dd") + " 23:59:59")) &&
                                                  (string.IsNullOrEmpty(model.FilterRequest.TransactionId) || t.TransactionId == model.FilterRequest.TransactionId.Trim()) &&
                                                  (string.IsNullOrEmpty(model.FilterRequest.Processor) || t.Processor.ToLower().Contains(model.FilterRequest.Processor.Trim().ToLower())) &&
                                                  (string.IsNullOrEmpty(model.FilterRequest.ResponseCode) || t.ResponseCode == model.FilterRequest.ResponseCode.Trim().ToLower()) &&
                                                  (string.IsNullOrEmpty(model.FilterRequest.TranType) || (model.FilterRequest.TranType == "INCOMING" && t.BenefBankCode == institutionCode)
                                                  || (model.FilterRequest.TranType == "OUTGOING" && t.SourceBankCode == institutionCode)))
                                      .Select(x => new TransactionTableViewModel
                                      {
                                          Amount = x.Amount,
                                          Date = x.Date,
                                          BenefBankCode = x.BenefBankCode,
                                          Id = x.Id,
                                          Processor = x.Processor,
                                          ResponseCode = x.ResponseCode,
                                          ResponseMessage = x.ResponseMessage,
                                          SourceBankCode = x.SourceBankCode,
                                          TransactionId = x.TransactionId,
                                          BenefAccountNumber = x.BenefAccountNumber,
                                          SourceAccountNumber = x.SourceAccountNumber,
                                          BenefBankName = x.BenefBankName,
                                          SourceBankName = x.SourceBankName,
                                      }).ToListAsync();


                }


                TempData["TransactionFilterRequest"] = JsonSerializer.Serialize(filterRequest);
                TempData.Keep();





                int Count = Data.Count;
                Data = Data
               .OrderByDescending(x => x.Date)
               .Skip((pageNumber - 1) * pageSize)
               .Take(pageSize)
               .ToList();

                var startSerialNumber = (pageNumber - 1) * pageSize + 1;

                model.Transactions = Data;


                model.PaginationMetaData = new(Count, pageNumber, pageSize);
                model.StartSerialNumber = startSerialNumber;
                ViewBag.startDate = model.FilterRequest.StartDate;
                return View(model);
            }
            catch (Exception ex)
            {
                Log.Write("TransactionsController:Index", $"eRR: {ex.Message}");
                return View("Error");
            }
        }

        [HttpGet("transactions/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var db = new MomoSwitchDbContext();
                var transaction = await db.TransactionTb.SingleOrDefaultAsync(x => x.Id == id);

                if (transaction == null)
                {
                    ToastNotification.Error("System Challenge");
                    return RedirectToAction("Index", "Home");
                }

                var viewModel = new TransactionDetailsViewModel
                {
                    Amount = transaction.Amount,
                    BenefAccountName = transaction.BenefAccountName,
                    BenefAccountNumber = transaction.BenefAccountNumber,
                    BenefBankCode = transaction.BenefBankCode,
                    BenefBvn = transaction.BenefBvn,
                    BenefKycLevel = transaction.BenefKycLevel,
                    ChannelCode = transaction.ChannelCode,
                    Date = transaction.Date,
                    Fee = transaction.Fee,
                    ManadateRef = transaction.ManadateRef,
                    NameEnquiryRef = transaction.NameEnquiryRef,
                    Narration = transaction.Narration,
                    PaymentDate = transaction.PaymentDate,
                    PaymentReference = transaction.PaymentReference,
                    Processor = transaction.Processor,
                    ResponseCode = transaction.ResponseCode,
                    ResponseMessage = transaction.ResponseMessage,
                    SessionId = transaction.SessionId,
                    SourceAccountName = transaction.SourceAccountName,
                    SourceAccountNumber = transaction.SourceAccountNumber,
                    SourceBankCode = transaction.SourceBankCode,
                    SourceBvn = transaction.SourceBvn,
                    SourceKycLevel = transaction.SourceKycLevel,
                    TransactionId = transaction.TransactionId,
                    ValidateDate = transaction.ValidateDate,
                    SourceBankName = transaction.SourceBankName,
                    BenefBankName = transaction.BenefBankName
                };
                return View(viewModel);
            }
            catch (Exception ex)
            {
                Log.Write("TransactionsController:Index", $"eRR: {ex.Message}");
                return View("Error");
            }
        }

        [HttpPost]
        public async Task<ActionResult> DownloadTransactionsReport(TransactionViewModel model)
        {
            try
            {
                var db = new MomoSwitchDbContext();



                var institutionCode = configuration.GetValue<string>("MomoInstitutionCode");


                var Data = new List<TransactionReport>();


                if (model.FilterRequest.EndDate == null && model.FilterRequest.StartDate == null
                    && string.IsNullOrEmpty(model.FilterRequest.TranType) && string.IsNullOrEmpty(model.FilterRequest.ResponseCode) && string.IsNullOrEmpty(model.FilterRequest.Processor)
                    && string.IsNullOrEmpty(model.FilterRequest.TransactionId))
                {
                    Data = await db.TransactionTb.OrderByDescending(x => x.Date).Take(50).Select(x => new TransactionReport
                    {
                        SessionId = x.SessionId,
                        TransactionId = x.TransactionId,
                        ResponseCode = x.ResponseCode,
                        Processor = x.Processor,
                        ValidateDate = Convert.ToDateTime(x.ValidateDate).ToString("dd/MM/yyyy HH:mm:ss.fff"),
                        BenefAccountName = x.BenefAccountName,
                        BenefAccountNumber = x.BenefAccountNumber,
                        BenefBankCode = x.BenefBankCode,
                        BenefBvn = x.BenefBvn,
                        BenefKycLevel = x.BenefKycLevel,
                        ChannelCode = x.ChannelCode,
                        Fee = x.Fee,
                        ManadateRef = x.ManadateRef,
                        NameEnquiryRef = x.NameEnquiryRef,
                        Narration = x.Narration,
                        PaymentDate = Convert.ToDateTime(x.PaymentDate).ToString("dd/MM/yyyy HH:mm:ss.fff"),
                        PaymentReference = x.PaymentReference,
                        ResponseMessage = x.ResponseMessage,
                        SourceAccountName = x.SourceAccountName,
                        SourceAccountNumber = x.SourceAccountNumber,
                        SourceBankCode = x.SourceBankCode,
                        SourceBvn = x.SourceBvn,
                        SourceKycLevel = x.SourceKycLevel,
                        Date = x.Date.ToString("dd/MM/yyyy HH:mm:ss.fff"),
                        Amount = x.Amount,
                        SourceBankName = x.SourceBankName,
                        BeneficiaryBankName = x.BenefBankName,
                    }).ToListAsync();
                }

                else
                {
                    db.TransactionTb.Where(x => (true || x.TransactionId == ""));

                    Data = await db.TransactionTb
                                       .Where(t => (!model.FilterRequest.StartDate.HasValue || t.Date >= DateTime.Parse(Convert.ToDateTime(model.FilterRequest.StartDate).ToString("yyyy-MM-dd") + " 00:00:00")) &&
                                                   (!model.FilterRequest.EndDate.HasValue || t.Date <= DateTime.Parse(Convert.ToDateTime(model.FilterRequest.EndDate).ToString("yyyy-MM-dd") + " 23:59:59")) &&
                                                   (string.IsNullOrEmpty(model.FilterRequest.TransactionId) || t.TransactionId == model.FilterRequest.TransactionId.Trim()) &&
                                                   (string.IsNullOrEmpty(model.FilterRequest.Processor) || t.Processor.ToLower().Contains(model.FilterRequest.Processor.Trim().ToLower())) &&
                                                   (string.IsNullOrEmpty(model.FilterRequest.ResponseCode) || t.ResponseCode == model.FilterRequest.ResponseCode.Trim().ToLower()) &&
                                                   (string.IsNullOrEmpty(model.FilterRequest.TranType) || (model.FilterRequest.TranType == "INCOMING" && t.BenefBankCode == institutionCode)
                                                   || (model.FilterRequest.TranType == "OUTGOING" && t.SourceBankCode == institutionCode)))
                                       .Select(x => new TransactionReport
                                       {
                                           SessionId = x.SessionId,
                                           TransactionId = x.TransactionId,
                                           ResponseCode = x.ResponseCode,
                                           Processor = x.Processor,
                                           ValidateDate = Convert.ToDateTime(x.ValidateDate).ToString("dd/MM/yyyy HH:mm:ss.fff"),
                                           BenefAccountName = x.BenefAccountName,
                                           BenefAccountNumber = x.BenefAccountNumber,
                                           BenefBankCode = x.BenefBankCode,
                                           BenefBvn = x.BenefBvn,
                                           BenefKycLevel = x.BenefKycLevel,
                                           ChannelCode = x.ChannelCode,
                                           Fee = x.Fee,
                                           ManadateRef = x.ManadateRef,
                                           NameEnquiryRef = x.NameEnquiryRef,
                                           Narration = x.Narration,
                                           PaymentDate = Convert.ToDateTime(x.PaymentDate).ToString("dd/MM/yyyy HH:mm:ss.fff"),
                                           PaymentReference = x.PaymentReference,
                                           ResponseMessage = x.ResponseMessage,
                                           SourceAccountName = x.SourceAccountName,
                                           SourceAccountNumber = x.SourceAccountNumber,
                                           SourceBankCode = x.SourceBankCode,
                                           SourceBvn = x.SourceBvn,
                                           SourceKycLevel = x.SourceKycLevel,
                                           Date = x.Date.ToString("dd/MM/yyyy HH:mm:ss.fff"),
                                           Amount = x.Amount,
                                           SourceBankName = x.SourceBankName,
                                           BeneficiaryBankName = x.BenefBankName,
                                           TranDate = x.Date
                                       }).ToListAsync();

                }

                Data = Data.OrderByDescending(x => x.TranDate).ToList();

                Log.Write("TransactionsController.DownloadTransactionsReport", $"Get report count {Data.Count}");

                var sheet = $"MomoSwitchTransactions-{DateTime.Now}";
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using (ExcelPackage excelPackage = new ExcelPackage())
                {
                    var workSheet = excelPackage.Workbook.Worksheets.Add(sheet);
                    var SheetRange = workSheet.Cells["A1"].LoadFromCollection(Data, true);
                    var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    workSheet.Row(1).Style.Font.Bold = true;
                    workSheet.Row(1).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                    SheetRange.AutoFitColumns();
                    SheetRange.Style.Numberformat.Format = "_( #,##0.00_);_( (#,##0.00);_(* \" - \"_);_(@_)";

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
                Log.Write("TransactionsController.DownloadTransactionsReport", $"Err:  {ex.Message}");
                ViewBag.BadNews = "System Challenge";
                return View();
            }
        }
    }
}
