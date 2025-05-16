using ManagementLabel.LogIn;

namespace ManagementLabel.ProductsF
{
    public class Categories
    {
        public int categoriesId { get; set; }
        public string? categoryName { get; set; }
        public int userId { get; set; }
        public Users? User { get; set; }   // Navigation Property

        public List<Products>? Products { get; set; }// Navigation Property
    }
}
