using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Momo.Common.Actions;
using Momo.Common.Models.Tables;
using SwitchPortal.Models;
using SwitchPortal.Models.DataBase;
using SwitchPortal.Models.Dtos;
using SwitchPortal.Models.ViewModels.User;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SwitchPortal.Actions;

public interface IUsers
{
    Task<UserSignInResponse> Login(LoginDto dto); 
    Task<ResponseHeader> SendForgotPasswordEmail(ForgotPasswordDto dto);
    Task<UserSignInResponse> RefreshAsync(string token);
    ClaimsPrincipal GetPrincipalFromToken(string token);
    Task<ResponseHeader> UserIsActive(string username);
    Task<ResponseHeader> ValidateUserKey(string username, string userKey);
    Task<ResponseHeader> ForgotPassword(ForgotPasswordDto dto);
    Task<ResponseHeader> ChangePassword(ChangePasswordRequest request);
}
public class Users : IUsers
{
	private readonly ILog Log;
	private readonly IEmail Email;
    private readonly ICommonUtilities CommonUtilities;
    private readonly TokenValidationParameters _tokenValidationParameters;
    private readonly JwtSettings _jwtSettings;
    private readonly NavigationManager navigationManager;

    public Users(ILog log, ICommonUtilities commonUtilities, TokenValidationParameters tokenValidationParameters, JwtSettings jwtSettings, NavigationManager navigationManager, IEmail email)
    {
        Log = log;
        CommonUtilities = commonUtilities;
        _tokenValidationParameters = tokenValidationParameters;
        _jwtSettings = jwtSettings;
        this.navigationManager = navigationManager;
        Email = email;
    }

    public async Task<UserSignInResponse> Login(LoginDto dto)
    {
		try
		{
            var db = new MomoSwitchDbContext();
            var userInDatabase = await db.PortalUserTb.SingleOrDefaultAsync(x => x.Username.ToLower() == dto.Username.ToLower());

            if (userInDatabase == null)
            {
                Log.Write("Users:Login", $"eRR: User with username: {dto.Username} doesn't exist");
                return new UserSignInResponse
                {
                    ResponseHeader = new ResponseHeader
                    {
                        ResponseCode = "01",
                        ResponseMessage = "Invalid username or password"
                    }
                };
            }

            if (!userInDatabase.IsActive)
            {
                Log.Write("Users:Login", $"eRR: User with username: {userInDatabase.Username} is deactivated");
                return new UserSignInResponse
                {
                    ResponseHeader = new ResponseHeader
                    {
                        ResponseCode = "01",
                        ResponseMessage = "Your account has been deactivated. Please contact the administrator for further assistance."
                    }
                };
            }

            var passwordHash = CommonUtilities.GetPasswordHash(dto.Password);

            if(passwordHash != userInDatabase.Password)
            {
                Log.Write("Users:Login", $"eRR: User with username: {userInDatabase.Username} and the entered password  doesn't match");
                return new UserSignInResponse
                {
                    ResponseHeader = new ResponseHeader
                    {
                        ResponseCode = "01",
                        ResponseMessage = "Invalid username or password"
                    }
                };
            }

            return GenerateAuthenticationResultForUser(userInDatabase);

        }
		catch (Exception ex)
		{
            Log.Write("Users:Login", $"eRR: {ex.Message}");
            return new UserSignInResponse
            {
                ResponseHeader = new ResponseHeader
                {
                    ResponseCode = "01",
                    ResponseMessage = "Something went wrong"
                }
            };
		}
    }

    public async Task<ResponseHeader> UserIsActive(string username)
    {
        try
        {
            var db = new MomoSwitchDbContext();
            var userInDatabase = await db.PortalUserTb.AsNoTracking().SingleOrDefaultAsync(x => x.Username.ToLower() == username.ToLower());

            if (userInDatabase == null)
            {              
                return new ResponseHeader
                {
                   
                    ResponseCode = "01"
                                    
                };
            }

            return userInDatabase.IsActive ? new ResponseHeader { ResponseCode = "00" } : new ResponseHeader { ResponseCode = "01" };
        }
        catch (Exception ex)
        {
            Log.Write("Users:UserIsActive", $"eRR: {ex.Message}");
            return new ResponseHeader { ResponseCode = "01" };
        }
    }
    public async Task<UserSignInResponse> RefreshAsync(string token)
    {
        var validatedToken = GetPrincipalFromToken(token);

        if (validatedToken == null)
        {
            return new UserSignInResponse
            {
                ResponseHeader = new ResponseHeader
                {
                    ResponseCode = "01",
                    ResponseMessage = "Invalid token"
                }
            };
        }        
       
        var username = validatedToken.Claims.Single(x => x.Type == "username").Value;


        var db = new MomoSwitchDbContext();
        var user = db.PortalUserTb.SingleOrDefault(x => x.Username == username);

        return GenerateAuthenticationResultForUser(user);
    }

