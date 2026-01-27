using ManagementLabel.Model;

namespace ManagementLabel.Components.CustomersF
{
    public class CustomersService(HttpClient http)
    {
        private readonly HttpClient _http = http;
        public List<Customers> DownloadedCustomers{ get; private set; } = [];

        public async Task<ValidationResult> GetAllCustomers()
        {
            if (DownloadedCustomers.Count > 0)
            {
                return new ValidationResult { Result = true, Message = "Bereits abgerufen." };
            }
            try
            {
                var response = await _http.GetAsync("api/Customers/getAllCustomers");
                if (!response.IsSuccessStatusCode)
                    return new ValidationResult { Result = false, Message = "Fehler beim Abrufen." };
                var customers = await response.Content.ReadFromJsonAsync<List<Customers>>();
                if (customers != null)
                {
                    DownloadedCustomers = customers;
                    return new ValidationResult { Result = true, Message = "erfolgreich abgerufen." };
                }
                return new ValidationResult { Result = false, Message = "Keine Items gefunden." };
            }
            catch (Exception ex)
            {
                return new ValidationResult { Result = false, Message = ex.Message };
            }
        }
        public async Task<ValidationResult> AddCustomer(Customers newCustomer)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/Customers/addCustomer", newCustomer);
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadFromJsonAsync<ValidationResult>();
                    return error ?? new ValidationResult { Result = false, Message = "Fehler beim PostAsJsonAsync." };
                }
                  
                var result = await response.Content.ReadFromJsonAsync<ValidationResult>();
                if (result?.Result == true)
                {
                    var idStr = result.Message?.Split(':').LastOrDefault()?.Trim().Split([' ', '.'], StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
                    if (result.Result && int.TryParse(idStr, out int id))
                    {
                        newCustomer.Id = id;
                        AddToLocal(newCustomer);
                    }
                }
                return result ?? new ValidationResult { Result = false, Message = "Es ist ein unbekannter Fehler aufgetreten." };
            }
            catch (Exception ex)
            {
                return new ValidationResult { Result = false, Message = ex.Message };
            }
        }
        public async Task<ValidationResult> UpdateCustomer(Customers updatedCustomer)
        {
            try
            {
                var response = await _http.PutAsJsonAsync("api/Customers/updateCustomer", updatedCustomer);
                if (!response.IsSuccessStatusCode)
                    return new ValidationResult { Result = false, Message = "Fehler beim Aktualisieren des Kunden." };
                var result = await response.Content.ReadFromJsonAsync<ValidationResult>();
                if (result?.Result == true)
                {
                    var index = DownloadedCustomers.FindIndex(p => p.Id == updatedCustomer.Id);
                    if (index != -1)
                    {
                        var resultLocal = await UpdateCustomerLocal(updatedCustomer.Id);
                        return resultLocal;
                    }
                }
                return result ?? new ValidationResult { Result = false, Message = "Es ist ein unbekannter Fehler aufgetreten." };
            }
            catch (Exception ex)
            {
                return new ValidationResult { Result = false, Message = ex.Message };
            }
        }
        public async Task<ValidationResult> DeleteCustomer(int customerId)
        {
            try
            {
                var response = await _http.DeleteAsync($"api/Customers/deleteCustomer/{customerId}");
                if (!response.IsSuccessStatusCode)
                    return new ValidationResult { Result = false, Message = "Fehler beim Löschen des Kunden." };
                var result = await response.Content.ReadFromJsonAsync<ValidationResult>();
                if (result?.Result == true)
                {
                    int index = DownloadedCustomers.FindIndex(p => p.Id == customerId);
                    if (index != -1)
                    {
                        DownloadedCustomers.RemoveAt(index);
                    }
                }
                return result ?? new ValidationResult { Result = false, Message = "Es ist ein unbekannter Fehler aufgetreten." };
            }
            catch (Exception ex)
            {
                return new ValidationResult { Result = false, Message = ex.Message };
            }
        }

        public async Task<Customers> GetCustomerByIdAsync(int id)
        {
            try
            {
                var response = await _http.GetAsync($"api/Customers/getCustomerById/{id}");
                if (!response.IsSuccessStatusCode)
                    return null!;
                var customer = await response.Content.ReadFromJsonAsync<Customers>();
                if (customer != null)
                {
                    AddToLocal(customer);
                    return customer;
                }
                return null!;
            }
            catch
            {
                return null!;
            }
        }

        // local
        public void AddToLocal(Customers customer)
        {
            if (!DownloadedCustomers.Any(p => p.Id == customer.Id))
            {
                DownloadedCustomers.Add(customer);
            }
        }
        public void AddToLocal(List<Customers> customers)
        {
            if (customers.Count > 0 && customers.Count == 0)
            {
                DownloadedCustomers.AddRange(customers);
                return;
            }

            foreach (var customer in DownloadedCustomers)
            {
                if (!DownloadedCustomers.Any(p => p.Id == customer.Id))
                {
                    DownloadedCustomers.Add(customer);
                }
            }
        }

        public async Task<ValidationResult> UpdateCustomerLocal(int id)
        {
            var index = DownloadedCustomers.FindIndex(p => p.Id == id);
            if (index != -1)
            {
                Customers updatedCustomerAsync = await GetCustomerByIdAsync(id);
                if (updatedCustomerAsync != null)
                {
                    DownloadedCustomers[index] = updatedCustomerAsync;
                    return new ValidationResult { Result = true, Message = "Kunde lokal aktualisiert." };
                }
                else
                    return new ValidationResult { Result = false, Message = "Fehler beim Abrufen des aktualisierten Kunden." };

            }
            return new ValidationResult { Result = false, Message = "Kunde nicht gefunden." };
        }

        public List<Customers> GetCustomerByDistributionLineIdLocal(int DistributionLineId)
        {
            return DownloadedCustomers.Where(p => p.DistributionLineId == DistributionLineId).ToList();
        }
        public bool IsEdited(Customers currentCustomer, Customers editCustomer)
        {
            return currentCustomer.DistributionLineId != editCustomer.DistributionLineId ||
                   currentCustomer.Name_de != editCustomer.Name_de ||
                   currentCustomer.Name_ar != editCustomer.Name_ar ||
                   currentCustomer.Street != editCustomer.Street ||
                   currentCustomer.City != editCustomer.City ||
                   currentCustomer.BuildingNumber != editCustomer.BuildingNumber ||
                   currentCustomer.PostalCode != editCustomer.PostalCode ||
                   currentCustomer.Latitude != editCustomer.Latitude ||
                   currentCustomer.Longitude != editCustomer.Longitude ||
                   currentCustomer.PhoneNumber != editCustomer.PhoneNumber ||
                   currentCustomer.Email != editCustomer.Email ||
                   currentCustomer.Notes_de != editCustomer.Notes_de ||
                   currentCustomer.Notes_ar != editCustomer.Notes_ar ||
                   currentCustomer.StopNumber != editCustomer.StopNumber;
        }
    }
}
