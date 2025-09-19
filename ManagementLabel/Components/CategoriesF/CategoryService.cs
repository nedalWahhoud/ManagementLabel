using ManagementLabel.Model;
using ManagementLabel.ProductsF;
using static System.Net.WebRequestMethods;

namespace ManagementLabel.Components.CategoriesF
{
    public class CategoryService(HttpClient http,ProductService productService)
    {
        private readonly HttpClient _http = http;
        private readonly ProductService _productService = productService;
        private List<Categories> DownloadedCategories { get; set; } = [];
        public async Task<List<Categories>> LoadCategories()
        {
            // if DownloadedCategories already has items, return them
            if (DownloadedCategories.Count > 0)
            {
                return DownloadedCategories;
            }
            try
            {
                var response = await _http.GetAsync($"api/Categories/getCategories");
                if (!response.IsSuccessStatusCode)
                    return [];

                var getItems = await response.Content.ReadFromJsonAsync<GetItems<Categories>>();
                // add the categories to the local list
                AddCategoriesToLocal(getItems?.Items ?? []);

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

        public async Task<GetItems<Products>> GetProductsByCategoryIdAsync(int categoryId, int? pageSize = null, List<int>? excludeProductsIds = null)
        {
            GetItems<Products> getItems = new();

            // define the page size
            if (pageSize.HasValue && pageSize.Value > 0)
            {
                getItems.PageSize = pageSize.Value;
            }
            try
            {

                String queryString = string.Empty;
                if (excludeProductsIds != null && excludeProductsIds.Count != 0)
                {
                    queryString = "&";
                    queryString += string.Join("&", excludeProductsIds.Select(id => $"excludeProductsIds={id}"));

                }
                queryString = $"?PageSize={getItems.PageSize}&IsAdmin={true}" + queryString;

                var response = await _http.GetAsync($"api/Categories/getProductsByCategoryId/{categoryId}{queryString}");


                if (!response.IsSuccessStatusCode)
                    return getItems;

                getItems = await response.Content.ReadFromJsonAsync<GetItems<Products>>() ?? new GetItems<Products>();

                // add to local list
                _productService.AddProductToLocal(getItems.Items);

                if (getItems.AllItemsLoaded == true)
                {
                    return getItems;
                }
                else
                {
                    getItems.CurrentPage++;
                    return getItems;
                }
            }
            catch
            {
                return getItems;
            }
        }
        // Local
        public void AddCategoriesToLocal(List<Categories> categories)
        {
            if (categories.Count > 0 && DownloadedCategories.Count == 0)
            {
                DownloadedCategories.AddRange(categories);
                return;
            }

            foreach (var category in categories)
            {
                if (!DownloadedCategories.Any(p => p.Id == category.Id))
                {
                    DownloadedCategories.Add(category);
                }
            }
        }
        public void AddCategoriesToLocal(Categories category)
        {
            if (!DownloadedCategories.Any(p => p.Id == category.Id))
            {
                DownloadedCategories.Add(category);
            }
        }
    }
}
