using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Momo.Common.Actions;
using MomoSwitchPortal.Actions;
using MomoSwitchPortal.Models.Database;
using MomoSwitchPortal.Models.Internals;
using MomoSwitchPortal.Models.ViewModels.Profile;

namespace MomoSwitchPortal.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private ILog Log;
        private ICommonUtilities CommonUtilities;
        private readonly IProfile profileManager;
        private readonly INotyfService ToastNotification;

        public ProfileController(ILog log, IProfile profileManager, ICommonUtilities commonUtilities, INotyfService toastNotification)
        {
            Log = log;
            this.profileManager = profileManager;
            CommonUtilities = commonUtilities;
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
                    Log.Write("ProfileController:Index", $"eRR: Logged in user not gotten");
                    return RedirectToAction("Logout", "Account");
                }

                if (!loggedInUserInDatabase.IsActive)
                {
                    //should they be logged out?
                    Log.Write("ProfileController:Index", $"eRR: User with username: {loggedInUserInDatabase.Username} is deactivated");
                    return RedirectToAction("Logout", "Account");
                }
              
                var result = await profileManager.GetProfile(loggedInUserInDatabase.Id);

                if (result.ResponseHeader.ResponseCode != "00")
                {
                    return View("Error");
                }

                return View(result.ProfileDetails);

            }
            catch (Exception ex)
            {
                Log.Write("ProfileController:Index", $"eRR: {ex.Message}");
                return View("Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            try
            {
                var db = new MomoSwitchDbContext();
                var loggedInUser = HttpContext.GetLoggedInUser();

                var loggedInUserInDatabase = await db.PortalUserTb.SingleOrDefaultAsync(x => x.Username.ToLower() == loggedInUser.ToLower());

                if (loggedInUserInDatabase == null)
                {
                    Log.Write("ProfileController:Edit", $"eRR: Logged in user not gotten");
                    return RedirectToAction("Logout", "Account");
                }

                if (!loggedInUserInDatabase.IsActive)
                {
                    //should they be logged out?
                    Log.Write("ProfileController:Edit", $"eRR: User with username: {loggedInUserInDatabase.Username} is deactivated");
                    return RedirectToAction("Logout", "Account");
                }

                var viewModel = new EditProfileViewModel
                {
                    FirstName = loggedInUserInDatabase.FirstName,
                    LastName = loggedInUserInDatabase.LastName       
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                Log.Write("ProfileController:Edit", $"eRR: {ex.Message}");
                return View("Error");
            }
        } 
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditProfileViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var db = new MomoSwitchDbContext();
                var loggedInUser = HttpContext.GetLoggedInUser();

                var loggedInUserInDatabase = await db.PortalUserTb.SingleOrDefaultAsync(x => x.Username.ToLower() == loggedInUser.ToLower());

                if (loggedInUserInDatabase == null)
                {
                    Log.Write("ProfileController:Edit", $"eRR: Logged in user not gotten");
                    return RedirectToAction("Logout", "Account");
                }

                if (!loggedInUserInDatabase.IsActive)
                {
                    //should they be logged out?
                    Log.Write("ProfileController:Edit", $"eRR: User with username: {loggedInUserInDatabase.Username} is deactivated");
                    ToastNotification.Error("Your account is de-activated");
                    return RedirectToAction("Logout", "Account");
                }

                loggedInUserInDatabase.FirstName = model.FirstName;
                loggedInUserInDatabase.LastName = model.LastName;

                if (!string.IsNullOrWhiteSpace(model.OldPassword) && !string.IsNullOrWhiteSpace(model.NewPassword))
                {
                    var oldPasswordHash = CommonUtilities.GetPasswordHash(model.OldPassword);
                    if(oldPasswordHash != loggedInUserInDatabase.Password)
                    {
                        ModelState.AddModelError("", "Old Password is incorrect!");
                        return View(model);
                    }
                  
                    var newPasswordHash = CommonUtilities.GetPasswordHash(model.NewPassword);
                    if(loggedInUserInDatabase.Password == newPasswordHash)
                    {
                        ModelState.AddModelError("", "New Password cannot be the same as existing one");
                        return View(model);
                    }
                    loggedInUserInDatabase.Password = newPasswordHash;
                    loggedInUserInDatabase.ModifyDate = DateTime.Now;
                    await db.SaveChangesAsync();

                    ToastNotification.Success("Password changed successfully23");
                    return RedirectToAction("Logout","Account");
                }
                await db.SaveChangesAsync();

                ToastNotification.Success("Account modified successfully");
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Log.Write("ProfileController:Edit", $"eRR: {ex.Message}");
                return View("Error");
            }
        } 
    }
}
