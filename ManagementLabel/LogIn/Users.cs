namespace ManagementLabel.LogIn
{
    public class Users
    {
        public int usersId { get; set; }
        public string? username { get; set; }
        public string? passwordHash { get; set; }
        public string? email { get; set; }
        public bool isActive { get; set; }
        public string? role { get; set; }
        public DateTime createdAt { get; set; }

      

    }
}
