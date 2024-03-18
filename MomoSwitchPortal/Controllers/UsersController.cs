using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Momo.Common.Actions;
using MomoSwitchPortal.Actions;
using MomoSwitchPortal.Models.Database;
using MomoSwitchPortal.Models.Internals;
using MomoSwitchPortal.Models.ViewModels.Transaction;
using MomoSwitchPortal.Models.ViewModels.User;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MomoSwitchPortal.Controllers
{
	[Authorize(Roles = "Administrator")]
    public class UsersController : Controller
    {
        private ILog Log;
        private readonly IUser userManager;
        private readonly ICommonUtilities commonUtilities;
        private readonly INotyfService ToastNotification;


        public UsersController(ILog log, IUser userManager, ICommonUtilities commonUtilities, INotyfService toastNotification)
        {
            Log = log;
            this.userManager = userManager;
            this.commonUtilities = commonUtilities;
            ToastNotification = toastNotification;
        }


        [HttpGet]
        public async Task<IActionResult> Index(int page)
        {
			try
			{
                int pageSize = 1;
                int pageNumber = (page == 0 ? 1 : page);

                var db = new MomoSwitchDbContext();
                var loggedInUser = HttpContext.GetLoggedInUser();

                var loggedInUserInDatabase = await db.PortalUserTb.SingleOrDefaultAsync(x => x.Username.ToLower() == loggedInUser.ToLower());

                if (loggedInUserInDatabase == null)
                {
                    Log.Write("UserController:Index", $"eRR: Logged in user not gotten");
                    return RedirectToAction("Logout", "Account");
                }

                if (!loggedInUserInDatabase.IsActive)
                {
                    //should they be logged out?
                    Log.Write("UserController:Index", $"eRR: User with username: {loggedInUserInDatabase.Username} is deactivated");
                    return RedirectToAction("Logout", "Account");
                }

                if (loggedInUserInDatabase.Role != Role.Administrator.ToString())
                {
                    Log.Write("UserController:Index", $"eRR: User with username: {loggedInUserInDatabase.Username} is unauthorized to complete action");
                    return RedirectToAction("Index", "Home");//or unauthorzed
                }

                if (page != 0 && TempData["UserFilterRequest"]?.ToString() != null)
                {
                    var FilterRequest = JsonSerializer.Deserialize<UserListViewModel>(TempData["UserFilterRequest"].ToString());
                    var result = await userManager.UserList(pageNumber, pageSize, loggedInUserInDatabase);

                    if (result.ResponseHeader.ResponseCode != "00")
                    {
                        ToastNotification.Error("System Challenge");
                        return RedirectToAction("Index", "Home");
                    }

                    int Count = result.UserList.Count;
                    result.UserList = result.UserList
                   .Skip((pageNumber - 1) * pageSize)
                   .Take(pageSize)
                   .ToList();

                    result.PaginationMetaData = new(Count, pageNumber, pageSize);

                    TempData.Keep();

                    return View(result);
                }

                UserListViewModel Users = new();
                await Task.Run((() =>
                {
                    Users = new UserListViewModel()
                    {
                        UserList = db.PortalUserTb.OrderByDescending(x => x.EntryDate).Select(x => new UserDetailsViewModel
                        {
                            EntryDate = x.EntryDate,
                            FirstName = x.FirstName,
                            Id = x.Id,
                            IsActive = x.IsActive,
                            LastName = x.LastName,
                            Role = x.Role,
                            Username = x.Username
                        }).ToList()
                    };
                }));

                int Count2 = Users.UserList.Count;
                Users.UserList = Users.UserList
               .Skip((pageNumber - 1) * pageSize)
               .Take(pageSize)
               .ToList();

                Users.PaginationMetaData = new(Count2, pageNumber, pageSize);
                return View(Users);

            }
			catch (Exception ex)
			{
                Log.Write("UserController:Index", $"eRR: {ex.Message}");             
                return View("Error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(UserFilterRequest model)
        {
            try
            {
                TempData["UserFilterRequest"] = null;
                int pageSize = 1;
                int pageNumber = 1;

                var db = new MomoSwitchDbContext();
                var loggedInUser = HttpContext.GetLoggedInUser();

                var loggedInUserInDatabase = await db.PortalUserTb.SingleOrDefaultAsync(x => x.Username.ToLower() == loggedInUser.ToLower());

                if (loggedInUserInDatabase == null)
                {
                    Log.Write("UserController:Users", $"eRR: Logged in user not gotten");
                    return RedirectToAction("Logout", "Account");
                }

                if (!loggedInUserInDatabase.IsActive)
                {
                    //should they be logged out?
                    Log.Write("UserController:Users", $"eRR: User with username: {loggedInUserInDatabase.Username} is deactivated");
                    return RedirectToAction("Logout", "Account");
                }

                if (loggedInUserInDatabase.Role != Role.Administrator.ToString())
                {
                    Log.Write("UserController:Users", $"eRR: User with username: {loggedInUserInDatabase.Username} is unauthorized to complete action");
                    return RedirectToAction("Index", "Home");//or unauthorzed
                }

                var filterRequest = new UserListViewModel
                {
                    UserFilterRequest = new UserFilterRequest
                    {
                        Username = model.Username
                    }
                };

                var result = await userManager.UserList(pageNumber, pageSize, loggedInUserInDatabase, model.Username);

                if (result.ResponseHeader.ResponseCode != "00")
                {
                    ToastNotification.Error("System Challenge");
                    return RedirectToAction("Index", "Home");
                }

                TempData["UserFilterRequest"] = JsonSerializer.Serialize(filterRequest);
                TempData.Keep();


                int Count = result.UserList.Count;
                result.UserList = result.UserList
               .Skip((pageNumber - 1) * pageSize)
               .Take(pageSize)
               .ToList();

                result.PaginationMetaData = new(Count, pageNumber, pageSize);
                result.UserFilterRequest = filterRequest.UserFilterRequest;
                return View(result);
            }
            catch (Exception ex)
            {
                Log.Write("UserController:Users", $"eRR: {ex.Message}");
                ModelState.AddModelError("", "Something went wrong. Please try again later");
                return View();
            }
        }
        [HttpGet]
        public IActionResult CreateUser()
        {
			try
			{       
                ViewBag.roles = new SelectList(new[] { "Administrator", "Ordinary" });
                return View();	
			}
			catch (Exception ex)
			{
                Log.Write("UserController:CreateUser", $"eRR: {ex.Message}");
                ModelState.AddModelError("", "Something went wrong. Please try again later");
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(CreateUserViewModel model)
        {
            try
            {         
                ViewBag.roles = new SelectList(new[] { "Administrator", "Ordinary" });
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                Log.Write($"UserController:CreateUser", $"Request: {model.Username}");

                var result = await userManager.CreateUser(model);

                if (result.ResponseCode != "00")
                {
                    ModelState.AddModelError("", result.ResponseMessage);
                    return View(model);
                }

                Log.Write($"UserController:CreateUser", $"User created successfully");

                ToastNotification.Success("User created successfully");
                return RedirectToAction("Index", "Users");
            }
            catch (Exception ex)
            {
                Log.Write("UserController:CreateUser", $"eRR: {ex.Message}");
                ModelState.AddModelError("", "Something went wrong. Please try again later");
                return View(model);
            }
        }

        [HttpGet("edituser/{id}")]
        public async Task<IActionResult> EditUser([FromRoute]int id)
        {
            try
            {
                var db = new MomoSwitchDbContext();
                ViewBag.status = new SelectList(new[] { true, false });
                ViewBag.roles = new SelectList(new[] { "Administrator", "Ordinary" });
                var loggedInUser = HttpContext.GetLoggedInUser();
                var existingUser = await db.PortalUserTb.SingleOrDefaultAsync(x => x.Id == id && x.Username != loggedInUser);  

                if (existingUser == null)
                {
                    Log.Write("UserController:EditUser", $"eRR: User doesn't exist");                    
                    //return View("NotFound");
                    return RedirectToAction("Index","Users");
                }
                var viewModel = new EditUserViewModel
                {
                    FirstName = existingUser.FirstName,
                    LastName = existingUser.LastName,
                    IsActive = existingUser.IsActive,
                    Role = existingUser.Role,
                    Username = existingUser.Username,
                    EntryDate = existingUser.EntryDate.ToString("dddd, dd MMMM yyyy")
                };
                return View(viewModel);
            }
            catch (Exception ex)
            {
                Log.Write("UserController:EditUser", $"eRR: {ex.Message}");                
                return View("Error");
            }
        }

        [HttpPost("edituser")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(EditUserViewModel model)
        {
            try
            {
                ViewBag.status = new SelectList(new[] { true, false });
                ViewBag.roles = new SelectList(new[] { "Administrator", "Ordinary" });
                if (!ModelState.IsValid)
                {
                    return View(model);
                }
                var db = new MomoSwitchDbContext();
                var loggedInUser = HttpContext.GetLoggedInUser();
                var existingUser = await db.PortalUserTb.SingleOrDefaultAsync(x => x.Id == model.Id && x.Username != loggedInUser);  
                if (existingUser == null)
                {
                    Log.Write("UserController:EditUser", $"eRR: User doesn't exist");
                    //return View("NotFound");
                    return RedirectToAction("Index", "Users"); //or error
                }

                var result = await userManager.EditUser(model);

                if(result.ResponseCode != "00")
                {
                    ModelState.AddModelError("", result.ResponseMessage);
                    return View(model);
                }

                ToastNotification.Success("User modified successfully");
                return RedirectToAction("Index");    
            }
            catch (Exception ex)
            {
                Log.Write("UserController:EditUser", $"eRR: {ex.Message}");
                return View("Error");
            }
        }
    }
}
