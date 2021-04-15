using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace InventoryApp.Models
{
    public class Prefix : BaseEntity
    {
        [Required]
        [StringLength(maximumLength: 60, MinimumLength = 3)]
        public string Name { get; set; }


        //Navigation Properties
        public virtual List<Product> Products { get; set; }
    }
}
