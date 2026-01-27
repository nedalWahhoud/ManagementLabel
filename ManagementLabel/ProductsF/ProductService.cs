using ManagementLabel.Model;


namespace ManagementLabel.ProductsF
{
    public class ProductService(HttpClient http)
    {
        private readonly HttpClient _http = http;

        private GetItems<Products> GetItems { get; set; } = new();

        public List<Products> DownloadedProduct { get; set; } = [];

        public List<Manufacturer> DownloadedManufacturers { get; set; } = [];
        public List<TaxRate> DownloadedTaxRates { get; set; } = [];
        public async Task<List<Products>> GetProductByIdsServer(List<int> productIds)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/Products/getProductByIds", productIds);

                if (!response.IsSuccessStatusCode)
                    return null!;
                var products = await response.Content.ReadFromJsonAsync<List<Products>>();
                if (products != null)
                {
                        // add the product to the local list
                        AddProductToLocal(products);
                    return products;
                }
                return null!;
            }
            catch
            {
                return null!;
            }
        }
        public async Task<GetItems<Products>> LoadMoreProducts()
        {
            if (GetItems.AllItemsLoaded)
                return new() { AllItemsLoaded = GetItems.AllItemsLoaded };
            try
            {
                var response = await _http.GetAsync($"api/Products/getProducts?CurrentPage={GetItems.CurrentPage}&PageSize={GetItems.PageSize}&AllItemsLoaded={GetItems.AllItemsLoaded}");

                if (!response.IsSuccessStatusCode)
                    return new();

                var getItems = await response.Content.ReadFromJsonAsync<GetItems<Products>>();

                GetItems.AllItemsLoaded = getItems!.AllItemsLoaded;
                GetItems.CurrentPage = getItems!.CurrentPage;

                // add to local list
                AddProductToLocal(getItems.Items);

                return getItems ?? new();
            }
            catch
            {
                return new();
            }
        }
        public async Task<List<Manufacturer>> GetAllManufacturers()
        {
            if(DownloadedManufacturers.Count > 0)
                return DownloadedManufacturers;
            try
            {
                var response = await _http.GetAsync($"api/Products/getManufacturers");
                if (!response.IsSuccessStatusCode)
                    return [];

                var getItems = await response.Content.ReadFromJsonAsync<GetItems<Manufacturer>>();
                // add the manufacturers to the local list
                DownloadedManufacturers.AddRange(getItems?.Items ?? []); 

                return DownloadedManufacturers;
            }
            catch
            {
                return [];
            }
        }
        public async Task<List<TaxRate>> GetAllTaxRates()
        {
            if (DownloadedTaxRates.Count > 0)
                return DownloadedTaxRates;
            try
            {
                var response = await _http.GetAsync($"api/Products/getTaxRates");
                if (!response.IsSuccessStatusCode)
                    return [];

                var getItems = await response.Content.ReadFromJsonAsync<GetItems<TaxRate>>();
                // add the tax rates to the local list
                DownloadedTaxRates.AddRange(getItems?.Items ?? []);

                return DownloadedTaxRates;
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
                    return await response.Content.ReadFromJsonAsync<ValidationResult>() ?? new ValidationResult { Result = false, Message = "Unknown error." };
                }

                var result = await response.Content.ReadFromJsonAsync<ValidationResult>();

                if(result?.Result == true)
                {
                    return result;
                }

                return result ?? new ValidationResult { Result = false, Message = "Unknown error." };
            }
            catch(Exception ex)
            {
                return new ValidationResult { Result = false, Message = ex.Message };
            }
        }
        public void Reset()
        {
            GetItems.Items.Clear();
            GetItems.AllItemsLoaded = false;
            GetItems.CurrentPage = 0;
            GetItems.PageSize = 11;
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
            if (currentProduct.IsShippable != editProduct.IsShippable)
                return true;
            if (currentProduct.DiscountedPrice != editProduct.DiscountedPrice)
                return true;
            if (currentProduct.ProductImages.FirstOrDefault(i => i.IsMain)?.LastModified != editProduct.ProductImages.FirstOrDefault(i => i.IsMain)?.LastModified)
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
        // local
        public void AddProductToLocal(List<Products> products)
        {
            if(products.Count > 0 && DownloadedProduct.Count == 0)
            {
                DownloadedProduct.AddRange(products);
                return;
            }
            foreach (var product in products)
            {
                if (!DownloadedProduct.Any(p => p.Id == product.Id))
                {
                    DownloadedProduct.Add(product);
                }
            }
        }
        public void AddProductToLocal(Products product)
        {
            if (!DownloadedProduct.Any(p => p.Id == product.Id))
            {
                DownloadedProduct.Add(product);
            }
        }
        public List<Products> GetProductByCategoryIdLocal(int categoryId)
        {
            return DownloadedProduct
                    .Where(p => p.CategoryId == categoryId)
                    .ToList();
        }
        public Products GetProductByIdLocal(int productId)
        {
            var product = DownloadedProduct.Find(p => p.Id == productId);
            if (product != null)
                return product;
            else
            {
                return null!;
            }
        }
        public async Task<Products> GetProductByIdAsync(int productId)
        {
            try
            {
                var response = await _http.GetAsync($"api/Products/getProductById/{productId}");

                if (!response.IsSuccessStatusCode)
                    return null!;
                var product = await response.Content.ReadFromJsonAsync<Products>();
                if (product != null)
                {
                    // add the product to the local list
                    AddProductToLocal(product!);
                    return product!;
                }
                return null!;
            }
            catch
            {
                return null!;
            }
        }
        // 
    }
}
