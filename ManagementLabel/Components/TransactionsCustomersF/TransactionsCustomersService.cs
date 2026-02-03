using ManagementLabel.Model;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using static System.Net.WebRequestMethods;

namespace ManagementLabel.Components.TransactionsCustomersF
{
    public class TransactionsCustomersService(HttpClient http, IOptions<JwtSettings> jwtSettings)
    {
        private readonly HttpClient _http = http;
        private readonly IOptions<JwtSettings> _jwtSettings = jwtSettings;

        public GetItems<TransactionsCustomers> GetItems { get; set; } = new();

        public List<TransactionsCustomers> DownloadedTransactionsCustomers { get; set; } = [];

        public async Task<ValidationResult> AddTransaction(TransactionsCustomers transaction)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/TransactionsCustomers/addTransaction", transaction);

                if (!response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ValidationResult>() ?? new ValidationResult { Result = false, Message = "Unbekannte Fehler." };
                }
                var result = await response.Content.ReadFromJsonAsync<ValidationResult>();

                if (result != null && result.Result == true)
                {
                    var idStr = result.Message?.Split(':').LastOrDefault();
                    if (int.TryParse(idStr, out int id))
                    {
                        transaction.Id = id;
                    }
                    AddToLocal(transaction,0);
                    return result;
                }
                else
                {
                    return result ?? new ValidationResult { Result = false, Message = "Unbekannte Fehler." };
                }
            }
            catch (Exception ex)
            {
                return new ValidationResult { Result = false, Message = $"Es ist ein Fehler aufgetreten: {ex.Message}" };
            }
        }
        public async Task<ValidationResult> GetTransactionsCustomersAsync()
        {

            if (GetItems.AllItemsLoaded)
            {
                return new ValidationResult() { Result = true, Message = "" };
            }

            try
            {
                var response = await _http.GetAsync($"api/TransactionsCustomers/getTransactionsCustomers?CurrentPage={GetItems.CurrentPage}&PageSize={GetItems.PageSize}&AllItemsLoaded={GetItems.AllItemsLoaded}&Id={GetItems.Id}");
                if (!response.IsSuccessStatusCode)
                {
                    return new ValidationResult() { Result = false, Message = "" };
                }


                var result = await response.Content.ReadFromJsonAsync<GetItems<TransactionsCustomers>>();
                if (result == null)
                    return new ValidationResult() { Result = false, Message = "" };
                else
                {
                    GetItems.CurrentPage = result.CurrentPage;
                    GetItems.AllItemsLoaded = result.AllItemsLoaded;

                    AddToLocal(result.Items);
                    return new ValidationResult() { Result = true, Message = "" };
                }
            }
            catch (Exception ex)
            {
                return new ValidationResult() { Result = false, Message = ex.Message };
            }
        }

        // local
        public void AddToLocal(List<TransactionsCustomers> transactionsCustomers)
        {
            if (transactionsCustomers.Count > 0 && DownloadedTransactionsCustomers.Count == 0)
            {
                DownloadedTransactionsCustomers.AddRange(transactionsCustomers);
                return;
            }
            foreach (var transactionsCustomer in transactionsCustomers)
            {
                if (!DownloadedTransactionsCustomers.Any(p => p.Id == transactionsCustomer.Id))
                {
                    DownloadedTransactionsCustomers.Add(transactionsCustomer);
                }
            }
        }
        public void AddToLocal(TransactionsCustomers transactionsCustomer, int index)
        {
            if (!DownloadedTransactionsCustomers.Any(p => p.Id == transactionsCustomer.Id))
            {
                DownloadedTransactionsCustomers.Insert(index, transactionsCustomer);
            }
        }

        public string GetCustomerNotificationToken(int customerId)
        {
            var claims = new[]  {new Claim(ClaimTypes.NameIdentifier, customerId.ToString()),new Claim("type", "transaction") };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_jwtSettings.Value.Key!)
            );

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Value.Issuer,
                audience: _jwtSettings.Value.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(30),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
