using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ManagementLabel.Model
{
    public class Order
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public int? DeliveryAddressId { get; set; }
        [ForeignKey("DeliveryAddressId")]
        public Address? Address { get; set; }
        public int PaymentMethodId { get; set; }
        public PaymentMethod? PaymentMethod { get; set; }
        public double TotalPrice { get; set; }
        public int StatusId { get; set; }
        public OrderStatus? Status { get; set; }
        public string Notes { get; set; } = string.Empty;
        public int? UserId { get; set; }
        public Users? User { get; set; }
        public List<OrderItems> OrderItems { get; set; } = [];
        public int? DiscountCodeId { get; set; }
        public DiscountCodes? DiscountCode { get; set; }
        public int? DiscountCategoryId { get; set; }
        public DiscountCategory? DiscountCategory { get; set; }
        public int? ShippingProviderId { get; set; }
        public ShippingProvider? ShippingProviders { get; set; }
        public double ShippingCost { get; set; }
        [Required(ErrorMessage = "Bitte geben Sie die Trackingnummer ein.")]
        [MinLength(8, ErrorMessage = "Die Trackingnummer muss mindestens 8 Zeichen lang sein.")]
        public string? TrackingNumber { get; set; } 
    }
}
