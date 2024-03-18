using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Momo.Common.Actions;
using Momo.Common.Models.Tables;
using MomoSwitchPortal.Models.Database;
using MomoSwitchPortal.Models.Internals;
using MomoSwitchPortal.Models.ViewModels.Transaction;
using OfficeOpenXml;
using System.Globalization;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MomoSwitchPortal.Controllers
{
    [Authorize(Roles = "Administrator")]
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
        public async Task<IActionResult> Index(int? page)
        {
            try
            {
                ViewBag.tranTypes = new SelectList(new[] { "INCOMING", "OUTGOING" });

                var db = new MomoSwitchDbContext();
                var transactions = await db.TransactionTb.OrderByDescending(x => x.Date).Select(x => new TransactionTableViewModel
                {
                    Amount = x.Amount,
                    BenefBankCode = x.BenefBankCode,
                    Date = x.Date.ToString("MM/dd/yyyy"),
                    Id = x.Id,
                    ResponseCode = x.ResponseCode,
                    Processor = x.Processor,
                    ResponseMessage = x.ResponseMessage,
                    SourceBankCode = x.SourceBankCode,
                    TransactionId = x.TransactionId
                }).ToListAsync();

                var viewModel = new TransactionViewModel
                {
                    Transactions = transactions
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(TransactionViewModel model, int? page)
        {
            try
            {
                ViewBag.tranTypes = new SelectList(new[] { "INCOMING", "OUTGOING" });

                var db = new MomoSwitchDbContext();
                var Data = await db.TransactionTb.OrderByDescending(x => x.Date).ToListAsync();
                var institutionCode = configuration.GetValue<string>("MomoInstitutionCode");

                if (!string.IsNullOrEmpty(model.TransactionId))
                {
                    Data = Data.Where(x => x.TransactionId == model.TransactionId).ToList();
                }
                
                if (!string.IsNullOrEmpty(model.Processor))
                {
                    Data = Data.Where(x => x.Processor.Contains(model.Processor)).ToList();
                }
                
                if (!string.IsNullOrEmpty(model.ResponseCode))
                {
                    Data = Data.Where(x => x.ResponseCode == model.ResponseCode).ToList();
                }

                if (!string.IsNullOrEmpty(model.TranType) && model.TranType == "INCOMING")
                {
                    Data = Data.Where(x => x.BenefBankCode == institutionCode).ToList();
                }
                
                if (!string.IsNullOrEmpty(model.TranType) && model.TranType == "OUTGOING")
                {
                    Data = Data.Where(x => x.SourceBankCode == institutionCode).ToList();
                }
               
                if (model.StartDate != null)
                {
                    var Date1 = DateTime.Parse(model.StartDate.ToString()).ToString("yyyy-MM-dd") + " 00:00:00";
                    var startDate = DateTime.Parse(Date1);
                    Data = Data.Where(x => x.Date >= Convert.ToDateTime(startDate)).ToList();
                }

                if (model.EndDate != null)
                {
                    var Date2 = DateTime.Parse(model.EndDate.ToString()).ToString("yyyy-MM-dd") + " 23:59:59";
                    var endDate = DateTime.Parse(Date2);

                    Data = Data.Where(x => x.Date <= endDate).ToList();
                }

                model.Transactions = Data.Select(x => new TransactionTableViewModel
                {
                    Amount = x.Amount,
                    BenefBankCode = x.BenefBankCode,
                    Date = x.Date.ToString("MM/dd/yyyy"),
                    Id = x.Id,
                    ResponseCode = x.ResponseCode,
                    Processor = x.Processor,
                    ResponseMessage = x.ResponseMessage,
                    SourceBankCode = x.SourceBankCode,
                    TransactionId = x.TransactionId
                }).ToList();

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

                if(transaction == null)
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
                    NameEnquiryRef  = transaction.NameEnquiryRef,                    
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
                    ValidateDate = transaction.ValidateDate
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
        public ActionResult DownloadTransactionsReport(TransactionViewModel model)
        {
            try
            {
                var db = new MomoSwitchDbContext();


                List<TransactionTb> Data = db.TransactionTb.AsNoTracking().Select(x => x).OrderByDescending(x => x.Date).ToList();
                var institutionCode = configuration.GetValue<string>("MomoInstitutionCode");


                if (!string.IsNullOrEmpty(model.TransactionId))
                {
                    Data = Data.Where(x => x.TransactionId == model.TransactionId).ToList();
                }

                if (!string.IsNullOrEmpty(model.Processor))
                {
                    Data = Data.Where(x => x.Processor.Contains(model.Processor)).ToList();
                }

                if (!string.IsNullOrEmpty(model.ResponseCode))
                {
                    Data = Data.Where(x => x.ResponseCode == model.ResponseCode).ToList();
                }

                if (!string.IsNullOrEmpty(model.TranType) && model.TranType == "INCOMING")
                {
                    Data = Data.Where(x => x.BenefBankCode == institutionCode).ToList();
                }

                if (!string.IsNullOrEmpty(model.TranType) && model.TranType == "OUTGOING")
                {
                    Data = Data.Where(x => x.SourceBankCode == institutionCode).ToList();
                }

                var report = Data.Select(x => new TransactionReport
                {
                    SessionId = x.SessionId,
                    TransactionId = x.TransactionId,
                    ResponseCode = x.ResponseCode,
                    Processor = x.Processor,
                    ValidateDate = x.ValidateDate,
                    BenefAccountName = x.BenefAccountName,
                    BenefAccountNumber  = x.BenefAccountNumber,
                    BenefBankCode   = x.BenefBankCode,
                    BenefBvn = x.BenefBvn,
                    BenefKycLevel   = x.BenefKycLevel,
                    ChannelCode = x.ChannelCode,
                    Fee = x.Fee,    
                    ManadateRef = x.ManadateRef,
                    NameEnquiryRef = x.NameEnquiryRef,
                    Narration = x.Narration,
                    PaymentDate = x.PaymentDate,
                    PaymentReference = x.PaymentReference,
                    ResponseMessage = x.ResponseMessage,    
                    SourceAccountName = x.SourceAccountName,
                    SourceAccountNumber = x.SourceAccountNumber,
                    SourceBankCode      = x.SourceBankCode,
                    SourceBvn = x.SourceBvn,
                    SourceKycLevel = x.SourceKycLevel    ,                                                     
                    Date = x.Date,
                    Amount = x.Amount
                }).ToList();

                var sheet = $"MomoSwitchTransactions-{DateTime.Now}";
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using (ExcelPackage excelPackage = new ExcelPackage())
                {
                    var workSheet = excelPackage.Workbook.Worksheets.Add(sheet);
                    var SheetRange = workSheet.Cells["A1"].LoadFromCollection(report, true);
                    var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    workSheet.Row(1).Style.Font.Bold = true;
                    workSheet.Row(1).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    SheetRange.AutoFitColumns();
                    SheetRange.Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \" - \"_);_(@_)";

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
