using System;
using System.ComponentModel.DataAnnotations;

namespace InventoryApp.Models
{
    public class SaleReturn : BaseEntity
    {
        public SaleReturn()
        {
        }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Return Date")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime ReturnDate { get; set; } = DateTime.Now.Date;

        [Required]
        [Display(Name = "Retrun By")]
        public string ReturneBy { get; set; }

        [Required]
        public int Quantity { get; set; } = 0;

        public string Remarks { get; set; }
        public Guid IssueId { get; set; }


        //Navigation property
        public Guid ProductId { get; set; }
        public virtual Product Product { get; set; }
    }
}
