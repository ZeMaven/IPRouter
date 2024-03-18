using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Momo.Common.Actions;
using MomoSwitchPortal.Actions;
using MomoSwitchPortal.Actions.Rules;
using MomoSwitchPortal.Models.Database;
using MomoSwitchPortal.Models.ViewModels.Rules.Time;

namespace MomoSwitchPortal.Controllers
{
    public class TimeRuleController : Controller
    {
        private ILog Log;
        private readonly ITimeRule timeManager;
        private readonly INotyfService ToastNotification;
        private readonly ISwitch switchManager;


        public TimeRuleController(ILog log, ITimeRule timeManager, INotyfService toastNotification, ISwitch switchManager)
        {
            Log = log;
            this.timeManager = timeManager;
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
                    Log.Write("TimeRuleController:Index", $"eRR: Logged in user not gotten");
                    return RedirectToAction("Logout", "Account");
                }

                if (!loggedInUserInDatabase.IsActive)
                {
                    //should they be logged out?
                    Log.Write("TimeRuleController:Index", $"eRR: User with username: {loggedInUserInDatabase.Username} is deactivated");
                    return RedirectToAction("Logout", "Account");
                }
                var result = timeManager.Get();

                if (result.ResponseHeader.ResponseCode != "00")
                {
                    ToastNotification.Error("System Challenge");
                    return RedirectToAction("Index", "Home");
                }

                return View(result);
            }
            catch (Exception ex)
            {
                Log.Write("TimeRuleController:Index", $"eRR: {ex.Message}");
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
                    Log.Write("TimeRuleController:Index", $"eRR: Logged in user not gotten");
                    return RedirectToAction("Logout", "Account");
                }

                if (!loggedInUserInDatabase.IsActive)
                {
                    //should they be logged out?
                    Log.Write("TimeRuleController:Index", $"eRR: User with username: {loggedInUserInDatabase.Username} is deactivated");
                    return RedirectToAction("Logout", "Account");
                }

                var result = timeManager.Get();

                if (result.ResponseHeader.ResponseCode != "00")
                {
                    ToastNotification.Error("System Challenge");
                    return RedirectToAction("Index", "Home");
                }

                if (!string.IsNullOrWhiteSpace(processor))
                {
                    var filteredList = result.TimeDetails.Where(x => x.Processor.Contains(processor, StringComparison.CurrentCultureIgnoreCase)).ToList();
                    result.TimeDetails = filteredList;
                }

