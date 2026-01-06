using System.ComponentModel.DataAnnotations;

namespace ManagementLabel.Model
{
    public class Categories
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Name de ist erforderlich.")]
        public string? Name_de { get; set; }
        [Required(ErrorMessage = "Name ar ist erforderlich.")]
        public string? Name_ar { get; set; }
    }
}
