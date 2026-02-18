namespace ManagementLabel.Model
{
    public class Users
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string BirthDate { get; set; } = DateTime.Now.ToString("yyyy.MM.dd");
        public bool IsGuest { get; set; } 
        public bool IsAktiv { get; set; }
        public string SignUpProvider { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } 
    }
}
