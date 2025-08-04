namespace ManagementLabel.Model
{
    public class ColumnOption
    {

        public string Label { get; set; }
        public string Key { get; set; }
        public bool IsVisible { get; set; }

        public ColumnOption(string label, string key, bool isVisible)
        {
            Label = label;
            Key = key;
            IsVisible = isVisible;
        }
    }
}
