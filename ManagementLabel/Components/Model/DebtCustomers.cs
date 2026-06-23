using System.Text.Json.Serialization;

namespace ManagementLabel.Model
{
    public class DebtCustomers
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public decimal Balance { get; set; }
        [JsonIgnore]
        public decimal DraftBalance { get; set; }
    }
}
