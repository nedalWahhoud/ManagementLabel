using BarcodeStandard;
using SkiaSharp;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using ManagementLabel.EitkettBarcode;
using ManagementLabel.Model;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace ManagementLabel.ProductsF
{
    public class DataProcessP
    {
        public byte[] BarcodeGenerator(Products product)
        {
            try
            {
                byte[] byteArray;
                BarCodeData barCodeData = new ()
                {
                    id = product.Id.ToString(),
                    n = product.Name_de,
                    p = (decimal)product.SalePrice,
                };

                string jsonString = System.Text.Json.JsonSerializer.Serialize(barCodeData);

                var barcode = new Barcode();

                SKImage barcodeImg = barcode.Encode(BarcodeStandard.Type.Code128, jsonString, 1000, 200);

                using (MemoryStream ms = new ())
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
        public byte[] ConvertByteToPdf(byte[] imgBytes)
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
        public async Task<byte[]> CompressImage(Microsoft.AspNetCore.Components.Forms.IBrowserFile imageFile)
        {
            var maxReadSize = 10 * 1024 * 1024;

            using var inputStream = imageFile.OpenReadStream(maxReadSize);
            using var image = await Image.LoadAsync(inputStream);

            using var outputStream = new MemoryStream();

            // Automatically reduce dimensions to 800x800
            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Mode = ResizeMode.Max,
                Size = new Size(800, 800)
            }));
            // Save as JPEG at 70% quality
            var encoder = new JpegEncoder { Quality = 70 };
            await image.SaveAsync(outputStream, encoder);
            return outputStream.ToArray();
        }
    }
}
