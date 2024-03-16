using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Momo.Common.Actions;
using MomoSwitchPortal.Actions;
using MomoSwitchPortal.Actions.Rules;
using MomoSwitchPortal.Models.Database;
using MomoSwitchPortal.Models.ViewModels.Rules.Switch;
using MomoSwitchPortal.Models.ViewModels.User;

namespace MomoSwitchPortal.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class SwitchRuleController : Controller
    {
        private ILog Log;
        private readonly ISwitch switchManager;
        private readonly INotyfService ToastNotification;

        public SwitchRuleController(ILog log, ISwitch switchManager, INotyfService toastNotification)
        {
            Log = log;
            this.switchManager = switchManager;
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
                    Log.Write("SwitchRuleController:Index", $"eRR: Logged in user not gotten");
                    return RedirectToAction("Logout", "Account");
                }

                if (!loggedInUserInDatabase.IsActive)
                {
                    //should they be logged out?
                    Log.Write("SwitchRuleController:Index", $"eRR: User with username: {loggedInUserInDatabase.Username} is deactivated");
                    return RedirectToAction("Logout", "Account");
                }
                var result = switchManager.Get();

                if (result.ResponseHeader.ResponseCode != "00")
                {
                    return View("Error");
                }


                return View(result);
            }
            catch (Exception ex)
            {
                Log.Write("SwitchRuleController:Index", $"eRR: {ex.Message}");
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
                    Log.Write("SwitchRuleController:Index", $"eRR: Logged in user not gotten");
                    return RedirectToAction("Logout", "Account");
                }

                if (!loggedInUserInDatabase.IsActive)
                {
                    //should they be logged out?
                    Log.Write("SwitchRuleController:Index", $"eRR: User with username: {loggedInUserInDatabase.Username} is deactivated");
                    return RedirectToAction("Logout", "Account");
                }
            
                var result = switchManager.Get();

                if (result.ResponseHeader.ResponseCode != "00")
                    return View("Error");

                if (!string.IsNullOrWhiteSpace(processor))
                {
                    var filteredList = result.SwitchDetails.Where(x => x.Processor.Contains(processor, StringComparison.CurrentCultureIgnoreCase)).ToList();
                    result.SwitchDetails = filteredList;
                }
               
                result.Processor = processor;//just to initialize the processor search
                return View(result);
            }
            catch (Exception ex)
            {
                Log.Write("SwitchRuleController:Index", $"eRR: {ex.Message}");
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
        public async Task<IActionResult> Create(CreateSwitchRuleViewModel model)
        {
            try
            {
                ViewBag.status = new SelectList(new[] { true, false });
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                Log.Write($"SwitchRuleController:Create", $"Request: {model.Processor}");
                var db = new MomoSwitchDbContext();
                var loggedInUser = HttpContext.GetLoggedInUser();
                var loggedInUserInDatabase = await db.PortalUserTb.SingleOrDefaultAsync(x => x.Username.ToLower() == loggedInUser.ToLower());

                if (loggedInUserInDatabase == null)
                {
                    Log.Write("SwitchRuleController:Index", $"eRR: Logged in user not gotten");
                    return RedirectToAction("Logout", "Account");
                }

                if (!loggedInUserInDatabase.IsActive)
                {
                    //should they be logged out?
                    Log.Write("SwitchRuleController:Index", $"eRR: User with username: {loggedInUserInDatabase.Username} is deactivated");
                    return RedirectToAction("Logout", "Account");
                }

                var request = new SwitchDetails
                {
                    IsActive = model.IsActive,
                    Processor = model.Processor,
                    IsDefault = false,
                    NameEnquiryUrl = model.NameEnquiryUrl,
                    TranQueryUrl = model.TranQueryUrl,
                    TransferUrl = model.TransferUrl
                };
                var result = await switchManager.Create(request);

                if (result.ResponseCode != "00")
                {
                    ModelState.AddModelError("", result.ResponseMessage);
                    return View(model);
                }

                ToastNotification.Success("Switch created successfully");
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Log.Write("SwitchRuleController:Create", $"eRR: {ex.Message}");
                ModelState.AddModelError("", "Something went wrong. Please try again later");
                return View();
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit([FromRoute]int id)
        {
            try
            {
                ViewBag.status = new SelectList(new[] { true, false });

                var db = new MomoSwitchDbContext();
                var loggedInUser = HttpContext.GetLoggedInUser();
                var loggedInUserInDatabase = await db.PortalUserTb.SingleOrDefaultAsync(x => x.Username.ToLower() == loggedInUser.ToLower());
                var existingSwitch = await db.SwitchTb.SingleOrDefaultAsync(x => x.Id == id);

                if (loggedInUserInDatabase == null)
                {
                    Log.Write("SwitchRuleController:Edit", $"eRR: Logged in user not gotten");
                    return RedirectToAction("Logout", "Account");
                }

                if (!loggedInUserInDatabase.IsActive)
                {
                    //should they be logged out?
                    Log.Write("SwitchRuleController:Edit", $"eRR: User with username: {loggedInUserInDatabase.Username} is deactivated");
                    return RedirectToAction("Logout", "Account");
                }

                if (existingSwitch == null)
                {
                    Log.Write("SwitchRuleController:Edit", $"eRR: Switch rule doesn't exist");
                    //return View("NotFound");
                    return RedirectToAction("Index");
                }

                var viewModel = new CreateSwitchRuleViewModel
                {
                    Id = existingSwitch.Id,
                    IsActive = existingSwitch.IsActive,
                    Processor = existingSwitch.Processor,
                    IsDefault = existingSwitch.IsDefault,
                    NameEnquiryUrl = existingSwitch.NameEnquiryUrl,
                    TranQueryUrl = existingSwitch.TranQueryUrl,
                    TransferUrl = existingSwitch.TransferUrl
                };


                return View(viewModel);
            }
            catch (Exception ex)
            {
                Log.Write("SwitchRuleController:Edit", $"eRR: {ex.Message}");
                return View("Error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CreateSwitchRuleViewModel model)
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
                var existingSwitch = await db.SwitchTb.SingleOrDefaultAsync(x => x.Id == model.Id);

                if (loggedInUserInDatabase == null)
                {
                    Log.Write("SwitchRuleController:Edit", $"eRR: Logged in user not gotten");
                    return RedirectToAction("Logout", "Account");
                }

                if (!loggedInUserInDatabase.IsActive)
                {
                    //should they be logged out?
                    Log.Write("SwitchRuleController:Edit", $"eRR: User with username: {loggedInUserInDatabase.Username} is deactivated");
                    return RedirectToAction("Logout", "Account");
                }

                if(existingSwitch == null)
                {
                    Log.Write("SwitchRuleController:EditUser", $"eRR: Switch doesn't exist");
                    //return View("NotFound");
                    return RedirectToAction("Index"); //or error
                }

                var switchDetails = new SwitchDetails
                {
                    Id = model.Id,
                    IsActive = model.IsActive,
                    IsDefault = model.IsDefault,
                    NameEnquiryUrl = model.NameEnquiryUrl,
                    Processor = model.Processor,
                    TranQueryUrl = model.TranQueryUrl,
                    TransferUrl = model.TransferUrl
                };

                var result = await switchManager.Edit(switchDetails);

                if (result.ResponseCode != "00")
                {
                    ModelState.AddModelError("", result.ResponseMessage);
                    return View(model);
                }

                ToastNotification.Success("Switch edited successfully");
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Log.Write("SwitchRuleController:Edit", $"eRR: {ex.Message}");
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
        //        var existingSwitch = await db.SwitchTb.SingleOrDefaultAsync(x => x.Id == id);

        //        if (loggedInUserInDatabase == null)
        //        {
        //            Log.Write("SwitchRuleController:Delete", $"eRR: Logged in user not gotten");
        //            return RedirectToAction("Logout", "Account");
        //        }

        //        if (!loggedInUserInDatabase.IsActive)
        //        {
        //            //should they be logged out?
        //            Log.Write("SwitchRuleController:Delete", $"eRR: User with username: {loggedInUserInDatabase.Username} is deactivated");
        //            return RedirectToAction("Logout", "Account");
        //        }

        //        if (existingSwitch == null)
        //        {
        //            Log.Write("UserController:Delete", $"eRR: Switch doesn't exist");
        //            //return View("NotFound");
        //            return RedirectToAction("Index"); //or error
        //        }

        //        var result = await switchManager.Delete(id);

        //        if (result.ResponseCode != "00")
        //        {                 
        //            return RedirectToAction("Index");
        //        }

        //        return RedirectToAction("Index");

        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Write("SwitchRuleController:Delete", $"eRR: {ex.Message}");
        //        return RedirectToAction("Index");
        //    }
        //}
    }
}
