using ManagementLabel.ProductsF;
using System.ComponentModel.DataAnnotations;

namespace ManagementLabel.Model
{
    public class Products
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Name ist erforderlich.")]
        public string? Name_de { get; set; }
        [Required(ErrorMessage = "Description ist erforderlich.")]
        public string? Description_de { get; set; }
        [Required(ErrorMessage = "Bitte wählen Sie eine Kategorie aus.")]
        public int CategoryId { get; set; }
        public Categories? Category { get; set; }
        public string? Barcode { get; set; } = "BarcodeNull";
        [Range(1, int.MaxValue, ErrorMessage = "Quantity muss größer als 0 sein.")]
        public int Quantity { get; set; }
        [Range(0.01, double.MaxValue, ErrorMessage = "Purchase Price muss größer als 0 sein.")]
        public double PurchasePrice { get; set; }
        [Range(0.01, double.MaxValue, ErrorMessage = "Sale Price muss größer als 0 sein.")]
        public double SalePrice { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Minimum Stock muss größer als 0 sein.")]
        public int MinimumStock { get; set; }
        [Required(ErrorMessage = "Startdatum ist erforderlich.")]
        [DateInFuture(ErrorMessage = "Startdatum muss in der Zukunft liegen.")]
        public DateTime EXPDate { get; set; }
        [Required(ErrorMessage = "Bitte wählen Sie eine Hersteller aus.")]
        public int? ManufacturerId { get; set; }
        public Manufacturer? Manufacturer { get; set; }
        public int UserId { get; set; }
        public Users? User { get; set; }
        public byte[]? Image { get; set; }
        [Required(ErrorMessage = "يجب ادخال اسم المنتج ايضا بل عربية")]
        public string? Name_ar { get; set; }
        [Required(ErrorMessage = "يجب ادخال وصف المنتج ايضا بل عربية")]
        public string? Description_ar { get; set; }
        [Required(ErrorMessage = "Bitte geben Sie die Steuersatz ein")]
        public int? TaxRateId { get; set; }
        public TaxRate? TaxRate { get; set; }
        public int? ProductGroupID { get; set; }
        public GroupProducts? ProductGroup { get; set; }
        public bool IsShippable { get; set; } = true;
        public double DiscountedPrice { get; set; } = 0;
        //
        public CartItem CartItem { get; set; } = null!;
        public Products()
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "DPImage.png");
            if(File.Exists(path))
                Image = File.ReadAllBytes(path); 
        }
        public void InitializeCartItem(int quantity)
        {
            CartItem = new CartItem
            {
                ProductId = this.Id, // تأكد أن Id معروف
                Quantity = quantity,
                Product = this! 
            };
        }

        // time validation attribute for future dates
        public class DateInFutureAttribute : ValidationAttribute
        {
            public override bool IsValid(object? value)
            {
                if (value is DateTime date)
                {
                    return date.Date >= DateTime.Today;
                }
                return false;
            }
        }
    }
}
