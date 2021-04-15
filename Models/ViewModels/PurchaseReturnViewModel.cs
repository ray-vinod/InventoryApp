using System.ComponentModel.DataAnnotations;

namespace InventoryApp.Models.ViewModels
{
    public class PurchaseReturnViewModel
    {
        public string Id { get; set; } = string.Empty;
        [Display(Name = "Return Date")]
        public string Return_Date { get; set; }
        public string Name { get; set; }
        public int Qty { get; set; }
        [Display(Name = "Return By")]
        public string Retrun_By { get; set; }
        public string Remarks { get; set; }


        public static implicit operator PurchaseReturnViewModel(PurchaseReturn x)
        {
            return new PurchaseReturnViewModel
            {
                Return_Date = x.ReturnDate.ToShortDateString(),
                Name = $"{x.Product.Prefix?.Name} {x.Product.Name} {x.Product.Suffix?.Name}",
                Qty = x.Quantity,
                Retrun_By = x.ReturnBy,
                Remarks = x.Remarks,
            };
        }

    }
}
