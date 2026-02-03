namespace ManagementLabel.Model
{
    public class GetItems<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public bool AllItemsLoaded { get; set; }
        public int CurrentPage { get; set; } = 0;
        public int PageSize { get; set; } = 9;
        public int? Id { get; set; }
    }
}
