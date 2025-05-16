using BarcodeStandard;
using SkiaSharp;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using ManagementLabel.EitkettBarcode;

namespace ManagementLabel.ProductsF
{
    public class DataProcessP
    {
        public byte[] barcodeGenerator(Products product)
        {
            try
            {
                byte[] byteArray;
                BarCodeData barCodeData = new BarCodeData
                {
                    id = product.productsId.ToString(),
                    n = product.productName,
                    p = (decimal)product.sellingPrice,
                };

                string jsonString = System.Text.Json.JsonSerializer.Serialize(barCodeData);

                var barcode = new Barcode();

                SKImage barcodeImg = barcode.Encode(BarcodeStandard.Type.Code128, jsonString, 600, 200);

                using (MemoryStream ms = new MemoryStream())
                {
                    using (SKData data = barcodeImg.Encode())
                    {
                        data.SaveTo(ms);
                    }
                    byteArray = ms.ToArray();
                }

                return byteArray;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return null!;
            }
        }
        public byte[] convertByteToPdf(byte[] imgBytes)
        {
            using var imageStream = new MemoryStream(imgBytes);
            using var pdfStream = new MemoryStream();

            using var document = new PdfDocument();
            var page = document.AddPage();

            using var image = XImage.FromStream(() => imageStream);

            // Passe die Seitengröße exakt an das Bild an
            page.Width = image.PointWidth;
            page.Height = image.PointHeight;

            using (var gfx = XGraphics.FromPdfPage(page))
            {
                gfx.DrawImage(image, 0, 0, image.PointWidth, image.PointHeight);
            }

            document.Save(pdfStream, false);
            return pdfStream.ToArray();
        }
 

    }
}
