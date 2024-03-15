using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Momo.Common.Actions;
using Momo.Common.Models.Tables;
using MomoSwitchPortal.Models;
using MomoSwitchPortal.Models.Database;
using MomoSwitchPortal.Models.Internals;
using MomoSwitchPortal.Models.ViewModels.User;
using X.PagedList;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace MomoSwitchPortal.Actions
{
    public interface IUser
    {
        Task<ResponseHeader> CreateUser(CreateUserViewModel model);
        Task<UserListViewModel> UserList(int pageNumber, int pageSize, PortalUserTb loggedInUser, string username = null);
        Task<ResponseHeader> EditUser(EditUserViewModel user);
    }
    public class User : IUser
    {
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly ILog Log;
        private readonly IEmail Email;
        private readonly ICommonUtilities CommonUtilities;

        public User(IHttpContextAccessor contextAccessor, IEmail email, ILog log, ICommonUtilities commonUtilities)
        {
            _contextAccessor = contextAccessor;
            Email = email;
            Log = log;
            CommonUtilities = commonUtilities;
        }

        public async Task<ResponseHeader> CreateUser(CreateUserViewModel model)
        {
            try
            {
                var db = new MomoSwitchDbContext();
                var loggedInUser = _contextAccessor.HttpContext.GetLoggedInUser();  

                var loggedInUserInDatabase = await db.PortalUserTb.SingleOrDefaultAsync(x => x.Username.ToLower() == loggedInUser.ToLower());
                var usernameExists = await db.PortalUserTb.SingleOrDefaultAsync(x => x.Username.ToLower() == model.Username.ToLower());

                if (loggedInUserInDatabase == null)
                {
                    Log.Write("User:CreateUser", $"eRR: Logged in user not gotten");
                    return new ResponseHeader
                    {
                        ResponseCode = "01",
                        ResponseMessage = "Something went wrong. Please try again"
                    };

                }

                if (!loggedInUserInDatabase.IsActive)
                {
                    //should they be logged out?
                    Log.Write("User:ChangePassword", $"eRR: User with username: {loggedInUserInDatabase.Username} is deactivated");
                    return new ResponseHeader
                    {
                        ResponseCode = "01",
                        ResponseMessage = "Your account has been deactivated. Please contact the administrator for further assistance."
                    };

                }

                if (loggedInUserInDatabase.Role != Role.Administrator.ToString())
                {
                    Log.Write("User:CreateUser", $"eRR: User with username: {loggedInUserInDatabase.Username} is unauthorized to complete action");
                    return new ResponseHeader
                    {
                        ResponseCode = "01",
                        ResponseMessage = "You are not authorized to complete this action"
                    };
                }

                if (usernameExists != null)
                {
                    Log.Write("Users:CreateUser", $"eRR: User with username: {usernameExists.Username} already exists");
                    return new ResponseHeader
                    {
                        ResponseCode = "01",
                        ResponseMessage = "User with username already exists"
                    };
                }
               
                var newUser = new PortalUserTb
                {
                    EntryDate = DateTime.Now,                    
                    FirstName = model.FirstName,
                    LastName = model.LastName,             
                    Role = model.Role,
                    Username = model.Username
                };

                var key = Guid.NewGuid().ToString("N");
                var request = _contextAccessor.HttpContext.Request;

                var host = request.Host.ToUriComponent();

                var pathBase = request.PathBase.ToUriComponent();
                var currentUrl = $"{request.Scheme}://{host}{pathBase}";

                newUser.UserKey = key;
                await db.PortalUserTb.AddAsync(newUser);
                await db.SaveChangesAsync();

                var result = await Email.SendEmail(new MailRequest
                {
                    Subject = "Welcome to Momo Router Portal",
                    Body = $"Welcome to Momo Router Portal, \n \n Click here to finish setting up your account: \n {currentUrl}/account/activateaccount?username={newUser.Username}&key={key} \n \n Yours truly,\nThe ETG Team",
                    FromName = "Coralpay",
                    From = "Corlpay",
                    To = newUser.Username,
                    ToName = "Momo Switch Portal User"
                });

                if (result.ResponseCode != "00")
                {
                    Log.Write("User:CreateUsser", $"Email failure: {result.ResponseMessage}. User:{model.Username}");
                    return new ResponseHeader
                    {
                        ResponseCode = "01",
                        ResponseMessage = "Something went wrong. Please try again later"
                    };
                }

                return new ResponseHeader
                {
                    ResponseCode = "00",
                    ResponseMessage = "User created successfully"
                };
            }
            catch (Exception ex)
            {
                Log.Write("User:CreateUser", $"eRR: {ex.Message}");
                return new ResponseHeader
                {
                    ResponseCode = "01",
                    ResponseMessage = "Something went wrong"                    
                };
            }
        }

        public async Task<ResponseHeader> EditUser(EditUserViewModel user)
        {
            try
            {
                var db = new MomoSwitchDbContext();
                var loggedInUser = _contextAccessor.HttpContext.GetLoggedInUser();

                var loggedInUserInDatabase = await db.PortalUserTb.SingleOrDefaultAsync(x => x.Username.ToLower() == loggedInUser.ToLower());
                var userInDatabase = await db.PortalUserTb.SingleOrDefaultAsync(x => x.Id == user.Id);

                if (loggedInUserInDatabase == null)
                {
                    Log.Write("User:EditUser", $"eRR: Logged in user not gotten");
                    return new ResponseHeader
                    {
                        ResponseCode = "01",
                        ResponseMessage = "Something went wrong. Please try again"
                    };

                }

                if (!loggedInUserInDatabase.IsActive)
                {
                    //should they be logged out?
                    Log.Write("User:EditUser", $"eRR: User with username: {loggedInUserInDatabase.Username} is deactivated");
                    return new ResponseHeader
                    {
                        ResponseCode = "01",
                        ResponseMessage = "Your account has been deactivated."
                    };

                }

                if (loggedInUserInDatabase.Role != Role.Administrator.ToString())
                {
                    Log.Write("User:EditUser", $"eRR: User with username: {loggedInUserInDatabase.Username} is unauthorized to complete action");
                    return new ResponseHeader
                    {
                        ResponseCode = "01",
                        ResponseMessage = "You are not authorized to complete this action"
                    };
                }

                if (userInDatabase == null)
                {
                    Log.Write("User:EditUser", $"eRR: User doesn't exist");
                    return new ResponseHeader
                    {
                        ResponseCode = "01",
                        ResponseMessage = "Something went wrong. Please try again"
                    };

                }

                userInDatabase.FirstName = user.FirstName;
                userInDatabase.LastName = user.LastName;
                userInDatabase.Role = user.Role;
                userInDatabase.IsActive = user.IsActive;
                userInDatabase.ModifyDate = DateTime.Now;
                await db.SaveChangesAsync();

                return new ResponseHeader
                {
                    ResponseCode = "00",
                    ResponseMessage = "User updated successfully"
                };
            }
            catch (Exception ex)
            {
                Log.Write("User:EditUser", $"eRR: {ex.Message}");
                return new ResponseHeader
                {
                    ResponseCode = "01",
                    ResponseMessage = "Something went wrong"
                };
            }
        }

        public async Task<UserListViewModel> UserList(int pageNumber, int pageSize, PortalUserTb loggedInUser, string username = null)
        {
            try
            {
                var db = new MomoSwitchDbContext();               
                
                var userList = new List<UserDetailsViewModel>();
                if (!string.IsNullOrWhiteSpace(username))
                {
                    userList = await db.PortalUserTb.Where(x => x.Username.Contains(username) && x.Id != loggedInUser.Id).Select(x => new UserDetailsViewModel
                    {
                        EntryDate = x.EntryDate,
                        FirstName = x.FirstName,
                        Id = x.Id,
                        IsActive = x.IsActive,
                        LastName = x.LastName,
                        Role = x.Role,
                        Username = x.Username
                    }).OrderByDescending(x => x.EntryDate).ToListAsync();
                }

                else
                {
                    userList = await db.PortalUserTb.Where(x => x.Id != loggedInUser.Id).Select(x => new UserDetailsViewModel
                    {
                        EntryDate = x.EntryDate,
                        FirstName = x.FirstName,
                        Id = x.Id,
                        IsActive = x.IsActive,
                        LastName = x.LastName,
                        Role = x.Role,
                        Username = x.Username
                    }).OrderByDescending(x => x.EntryDate).ToListAsync();
                }
               

                return new UserListViewModel
                {
                    ResponseHeader = new ResponseHeader
                    {
                        ResponseCode = "00",
                        ResponseMessage = "Success"
                    },
                    UserList = userList.ToPagedList(pageNumber, pageSize)
                };
            }
            catch (Exception ex)
            {
                Log.Write("User:UserList", $"eRR: {ex.Message}");
                return new UserListViewModel
                {
                    ResponseHeader = new ResponseHeader
                    {
                        ResponseCode = "01",
                        ResponseMessage = "Something went wrong"
                    }
                };
            }
        }
    }
}
