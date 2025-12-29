using ManagementLabel.Model;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace ManagementLabel.Components.ImagesF
{
    public class CarouselImageService(HttpClient http, IOptions<AppConfig> appConfig, IWebHostEnvironment env)
    {
        private readonly HttpClient _http = http;
        private readonly IOptions<AppConfig> _appConfig = appConfig;
        private readonly IWebHostEnvironment _env = env;
        public List<CarouselImage> DownloadedCarouselImage { get; private set; } = [];
        public string GetImageUrl(CarouselImage carouselImage)
        {
            if (carouselImage == null)
                return "/images/sample.jpg";

            if (carouselImage.ImageUrl != null)
            {
                string dbImageUrl = carouselImage.ImageUrl.TrimStart('/');
                // ✅ Füge eine Zufallszahl hinzu, um Cash zu vermeiden.
                string unique = $"?v={carouselImage.LastModified}";
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
                    if (dbImageUrl.StartsWith("CarouselImages/", StringComparison.OrdinalIgnoreCase))
                    {
                        dbImageUrl = dbImageUrl["CarouselImages/".Length..];
                    }
                    string domin = _appConfig.Value.Domin.TrimEnd('/');

                    string completteUrl = $"{domin}/{_appConfig.Value.CarouselImagesproxy}/{dbImageUrl}{unique}";
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
        public CarouselImage GetDefaultImage()
        {
            string defaultImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", "DPImage.png");
            var imageBytes = System.IO.File.ReadAllBytes(defaultImagePath);
            string base64 = Convert.ToBase64String(imageBytes);
            string imageUrl = $"data:image/jpeg;base64,{base64}";

            return new CarouselImage
            {
                ImageBytes = imageBytes,
                ImageUrlLocal = imageUrl,
            };
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
            var maxReadSize = 30 * 1024 * 1024;

            using var inputStream = imageFile.OpenReadStream(maxReadSize);
            using var image = await Image.LoadAsync(inputStream);

            using var outputStream = new MemoryStream();

            var maxWidth = 2016;  
            var maxHeight = 1512; 

            // Automatically reduce dimensions to 800x800
            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Mode = ResizeMode.Max,
                Size = new Size(maxWidth, maxHeight)
            }));
            // Save as JPEG at 70% quality
            var encoder = new JpegEncoder { Quality = 70 };
            await image.SaveAsync(outputStream, encoder);
            return outputStream.ToArray();
        }
        // async
        public async Task<List<CarouselImage>> GetAllCarouselAsync()
        {
            if (DownloadedCarouselImage.Count > 0)
                return DownloadedCarouselImage;

            try
            {
                var response = await _http.GetAsync("api/Carousel/getAllCarouselImages");
                if (!response.IsSuccessStatusCode)
                {
                    return [];
                }
                var carouselImages = await response.Content.ReadFromJsonAsync<List<CarouselImage>>();
                if (carouselImages == null)
                {
                    return [];
                }

                // add to local list
                AddProductToLocal(carouselImages);

                return carouselImages;
            }
            catch
            {
                return [];
            }
        }
        public async Task<ValidationResult> AddCarouselImageAsync(CarouselImage carouselImage)
        {
            if (carouselImage == null || carouselImage.ImageBytes == null || carouselImage.ImageBytes.Length == 0)
            {
                return new ValidationResult { Result = false, Message = "Bilddaten sind erforderlich." };
            }
            if (carouselImage == null || carouselImage.ImageBytes == null || carouselImage.ImageBytes.Length == 0)
            {
                return new ValidationResult { Result = false, Message = "" };
            }
            try
            {
                var response = await _http.PostAsJsonAsync("api/Carousel/addCarouselImage", carouselImage);
                if (!response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ValidationResult>() ?? new ValidationResult { Result = false, Message = "Unknown error." }; ;
                }

                var result = await response.Content.ReadFromJsonAsync<ValidationResult>();
                if(result == null || !result.Result)
                {
                    return new ValidationResult { Result = false, Message = "Unknown error." };
                }

                return result;
            }
            catch (Exception ex)
            {
                return new ValidationResult { Result = false, Message = ex.Message };
            }
        }
        public async Task<CarouselImage> GetCarouselImageByIdAsync(int id)
        {
            try
            {
                var response = await _http.GetAsync($"api/Carousel/getCarouselImageById/{id}");
                if (response.IsSuccessStatusCode)
                {
                    var carouselImage = await response.Content.ReadFromJsonAsync<CarouselImage>();
                    if (carouselImage != null)
                    {
                        return carouselImage;
                    }
                }
                return null!;
            }
            catch
            {
                return null!;
            }
        }
        public async  Task<ValidationResult> UpdateCarouselImageAsync(CarouselImage carouselImage)
        {
            if (carouselImage == null)
            {
                return new ValidationResult { Result = false, Message = "Carousel daten sind erforderlich." };
            }
            try
            {
                var response = await _http.PutAsJsonAsync("api/Carousel/updateCarouselImage", carouselImage);
                if (!response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ValidationResult>() ?? new ValidationResult { Result = false, Message = "Unknown error." }; ;
                }
                var result = await response.Content.ReadFromJsonAsync<ValidationResult>();
                if (result == null || !result.Result)
                {
                    return new ValidationResult { Result = false, Message = "Unknown error." };
                }
                // update local list
                var index = DownloadedCarouselImage.FindIndex(ci => ci.Id == carouselImage.Id);
                if (index != -1)
                {
                    DownloadedCarouselImage[index] = carouselImage;
                }
                return result;
            }
            catch (Exception ex)
            {
                return new ValidationResult { Result = false, Message = ex.Message };
            }
        }
        public async Task<ValidationResult> DeleteCarouselImageAsync(int id)
        {
            try
            {
                var response = await _http.DeleteAsync($"api/Carousel/deleteCarouselImage/{id}");
                if (!response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ValidationResult>() ?? new ValidationResult { Result = false, Message = "Unknown error." }; ;
                }
                var result = await response.Content.ReadFromJsonAsync<ValidationResult>();
                if (result == null || !result.Result)
                {
                    return new ValidationResult { Result = false, Message = "Unknown error." };
                }
                // remove from local list
                DownloadedCarouselImage.RemoveAll(ci => ci.Id == id);
                return result;
            }
            catch (Exception ex)
            {
                return new ValidationResult { Result = false, Message = ex.Message };
            }
        }
        // local
        public void AddProductToLocal(CarouselImage carouselImage)
        {
            if (!DownloadedCarouselImage.Any(p => p.Id == carouselImage.Id))
            {
                DownloadedCarouselImage.Add(carouselImage);
            }
        }
        public void AddProductToLocal(List<CarouselImage> carouselImage)
        {
            if (carouselImage.Count > 0 && DownloadedCarouselImage.Count == 0)
            {
                DownloadedCarouselImage.AddRange(carouselImage);
                return;
            }
            foreach (var product in carouselImage)
            {
                if (!DownloadedCarouselImage.Any(p => p.Id == product.Id))
                {
                    DownloadedCarouselImage.Add(product);
                }
            }
        }
    } 
}
