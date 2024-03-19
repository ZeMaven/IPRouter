using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Momo.Common.Actions;
using MomoSwitchPortal.Models;
using MomoSwitchPortal.Models.Database;
using System.Security.Claims;

namespace MomoSwitchPortal.Actions;

public interface IAccount
{
    Task<ResponseHeader> SignIn(string username, string password, bool keepLoggedIn);
    Task LogOut();

}

public class Account : IAccount
{

    private readonly IHttpContextAccessor _contextAccessor;
    private ILog Log;
    private readonly IConfiguration _config;
    private readonly ICommonUtilities commonUtilities;

    public Account(IHttpContextAccessor contextAccessor, ILog log, IConfiguration config, ICommonUtilities commonUtilities)
    {
        _contextAccessor = contextAccessor;
        Log = log;
        _config = config;
        this.commonUtilities = commonUtilities;
    }

    public async Task<ResponseHeader> SignIn(string username, string password, bool keepLoggedIn)
    {
        try
        {
            var _context = new MomoSwitchDbContext();
            var userInDatabase = await _context.PortalUserTb.SingleOrDefaultAsync(x => x.Username == username);

            if (userInDatabase == null)
            {
                //log here
                Log.Write("Account:SignIn", $"eRR: User with username: {username} doesn't exist");
                return new ResponseHeader
                {
                    ResponseCode = "01",
                    ResponseMessage = $"Invalid username or password"

                };
            }

            if (!userInDatabase.IsActive)
            {
                //log here
                Log.Write("Account:SignIn", $"eRR: Account with username: {userInDatabase.Username} is deactivated");
                return new ResponseHeader
                {
                    ResponseCode = "01",
                    ResponseMessage = $"Your account has been deactivated."

                };
            }

            var passwordHash = commonUtilities.GetPasswordHash(password);

            if (passwordHash != userInDatabase.Password)
            {
                //log here 
                Log.Write("Account:SignIn", $"eRR: User with username: {userInDatabase.Username} and the entered password  doesn't exist");
                return new ResponseHeader
                {

                    ResponseCode = "01",
                    ResponseMessage = $"Invalid username or password"

                };
            }
          
            List<Claim> claims = new()
            {
                new Claim("firstname", userInDatabase.FirstName),
                new Claim("lastname", userInDatabase.LastName),
                new Claim("username",userInDatabase.Username),
                new Claim(ClaimTypes.Role, userInDatabase.Role)                    
            };
        
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            AuthenticationProperties Properties = new AuthenticationProperties
            {
                AllowRefresh = true,
                IsPersistent = keepLoggedIn,
            };

            Log.Write("Account:SignIn", "Signing in the user");

            await _contextAccessor.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), Properties);

            return new ResponseHeader
            {
                ResponseCode = "00",
                ResponseMessage = "Success"
            };
        }
        catch (Exception ex)
        {
            Log.Write("Account:SignIn", $"eRR: {ex.Message}");

            return new ResponseHeader
            {
                ResponseCode = "01",
                ResponseMessage = "Something went wrong"
            };
        }

    }

    public async Task LogOut()
    {
        Log.Write("Account:LogOut", "Signing out the user");
        await _contextAccessor.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }

}