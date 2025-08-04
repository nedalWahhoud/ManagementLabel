using ManagementLabel.Model;
using System.Net.Http;

namespace ManagementLabel.ProductsF
{
    public class ProductService(HttpClient http)
    {
        private readonly HttpClient _http = http;
        private readonly GetItems<Products> _getItems = new();
        public GetItems<Products> GetItems => _getItems;
        public async Task<GetItems<Products>> LoadMoreProducts()
        {
            if (_getItems.AllItemsLoaded)
                return new() { AllItemsLoaded = _getItems.AllItemsLoaded };
            try
            {
                var response = await _http.GetAsync($"api/Products/getProducts?CurrentPage={_getItems.CurrentPage}&PageSize={_getItems.PageSize}&AllItemsLoaded={_getItems.AllItemsLoaded}");

                if (!response.IsSuccessStatusCode)
                    return new();

                var getItems = await response.Content.ReadFromJsonAsync<GetItems<Products>>();

                _getItems.AllItemsLoaded = getItems!.AllItemsLoaded;
                _getItems.CurrentPage = getItems!.CurrentPage;
                return getItems ?? new();
            }
            catch
            {
                return new();
            }
        }
       /* public async Task<List<Categories>> LoadCategories()
        {
            try
            {
                var response = await _http.GetAsync($"api/Products/getCategories");
                if (!response.IsSuccessStatusCode)
                    return [];

                var getItems = await response.Content.ReadFromJsonAsync<GetItems<Categories>>();

                return getItems?.Items ?? [];
            }
            catch
            {
                return [];
            }
        }*/
        public async Task<List<Manufacturer>> LoadManufacturers()
        {
            try
            {
                var response = await _http.GetAsync($"api/Products/getManufacturers");
                if (!response.IsSuccessStatusCode)
                    return [];

                var getItems = await response.Content.ReadFromJsonAsync<GetItems<Manufacturer>>();

                return getItems?.Items ?? [];
            }
            catch
            {
                return [];
            }
        }
        public async Task<List<TaxRate>> LoadTaxRates()
        {
            try
            {
                var response = await _http.GetAsync($"api/Products/getTaxRates");
                if (!response.IsSuccessStatusCode)
                    return [];

                var getItems = await response.Content.ReadFromJsonAsync<GetItems<TaxRate>>();

                return getItems?.Items ?? [];
            }
            catch
            {
                return [];
            }
        }
        public async Task<ValidationResult> AddProductAsync(Products newProduct)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/Products/addProduct", newProduct);
                if (!response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ValidationResult>() ?? new ValidationResult { Result = false, Message = "Unknown error." }; ;
                }

                var result = await response.Content.ReadFromJsonAsync<ValidationResult>();
                return result!;
            }
            catch(Exception ex)
            {
                return new ValidationResult { Result = false, Message = ex.Message };
            }
        }
        public async Task<ValidationResult> DeleteProductAsync(int productId)
        {
            try
            {
                var response = await _http.DeleteAsync($"api/Products/deleteProduct/{productId}");
                if (!response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ValidationResult>() ?? new ValidationResult { Result = false, Message = "Unknown error." }; ;
                }
                
                var result = await response.Content.ReadFromJsonAsync<ValidationResult>();
                return result!;
            }
            catch(Exception ex)
            {
                return new ValidationResult { Result = false, Message = ex.Message };
            }
        }
        public async Task<ValidationResult> UpdateProductAsync(Products editProduct)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/Products/updateProduct", editProduct);
                if (!response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ValidationResult>() ?? new ValidationResult { Result = false, Message = "Unknown error." }; ;
                }

                var result = await response.Content.ReadFromJsonAsync<ValidationResult>();
                return result!;
            }
            catch(Exception ex)
            {
                return new ValidationResult { Result = false, Message = ex.Message };
            }
        }

        public void Reset()
        {
            _getItems.Items.Clear();
            _getItems.AllItemsLoaded = false;
            _getItems.CurrentPage = 0;
            _getItems.PageSize = 11;
        }
        public bool IsEditedProduct(Products currentProduct, Products editProduct)
        {
            if (editProduct == null || currentProduct == null) return false;
            if (currentProduct.Name_de != editProduct.Name_de)
                return true;
            if (currentProduct.Description_de != editProduct.Description_de)
                return true;
            if (currentProduct.Name_ar != editProduct.Name_ar)
                return true;
            if (currentProduct.Description_ar != editProduct.Description_ar)
                return true;
            if (currentProduct.CategoryId != editProduct.CategoryId)
                return true;
            if (currentProduct.Barcode != editProduct.Barcode)
                return true;
            if (currentProduct.Quantity != editProduct.Quantity)
                return true;
            if (currentProduct.PurchasePrice != editProduct.PurchasePrice)
                return true;
            if (currentProduct.SalePrice != editProduct.SalePrice)
                return true;
            if (currentProduct.MinimumStock != editProduct.MinimumStock)
                return true;
            if (currentProduct.EXPDate != editProduct.EXPDate)
                return true;
            if (currentProduct.ManufacturerId != editProduct.ManufacturerId)
                return true;
            if (currentProduct.TaxRateId != editProduct.TaxRateId)
                return true;
            if (currentProduct.ProductGroupID != editProduct.ProductGroupID)
                return true;
            if (currentProduct.Image!.Length != editProduct.Image!.Length)
                return true;

            return false;
        }
        //
        public async Task<List<PaymentMethod>> GetPaymentMethodsAsync()
        {
            try
            {
                var response = await _http.GetAsync("api/Orders/getPaymentMethods");
                if (!response.IsSuccessStatusCode)
                {
                    return [];
                }
               return  await response.Content.ReadFromJsonAsync<List<PaymentMethod>>() ?? [];
                 
            }
            catch
            {
                return [];
            }
        }
    }
}
