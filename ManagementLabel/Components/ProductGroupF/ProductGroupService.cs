using ManagementLabel.Model;

namespace ManagementLabel.Components.ProductGroupF
{
    public class ProductGroupService(HttpClient http)
    {
        private readonly HttpClient _http = http;
        public List<GroupProducts> DownloadedproductGroups { get; private set; } = [];
        public async Task<List<GroupProducts>> GetAllGroupProductsAsync()
        {
            if (DownloadedproductGroups.Count != 0)
                return DownloadedproductGroups;

            try
            {
                var response = await _http.GetAsync("api/GroupProducts/getAllGroupProducts");

                if (!response.IsSuccessStatusCode)
                {
                    return [];
                }

                var groupProducts = await response.Content.ReadFromJsonAsync<List<GroupProducts>>() ?? [];

                // add to local list
                DownloadedproductGroups.AddRange(groupProducts);

                return DownloadedproductGroups;
            }
            catch
            {
                return [];
            }
        }
    }
}
