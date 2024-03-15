using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Momo.Common.Actions;
using MomoSwitchPortal.Actions;
using MomoSwitchPortal.Models.Database;
using MomoSwitchPortal.Models.ViewModels.User;

namespace MomoSwitchPortal.Controllers
{
	[Authorize(Roles = "Administrator")]
    public class UsersController : Controller
    {
        private ILog Log;
        private readonly IUser userManager;
        private readonly ICommonUtilities commonUtilities;

        public UsersController(ILog log, IUser userManager, ICommonUtilities commonUtilities)
        {
            Log = log;
            this.userManager = userManager;
            this.commonUtilities = commonUtilities;
        }


        [HttpGet]
        public async Task<IActionResult> Index(int? page)
        {
			try
			{
                int pageSize = 25;
                int pageNumber = (page ?? 1);
                var result = await userManager.UserList(pageNumber, pageSize);
                
                if (result.ResponseHeader.ResponseCode != "00")
                    return View("Error");
               
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
                return RedirectToAction("Index", "Home");
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
                var existingUser = await db.PortalUserTb.SingleOrDefaultAsync(x => x.Id == id);  

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
                    Username = existingUser.Username
                };
                return View(viewModel);
            }
            catch (Exception ex)
            {
                Log.Write("UserController:EditUser", $"eRR: {ex.Message}");
                ModelState.AddModelError("", "Something went wrong. Please try again later");
                return View();
            }
        }
    }
}
