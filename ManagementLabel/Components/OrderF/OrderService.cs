using ManagementLabel.Components.Pages;
using ManagementLabel.Model;

namespace ManagementLabel.Components.OrderF
{
    public class OrderService(HttpClient http)
    {
        private readonly HttpClient _http = http;
        public List<Order> DownloadedOrders { get; private set; } = [];
        public List<OrderStatus> OrderStatusList { get; private set; } = [];
        public List<PaymentMethod> DownloadedPaymentMethods { get; private set; } = [];
        public List<ShippingProvider> DownloadedShippingProviders { get; private set; } = [];
        
        public event Action? OnChange;
        public void NotifyStateChanged() => OnChange?.Invoke();
        public void InitializeAsync()
        {
            _ = RefreshCountOpenOrders();
        }
        public async Task<ValidationResult> AddOrderAsync(Order order)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/Orders/addOrder", order);
                if (!response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ValidationResult>() ?? new ValidationResult { Result = false, Message = "Unknown error." };
                }

                var result = await response.Content.ReadFromJsonAsync<ValidationResult>();
                if(result != null && result.Result)
                {
                    var getId = result.Message?.Split(':').LastOrDefault();
                    if (int.TryParse(getId, out int id))
                    {
                        order.Id = id; // Set the ID of the new order	
                        AddProductToLocal(order); // Add the new order to the list
                    }
                    return result;
                }
                else
                  return new ValidationResult { Result = false, Message = "Unknown error." };
            }
            catch (Exception ex)
            {
                return new ValidationResult { Result = false, Message = ex.Message };
            }
        }
        private GetItems<Order> getItems = new GetItems<Order>() { PageSize = 5 };
        public async Task<ValidationResult> GetAllOrdersbyStatusAsync(string statusId, List<int>? excludeIds = null)
        {
            try
            {
                String? excludeIdsQuery = null;
                if (excludeIds != null && excludeIds.Count > 0)
                {
                    excludeIdsQuery = string.Join("&", excludeIds.Select(id => $"excludeIds={id}"));
                }

                var response = await _http.GetAsync($"api/Orders/getAllOrderByStatusId/{statusId}?PageSize={getItems.PageSize}&AllItemsLoaded={getItems.AllItemsLoaded}" +
                    $"&{excludeIdsQuery}");

                if (!response.IsSuccessStatusCode)
                {
                    return new ValidationResult { Result = false, Message = "Failed to retrieve Orders ." };
                }

                getItems = await response.Content.ReadFromJsonAsync<GetItems<Order>>() ?? new GetItems<Order>();
                if (getItems.AllItemsLoaded == true)
                {
                    getItems.AllItemsLoaded = true; // No more items to load
                    // add to local if exists items
                    if (getItems.Items.Count == 0)
                        AddProductToLocal(getItems.Items);

                    return new ValidationResult { Result = true, Message = "AllItemsLoaded" };
                }
                else
                {
                    AddProductToLocal(getItems.Items);

                    return new ValidationResult { Result = true, Message = "Orders retrieved successfully." };

                }
            }
            catch (Exception ex)
            {
                return new ValidationResult { Result = false, Message = ex.Message };
            }
        }
        public async Task<ValidationResult> GetOrderStatusListAsync()
        {
            // Check if OrderStatusList is already loaded
            if (OrderStatusList.Count > 0)
                return new ValidationResult { Result = true, Message = "Order Statuses already loaded." };

            try
            {
                var response = await _http.GetAsync("api/Orders/getOrderStatusList");
                if (!response.IsSuccessStatusCode)
                {
                    return new ValidationResult { Result = false, Message = "Failed to retrieve Order Statuses." };
                }
                var orderStatuses = await response.Content.ReadFromJsonAsync<List<OrderStatus>>() ?? new ();
                if (orderStatuses.Count == 0)
                {
                    return new ValidationResult { Result = false, Message = "No Order Statuses found." };
                }
                OrderStatusList.AddRange(orderStatuses);
                return new ValidationResult { Result = true, Message = "Order Statuses retrieved successfully." };
            }
            catch (Exception ex)
            {
                return new ValidationResult { Result = false, Message = ex.Message };
            }
        }
        public async Task<ValidationResult> UpdateStatusOrder(int orderId,int newStatusId)
        {
            try
            {
                var response = await _http.PutAsJsonAsync($"api/Orders/updateStatusOrder/{orderId}", newStatusId);
                if (!response.IsSuccessStatusCode)
                {
                    return new ValidationResult { Result = false, Message = "Failed to update Order Status." } ?? new ValidationResult { Result = false, Message = "Unknown error." };
                }
                return new ValidationResult { Result = true, Message = "Order Status updated successfully." } ?? new ValidationResult { Result = false, Message = "Unknown error." };
            }
            catch (Exception ex)
            {
                return new ValidationResult { Result = false, Message = ex.Message };
            }
        }
        public async Task <ValidationResult> AddOrUpdateTrackingNumber(int orderId,string newTrackingNumber)
        {
            try
            {
                var response = await _http.PutAsJsonAsync($"api/Orders/addOrUpdateTrackingNumber/{orderId}", newTrackingNumber);
                if (!response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ValidationResult>() ?? new ValidationResult { Result = false, Message = "Unbekannter Fehler." };
                }
                return await response.Content.ReadFromJsonAsync<ValidationResult>() ?? new ValidationResult { Result = false, Message = "Unbekannter Fehler" };
            }
            catch (Exception ex)
            {
                return new ValidationResult { Result = false, Message = ex.Message };
            }
        }
        public int OpenOrderCount = 0;
        public int CanceledReturnedOrderCount = 0;
        public async Task<OrdersCount> GetOrderCountByStatusId(List<int> statusIds)
        {
            try
            {
                var queryString = string.Join("&", statusIds.Select(id => $"statusIds={id}"));
                var response = await _http.GetAsync($"api/Orders/getOrderCountByStatusId?{queryString}");

                if (!response.IsSuccessStatusCode)
                {
                    return new OrdersCount { Count = 0 };
                }
                var ordersCount = await response.Content.ReadFromJsonAsync<OrdersCount>() ?? new OrdersCount();
                return ordersCount;
            }
            catch
            {
                return new OrdersCount { Count = 0 };
            }
        }
        public async Task RefreshCountOpenOrders()
        {
            while (true)
            {
                // prüf die Count von open orders 
                int previousCount = 0;
                var countOrdersOpen = await GetOrderCountByStatusId(new List<int>() { 1 });
                int currentCount = countOrdersOpen.Count;
                if (currentCount != previousCount)
                {
                    OpenOrderCount = currentCount;
                    NotifyStateChanged();
                }
                // prüf die Count von storniert orders 
                previousCount = 0;
                var countOrdersCanceled = await GetOrderCountByStatusId(new List<int>() { 8, 9 });
                currentCount = countOrdersCanceled.Count;
                if (currentCount != previousCount)
                {
                    CanceledReturnedOrderCount = currentCount;
                    NotifyStateChanged();
                }
                await Task.Delay(20000); // Wait for 20 seconds before checking again
            }
        }
        public async Task<ValidationResult> GetPaymentMethodsAsync()
        {
            if (DownloadedPaymentMethods.Count > 0)
            {
                return new ValidationResult { Result = true, Message = "Payment methods already loaded." };
            }
            try
            {
                var response = await _http.GetAsync("api/Orders/getPaymentMethods");
                if (!response.IsSuccessStatusCode)
                {
                    return new ValidationResult { Result = false, Message = "Failed to retrieve payment methods." };
                }
                DownloadedPaymentMethods = await response.Content.ReadFromJsonAsync<List<PaymentMethod>>() ?? [];
                return new ValidationResult { Result = true, Message = "Payment methods retrieved successfully." };
            }
            catch (Exception ex)
            {
                return new ValidationResult { Result = false, Message = ex.Message };
            }
        }
        public async Task<ValidationResult> GetShippingProvidersAsync()
        {
            if (DownloadedShippingProviders.Count > 0)
            {
                return new ValidationResult { Result = true, Message = "Shipping providers already loaded." };
            }
            try
            {
                var response = await _http.GetAsync("api/Orders/getShippingProvider");
                if (!response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ValidationResult>() ?? new ValidationResult { Result = false, Message = "Unknown error" };
                }
                var shippingProvider = await response.Content.ReadFromJsonAsync<List<ShippingProvider>>();

                if (shippingProvider == null)
                {
                    return new ValidationResult { Result = false, Message = "Unknown error" };
                }
                DownloadedShippingProviders = shippingProvider;
                return new ValidationResult { Result = true, Message = "Shipping providers retrieved successfully." };
            }
            catch (Exception ex)
            {
                return new ValidationResult { Result = false, Message = ex.Message };
            }
        }
        // local
        public void AddProductToLocal(List<Order> orders)
        {
            foreach (var order in orders)
            {
                if (!DownloadedOrders.Any(p => p.Id == order.Id))
                {
                    DownloadedOrders.Add(order);
                }
            }
        }
        public void AddProductToLocal(Order order)
        {
            if (!DownloadedOrders.Any(p => p.Id == order.Id))
            {
                DownloadedOrders.Add(order);
            }
        }
        public List<Order> GetOrdersByStatusLocal(string statusId, List<int>? excludeIds = null)
        {
            List<Order> searchedOrders = [];
            if (int.TryParse(statusId, out int Id) && Id > 0)
            {
                searchedOrders = DownloadedOrders
               .Where(o => o.StatusId.ToString() == statusId &&
               (excludeIds == null || !excludeIds.Contains(o.Id)))
               .ToList();
            }
            else
                searchedOrders = DownloadedOrders
                    .Where(o => (excludeIds == null || !excludeIds.Contains(o.Id)))
                    .ToList();


            return searchedOrders;
        }
        public List<int> getIdsFromOrdersLocal(List<Order> orders)
        {
            return orders.Select(o => o.Id).ToList();
        }
        public void Rest()
        {
            getItems = new GetItems<Order>() { PageSize = 5, CurrentPage = 0, AllItemsLoaded = false };
        }
    }
}
