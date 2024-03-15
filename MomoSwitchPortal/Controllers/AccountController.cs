using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Momo.Common.Actions;
using MomoSwitchPortal.Actions;
using MomoSwitchPortal.Models.Database;
using MomoSwitchPortal.Models.Internals;
using MomoSwitchPortal.Models.ViewModels.Account;

namespace MomoSwitchPortal.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private ILog Log;
        private readonly IAccount accountManager;
        private readonly ICommonUtilities commonUtilities;
        private readonly IEmail Email;
        public AccountController(ILog log, IAccount account, IEmail email, ICommonUtilities commonUtilities)
        {
            Log = log;
            accountManager = account;
            Email = email;
            this.commonUtilities = commonUtilities;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult SignIn()
        {
            if (HttpContext.User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> SignIn(SignInViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                Log.Write($"AccountController:SignIn", $"Request: {model.Username}");

                var authResult = await accountManager.SignIn(model.Username, model.Password, model.KeepLoggedIn);

                if (authResult.ResponseCode != "00")
                {
                    ModelState.AddModelError("", authResult.ResponseMessage);
                    return View(model);
                }

                Log.Write($"AccountController:SignIn", $"Authentication Successful");
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                Log.Write("AccountController:SignIn", $"eRR: {ex.Message}");
                ModelState.AddModelError("", "Something went wrong. Please try again later");
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Logout() //Signin
        {
            await accountManager.LogOut();
            return RedirectToAction("SignIn", "Account");
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromQuery] string username, [FromQuery] string key) //Signin
        {
            try
            {
                if (HttpContext.User.Identity.IsAuthenticated)
                    return RedirectToAction("Index", "Home");

                var viewModel = new ForgotPasswordUsernameViewModel();
                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(key))
                {
                    return View("EnterEmail", viewModel);
                }
                var db = new MomoSwitchDbContext();
                var userInDatabase = await db.PortalUserTb.SingleOrDefaultAsync(x => x.Username.ToLower() == username.ToLower());

                if (userInDatabase == null)
                {
                    Log.Write("AccountController:ForgotPassword", $"eRR: User with username: {username} doesn't exist");
                    ModelState.AddModelError("", "Something went wrong. Please try again");
                    return View("EnterEmail", viewModel);
                }

                if (!userInDatabase.IsActive)
                {
                    Log.Write("Users:ValidateUserKey", $"eRR: User with username: {userInDatabase.Username} is deactivated");
                    ModelState.AddModelError("", "Your account has been deactivated. Please contact the administrator for further assistance.");
                    return View("EnterEmail", viewModel);

                }

                if (userInDatabase.UserKey != key)
                {
                    Log.Write("Users:ValidateUserKey", $"eRR: Invalid UserKey");
                    ModelState.AddModelError("", "Invalid UserKey");
                    return View("EnterEmail", viewModel);
                }

                var fpViewModel = new ForgotPasswordViewModel
                {
                    Username = username
                };
                return View(fpViewModel);

            }
            catch (Exception ex)
            {
                Log.Write("AccountController:ForgotPassword", $"eRR: {ex.Message}. User:{username}");
                ModelState.AddModelError("", "Something went wrong. Please try again later");
                var viewModel = new ForgotPasswordUsernameViewModel();
                return View("EnterEmail", viewModel);
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ForgotPasswordViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View("ForgotPassword", model);
                }

                var db = new MomoSwitchDbContext();
                var userInDatabase = await db.PortalUserTb.SingleOrDefaultAsync(x => x.Username.ToLower() == model.Username.ToLower());

                if (userInDatabase == null)
                {
                    Log.Write("AccountController:ResetPassword", $"eRR: User with username: {model.Username} doesn't exist");
                    ModelState.AddModelError("", "Something went wrong. Please try again");
                    return View("ForgotPassword", model);
                }

                if (!userInDatabase.IsActive)
                {
                    Log.Write("AccountController:ResetPassword", $"eRR: User with username: {userInDatabase.Username} is deactivated");
                    ModelState.AddModelError("", "Your account has been deactivated. Please contact the administrator for further assistance.");
                    return View("ForgotPassword", model);

                }

                if (userInDatabase.UserKey != model.Key)
                {
                    Log.Write("AccountController:ResetPassword", $"eRR: Invalid UserKey");
                    ModelState.AddModelError("", "Invalid UserKey");
                    return View("ForgotPassword", model);

                }


                var passwordHash = commonUtilities.GetPasswordHash(model.Password);

                if (passwordHash == userInDatabase.Password)
                {
                    Log.Write("AccountController:ResetPassword", $"eRR: New Password the same as existing one");
                    ModelState.AddModelError("", "New Password cannot be the same as existing one");
                    return View("ForgotPassword", model);

                }

                userInDatabase.Password = passwordHash;
                userInDatabase.UserKey = string.Empty;
                userInDatabase.ModifyDate = DateTime.Now;
                await db.SaveChangesAsync();

                return RedirectToAction("Signin");
            }
            catch (Exception ex)
            {
                Log.Write("AccountController:ResetPassword", $"eRR: {ex.Message}. User:{model.Username}");
                ModelState.AddModelError("", "Something went wrong. Please try again later");
                return View("ForgotPassword", model);

            }
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordUsernameViewModel model)
        {
            try
            {
                //this endpoint should send the otp to the user email
                var viewModel = new ForgotPasswordUsernameViewModel();

                if (!ModelState.IsValid)
                {
                    return View("EnterEmail",model);
                }

                if (string.IsNullOrWhiteSpace(model.Username))
                {
                    ModelState.AddModelError("", "Username is required");
                    return View(model);
                }

                Log.Write($"AccountController:ForgotPassword", $"Request: {model.Username}");

                var _context = new MomoSwitchDbContext();
                var userInDatabase = await _context.PortalUserTb.SingleOrDefaultAsync(x => x.Username == model.Username);

                if (userInDatabase == null)
                {
                    Log.Write($"AccountController:ForgotPassword", $"eRR: User with username: {model.Username} doesn't exist");
                    ModelState.AddModelError("", "User with the entered username doesn't exist");
                    return View("EnterEmail", model);

                }

                if (!userInDatabase.IsActive)
                {
                    //log here
                    Log.Write($"AccountController:ForgotPassword", $"Account deactivated. Username: {model.Username} ");
                    ModelState.AddModelError("", "Your account has been deactivated. Please contact the administrator for further assistance.");
                    return View("EnterEmail", model);

                }

                var key = Guid.NewGuid().ToString("N");
                var request = HttpContext.Request;

                var host = request.Host.ToUriComponent();

                var pathBase = request.PathBase.ToUriComponent();
                var currentUrl = $"{request.Scheme}://{host}{pathBase}";

                userInDatabase.UserKey = key;
                await _context.SaveChangesAsync();

                var result = await Email.SendEmail(new MailRequest
                {
                    Subject = "Reset your Momo Switch portal password",
                    Body = $"Dear {userInDatabase.FirstName}, \n \n To reset your password click the link below: \n {currentUrl}/account/forgotpassword?username={userInDatabase.Username}&key={key} \n \n Yours truly,\nThe ETG Team",
                    FromName = "Coralpay",
                    From = "Corlpay",
                    To = userInDatabase.Username,
                    ToName = "Momo Switch Portal User"
                });

                if (result.ResponseCode != "00")
                {
                    Log.Write("AccountController:ForgotPassword", $"Email failure: {result.ResponseMessage}. User:{model.Username}");
                    ModelState.AddModelError("", "Something went wrong. Please try again later");
                    return View(model);
                }

                Log.Write($"AccountController:ForgotPassword", $"ForgotPassword email sent to user:{model.Username}");

              
                viewModel = new ForgotPasswordUsernameViewModel
                {
                    Username = model.Username,
                    EmailSent = true
                };

                return View("EnterEmail", viewModel);                              
            }
            catch (Exception ex)
            {
                Log.Write("AccountController:ForgotPassword", $"eRR: {ex.Message}. User:{model.Username}");
                ModelState.AddModelError("", "Something went wrong. Please try again later");
                return View("EnterEmail", model);
            }
        }

    }
}
