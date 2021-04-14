using System;
using System.ComponentModel.DataAnnotations;

namespace InventoryApp.Models
{
    public class Receive : BaseEntity
    {
        public Receive()
        {
        }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Manufacture Date")]
        public DateTime ManufactureDate { get; set; } = DateTime.Now.Date;

        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Expiry Date")]
        public DateTime ExpiryDate { get; set; } = DateTime.Now.Date;

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Recieve Date")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime ReceiveDate { get; set; } = DateTime.Now.Date;

        [Required]
        [Display(Name = "Receive By")]
        public string ReceiveBy { get; set; }

        [Required]
        public int Quantity { get; set; } = 0;

        public int UseQuantity { get; set; } = 0;

        public bool IsUse { get; set; } = false;

        public bool IsDelete { get; set; } = false;

        public int Batch { get; set; } = 0;
        public string Remarks { get; set; }
        public string Note { get; set; }


        //Navigation property
        //many-to-one
        public Guid ProductId { get; set; }
        public virtual Product Product { get; set; }
    }
}
