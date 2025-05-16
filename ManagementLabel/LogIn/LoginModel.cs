using ManagementLabel.Data;
using System.Text;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Newtonsoft.Json;
using Blazored.LocalStorage;


namespace ManagementLabel.LogIn
{
    public class LoginModel
    {
        public int userId { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? Email { get; set; }
        public bool RememberMe { get; set; } = true;
        public bool isAdmin { get; set; }
    }
}
