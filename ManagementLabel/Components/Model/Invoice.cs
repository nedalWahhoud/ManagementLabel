using Microsoft.Extensions.Options;

namespace ManagementLabel.Model
{
    public class Invoice
    {
        public IOptions<ProjectInfo> projectInfo { get; set; } = null!;
        public string InvoceeNumber { get; set; } = string.Empty;
        public Order order { get; set; } = new();
    }
}
