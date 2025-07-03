using System.ComponentModel.DataAnnotations;

namespace ManagementLabel.Model
{
    public class LoginModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Benutzername oder die Email ist erforderlich.")]
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        [Required(ErrorMessage = "Passwort ist erforderlich.")]
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string BirthDate { get; set; } = DateTime.Now.ToString("yyyy.MM.dd");
    }
}
