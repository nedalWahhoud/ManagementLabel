using ManagementLabel.Data;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using static System.Net.WebRequestMethods;


namespace ManagementLabel.LogIn
{
    public class AuthService
    {
        private readonly ILogger<AuthService> _logger;
        private readonly HttpClient _http;
        private readonly AuthenticationStateProvider _authStateProvider;

        public AuthService(MyDbContext context, ILogger<AuthService> logger, HttpClient http, AuthenticationStateProvider authStateProvider)
        {
            _logger = logger;
            _http = http;
            _authStateProvider = authStateProvider;
        }

        private class LoginResponse
        {
            public bool success { get; set; }
            public string? token { get; set; }
        }

        public async Task<bool> LoginAsync(LoginModel loginModel)
        {
            try
            {
                loginModel.Password = string.Concat(SHA256.HashData(Encoding.UTF8.GetBytes(loginModel.Password!)).Select(b => b.ToString("x2")));

               var response = await _http.PostAsJsonAsync("http://localhost/APIs/logIn.php", new
                {
                    username = loginModel.Username,
                    password = loginModel.Password
                });

                loginModel.Password = string.Empty;
                if (!response.IsSuccessStatusCode)
                    return false;
                
                var result = await response.Content.ReadFromJsonAsync<LoginResponse>();

                if (result == null || !result.success || string.IsNullOrEmpty(result.token))
                    return false;
               

                // Notify authentication state
                if (_authStateProvider is CustomAuthStateProvider customProvider)
                    customProvider?.NotifyUserAuthentication(result.token!);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Logout failed");
                return false;
            }
        }
    }
}
