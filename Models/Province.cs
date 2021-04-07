namespace InventoryApp.Models
{
    public class Province
    {
        public int Id { get; set; }
        public string Name { get; set; }


        //Navigation Proterty
        public int CountryId { get; set; }
        public Country Country { get; set; }
    }
}
