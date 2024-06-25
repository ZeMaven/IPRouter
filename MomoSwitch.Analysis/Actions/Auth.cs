using Microsoft.IdentityModel.Tokens;
using Momo.Common.Actions;
using MomoSwitch.Analysis.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace MomoSwitch.Analysis.Actions
{
    public interface IAuth
    {
        AuthResponse Reset(AuthRequest Req);
    }

    public class Auth : IAuth
    {
        private readonly ILog _log;
        private readonly IConfiguration _config;

        public Auth(ILog log, IConfiguration config)
        {
            _log = log;
            _config = config;
        }

        public AuthResponse Reset(AuthRequest Req)
        {
            try
            {
                _log.Write($"AuthController", $"Request: {JsonSerializer.Serialize(Req)}");


                var client_id = _config["client_id"];
                var scope = _config["scope"];
                var client_secret = _config["client_secret"];
                var grant_type = _config["grant_type"];

                if (client_id != Req.client_id)
                {
                    _log.Write($"AuthController", $"Error: INVALID CLIENTID");
                    return new AuthResponse { access_token = "", status = "INVALID CLIENTID" };
                }
                else if (scope != Req.scope)
                {
                    _log.Write($"AuthController", $"Error: INVALID SCOPE");
                    return new AuthResponse { access_token = "", status = "INVALID SCOPE" };
                }
                else if (grant_type != Req.grant_type)
                {
                    _log.Write($"AuthController", $"Error: INVALID GRANT TYPE");
                    return new AuthResponse { access_token = "", status = "INVALID GRANT TYPE" };

                }
                else if (client_secret != Req.client_secret)
                {
                    _log.Write($"AuthController", $"Error: INVALID CLIENT SECRET");
                    return new AuthResponse { access_token = "", status = "INVALID CLIENT SECRET" };
                }

                int JwtSpanHours = int.Parse(_config.GetSection("JwtSpanMinute").Value);

                //   string key = "1824a0f8-0b89-4a02-a397-db0123000d26";
                string key = _config["Jwt:Key"];
                var issuer = _config["Jwt:Issuer"];
                var audience = _config["Jwt:Audience"];

                var ClientKey = Guid.NewGuid().ToString();


                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha512Signature);


                var Claims = new List<Claim>();
                Claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
                Claims.Add(new Claim("ClientId", client_id));
                //  Claims.Add(new Claim("Username", ClientDetails.Username));
                //Claims.Add(new Claim("Password", "9@ssm02dS"));

                //Create Security Token object by giving required parameters    
                var ExpDate = DateTime.Now.AddMinutes(JwtSpanHours);
                var token = new JwtSecurityToken(
                                audience: audience,
                                issuer: issuer,
                                claims: Claims,
                                expires: ExpDate,
                                signingCredentials: credentials);
                var jwt_token = new JwtSecurityTokenHandler().WriteToken(token);
                _log.Write($"AuthController", $"Auth: OK");
                return new AuthResponse { access_token = jwt_token, token_type = "Bearer", status = "SUCCESS", expires_in = JwtSpanHours, ext_expires_in = JwtSpanHours };

            }
            catch (Exception Ex)
            {
                _log.Write($"AuthController", $"Error: {Ex.Message}");
                return new AuthResponse { access_token = "", status = "FAILED" };
            }
        }


    }
}
