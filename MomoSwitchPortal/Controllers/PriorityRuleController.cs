using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Momo.Common.Actions;
using MomoSwitchPortal.Actions;
using MomoSwitchPortal.Actions.Rules;
using MomoSwitchPortal.Models.Database;
using MomoSwitchPortal.Models.ViewModels.Rules.Priority;
using Newtonsoft.Json.Linq;


namespace MomoSwitchPortal.Controllers
{
    public class PriorityRuleController : Controller
    {
        private ILog Log;
        private readonly IPriority priorityManager;
        private readonly INotyfService ToastNotification;

        public PriorityRuleController(ILog log, IPriority priorityManager, INotyfService toastNotification)
        {
            Log = log;
            this.priorityManager = priorityManager;
            ToastNotification = toastNotification;
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
                    Log.Write("PriorityRuleController:Index", $"eRR: Logged in user not gotten");
                    return RedirectToAction("Logout", "Account");
                }

                if (!loggedInUserInDatabase.IsActive)
                {
                    //should they be logged out?
                    Log.Write("PriorityRuleController:Index", $"eRR: User with username: {loggedInUserInDatabase.Username} is deactivated");
                    return RedirectToAction("Logout", "Account");
                }
                var result = priorityManager.Get();

                if (result.ResponseHeader.ResponseCode != "00")
                {
                    ToastNotification.Error("System Challenge");
                    return RedirectToAction("Index", "Home");
                }

