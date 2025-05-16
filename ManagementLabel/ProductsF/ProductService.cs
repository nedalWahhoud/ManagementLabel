using ManagementLabel.Data;
using ManagementLabel.ManufacturerF;
using Microsoft.EntityFrameworkCore;

namespace ManagementLabel.ProductsF
{
    public class ProductService
    {
        private readonly MyDbContext _context;
        private readonly ILogger<ProductService> _logger;
        public ProductService(MyDbContext context, ILogger<ProductService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<Products>> LoadMoreProducts()
        {
            if (Flags.allProductsLoaded) return new List<Products>();

            try
            {

                var products = await _context.products
                    .OrderByDescending(p => p.productsId)
                    .Skip(ConstantsP.currentPage * ConstantsP.pageSize)
                    .Take(ConstantsP.pageSize)
                    .Include(p => p.Category)
                    .Include(p => p.manufacturer)
                    .ToListAsync();

                if (products.Count == 0)
                {
                    Flags.allProductsLoaded = true;
                }

                ConstantsP.currentPage++;
                return products;
            }
            catch (Exception ex)
            {
                // استخدام ILogger لتسجيل الأخطاء
                _logger.LogError($"Error loading products: {ex.Message}", ex);
                return new List<Products>();
            }
        }
        public async Task<List<Categories>> LoadCategories()
        {
            try
            {
                return await _context.categories.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading categories: {ex.Message}", ex);
                return new List<Categories>();
            }
        }
        public async Task<List<Manufacturer>> LoadManufacturers()
        {
            try
            {
                return await _context.manufacturer.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading manufacturer: {ex.Message}", ex);
                return new List<Manufacturer>();
            }
        }

        public async Task<bool> AddProductAsync(Products newProduct)
        {
            try
            {
                _context.products.Add(newProduct);
                int result = await _context.SaveChangesAsync();
                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error adding product: {ex.Message}", ex);
                return false;


            }
        }
        public async Task<bool> DeleteProductAsync(int productId)
        {
            try
            {
                int result = 0;

                var existingProduct = await _context.products.FindAsync(productId);
                if (existingProduct != null)
                {
                    _context.products.Remove(existingProduct);
                    result = await _context.SaveChangesAsync();
                    return result > 0;
                }
                else
                    return false;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting product: {ex.Message}", ex);
                return false;
            }
        }
        public async Task<bool> UpdateProductAsync(Products editProduct)
        {
            try
            {
                var existingProduct = await _context.products.FindAsync(editProduct.productsId);
                if (existingProduct != null)
                {
                    _context.Entry(existingProduct).CurrentValues.SetValues(editProduct);
                    int result = await _context.SaveChangesAsync();
                    return result > 0;
                }
                else
                    return false;
                
            }
            catch (Exception e)
            {
                _logger.LogError($"Error updating product: {e.Message}", e);
                return false;
            }
        }

        public bool IsValidProduct(Products newProduct, out string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(newProduct.productName))
            {
                errorMessage = "Product name is required.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(newProduct.description))
            {
                errorMessage = "Description is required.";
                return false;
            }

            if (newProduct!.categoriesId <= 0)
            {
                errorMessage = "Category is required.";
                return false;
            }
            if (newProduct.quantity < 0)
            {
                errorMessage = "Quantity must be greater than -1.";
                return false;
            }
            if (newProduct.sellingPrice <= 0)
            {
                errorMessage = "Selling price must be greater than 0.";
                return false;
            }
            if (newProduct.minimumStock < 0)
            {
                errorMessage = "Minimum Stock must be greater than -1.";
                return false;
            }
            if (newProduct.manufacturerId <= 0)
            {
                errorMessage = "manufacturer is required.";
                return false;
            }

            if (newProduct.img == null || (newProduct.img != null && newProduct.img.Length <= 0))
            {
                errorMessage = "img is required.";
                return false;
            }

            errorMessage = string.Empty;
            return true;
        }

        public bool IsEditedProduct(Products currentProduct, Products editProduct)
        {
            if (editProduct == null || currentProduct == null) return false;
            if (currentProduct.productName != editProduct.productName)
                return true;
            if (currentProduct.description != editProduct.description)
                return true;
            if (currentProduct.categoriesId != editProduct.categoriesId)
                return true;
            if (currentProduct.quantity != editProduct.quantity)
                return true;
            if (currentProduct.sellingPrice != editProduct.sellingPrice)
                return true;
            if (currentProduct.minimumStock != editProduct.minimumStock)
                return true;
            if (currentProduct.expirationDate != editProduct.expirationDate)
                return true;
            if (currentProduct.manufacturerId != editProduct.manufacturerId)
                return true;
            if (currentProduct.img!.Length != editProduct.img!.Length)
                return true;

            return false;
        }
    }
}
