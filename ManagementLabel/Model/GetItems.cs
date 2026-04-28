namespace ManagementLabel.Model
{
    public class GetItems<T>
    {
        public List<T> Items { get; set; } = [];
        public bool AllItemsLoaded { get; set; }
        public int CurrentPage { get; set; } = 0;
        public int PageSize { get; set; } = 9;
        public FilterOption? Filter { get; set; } = new();
    }
    public enum GetItemFilterType
    {
        None,
        Category,
        Custom,
        Supplier,
        LowStock,
        OnOffer
    }

    public class FilterOption
    {
        public int Id { get; set; } = 0;
        public GetItemFilterType Type { get; set; } = GetItemFilterType.None;
    }
}
