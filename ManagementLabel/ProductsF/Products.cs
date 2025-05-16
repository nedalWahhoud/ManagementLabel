using ManagementLabel.LogIn;
using ManagementLabel.ManufacturerF;

namespace ManagementLabel.ProductsF
{
    public class Products
    {
        public int productsId { get; set; }
        public string? productName { get; set; }
        public string? description { get; set; }
        public int categoriesId { get; set; }
        public Categories? Category { get; set; }   // Navigation Property
        public int articleNumber { get; set; }
        public int quantity { get; set; }
        public decimal sellingPrice { get; set; }
        public int minimumStock { get; set; }
        public DateTime expirationDate { get; set; }
        public int manufacturerId { get; set; }
        public Manufacturer? manufacturer { get; set; }   // Navigation Property
        public byte[]? img { get; set; }
        public int userId { get; set; }
        public Users? User { get; set; }   // Navigation Property
    }
}
