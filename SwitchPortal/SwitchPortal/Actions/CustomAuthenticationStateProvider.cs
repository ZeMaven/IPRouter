using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Security.Principal;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Momo.Common.Models.Tables;
using Momo.Common.Actions;
using System.IdentityModel.Tokens.Jwt;

namespace SwitchPortal.Actions
{
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly IUsers userManager;
        private readonly ILog Log; 
        private readonly ProtectedLocalStorage ProtectedLocalStore;
        public PortalUserTb CurrentUser { get; private set; } = new();
        public CustomAuthenticationStateProvider(IUsers userManager, ProtectedLocalStorage protectedLocalStore, ILog log)
        {
            this.userManager = userManager;
            ProtectedLocalStore = protectedLocalStore;
            Log = log;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {

            try
            {
                var token = await ProtectedLocalStore.GetAsync<string>("authToken");
                var identity = new ClaimsIdentity();
                var user = new ClaimsPrincipal();
                var state = new AuthenticationState(user);

                if (!token.Success)
                {
                    identity = new ClaimsIdentity();
                    user = new ClaimsPrincipal(identity);
                    state = new AuthenticationState(user);

                    NotifyAuthenticationStateChanged(Task.FromResult(state));

                    return state;
                }

                var claimsPrincipal = userManager.GetPrincipalFromToken(token.Value);


                if (claimsPrincipal == null)
                {
                    await ProtectedLocalStore.DeleteAsync("authToken");
                    identity = new ClaimsIdentity();
                    user = new ClaimsPrincipal(identity);
                    state = new AuthenticationState(user);

                    NotifyAuthenticationStateChanged(Task.FromResult(state));

                    return state;
                }

                var username = claimsPrincipal.FindFirstValue("username");
                var userIsActive = await userManager.UserIsActive(username);

                if(userIsActive.ResponseCode == "01")
                {
                    await ProtectedLocalStore.DeleteAsync("authToken");
                    identity = new ClaimsIdentity();
                    user = new ClaimsPrincipal(identity);
                    state = new AuthenticationState(user);

                    NotifyAuthenticationStateChanged(Task.FromResult(state));

                    return state;
                }

                var tokenExpiration = Convert.ToDouble(claimsPrincipal.FindFirstValue(JwtRegisteredClaimNames.Exp));
                var tokenRefreshExpiration =Convert.ToDateTime(claimsPrincipal.FindFirstValue("refreshExpiration"));
               
                var expirationDateUtc = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                    .AddSeconds(tokenExpiration);


                if(DateTime.Now < tokenRefreshExpiration)
                {
                    if (DateTime.UtcNow > expirationDateUtc)
                    {
                        var refreshResult = await userManager.RefreshAsync(token.Value);

                        if (refreshResult.ResponseHeader.ResponseCode == "01")
                        {
                            await ProtectedLocalStore.DeleteAsync("authToken");
                            identity = new ClaimsIdentity();
                            user = new ClaimsPrincipal(identity);
                            state = new AuthenticationState(user);

                            NotifyAuthenticationStateChanged(Task.FromResult(state));

                            return state;
                        }
                        await ProtectedLocalStore.SetAsync("authToken", refreshResult.Token);
                        identity = new ClaimsIdentity(claimsPrincipal.Claims, "jwt");
                        user = new ClaimsPrincipal(identity);
                        state = new AuthenticationState(user);

                        NotifyAuthenticationStateChanged(Task.FromResult(state));

                        return state;
                    }

                    identity = new ClaimsIdentity(claimsPrincipal.Claims, "jwt");
                    user = new ClaimsPrincipal(identity);
                    state = new AuthenticationState(user);

                    NotifyAuthenticationStateChanged(Task.FromResult(state));

                    return state;

                }

                await ProtectedLocalStore.DeleteAsync("authToken");
                identity = new ClaimsIdentity();
                user = new ClaimsPrincipal(identity);
                state = new AuthenticationState(user);

                NotifyAuthenticationStateChanged(Task.FromResult(state));

                return state;

            }
            catch (Exception ex)
            {
                Log.Write("CustomAuthenticationStateProvider:GetAuthenticationStateAsync", $"eRR: {ex.Message}");
                var identity = new ClaimsIdentity();
                var user = new ClaimsPrincipal(identity);
                var state = new AuthenticationState(user);

                NotifyAuthenticationStateChanged(Task.FromResult(state));

                return state;
            }
        }

        public async Task MarkUserAsAuthenticated()
        {
            var token = await ProtectedLocalStore.GetAsync<string>("authToken");
            var claimsPrincipal = userManager.GetPrincipalFromToken(token.Value);
            var identity = new ClaimsIdentity(claimsPrincipal.Claims, "authCheck");
            var user = new ClaimsPrincipal(identity);
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
        }

        public async Task MarkUserAsAnonymous()
        {
            await ProtectedLocalStore.DeleteAsync("authToken");
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(new())));
        }
    }
}
