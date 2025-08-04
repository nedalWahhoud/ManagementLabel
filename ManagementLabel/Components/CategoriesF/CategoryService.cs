using ManagementLabel.Model;
using ManagementLabel.ProductsF;
using static System.Net.WebRequestMethods;

namespace ManagementLabel.Components.CategoriesF
{
    public class CategoryService(HttpClient http)
    {
        private readonly HttpClient _http = http;
        private static List<Categories> DownloadedCategories { get; set; } = [];
        public async Task<List<Categories>> LoadCategories()
        {
            // if DownloadedCategories already has items, return them
            if (DownloadedCategories.Count > 0)
                return DownloadedCategories;

            try
            {
                var response = await _http.GetAsync($"api/Products/getCategories");
                if (!response.IsSuccessStatusCode)
                    return [];

                var getItems = await response.Content.ReadFromJsonAsync<GetItems<Categories>>();

                // add the categories to the local list
                DownloadedCategories.AddRange(getItems?.Items ?? []);

                return getItems?.Items ?? [];
            }
            catch
            {
                return [];
            }
        }
        public async Task<Categories> GetCategoryById(int categoryId)
        {
            if (categoryId <= 0)
                return null!;

            // check if the category is already downloaded
            var existingCategory = DownloadedCategories.FirstOrDefault(c => c.Id == categoryId);
            if (existingCategory != null)
                return existingCategory;

            try
            {
                var response = await _http.GetAsync($"api/Categories/getCategoryById/{categoryId}");
                if (!response.IsSuccessStatusCode)
                    return null!;
                var category = await response.Content.ReadFromJsonAsync<Categories>();
                // add to local list if not exists
                if (category != null)
                {
                    DownloadedCategories.Add(category);
                }
                return category!;
            }
            catch
            {
                return null!;
            }
        }
    }
}
