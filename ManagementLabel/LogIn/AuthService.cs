using Microsoft.AspNetCore.Components.Authorization;
using ManagementLabel.Model;
using Newtonsoft.Json.Linq;
using System.Security.Claims;

namespace ManagementLabel.LogIn
{
    public class AuthService
    {
        private readonly HttpClient _http;
        private readonly AuthenticationStateProvider _authStateProvider;

        public AuthService(HttpClient http, AuthenticationStateProvider authStateProvider)
        {
           
            _http = http;
            _authStateProvider = authStateProvider;
        }

        public async Task<ValidationResult> LoginAsync(LoginModel loginModel)
        {
            try
            {
                HttpResponseMessage response = await _http!.PostAsJsonAsync("api/Users/login", loginModel);

                if (!response.IsSuccessStatusCode)
                    return new ValidationResult { Result = false, Message = "Login failed. Please check your credentials." };
                // get result
                var result = await response.Content.ReadFromJsonAsync<LoginResponse>();

                if (result == null || string.IsNullOrEmpty(result.Token))
                    return new ValidationResult { Result = false, Message = "Login failed. No token received." };

                // check admin role 
                var claimsIdentity = (_authStateProvider as CustomAuthStateProvider)?.GetIdentity(result.Token);
                if(!claimsIdentity!.HasClaim(ClaimTypes.Role,"admin"))
                    return new ValidationResult { Result = false, Message = "Sie haben keine Admin Rechte" };

                // set the authorization header
                (_authStateProvider as CustomAuthStateProvider)?.NotifyUserAuthentication(result.Token);

                return new ValidationResult { Result = true, Message = "Login successful." };
            }
            catch (Exception ex)
            {
                return new ValidationResult{ Result = false, Message = $"An error occurred during login: {ex.Message}"};
            }
        }
        public async Task Logout()
        {
            if (_authStateProvider is CustomAuthStateProvider customAuthStateProvider)
            {
                await customAuthStateProvider.NotifyUserLogout();
            }
            _http!.DefaultRequestHeaders.Authorization = null;
        }
    }
}