                result.Processor = processor;//just to initialize the rule search
                return View(result);
            }
            catch (Exception ex)
            {
                Log.Write("TimeRuleController:Index", $"eRR: {ex.Message}");
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
        public async Task<IActionResult> Create(CreateTimeRuleViewModel model)
        {
            try
            {
                var switches = switchManager.Get();

                ViewBag.switches = new SelectList(switches.SwitchDetails.Select(x => x.Processor).ToList());
                ViewBag.status = new SelectList(new[] { true, false });              

                if (!ModelState.IsValid)
                {
                    return View(model);
                }
                
                Log.Write($"TimeRuleController:Create", $"Request: {model.Processor}");
                var db = new MomoSwitchDbContext();
                var loggedInUser = HttpContext.GetLoggedInUser();
                var loggedInUserInDatabase = await db.PortalUserTb.SingleOrDefaultAsync(x => x.Username.ToLower() == loggedInUser.ToLower());

                if (loggedInUserInDatabase == null)
                {
                    Log.Write("TimeRuleController:Create", $"eRR: Logged in user not gotten");
                    return RedirectToAction("Logout", "Account");
                }

                if (!loggedInUserInDatabase.IsActive)
                {
                    Log.Write("TimeRuleController:Create", $"eRR: User with username: {loggedInUserInDatabase.Username} is deactivated");
                    return RedirectToAction("Logout", "Account");
                }
                
                var switchExists = await db.SwitchTb.SingleOrDefaultAsync(x => x.Processor.ToLower() == model.Processor.ToLower());

                if (switchExists == null)
                {
                    Log.Write("TimeRuleController.Create", $"Switch named {model.Processor} doesn't exist");
                    ModelState.AddModelError("", "System Challenge");
                    return View(model);
                }

                DateTime timeA = DateTime.Parse(model.TimeA);
                DateTime timeZ = DateTime.Parse(model.TimeZ);
                var existingRules = await db.TimeRuleTb.ToListAsync();
                bool overlapsExisting = existingRules.Any(ad =>
                    (timeA <= DateTime.Parse(ad.TimeZ) && timeA >= DateTime.Parse(ad.TimeA)) ||
                    (timeZ >= DateTime.Parse(ad.TimeA) && timeZ <= DateTime.Parse(ad.TimeZ)));

                if (!overlapsExisting)
                {
                    var request = new TimeDetails
                    {
                        Processor = model.Processor,
                        TimeA = model.TimeA,
                        TimeZ = model.TimeZ,
                    };
                    db.TimeRuleTb.Add(request);
                    await db.SaveChangesAsync();
                }
                else
                {
                    ModelState.AddModelError("", "The new Time range overlaps with existing ranges.");
                    return View(model);
                }

                ToastNotification.Success("Time rule created successfully");
                return RedirectToAction("Index");

            }
            catch (Exception ex)
            {
                Log.Write("TimeRuleController:Create", $"eRR: {ex.Message}");
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

                ViewBag.switches = new SelectList(switches.SwitchDetails.Select(x => x.Processor).ToList());
                ViewBag.status = new SelectList(new[] { true, false });

                var db = new MomoSwitchDbContext();
                var loggedInUser = HttpContext.GetLoggedInUser();
                var loggedInUserInDatabase = await db.PortalUserTb.SingleOrDefaultAsync(x => x.Username.ToLower() == loggedInUser.ToLower());
                var existingUser = await db.TimeRuleTb.SingleOrDefaultAsync(x => x.Id == id);

                if (loggedInUserInDatabase == null)
                {
                    Log.Write("TimeRuleController:Edit", $"eRR: Logged in user not gotten");
                    return RedirectToAction("Logout", "Account");
                }

                if (!loggedInUserInDatabase.IsActive)
                {
                    //should they be logged out?
                    Log.Write("TimeRuleController:Edit", $"eRR: User with username: {loggedInUserInDatabase.Username} is deactivated");
                    return RedirectToAction("Logout", "Account");
                }

                if (existingUser == null)
                {
                    Log.Write("TimeRuleController:Edit", $"eRR: Time rule doesn't exist");
                    //return View("NotFound");
                    return RedirectToAction("Index");
                }

                var viewModel = new CreateTimeRuleViewModel
                {
                    Id = existingUser.Id,
                    TimeA = existingUser.TimeA,
                    TimeZ = existingUser.TimeZ,
                    Processor = existingUser.Processor
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                Log.Write("TimeRuleController:Edit", $"eRR: {ex.Message}");
                return View("Error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CreateTimeRuleViewModel model)
        {
            try
            {
                var switches = switchManager.Get();

                ViewBag.switches = new SelectList(switches.SwitchDetails.Select(x => x.Processor).ToList());

                ViewBag.status = new SelectList(new[] { true, false });
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var db = new MomoSwitchDbContext();
                var loggedInUser = HttpContext.GetLoggedInUser();
                var loggedInUserInDatabase = await db.PortalUserTb.SingleOrDefaultAsync(x => x.Username.ToLower() == loggedInUser.ToLower());
                var existingUser = await db.TimeRuleTb.SingleOrDefaultAsync(x => x.Id == model.Id);
                if (loggedInUserInDatabase == null)
                {
                    Log.Write("TimeRuleController:Edit", $"eRR: Logged in user not gotten");
                    return RedirectToAction("Logout", "Account");
                }

                if (!loggedInUserInDatabase.IsActive)
                {
                    //should they be logged out?
                    Log.Write("TimeRuleController:Edit", $"eRR: User with username: {loggedInUserInDatabase.Username} is deactivated");
                    return RedirectToAction("Logout", "Account");
                }

                var switchExists = await db.SwitchTb.SingleOrDefaultAsync(x => x.Processor.ToLower() == model.Processor.ToLower());

                if (switchExists == null)
                {
                    Log.Write("TimeRuleController.Edit", $"Switch named {model.Processor} doesn't exist");
                    ModelState.AddModelError("", "System Challenge");
                    return View(model);
                }

                DateTime timeA = DateTime.Parse(model.TimeA);
                DateTime timeZ = DateTime.Parse(model.TimeZ);
                var existingRules = await db.TimeRuleTb.ToListAsync();
                bool overlapsExisting = existingRules.Any(ad =>(timeA <= DateTime.Parse(ad.TimeZ) && timeA >= DateTime.Parse(ad.TimeA)) || (timeZ >= DateTime.Parse(ad.TimeA) && timeZ <= DateTime.Parse(ad.TimeZ)));

                if (!overlapsExisting)
                {
                    var timeDetails = new TimeDetails
                    {
                        Id = model.Id,
                        Processor = model.Processor,
                        TimeA = model.TimeA,
                        TimeZ = model.TimeZ,
                    };
                    var result = await timeManager.Edit(timeDetails);
                    if (result.ResponseCode != "00")
                    {
                        ModelState.AddModelError("", result.ResponseMessage);
                        return View(model);
                    }
                }
                else
                {
                    ModelState.AddModelError("", "The Edited Time range overlaps with existing ranges.");
                    return View(model);
                }

                ToastNotification.Success("Time rule modified successfully");
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Log.Write("TimeRuleController:Edit", $"eRR: {ex.Message}");
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
                var existingTime = await db.TimeRuleTb.SingleOrDefaultAsync(x => x.Id == id);

                if (loggedInUserInDatabase == null)
                {
                    Log.Write("TimeRuleController:Delete", $"eRR: Logged in user not gotten");
                    return RedirectToAction("Logout", "Account");
                }

                if (!loggedInUserInDatabase.IsActive)
                {
                    //should they be logged out?
                    Log.Write("TimeRuleController:Delete", $"eRR: User with username: {loggedInUserInDatabase.Username} is deactivated");
                    return RedirectToAction("Logout", "Account");
                }

                if (existingTime == null)
                {
                    Log.Write("UserController:Delete", $"eRR: Time doesn't exist");
                    //return View("NotFound");
                    return RedirectToAction("Index"); //or error
                }

                var result = await timeManager.Delete(id);

                if (result.ResponseCode != "00")
                {
                    ToastNotification.Error("System Challenge");
                    return RedirectToAction("Index");
                }

                ToastNotification.Success("Time rule deleted successfully");
                return RedirectToAction("Index");

            }
            catch (Exception ex)
            {
                Log.Write("TimeRuleController:Delete", $"eRR: {ex.Message}");
                return RedirectToAction("Index");
            }
        }
    }
}
