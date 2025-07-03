using ManagementLabel.Components.Pages;
using ManagementLabel.Model;

namespace ManagementLabel.Components.OrderF
{
    public class OrderService(HttpClient http)
    {
        private readonly HttpClient _http = http;
        public List<Order> DownloadedOrders { get; private set; } = [];
        public List<OrderStatus> OrderStatusList { get; private set; } = [];

        public event Action? OnChange;
        public void NotifyStateChanged() => OnChange?.Invoke();

        public void InitializeAsync()
        {
            _ = RefreshCountOpenOrders();
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
                var orderStatuses = await response.Content.ReadFromJsonAsync<List<OrderStatus>>() ?? new List<OrderStatus>();
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
        public async Task<ValidationResult> UpdateOrder(Order order)
        {
            try
            {
                var response = await _http.PutAsJsonAsync($"api/Orders/updateOrder", order);
                if (!response.IsSuccessStatusCode)
                {
                    return new ValidationResult { Result = false, Message = "Failed to update Order Status." };
                }
                return new ValidationResult { Result = true, Message = "Order Status updated successfully." };
            }
            catch (Exception ex)
            {
                return new ValidationResult { Result = false, Message = ex.Message };
            }
        }
        public int OpenOrderCount = 0;
        public async Task<OrdersCount> GetOrderCountByStatusId(int statusId)
        {
            try
            {
                var response = await _http.GetAsync($"api/Orders/getOrderCountByStatusId{statusId}");
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
                int previousCount = 0;
                var countOrdersOpen = await GetOrderCountByStatusId(1);
                int currentCount = countOrdersOpen.Count;
                if (currentCount != previousCount)
                {
                    OpenOrderCount = currentCount;
                    NotifyStateChanged();
                }
                await Task.Delay(20000); // Wait for 20 seconds before checking again
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
        public bool IsEditedOrder(Order currentOrder, Order editedOrder)
        {
            if (currentOrder == null || editedOrder == null)
            {
                return false;
            }
            // Compare properties of currentOrder and editedOrder
            return currentOrder.OrderDate != editedOrder.OrderDate ||
                   currentOrder.DeliveryAddressId != editedOrder.DeliveryAddressId ||
                   currentOrder.PaymentMethodId != editedOrder.PaymentMethodId ||
                   currentOrder.TotalPrice != editedOrder.TotalPrice ||
                   currentOrder.StatusId != editedOrder.StatusId ||
                   currentOrder.Notes != editedOrder.Notes;
        }
        public void Rest()
        {
            getItems = new GetItems<Order>() { PageSize = 5, CurrentPage = 0, AllItemsLoaded = false };
        }
    }
}
