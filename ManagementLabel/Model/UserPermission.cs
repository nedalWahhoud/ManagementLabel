using System.Security;

namespace ManagementLabel.Model
{
    public class UserPermission
    {
        public int UserId { get; set; }
        public int PermissionId { get; set; }

        public required Users User { get; set; }
        public required Permission Permission { get; set; }
    }
}
