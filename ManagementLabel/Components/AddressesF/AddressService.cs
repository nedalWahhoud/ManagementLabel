using ManagementLabel.Model;

namespace ManagementLabel.Components.AddressesF
{
    public class AddressService(HttpClient http)
    {
        private readonly HttpClient _http = http;
        public List<Address> DownloadedAddresses { get; private set; } = [];
        public async Task<ValidationResult> AddAddressAsync(Address newAddress)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/Addresses/addAddress", newAddress);
                if (!response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ValidationResult>() ?? new ValidationResult { Result = false, Message = "Unknown error." }; ;
                }
                var result = await response.Content.ReadFromJsonAsync<ValidationResult>();

                var getId = result!.Message?.Split(':').LastOrDefault();
                if (result.Result && int.TryParse(getId, out int id))
                {
                    newAddress.Id = id; // Set the ID of the new address	
                    DownloadedAddresses.Insert(0, newAddress); // Add the new address to the list
                }

                return result!;
            }
            catch (Exception ex)
            {
                return new ValidationResult { Result = false, Message = ex.Message };
            }
        }
        public async Task<List<Address>> GetAddressesByUserIdAsync(int UserId)
        {
            try
            {
                List<int> excludeAddressesIds = DownloadedAddresses
                    .Select(a => a.Id)
                    .ToList();
                String query = string.Empty;
                if (excludeAddressesIds != null && excludeAddressesIds.Any())
                {
                    // تحويل القائمة إلى سلسلة مثل excludeCategoryIds=1&excludeCategoryIds=2
                    query = string.Join("&", excludeAddressesIds.Select(id => $"excludeAddressesIds={id}{query}"));
                    query = "?" + query;
                }

                var response = await _http.GetAsync($"api/Addresses/getAddressesByUserId/{UserId}{query}");
                if (!response.IsSuccessStatusCode)
                {
                    return [];
                }
                var addresses = await response.Content.ReadFromJsonAsync<List<Address>>();

                // add to center addresslist
                if (addresses != null && addresses.Count > 0)
                {
                    DownloadedAddresses.AddRange(addresses);
                }

                return addresses ?? [];
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching addresses: {ex.Message}");
                return [];
            }
        }
        public async Task<ValidationResult> DeleteAddressAsync(int addressId)
        {
            try
            {
                var response = await _http.DeleteAsync($"api/Addresses/deleteAddress/{addressId}");
                if (!response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ValidationResult>() ?? new ValidationResult { Result = false, Message = "Unknown error." };
                }
                var result = await response.Content.ReadFromJsonAsync<ValidationResult>();

                // remove the address from the local list
                if (result!.Result)
                {
                    DownloadedAddresses.RemoveAll(a => a.Id == addressId);
                }
                return result!;
            }
            catch (Exception ex)
            {
                return new ValidationResult { Result = false, Message = ex.Message };
            }
        }
        public async Task<ValidationResult> UpdateAddressAsync(Address updatedAddress)
        {
            try
            {
                var response = await _http.PutAsJsonAsync($"api/Addresses/updateAddress", updatedAddress);
                if (!response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ValidationResult>() ?? new ValidationResult { Result = false, Message = "Unknown error." };
                }
                var result = await response.Content.ReadFromJsonAsync<ValidationResult>();
                // update the address in the local list
                if (result!.Result)
                {
                    var index = DownloadedAddresses.FindIndex(a => a.Id == updatedAddress.Id);
                    if (index != -1)
                    {
                        DownloadedAddresses[index] = updatedAddress;
                    }
                }
                return result!;
            }
            catch (Exception ex)
            {
                return new ValidationResult { Result = false, Message = ex.Message };
            }
        }
    }
}
