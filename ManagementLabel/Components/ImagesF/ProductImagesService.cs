using ManagementLabel.Model;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace ManagementLabel.Components.ImagesF
{
    public class ProductImagesService(IOptions<AppConfig> appConfig, IWebHostEnvironment env)
    {
        private readonly IOptions<AppConfig> _appConfig = appConfig;
        private readonly IWebHostEnvironment _env = env;

        public string GetProductImageUrl(ProductImages productImages)
        {
            if (productImages == null)
                return "/images/sample.jpg";

            if (productImages.ImageUrl != null)
            {
                string dbImageUrl = productImages.ImageUrl.TrimStart('/');
                // ✅ Füge eine Zufallszahl hinzu, um Cash zu vermeiden.
                string unique = $"?v={productImages.LastModified}";
                //
                if (_env.IsDevelopment())
                {
                    string baseUri = _appConfig.Value.ApiUri.ToString().TrimEnd('/');
                    string path = _appConfig.Value.WebRequestProductImagePath.Trim('/');

                    string completteUrl = $"{baseUri}/{path}/{dbImageUrl}{unique}";
                    return completteUrl;
                }
                else
                {
                    if (dbImageUrl.StartsWith("ProductsImages/", StringComparison.OrdinalIgnoreCase))
                    {
                        dbImageUrl = dbImageUrl["ProductsImages/".Length..];
                    }
                    string domin = _appConfig.Value.Domin.TrimEnd('/');

                    string completteUrl = $"{domin}/{_appConfig.Value.ProductImagesproxy}/{dbImageUrl}{unique}";
                    return completteUrl;
                }
            }
            else
            {
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", "DPImage.png");
                var relativePath = path.Split("wwwroot")[1].Replace("\\", "/");

                return relativePath;
            }
        }
        public async Task<byte[]> GetImageBytesAndCheckSize(IBrowserFile imageFile)
        {
            using var stream = new MemoryStream();
            byte[] imageBytes = null!;

            // Check if the file size exceeds the limit
            if (imageFile.Size > 512000)
            {
                imageBytes = await CompressImage(imageFile);
            }
            else
            {
                await imageFile.OpenReadStream().CopyToAsync(stream);
                imageBytes = stream.ToArray();
            }

            stream.Dispose();

            return imageBytes;
        }
        public async Task<byte[]> CompressImage(IBrowserFile imageFile)
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
        public ProductImages GetDefaultImage()
        {
            string defaultImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", "DPImage.png");
            var imageBytes = File.ReadAllBytes(defaultImagePath);
            string base64 = Convert.ToBase64String(imageBytes);
            string imageUrl = $"data:image/jpeg;base64,{base64}";

            return new ProductImages
            {
                ImageBytes = imageBytes,
                ImageUrlLocal = imageUrl,
                IsMain = true
            };
        }
    }
}