    public ClaimsPrincipal GetPrincipalFromToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        try
        {
            var principal = tokenHandler.ValidateToken(token, _tokenValidationParameters, out var validatedToken);

            return IsJwtWithValidSecurityAlgorithm(validatedToken) ? principal : null;
        }
        catch
        {
            return null;
        }
    }

    private static bool IsJwtWithValidSecurityAlgorithm(SecurityToken validatedToken)
    {
        return (validatedToken is JwtSecurityToken securityToken)
               && securityToken.Header.Alg
                   .Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase);
    }

    private UserSignInResponse GenerateAuthenticationResultForUser(PortalUserTb user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_jwtSettings.Secret));
        var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("username", user.Username),
                new Claim("role", user.Role),
                new Claim("refreshExpiration", DateTime.Now.AddDays(_jwtSettings.RefreshExpiration).ToString())
            };

       
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.Add(_jwtSettings.TokenLifeTime),
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        };



        var token = tokenHandler.CreateToken(tokenDescriptor);

      
        return new UserSignInResponse
        {
            ResponseHeader = new ResponseHeader
            {
                ResponseCode = "00",
                ResponseMessage = "Success"
            },
            Token = tokenHandler.WriteToken(token)
            
        };
    }

    public async Task<ResponseHeader> SendForgotPasswordEmail(ForgotPasswordDto dto)
    {
        try
        {
            var db = new MomoSwitchDbContext();
            var userInDatabase = await db.PortalUserTb.SingleOrDefaultAsync(x => x.Username.ToLower() == dto.Username.ToLower());

            if (userInDatabase == null)
            {
                Log.Write("Users:SendForgotPasswordEmail", $"eRR: User with username: {dto.Username} doesn't exist");
                return new ResponseHeader
                    {
                        ResponseCode = "01",
                        ResponseMessage = "User with username doesn't exist"
                    };
                
            }

            if (!userInDatabase.IsActive)
            {
                Log.Write("Users:SendForgotPasswordEmail", $"eRR: User with username: {userInDatabase.Username} is deactivated");
                return new ResponseHeader
                {
                    ResponseCode = "01",
                    ResponseMessage = "Your account has been deactivated. Please contact the administrator for further assistance."
                };
                
            }

            var key = Guid.NewGuid().ToString("N");
            var currentUrl = navigationManager.BaseUri;

            userInDatabase.UserKey = key;
            await db.SaveChangesAsync();
          
            var result = await Email.SendEmail(new MailRequest
            {
                Subject = "Reset your Momo Switch portal password",
                Body = $"Dear {userInDatabase.FirstName}, \n \n To reset your password click the link below: \n {currentUrl}forgotpassword?username={userInDatabase.Username}&key={key} \n \n Yours truly,\nThe ETG Team",
                FromName = "Coralpay",
                From = "Corlpay",
                To = userInDatabase.Username,
                ToName = "Momo Switch Portal User"
            });

            if (result.ResponseCode != "00")
            {
                Log.Write("Users:SendForgotPasswordEmail", $"ForgotPassword Email failure: {result.ResponseMessage}. User:{userInDatabase.Username}");
                return new ResponseHeader
                {
                    ResponseCode = "01",
                    ResponseMessage = "Something went wrong. Please try again"
                };              
            }

            Log.Write("Users:SendForgotPasswordEmail", $"ForgotPassword Email Success: {result.ResponseMessage}. User:{userInDatabase.Username}");
            return new ResponseHeader
            {
                ResponseCode = "00",
                ResponseMessage = "Email sent"
            };

        }
        catch (Exception ex)
        {
            Log.Write("Users:ForgotPassword", $"eRR: {ex.Message}");
            return new ResponseHeader
            {
                ResponseCode = "01",
                ResponseMessage = "Something went wrong"
            };    
        }
    }

    public async Task<ResponseHeader> ValidateUserKey( string username, string userKey)
    {
        try
        {
            var db = new MomoSwitchDbContext();
            var userInDatabase = await db.PortalUserTb.SingleOrDefaultAsync(x => x.Username.ToLower() == username.ToLower());

            if (userInDatabase == null)
            {
                Log.Write("Users:ValidateUserKey", $"eRR: User with username: {username} doesn't exist");
                return new ResponseHeader
                {
                    ResponseCode = "01",
                    ResponseMessage = "User with username doesn't exist"
                };

            }

            if (!userInDatabase.IsActive)
            {
                Log.Write("Users:ValidateUserKey", $"eRR: User with username: {userInDatabase.Username} is deactivated");
                return new ResponseHeader
                {
                    ResponseCode = "01",
                    ResponseMessage = "Your account has been deactivated. Please contact the administrator for further assistance."
                };

            }

            if(userInDatabase.UserKey != userKey)
            {
                Log.Write("Users:ValidateUserKey", $"eRR: Invalid UserKey");
                return new ResponseHeader
                {
                    ResponseCode = "01",
                    ResponseMessage = "Invalid UserKey"
                };
            }

            return new ResponseHeader
            {
                ResponseCode = "00",
                ResponseMessage = "Valid UserKey"
            };

        }
        catch (Exception ex)
        {
            Log.Write("Users:ValidateUserKey", $"eRR: {ex.Message}");
            return new ResponseHeader
            {
                ResponseCode = "01",
                ResponseMessage = "Something went wrong"
            };
        }
    }

    public async Task<ResponseHeader> ForgotPassword(ForgotPasswordDto dto)
    {
        try
        {
            var db = new MomoSwitchDbContext();
            var userInDatabase = await db.PortalUserTb.SingleOrDefaultAsync(x => x.Username.ToLower() == dto.Username.ToLower());

            if (userInDatabase == null)
            {
                Log.Write("Users:ForgotPassword", $"eRR: User with username: {dto.Username} doesn't exist");
                return new ResponseHeader
                {
                    ResponseCode = "01",
                    ResponseMessage = "Something went wrong. Please try again"
                };

            }

            if (!userInDatabase.IsActive)
            {
                Log.Write("Users:ForgotPassword", $"eRR: User with username: {userInDatabase.Username} is deactivated");
                return new ResponseHeader
                {
                    ResponseCode = "01",
                    ResponseMessage = "Your account has been deactivated. Please contact the administrator for further assistance."
                };

            }

            if (userInDatabase.UserKey != dto.Key)
            {
                Log.Write("Users:ForgotPassword", $"eRR: Invalid UserKey");
                return new ResponseHeader
                {
                    ResponseCode = "01",
                    ResponseMessage = "Something went wrong. Please try again"
                };
            }

            var passwordHash = CommonUtilities.GetPasswordHash(dto.NewPassword);

            if (passwordHash == userInDatabase.Password)
            {
                Log.Write("Users:ForgotPassword", $"eRR: Password the same as existing one");                
                return new ResponseHeader
                {
                    ResponseCode = "01",
                    ResponseMessage = "New Password cannot be the same as existing one"
                };               
            }

            userInDatabase.Password = passwordHash;
            userInDatabase.UserKey = string.Empty;
            await db.SaveChangesAsync();

            return new ResponseHeader
            {
                ResponseCode = "00",
                ResponseMessage = "Your password changed successfully"
            };
        }
        catch (Exception ex)
        {
            Log.Write("Users:ForgotPassword", $"eRR: {ex.Message}");
            return new ResponseHeader
            {
                ResponseCode = "01",
                ResponseMessage = "Something went wrong"
            };
        }
    }

    public async Task<ResponseHeader> ChangePassword(ChangePasswordRequest request)
    {
        try
        {
            var db = new MomoSwitchDbContext();
            var userInDatabase = await db.PortalUserTb.SingleOrDefaultAsync(x => x.Username.ToLower() == request.Username.ToLower());

            if (userInDatabase == null)
            {
                Log.Write("Users:ChangePassword", $"eRR: User with username: {request.Username} doesn't exist");
                return new ResponseHeader
                {
                    ResponseCode = "01",
                    ResponseMessage = "Something went wrong. Please try again"
                };

            }

            if (!userInDatabase.IsActive)
            {
                //should they be logged out?
                Log.Write("Users:ChangePassword", $"eRR: User with username: {userInDatabase.Username} is deactivated");
                return new ResponseHeader
                {
                    ResponseCode = "01",
                    ResponseMessage = "Your account has been deactivated. Please contact the administrator for further assistance."
                };

            }

            var oldPasswordHash = CommonUtilities.GetPasswordHash(request.OldPassword);

            if(oldPasswordHash != userInDatabase.Password)
            {
                Log.Write("Users:ChangePassword", $"eRR: Incorrect old password");
                return new ResponseHeader
                {
                    ResponseCode = "01",
                    ResponseMessage = "Old Password is incorrect"
                };
            }

            var newPasswordHash = CommonUtilities.GetPasswordHash(request.NewPassword);

            if(newPasswordHash == oldPasswordHash)
            {
                Log.Write("Users:ForgotPassword", $"eRR: New password cannot be the same as old password");
                return new ResponseHeader
                {
                    ResponseCode = "01",
                    ResponseMessage = "Old Password is incorrect"
                };
            }

            userInDatabase.Password = newPasswordHash;
            await db.SaveChangesAsync();

            return new ResponseHeader
            {
                ResponseCode = "00",
                ResponseMessage = "Your password changed successfully"
            };
        }
        catch (Exception ex)
        {

            Log.Write("Users:ChangePassword", $"eRR: {ex.Message}");
            return new ResponseHeader
            {
                ResponseCode = "01",
                ResponseMessage = "Something went wrong"
            };
        }
    }
}


