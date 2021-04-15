using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace InventoryApp.Models
{
    public class PurchaseReturn : BaseEntity
    {
        public PurchaseReturn()
        {
        }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Return Date")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime ReturnDate { get; set; } = DateTime.Now.Date;

        [Required]
        [Display(Name = "Return By")]
        public string ReturnBy { get; set; }

        [Required]
        public int Quantity { get; set; } = 0;

        public string Remarks { get; set; }
        public Guid PurchaseId { get; set; }



        //Navigation property
        public Guid ProductId { get; set; }
        public virtual Product Product { get; set; }

    }
}