                return View(result);
            }
            catch (Exception ex)
            {
                Log.Write("PriorityRuleController:Index", $"eRR: {ex.Message}");
                ModelState.AddModelError("", "Something went wrong. Please try again later");
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(string rule)
        {
            try
            {
                var db = new MomoSwitchDbContext();
                var loggedInUser = HttpContext.GetLoggedInUser();

                var loggedInUserInDatabase = await db.PortalUserTb.SingleOrDefaultAsync(x => x.Username.ToLower() == loggedInUser.ToLower());

                if (loggedInUserInDatabase == null)
                {
                    Log.Write("PriorityRuleController:Index", $"eRR: Logged in user not gotten");
                    return RedirectToAction("Logout", "Account");
                }

                if (!loggedInUserInDatabase.IsActive)
                {
                    //should they be logged out?
                    Log.Write("PriorityRuleController:Index", $"eRR: User with username: {loggedInUserInDatabase.Username} is deactivated");
                    return RedirectToAction("Logout", "Account");
                }

                var result = priorityManager.Get();

                if (result.ResponseHeader.ResponseCode != "00")
                {
                    ToastNotification.Error("System Challenge");
                    return RedirectToAction("Index", "Home");
                }

                if (!string.IsNullOrWhiteSpace(rule))
                {
                    var filteredList = result.PriorityDetails.Where(x => x.Rule.Contains(rule, StringComparison.CurrentCultureIgnoreCase)).ToList();
                    result.PriorityDetails = filteredList;
                }

                result.Rule = rule;//just to initialize the rule search
                return View(result);
            }
            catch (Exception ex)
            {
                Log.Write("PiorityRuleController:Index", $"eRR: {ex.Message}");
                ModelState.AddModelError("", "Something went wrong. Please try again later");
                return View("Error");

            }
        }

        //[HttpGet]
        //public async Task<IActionResult> Create()
        //{
        //    ViewBag.status = new SelectList(new[] { true, false });

        //    return View();
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create(CreatePriorityRuleViewModel model)
        //{
        //    try
        //    {
        //        ViewBag.status = new SelectList(new[] { true, false });
        //        if (!ModelState.IsValid)
        //        {
        //            return View(model);
        //        }

        //        Log.Write($"PriorityRuleController:Create", $"Request: {model.Rule}");
        //        var db = new MomoSwitchDbContext();
        //        var loggedInUser = HttpContext.GetLoggedInUser();
        //        var loggedInUserInDatabase = await db.PortalUserTb.SingleOrDefaultAsync(x => x.Username.ToLower() == loggedInUser.ToLower());

        //        if (loggedInUserInDatabase == null)
        //        {
        //            Log.Write("PriorityRuleController:Index", $"eRR: Logged in user not gotten");
        //            return RedirectToAction("Logout", "Account");
        //        }

        //        if (!loggedInUserInDatabase.IsActive)
        //        {    
        //            Log.Write("PriorityRuleController:Index", $"eRR: User with username: {loggedInUserInDatabase.Username} is deactivated");
        //            return RedirectToAction("Logout", "Account");
        //        }

        //        var existingPriority = db.PriorityTb.FirstOrDefault(x => x.Priority == model.Priority || x.Rule == model.Rule);

        //        if (existingPriority != null)
        //        {
        //            ModelState.AddModelError("", "Priority or Rule already exists.");
        //            return View(model);
        //        }
        //        else
        //        {
        //            var request = new PriorityDetails
        //            {
        //                Priority = model.Priority,
        //                Rule = model.Rule
        //            };

        //            var result = await priorityManager.Create(request);

        //            if (result.ResponseCode != "00")
        //            {
        //                ModelState.AddModelError("", result.ResponseMessage);
        //                return View(model);
        //            }
                    
        //            return RedirectToAction("Index");
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Write("PiorityRuleController:Create", $"eRR: {ex.Message}");
        //        ModelState.AddModelError("", "Something went wrong. Please try again later");
        //        return View();
        //    }
        //}

        [HttpGet]
        public async Task<IActionResult> Edit([FromRoute] int id)
        {
            try
            {
                ViewBag.status = new SelectList(new[] { true, false });
                ViewBag.priorities = new SelectList(new string[] {"1", "2", "3"});


                var db = new MomoSwitchDbContext();
                var loggedInUser = HttpContext.GetLoggedInUser();
                var loggedInUserInDatabase = await db.PortalUserTb.SingleOrDefaultAsync(x => x.Username.ToLower() == loggedInUser.ToLower());
                var existingUser = await db.PriorityTb.SingleOrDefaultAsync(x => x.Id == id);

                if (loggedInUserInDatabase == null)
                {
                    Log.Write("PriorityRuleController:Edit", $"eRR: Logged in user not gotten");
                    return RedirectToAction("Logout", "Account");
                }

                if (!loggedInUserInDatabase.IsActive)
                {
                    //should they be logged out?
                    Log.Write("PriorityRuleController:Edit", $"eRR: User with username: {loggedInUserInDatabase.Username} is deactivated");
                    return RedirectToAction("Logout", "Account");
                }

                if (existingUser == null)
                {
                    Log.Write("PriorityRuleController:Edit", $"eRR: Priority rule doesn't exist");
                    //return View("NotFound");
                    return RedirectToAction("Index");
                }

                var viewModel = new CreatePriorityRuleViewModel
                {
                    Id = existingUser.Id,
                    Priority = existingUser.Priority,
                    Rule = existingUser.Rule,
                };


                return View(viewModel);
            }
            catch (Exception ex)
            {
                Log.Write("PriorityRuleController:Edit", $"eRR: {ex.Message}");
                return View("Error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CreatePriorityRuleViewModel model)
        {
            try
            {

                ViewBag.priorities = new SelectList(new string[] {"1", "2", "3"});
                ViewBag.status = new SelectList(new[] { true, false });
                
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var priorities = (priorityManager.Get()).PriorityDetails.Count;

                var db = new MomoSwitchDbContext();
                var loggedInUser = HttpContext.GetLoggedInUser();
                var loggedInUserInDatabase = await db.PortalUserTb.SingleOrDefaultAsync(x => x.Username.ToLower() == loggedInUser.ToLower());
                var priority = await db.PriorityTb.SingleOrDefaultAsync(x => x.Id == model.Id);
                if (loggedInUserInDatabase == null)
                {
                    Log.Write("PriorityRuleController:Edit", $"eRR: Logged in user not gotten");
                    return RedirectToAction("Logout", "Account");
                }

                if (!loggedInUserInDatabase.IsActive)
                {
                    //should they be logged out?
                    Log.Write("PriorityRuleController:Edit", $"eRR: User with username: {loggedInUserInDatabase.Username} is deactivated");
                    return RedirectToAction("Logout", "Account");
                }

                if (priority == null)
                {
                    Log.Write("UserController:EditUser", $"eRR: Priority doesn't exist");
                    //return View("NotFound");
                    ToastNotification.Error("System Challenge");
                    return RedirectToAction("Index"); //or error
                }

                if(model.Priority > priorities || model.Priority < 1)
                {
                    Log.Write("UserController:EditUser", $"eRR: Priority is invalid");
                    //return View("NotFound");
                    ToastNotification.Error("System Challenge");
                    return RedirectToAction("Index"); //or error
                }
                var existingPriority = await db.PriorityTb.SingleOrDefaultAsync( x => x.Priority == model.Priority);
                if (existingPriority.Id != model.Id)
                {
                    existingPriority.Priority = priority.Priority;
                    priority.Priority = model.Priority;

                    db.SaveChanges();
                }
                else
                {
                    var priorityDetails = new PriorityDetails
                    {
                        Id = model.Id,
                        Priority = model.Priority,
                        Rule = model.Rule
                    };
                    var result = await priorityManager.Edit(priorityDetails);
                    if (result.ResponseCode != "00")
                    {
                        ModelState.AddModelError("", result.ResponseMessage);
                        return View(model);
                    }
                }

                ToastNotification.Success("Priority Modified Successfully");
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Log.Write("PriorityRuleController:Edit", $"eRR: {ex.Message}");
                return View("Error");
            }
        }

        //[HttpGet]
        //public async Task<IActionResult> Delete(int id)
        //{
        //    try
        //    {
        //        var db = new MomoSwitchDbContext();
        //        var loggedInUser = HttpContext.GetLoggedInUser();
        //        var loggedInUserInDatabase = await db.PortalUserTb.SingleOrDefaultAsync(x => x.Username.ToLower() == loggedInUser.ToLower());
        //        var existingPriority = await db.PriorityTb.SingleOrDefaultAsync(x => x.Id == id);

        //        if (loggedInUserInDatabase == null)
        //        {
        //            Log.Write("PriorityRuleController:Delete", $"eRR: Logged in user not gotten");
        //            return RedirectToAction("Logout", "Account");
        //        }

        //        if (!loggedInUserInDatabase.IsActive)
        //        {
        //            //should they be logged out?
        //            Log.Write("PriorityRuleController:Delete", $"eRR: User with username: {loggedInUserInDatabase.Username} is deactivated");
        //            return RedirectToAction("Logout", "Account");
        //        }

        //        if (existingPriority == null)
        //        {
        //            Log.Write("UserController:Delete", $"eRR: Priority doesn't exist");
        //            //return View("NotFound");
        //            return RedirectToAction("Index"); //or error
        //        }

        //        var result = await priorityManager.Delete(id);

        //        if (result.ResponseCode != "00")
        //        {
        //            return RedirectToAction("Index");
        //        }

        //        return RedirectToAction("Index");

        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Write("PriorityRuleController:Delete", $"eRR: {ex.Message}");
        //        return RedirectToAction("Index");
        //    }
        //}
    }
}
