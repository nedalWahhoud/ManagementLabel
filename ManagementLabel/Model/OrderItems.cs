using ManagementLabel.Model;
namespace ManagementLabel.Model
{
    public class OrderItems
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public Products? Product { get; set; }
        public int Quantity { get; set; }
        public double UnitPrice { get; set; }
        public int CategoryId { get; set; }
    }
}
