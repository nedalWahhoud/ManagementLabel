namespace ManagementLabel.EitkettBarcode
{
    public class Etikett
    {
        public int productsId { get; set; }
        public string? productName { get; set; }
        public string? categoryName { get; set; }
        public decimal sellingPrice { get; set; }
        public string? manufacturerName { get; set; }
        public string? expirationDate { get; set; }
        public string? barcodeBase64 { get; set; }
        public string? manufacturerWebsite { get; set; }
    }
}
