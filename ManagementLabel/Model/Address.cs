using System.ComponentModel.DataAnnotations;

namespace ManagementLabel.Model
{
    public class Address
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Vorname ist erforderlich.")]
        [RegularExpression(@"^[a-zA-ZäöüÄÖÜß\s\-']{2,50}$", ErrorMessage = "Bitte geben Sie einen gültigen Vornamen ein.")]
        public string? FirstName { get; set; }
        [Required(ErrorMessage = "Nachname ist erforderlich.")]
        [RegularExpression(@"^[a-zA-ZäöüÄÖÜß\s\-']{2,50}$", ErrorMessage = "Bitte geben Sie einen gültigen Nachnamen ein.")]
        public string? LastName { get; set; }
        [Required(ErrorMessage = "Telefonnummer ist erforderlich.")]
        [RegularExpression(@"^\+?[0-9\s\-]{7,20}$", ErrorMessage = "Bitte geben Sie eine gültige Telefonnummer ein.")]
        public string? Phone { get; set; }
        [Required(ErrorMessage = "Straße und Hausnummer ist erforderlich.")]
        [RegularExpression(@"^.+\s+\d+.*$", ErrorMessage = "Bitte geben Sie eine gültige Straße mit Hausnummer ein.")]
        public string? Street { get; set; }
        [Required(ErrorMessage = "PLZ ist erforderlich.")]
        [RegularExpression(@"^\d{5}$", ErrorMessage = "Bitte geben Sie eine gültige 5-stellige PLZ ein.")]
        public string? ZipCode { get; set; }
        [Required(ErrorMessage = "Stadt ist erforderlich.")]
        [RegularExpression(@"^[a-zA-ZäöüÄÖÜß\s\-]{2,50}$", ErrorMessage = "Bitte geben Sie einen gültigen Stadtnamen ein.")]
        public string? City { get; set; }
        public string? Country { get; set; } = "DE";
        public int? UserId { get; set; }
    }
}
