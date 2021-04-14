using System;
using System.ComponentModel.DataAnnotations;

namespace InventoryApp.Models
{
    public class Issue : BaseEntity
    {
        public Issue()
        {
        }

        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Issue Date")]
        public DateTime IssueDate { get; set; } = DateTime.Now.Date;

        [Required]
        public int Quantity { get; set; }
        public int ReturnQty { get; set; } = 0;

        [Required]
        [Display(Name = "Issue By")]
        public string IssueBy { get; set; } //User Name

        public string Remarks { get; set; }//Issue for like patient/ or ot use

        public Guid PurchaseId { get; set; }
        public string Note { get; set; }
        public bool IsDelete { get; set; } = false;
        public bool IsUse { get; set; } = false;


        //Navigation property
        //many-to-one
        public Guid ProductId { get; set; }
        public virtual Product Product { get; set; }

    }
}
