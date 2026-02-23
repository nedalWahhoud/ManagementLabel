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
                //  unique 4-digit PIN wird in Trigger generatieret generiert
             
                //
                var response = await _http.PostAsJsonAsync("api/Customers/addCustomer", newCustomer);
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadFromJsonAsync<ValidationResult>();
                    return error ?? new ValidationResult { Result = false, Message = "Fehler beim PostAsJsonAsync." };
                }
                  
                var result = await response.Content.ReadFromJsonAsync<ValidationResult>();
                if (result?.Result == true)
                {
                    if (result.NewId.HasValue)
                    {
                        newCustomer.Id = result.NewId.Value;

                        var addedCustomer = await GetCustomerByIdAsync(newCustomer.Id);

                        if (addedCustomer != null)
                        {
                            AddToLocal(addedCustomer);
                            return new ValidationResult { Result = true, Message = "Kunde erfolgreich hinzugefügt." };
                        }
                        else
                        {
                            return new ValidationResult { Result = false, Message = "Fehler beim Abrufen des hinzugefügten Kunden." };
                        }
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
                    var customerToRemove = DownloadedCustomers.FirstOrDefault(p => p.Id == customerId);
                    if (customerToRemove != null)
                    {
                        DownloadedCustomers.Remove(customerToRemove);
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
        private void AddToLocal(Customers customer)
        {
            if (!DownloadedCustomers.Any(p => p.Id == customer.Id))
            {
                DownloadedCustomers.Add(customer);
            }
        }
        private void AddToLocal(List<Customers> customers)
        {
            if (customers == null || customers.Count == 0) return;

            var newItems = customers.ExceptBy(DownloadedCustomers.Select(c => c.Id), c => c.Id).ToList();

            DownloadedCustomers.AddRange(newItems);
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
        public Customers? GetCustomerByIdLocal(int id)
        {
            return DownloadedCustomers.Find(p => p.Id == id);
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
                   currentCustomer.StopNumber != editCustomer.StopNumber ||
                   currentCustomer.PIN != editCustomer.PIN;
        }
    }
}
