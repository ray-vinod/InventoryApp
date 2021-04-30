namespace InventoryApp.Models
{
    public class UpdateModel
    {
        public Prefix Prefix { get; set; }
        public Suffix Suffix { get; set; }
        public Product Product { get; set; }
        public Receive Receive { get; set; }
        public Issue Issue { get; set; }
        public Stock Stock { get; set; }
    }
}
