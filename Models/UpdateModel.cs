namespace InventoryApp.Models
{
    public class UpdateModel
    {
        public Prefix Prefix { get; set; }
        public Suffix Suffix { get; set; }
        public Product Product { get; set; }
        public Receive Receive { get; set; }
        public PurchaseReturn PurchaseReturn { get; set; }
        public Issue Issue { get; set; }
        public SaleReturn SaleReturn { get; set; }
        public Stock Stock { get; set; }

    }
}
