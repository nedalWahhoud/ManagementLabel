using ManagementLabel.LogIn;
using ManagementLabel.ProductsF;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly IJSRuntime _js;
    private readonly ProductService _productService;

    public CustomAuthStateProvider(IJSRuntime js, ProductService productService)
    {
        _js = js;
        _productService = productService;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            ClaimsPrincipal user;
            string token = await LocalstorageGet("authToken");
            if (string.IsNullOrWhiteSpace(token))
            {
                // User is not logged
                user = new ClaimsPrincipal(new ClaimsIdentity());
            }
            else
            {
                var identity = GetIdentity(token);
                NotifyUserAuthentication(token);

                user = new ClaimsPrincipal(identity);
            }

            return new AuthenticationState(user);
        }
        catch
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
    }
    public void NotifyUserAuthentication(string token)
    {
        // Save token to localStorage
        LocalstorageSet("authToken", token);
        var identity = GetIdentity(token);

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity))));
    }
    public async Task NotifyUserLogout()
    {
        await _js.InvokeVoidAsync("localStorage.removeItem", "authToken");
        // Reset the product service state
        _productService.Reset();
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()))));
    }
    private void LocalstorageSet(string key, string value)
    {
        _ = _js.InvokeVoidAsync("localStorage.setItem", key, value);
    }
    private async Task<string> LocalstorageGet(string key)
    {
        string value = await _js.InvokeAsync<string>("localStorage.getItem", key);
        return value;
    }
    public ClaimsIdentity GetIdentity(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        // get claims from jwtToken
        var wantedClaims = new[] {
             ClaimTypes.NameIdentifier,
             ClaimTypes.Name,
             ClaimTypes.Email,
             ClaimTypes.Role,
             ClaimTypes.DateOfBirth
            };

        var filteredClaims = jwtToken.Claims
            .Where(c => wantedClaims.Contains(c.Type))
            .Select(c => new Claim(c.Type, c.Value))
            .ToList();

        var identity = new ClaimsIdentity(filteredClaims, "claim");

        return identity;
    }
}
