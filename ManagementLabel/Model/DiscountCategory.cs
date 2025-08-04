using ManagementLabel.ProductsF;
using System.ComponentModel.DataAnnotations;

namespace ManagementLabel.Model
{
    public class DiscountCategory
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Category ist erforderlich.")]
        public int CategoriesId { get; set; }
        public Categories Category { get; set; } = new ();
        [Required(ErrorMessage = "Name ist erforderlich.")]
        [StringLength(8, MinimumLength = 8, ErrorMessage = "Code muss genau 8 Zeichen lang sein.")]
        public string Code { get; set; } = string.Empty;
        [Range(1, int.MaxValue, ErrorMessage = "Wert muss größer als 0 sein.")]
        public int DiscountPercentage { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Wert muss größer als 0 sein.")]
        public int UsageLimit { get; set; }
        public int TimesUsed { get; set; }
        [Required(ErrorMessage = "Startdatum ist erforderlich.")]
        [DateInFuture(ErrorMessage = "Startdatum muss in der Zukunft liegen.")]
        public DateTime StartDate { get; set; } = DateTime.Today;
        [Required(ErrorMessage = "Enddatum ist erforderlich.")]
        [DateInFuture(ErrorMessage = "Enddatum muss in der Zukunft liegen.")]
        public DateTime EndDate { get; set; } = DateTime.Today.AddDays(30);
        public bool IsActive { get; set; } = true;
    }
}
