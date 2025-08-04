using ManagementLabel.Model;
namespace ManagementLabel.Components.DiscountF
{
    public class DiscountService(HttpClient http)
    {
        private readonly HttpClient _http = http;
        public async Task<ValidationResult> AddDiscountCode(DiscountCodes newDiscountCode)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/Discounts/addDiscountCode", newDiscountCode);
                if (!response.IsSuccessStatusCode)
                    return new ValidationResult { Result = false, Message = "Failed to add discount code." };
                var result = await response.Content.ReadFromJsonAsync<ValidationResult>();

                if (result?.Result == true)
                {
                    var idStr = result.Message?.Split(':').LastOrDefault();
                    if (result.Result && int.TryParse(idStr, out int id))
                    {
                        newDiscountCode.Id = id;
                    }
                }

                return result ?? new ValidationResult { Result = false, Message = "Unknown error occurred." };
            }
            catch (Exception ex)
            {
                return new ValidationResult { Result = false, Message = ex.Message };
            }
        }
        public async Task<ValidationResult> AddDiscountCategory(DiscountCategory newDiscountCategory)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/Discounts/addDiscountCategory", newDiscountCategory);
                if (!response.IsSuccessStatusCode)
                    return new ValidationResult { Result = false, Message = "Failed to add discount category." };
                var result = await response.Content.ReadFromJsonAsync<ValidationResult>();
                if (result?.Result == true)
                {
                    var idStr = result.Message?.Split(':').LastOrDefault();
                    if (result.Result && int.TryParse(idStr, out int id))
                    {
                        newDiscountCategory.Id = id;
                    }
                }
                return result ?? new ValidationResult { Result = false, Message = "Unknown error occurred." };
            }
            catch (Exception ex)
            {
                return new ValidationResult { Result = false, Message = ex.Message };
            }
        }
    }
}
