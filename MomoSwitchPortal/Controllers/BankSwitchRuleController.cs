using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Momo.Common.Actions;
using MomoSwitchPortal.Actions;
using MomoSwitchPortal.Actions.Rules;
using MomoSwitchPortal.Models.Database;
using MomoSwitchPortal.Models.ViewModels.Rules.BankSwitch;
using MomoSwitchPortal.Models.ViewModels.Rules.Switch;

namespace MomoSwitchPortal.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class BankSwitchRuleController : Controller
    {
        private ILog Log;
        private readonly IBankSwitch bankSwitchManager;
        private readonly ISwitch switchManager;

        public BankSwitchRuleController(IBankSwitch bankSwitchManager, ILog log, ISwitch switchManager)
        {
            this.bankSwitchManager = bankSwitchManager;
            Log = log;
            this.switchManager = switchManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var db = new MomoSwitchDbContext();
                var loggedInUser = HttpContext.GetLoggedInUser();

                var loggedInUserInDatabase = await db.PortalUserTb.SingleOrDefaultAsync(x => x.Username.ToLower() == loggedInUser.ToLower());

                if (loggedInUserInDatabase == null)
                {
                    Log.Write("BankSwitchController:Index", $"eRR: Logged in user not gotten");
                    return RedirectToAction("Logout", "Account");
                }

                if (!loggedInUserInDatabase.IsActive)
                {
                    //should they be logged out?
                    Log.Write("BankSwitchController:Index", $"eRR: User with username: {loggedInUserInDatabase.Username} is deactivated");
                    return RedirectToAction("Logout", "Account");
                }
                var result = bankSwitchManager.Get();

                if (result.ResponseHeader.ResponseCode != "00")
                {
                    return View("Error");
                }

                return View(result);
            }
            catch (Exception ex)
            {
                Log.Write("BankSwitchController:Index", $"eRR: {ex.Message}");
                ModelState.AddModelError("", "Something went wrong. Please try again later");
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(BankSwitchFilter model)
        {
            try
            {
                var db = new MomoSwitchDbContext();
                var loggedInUser = HttpContext.GetLoggedInUser();

                var loggedInUserInDatabase = await db.PortalUserTb.SingleOrDefaultAsync(x => x.Username.ToLower() == loggedInUser.ToLower());

                if (loggedInUserInDatabase == null)
                {
                    Log.Write("BankSwitchController:Index", $"eRR: Logged in user not gotten");
                    return RedirectToAction("Logout", "Account");
                }

                if (!loggedInUserInDatabase.IsActive)
                {
                    //should they be logged out?
                    Log.Write("BankSwitchController:Index", $"eRR: User with username: {loggedInUserInDatabase.Username} is deactivated");
                    return RedirectToAction("Logout", "Account");
                }

                var result = bankSwitchManager.Get();

                if (result.ResponseHeader.ResponseCode != "00")
                    return View("Error");

                var filteredList = new List<BankSwitchDetails>();
                if (!string.IsNullOrWhiteSpace(model.Processor))
                {
                    filteredList = result.BankSwitchDetails.Where(x => x.Processor.Contains(model.Processor, StringComparison.CurrentCultureIgnoreCase)).ToList();
                    result.BankSwitchDetails = filteredList;
                }
                if (!string.IsNullOrWhiteSpace(model.BankCode))
                {
                    filteredList = result.BankSwitchDetails.Where(x => x.BankCode.Contains(model.BankCode, StringComparison.CurrentCultureIgnoreCase)).ToList();
                    result.BankSwitchDetails = filteredList;
                }

                result.BankCode = model.BankCode;
                result.Processor = model.Processor;//just to initialize the processor search
                return View(result);
            }
            catch (Exception ex)
            {
                Log.Write("BankSwitchController:Index", $"eRR: {ex.Message}");             
                return View("Error");

            }
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            try
            {
                var db = new MomoSwitchDbContext();
                var switches = switchManager.Get();

                ViewBag.switches = new SelectList(switches.SwitchDetails.Select(x => x.Processor).ToList());

                return View();
            }
            catch (Exception ex)
            {
                Log.Write("BankSwitchController:Create", $"eRR: {ex.Message}");
                return View("Error");
            }
        }
       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateBankSwitchRuleViewModel model)
        {
            try
            {
                var db = new MomoSwitchDbContext();
                var switches = switchManager.Get();

                ViewBag.switches = new SelectList(switches.SwitchDetails.Select(x => x.Processor).ToList());

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                Log.Write($"BankSwitchController:Create", $"Request: {model.Processor}");             
                var loggedInUser = HttpContext.GetLoggedInUser();
                var loggedInUserInDatabase = await db.PortalUserTb.SingleOrDefaultAsync(x => x.Username.ToLower() == loggedInUser.ToLower());

                if (loggedInUserInDatabase == null)
                {
                    Log.Write("BankSwitchController:Index", $"eRR: Logged in user not gotten");
                    return RedirectToAction("Logout", "Account");
                }

                if (!loggedInUserInDatabase.IsActive)
                {
                    //should they be logged out?
                    Log.Write("BankSwitchController:Index", $"eRR: User with username: {loggedInUserInDatabase.Username} is deactivated");
                    return RedirectToAction("Logout", "Account");
                }

                var request = new BankSwitchDetails
                {
                    BankCode = model.BankCode,
                    Processor = model.Processor
                };
                var result = await bankSwitchManager.Create(request);

                if (result.ResponseCode != "00")
                {
                    ModelState.AddModelError("", result.ResponseMessage);
                    return View(model);
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Log.Write("BankSwitchController:Create", $"eRR: {ex.Message}");
                ModelState.AddModelError("", "Something went wrong. Please try again later");
                return View();
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit([FromRoute] int id)
        {
            try
            {
                var db = new MomoSwitchDbContext();
                var switches = switchManager.Get();

                ViewBag.switches = new SelectList(switches.SwitchDetails.Select(x => x.Processor).ToList());

                var loggedInUser = HttpContext.GetLoggedInUser();
                var loggedInUserInDatabase = await db.PortalUserTb.SingleOrDefaultAsync(x => x.Username.ToLower() == loggedInUser.ToLower());
                var existingBankSwitch = await db.BankSwitchTb.SingleOrDefaultAsync(x => x.Id == id);

                if (loggedInUserInDatabase == null)
                {
                    Log.Write("BankSwitchController:Edit", $"eRR: Logged in user not gotten");
                    return RedirectToAction("Logout", "Account");
                }

                if (!loggedInUserInDatabase.IsActive)
                {
                    //should they be logged out?
                    Log.Write("BankSwitchController:Edit", $"eRR: User with username: {loggedInUserInDatabase.Username} is deactivated");
                    return RedirectToAction("Logout", "Account");
                }

                if (existingBankSwitch == null)
                {
                    Log.Write("BankSwitchController:Edit", $"eRR: Bank Switch rule doesn't exist");
                    //return View("NotFound");
                    return RedirectToAction("Index");
                }

                var viewModel = new CreateBankSwitchRuleViewModel
                {
                    Id = existingBankSwitch.Id,                   
                    Processor = existingBankSwitch.Processor,
                    BankCode = existingBankSwitch.BankCode
                };


                return View(viewModel);
            }
            catch (Exception ex)
            {
                Log.Write("BankSwitchController:Edit", $"eRR: {ex.Message}");
                return View("Error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CreateBankSwitchRuleViewModel model)
        {
            try
            {
                var db = new MomoSwitchDbContext();
                var switches = switchManager.Get();

                ViewBag.switches = new SelectList(switches.SwitchDetails.Select(x => x.Processor).ToList());

                if (!ModelState.IsValid)
                {
                    return View(model);
                }
                
                var loggedInUser = HttpContext.GetLoggedInUser();
                var loggedInUserInDatabase = await db.PortalUserTb.SingleOrDefaultAsync(x => x.Username.ToLower() == loggedInUser.ToLower());
                var existingBankSwitch = await db.BankSwitchTb.SingleOrDefaultAsync(x => x.Id == model.Id);

                if (loggedInUserInDatabase == null)
                {
                    Log.Write("BankSwitchController:Edit", $"eRR: Logged in user not gotten");
                    return RedirectToAction("Logout", "Account");
                }

                if (!loggedInUserInDatabase.IsActive)
                {
                    //should they be logged out?
                    Log.Write("BankSwitchController:Edit", $"eRR: User with username: {loggedInUserInDatabase.Username} is deactivated");
                    return RedirectToAction("Logout", "Account");
                }

                if (existingBankSwitch == null)
                {
                    Log.Write("BankSwitchController:EditUser", $"eRR: Bank Switch rule doesn't exist");
                    //return View("NotFound");
                    return RedirectToAction("Index"); //or error
                }

                var bankSwitchDetails = new BankSwitchDetails
                {
                    Id = model.Id,                    
                    Processor = model.Processor,
                    BankCode = model.BankCode
                };

                var result = await bankSwitchManager.Edit(bankSwitchDetails);

                if (result.ResponseCode != "00")
                {
                    ModelState.AddModelError("", result.ResponseMessage);
                    return View(model);
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Log.Write("BankSwitchController:Edit", $"eRR: {ex.Message}");
                return View("Error");
            }
        }


        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var db = new MomoSwitchDbContext();
                var loggedInUser = HttpContext.GetLoggedInUser();
                var loggedInUserInDatabase = await db.PortalUserTb.SingleOrDefaultAsync(x => x.Username.ToLower() == loggedInUser.ToLower());
                var existingBankSwitch = await db.BankSwitchTb.SingleOrDefaultAsync(x => x.Id == id);

                if (loggedInUserInDatabase == null)
                {
                    Log.Write("BankSwitchController:Delete", $"eRR: Logged in user not gotten");
                    return RedirectToAction("Logout", "Account");
                }

                if (!loggedInUserInDatabase.IsActive)
                {
                    //should they be logged out?
                    Log.Write("BankSwitchController:Delete", $"eRR: User with username: {loggedInUserInDatabase.Username} is deactivated");
                    return RedirectToAction("Logout", "Account");
                }
                    
                if (existingBankSwitch == null)
                {
                    Log.Write("BankSwitchController:Delete", $"eRR: Bank Switch rule doesn't exist");
                    //return View("NotFound");
                    return RedirectToAction("Index"); //or error
                }

                var result = await bankSwitchManager.Delete(id);

                if (result.ResponseCode != "00")
                {
                    return RedirectToAction("Index");
                }

                return RedirectToAction("Index");

            }
            catch (Exception ex)
            {
                Log.Write("BankSwitchController:Delete", $"eRR: {ex.Message}");
                return RedirectToAction("Index");
            }
        }
    }


}
