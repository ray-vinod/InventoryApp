using System.Collections.Generic;

namespace InventoryApp.Models
{
    public class Country
    {
        public int Id { get; set; }
        public string Name { get; set; }


        //Navigation Property
        public List<Province> Provinces { get; set; }
        public List<ApplicationUser> ApplicationUsers { get; set; }

    }
}
