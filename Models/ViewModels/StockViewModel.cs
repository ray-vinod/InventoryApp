namespace InventoryApp.Models.ViewModels
{
    public class StockViewModel
    {
        public string Name { get; set; }
        public int Total_Receive { get; set; }
        public int Total_Receive_Return { get; set; }
        public int Total_Issue { get; set; }
        public int Total_Issue_Return { get; set; }
        public int In_Stock { get; set; }
    }
}
