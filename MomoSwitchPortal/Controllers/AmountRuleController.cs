using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Momo.Common.Actions;
using MomoSwitchPortal.Actions;
using MomoSwitchPortal.Actions.Rules;
using MomoSwitchPortal.Models.Database;
using MomoSwitchPortal.Models.ViewModels.Rules.Amount;

namespace MomoSwitchPortal.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class AmountRuleController : Controller
    {
        private ILog Log;
        private readonly IAmountRule amountManager;
        private readonly INotyfService ToastNotification;
        private readonly ISwitch switchManager;

        public AmountRuleController(ILog log, IAmountRule amountManager, INotyfService toastNotification, ISwitch switchManager)
        {
            Log = log;
            this.amountManager = amountManager;
            ToastNotification = toastNotification;
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
                    Log.Write("AmountRuleController:Index", $"eRR: Logged in user not gotten");
                    return RedirectToAction("Logout", "Account");
                }

                if (!loggedInUserInDatabase.IsActive)
                {
                    //should they be logged out?
                    Log.Write("AmountRuleController:Index", $"eRR: User with username: {loggedInUserInDatabase.Username} is deactivated");
                    return RedirectToAction("Logout", "Account");
                }
                var result = amountManager.Get();

                if (result.ResponseHeader.ResponseCode != "00")
                {
                    ToastNotification.Error("System Challenge");
                    return RedirectToAction("Index", "Home");
                }

                return View(result);
            }
            catch (Exception ex)
            {
                Log.Write("AmountRuleController:Index", $"eRR: {ex.Message}");
                ModelState.AddModelError("", "Something went wrong. Please try again later");
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(string processor)
        {
            try
            {
                var db = new MomoSwitchDbContext();
                var loggedInUser = HttpContext.GetLoggedInUser();

                var loggedInUserInDatabase = await db.PortalUserTb.SingleOrDefaultAsync(x => x.Username.ToLower() == loggedInUser.ToLower());

                if (loggedInUserInDatabase == null)
                {
                    Log.Write("AmountRuleController:Index", $"eRR: Logged in user not gotten");
                    return RedirectToAction("Logout", "Account");
                }

                if (!loggedInUserInDatabase.IsActive)
                {
                    //should they be logged out?
                    Log.Write("AmountRuleController:Index", $"eRR: User with username: {loggedInUserInDatabase.Username} is deactivated");
                    return RedirectToAction("Logout", "Account");
                }

                var result = amountManager.Get();

                if (result.ResponseHeader.ResponseCode != "00")
                {
                    ToastNotification.Error("System Challenge");
                    return RedirectToAction("Index", "Home");
                }

                if (!string.IsNullOrWhiteSpace(processor))
                {
                    var filteredList = result.AmountDetails.Where(x => x.Processor.Contains(processor, StringComparison.CurrentCultureIgnoreCase)).ToList();
                    result.AmountDetails = filteredList;
                }

                result.Processor = processor;//just to initialize the rule search
                return View(result);
            }
            catch (Exception ex)
            {
                Log.Write("AmountRuleController:Index", $"eRR: {ex.Message}");
                ModelState.AddModelError("", "Something went wrong. Please try again later");
                return View("Error");

            }
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var switches = switchManager.Get();

            ViewBag.status = new SelectList(new[] { true, false });
            ViewBag.switches = new SelectList(switches.SwitchDetails.Select(x => x.Processor).ToList());

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateAmountRuleViewModel model)
        {
            try
            {
                var switches = switchManager.Get();

                ViewBag.status = new SelectList(new[] { true, false });
                ViewBag.switches = new SelectList(switches.SwitchDetails.Select(x => x.Processor).ToList());

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                Log.Write($"AmountRuleController:Create", $"Request: {model.Processor}");
                var db = new MomoSwitchDbContext();
                var loggedInUser = HttpContext.GetLoggedInUser();
                var loggedInUserInDatabase = await db.PortalUserTb.SingleOrDefaultAsync(x => x.Username.ToLower() == loggedInUser.ToLower());

                if (loggedInUserInDatabase == null)
                {
                    Log.Write("AmountRuleController:Create", $"eRR: Logged in user not gotten");
                    return RedirectToAction("Logout", "Account");
                }

                if (!loggedInUserInDatabase.IsActive)
                {
                    Log.Write("AmountRuleController:Create", $"eRR: User with username: {loggedInUserInDatabase.Username} is deactivated");
                    return RedirectToAction("Logout", "Account");
                }

                var switchExists = await db.SwitchTb.SingleOrDefaultAsync(x => x.Processor.ToLower() == model.Processor.ToLower());

                if (switchExists == null)
                {
                    Log.Write("AmountRuleController.Create", $"Switch named {model.Processor} doesn't exist");
                    ModelState.AddModelError("", "System Challenge");
                    return View(model);
                }

                bool overlapsExisting = await db.AmountRuleTb.AnyAsync(ad => (model.AmountA <= ad.AmountZ && model.AmountA >= ad.AmountA) || (model.AmountZ >= ad.AmountA && model.AmountZ <= ad.AmountZ));

                if (!overlapsExisting)
                {
                    var request = new AmountDetails
                    {
                        Processor = model.Processor,
                        AmountA = model.AmountA,
                        AmountZ = model.AmountZ,
                    };
                    db.AmountRuleTb.Add(request);
                    await db.SaveChangesAsync();
                }
                else
                {
                    ModelState.AddModelError("", "The new amount range overlaps with existing ranges.");
                    return View(model);
                }

                ToastNotification.Success("Amount rule created successfully");
                return RedirectToAction("Index");

            }
            catch (Exception ex)
            {
                Log.Write("AmountRuleController:Create", $"eRR: {ex.Message}");
                ModelState.AddModelError("", "Something went wrong. Please try again later");
                return View();
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit([FromRoute] int id)
        {
            try
            {
                var switches = switchManager.Get();

                ViewBag.status = new SelectList(new[] { true, false });
                ViewBag.switches = new SelectList(switches.SwitchDetails.Select(x => x.Processor).ToList());

                var db = new MomoSwitchDbContext();
                var loggedInUser = HttpContext.GetLoggedInUser();
                var loggedInUserInDatabase = await db.PortalUserTb.SingleOrDefaultAsync(x => x.Username.ToLower() == loggedInUser.ToLower());
                var existingUser = await db.AmountRuleTb.SingleOrDefaultAsync(x => x.Id == id);

                if (loggedInUserInDatabase == null)
                {
                    Log.Write("AmountRuleController:Edit", $"eRR: Logged in user not gotten");
                    return RedirectToAction("Logout", "Account");
                }

                if (!loggedInUserInDatabase.IsActive)
                {
                    //should they be logged out?
                    Log.Write("AmountRuleController:Edit", $"eRR: User with username: {loggedInUserInDatabase.Username} is deactivated");
                    return RedirectToAction("Logout", "Account");
                }

                if (existingUser == null)
                {
                    Log.Write("AmountRuleController:Edit", $"eRR: Amount rule doesn't exist");
                    //return View("NotFound");
                    return RedirectToAction("Index");
                }

                var viewModel = new CreateAmountRuleViewModel
                {
                    Id = existingUser.Id,
                    AmountA = existingUser.AmountA,
                    AmountZ = existingUser.AmountZ,
                    Processor = existingUser.Processor
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                Log.Write("AmountRuleController:Edit", $"eRR: {ex.Message}");
                return View("Error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CreateAmountRuleViewModel model)
        {
            try
            {
                var switches = switchManager.Get();

                ViewBag.status = new SelectList(new[] { true, false });
                ViewBag.switches = new SelectList(switches.SwitchDetails.Select(x => x.Processor).ToList());

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var db = new MomoSwitchDbContext();
                var loggedInUser = HttpContext.GetLoggedInUser();
                var loggedInUserInDatabase = await db.PortalUserTb.SingleOrDefaultAsync(x => x.Username.ToLower() == loggedInUser.ToLower());
                var existingUser = await db.AmountRuleTb.SingleOrDefaultAsync(x => x.Id == model.Id);
                if (loggedInUserInDatabase == null)
                {
                    Log.Write("AmountRuleController:Edit", $"eRR: Logged in user not gotten");
                    return RedirectToAction("Logout", "Account");
                }

                if (!loggedInUserInDatabase.IsActive)
                {
                    //should they be logged out?
                    Log.Write("AmountRuleController:Edit", $"eRR: User with username: {loggedInUserInDatabase.Username} is deactivated");
                    return RedirectToAction("Logout", "Account");
                }

                var switchExists = await db.SwitchTb.SingleOrDefaultAsync(x => x.Processor.ToLower() == model.Processor.ToLower());

                if (switchExists == null)
                {
                    Log.Write("AmountRuleController.Edit", $"Switch named {model.Processor} doesn't exist");
                    ModelState.AddModelError("", "System Challenge");
                    return View(model);
                }


                bool overlapsExisting = await db.AmountRuleTb.AnyAsync(ad => (model.AmountA <= ad.AmountZ && model.AmountA >= ad.AmountA && ad.Id != model.Id) || (model.AmountZ >= ad.AmountA && model.AmountZ <= ad.AmountZ && ad.Id != model.Id));

                if (!overlapsExisting)
                {
                    var amountDetails = new AmountDetails
                    {
                        Id = model.Id,
                        Processor = model.Processor,
                        AmountA = model.AmountA,
                        AmountZ = model.AmountZ,
                    };
                    var result = await amountManager.Edit(amountDetails);
                    if (result.ResponseCode != "00")
                    {
                        ModelState.AddModelError("", result.ResponseMessage);
                        return View(model);
                    }
                }
                else
                {
                    ModelState.AddModelError("", "The Edited amount range overlaps with existing ranges.");
                    return View(model);
                }

                ToastNotification.Success("Amount rule edited successfully");
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Log.Write("AmountRuleController:Edit", $"eRR: {ex.Message}");
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
                var existingAmount = await db.AmountRuleTb.SingleOrDefaultAsync(x => x.Id == id);

                if (loggedInUserInDatabase == null)
                {
                    Log.Write("AmountRuleController:Delete", $"eRR: Logged in user not gotten");
                    return RedirectToAction("Logout", "Account");
                }

                if (!loggedInUserInDatabase.IsActive)
                {
                    //should they be logged out?
                    Log.Write("AmountRuleController:Delete", $"eRR: User with username: {loggedInUserInDatabase.Username} is deactivated");
                    return RedirectToAction("Logout", "Account");
                }

                if (existingAmount == null)
                {
                    Log.Write("UserController:Delete", $"eRR: Amount doesn't exist");
                    //return View("NotFound");
                    return RedirectToAction("Index"); //or error
                }

                var result = await amountManager.Delete(id);

                if (result.ResponseCode != "00")
                {
                    ToastNotification.Error("System Challenge");
                    return RedirectToAction("Index");
                }

                ToastNotification.Success("Amount rule deleted successfully");
                return RedirectToAction("Index");

            }
            catch (Exception ex)
            {
                Log.Write("AmountRuleController:Delete", $"eRR: {ex.Message}");
                return RedirectToAction("Index");
            }
        }
    }
}
