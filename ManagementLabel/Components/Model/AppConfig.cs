namespace ManagementLabel.Model
{
    public class AppConfig
    {
        public string ApiBaseUrl { get; set; } = string.Empty;

        public Uri ApiUri => new(ApiBaseUrl);
        public string Domin { get; set; } = string.Empty;
        public string ProductImagesproxy { get; set; } = string.Empty;
        public string CarouselImagesproxy { get; set; } = string.Empty;
        public string WebRequestProductImagePath { get; set; } = string.Empty;
        public string GoogleApiUrl { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;

    }
}
