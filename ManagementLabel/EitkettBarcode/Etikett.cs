namespace ManagementLabel.EitkettBarcode
{
    public class Etikett
    {
        public int productsId { get; set; }
        public string? Name { get; set; }
        public string? categoryName { get; set; }
        public double SalePrice { get; set; }
        public string? manufacturerName { get; set; }
        public string? EXPDate { get; set; }
        public string? barcodeBase64 { get; set; }
        public string? manufacturerWebsite { get; set; }
    }
}
