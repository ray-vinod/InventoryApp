namespace InventoryApp.Helpers
{
    public class PageUrl
    {
        public PageUrl(string url, string buttonText, string icontClass, string buttonClass)
        {
            Url = url;
            ButtonClass = buttonClass;
            ButtonText = buttonText;
            IconClass = icontClass;
        }

        public string Url { get; set; } = string.Empty;
        public string ButtonClass { get; set; } = string.Empty;
        public string IconClass { get; set; } = string.Empty;
        public string ButtonText { get; set; } = string.Empty;

    }
}
