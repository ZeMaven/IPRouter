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
    public class AmountRuleController : Controller
    {
        private ILog Log;
        private readonly IAmountRule amountManager;
        public AmountRuleController(ILog log, IAmountRule amountManager)
        {
            Log = log;
            this.amountManager = amountManager;
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
                    return View("Error");
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
                    return View("Error");

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
            ViewBag.status = new SelectList(new[] { true, false });

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateAmountRuleViewModel model)
        {
            try
            {
                ViewBag.status = new SelectList(new[] { true, false });
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
                    Log.Write("AmountRuleController:Index", $"eRR: Logged in user not gotten");
                    return RedirectToAction("Logout", "Account");
                }

                if (!loggedInUserInDatabase.IsActive)
                {
                    Log.Write("AmountRuleController:Index", $"eRR: User with username: {loggedInUserInDatabase.Username} is deactivated");
                    return RedirectToAction("Logout", "Account");
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
                ViewBag.status = new SelectList(new[] { true, false });

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
                ViewBag.status = new SelectList(new[] { true, false });
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
                bool overlapsExisting = await db.AmountRuleTb.AnyAsync(ad => (model.AmountA <= ad.AmountZ && model.AmountA >= ad.AmountA) || (model.AmountZ >= ad.AmountA && model.AmountZ <= ad.AmountZ));

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
                    return RedirectToAction("Index");
                }

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
