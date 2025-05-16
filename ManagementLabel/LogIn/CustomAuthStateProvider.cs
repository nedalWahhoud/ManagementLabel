using ManagementLabel.LogIn;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Principal;
using System.Text.Json;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly IJSRuntime _js;
    private readonly MainRenderState _mainRenderState;
    public CustomAuthStateProvider(IJSRuntime js, MainRenderState mainRenderState)
    {
        _js = js;
        _mainRenderState = mainRenderState;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        if (_mainRenderState.IsRendered)
        {
            try
            {
                string token = await LocalstorageGet("authToken");
                if (string.IsNullOrWhiteSpace(token))
                {
                    // User is not logged
                    return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
                }

                var identity = GetIdentity(token);
                 NotifyUserAuthentication(token);


                var user = new ClaimsPrincipal(identity);
                return new AuthenticationState(user);
            }
            catch
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
        }
        return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
    }

    public  void NotifyUserAuthentication(string token)
    {
        // Save token to localStorage
        LocalstorageSet("authToken", token);
        var identity = GetIdentity(token);

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity))));
    }

    public async Task NotifyUserLogout()
    {
        await _js.InvokeVoidAsync("localStorage.removeItem", "authToken");

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()))));
    }

    private void LocalstorageSet(string key, string value)
    {
        _ = _js.InvokeVoidAsync("localStorage.setItem", key, value);
    }
    public async Task<string> LocalstorageGet(string key)
    {
        string value = await _js.InvokeAsync<string>("localStorage.getItem", key);
        return value;
    }
    private ClaimsIdentity GetIdentity(string token)
    {
        var data = DecodeToJson(token);
        // Extract user data from the token
        string userid = data.GetProperty("userid").GetString()!;
        string username = data.GetProperty("username").GetString()!;
        string email = data.GetProperty("email").GetString()!;
        string role = data.GetProperty("role").GetString()!;

        // save username and email to localStorage to use in other places
        {
            LoginModel loginModel = new LoginModel
            {
                Username = username,
                Email = email,
            };
            string userdataJason = JsonSerializer.Serialize(loginModel);
            LocalstorageSet("userdata", userdataJason);
        }
        // clamin
        var claims = new List<Claim>
                {
                  new Claim(ClaimTypes.NameIdentifier, userid),
                  new Claim(ClaimTypes.Name, username),
                  new Claim(ClaimTypes.Email, email),
                  new Claim(ClaimTypes.Role, role),
                };

        var identity = new ClaimsIdentity(claims, "jwt");

        return identity;
    }
    private JsonElement DecodeToJson(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        string? dataClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "data")?.Value;
        // JSON from "data"-Claim parsen
        var json = JsonDocument.Parse(dataClaim!);
        JsonElement data = json.RootElement;

        return data!;
    }
  
    public class MainRenderState
    {
        public bool IsRendered { get; set; } = false;
    }
}
