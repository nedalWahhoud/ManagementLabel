using ManagementLabel.Model;

namespace ManagementLabel.Components.ProductGroupF
{
    public class ProductGroupService(HttpClient http)
    {
        private readonly HttpClient _http = http;
        public List<GroupProducts> DownloadedproductGroups { get; private set; } = [];
        public async Task<ValidationResult> GetAllGroupProductsAsync()
        {
            if(DownloadedproductGroups.Count != 0)
                return new ValidationResult() { Result = true, Message = "Product groups already loaded" };

            try
            {
                var response = await _http.GetAsync("api/GroupProducts/getAllGroupProducts");

                if (!response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ValidationResult>() ?? new ValidationResult() { Message = response.RequestMessage?.ToString() ?? "Failed to load product groups", Result = false };
                }

                var groupProducts = await response.Content.ReadFromJsonAsync<List<GroupProducts>>() ?? [];

                // add to local list
                DownloadedproductGroups.AddRange(groupProducts);

                return new ValidationResult() { Result = true, Message = "Product groups loaded successfully" };
            }
            catch (Exception ex)
            {
                return new ValidationResult() { Message = ex.Message, Result = false };
            }
        }
    }
}
