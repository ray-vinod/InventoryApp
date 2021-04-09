namespace InventoryApp.Helpers
{
    public class PagingParameter
    {
        public static int PageNumber { get; set; } = 1;
        public static int CurrentPage { get; set; } = 1;
        public static int TotalPages { get; set; } = 0;
        public static int PageSize { get; set; } = 10;
    }
}
