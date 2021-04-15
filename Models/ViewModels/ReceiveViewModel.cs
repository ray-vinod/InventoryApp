using System;
using System.ComponentModel.DataAnnotations;

namespace InventoryApp.Models.ViewModels
{
    public class ReceiveViewModel
    {
        public Guid Id { get; set; }
        [Display(Name = "Receive Date")]
        public string ReceiveDate { get; set; }
        public string Name { get; set; }
        public int Qty { get; set; }
        [Display(Name = "Receive By")]
        public string ReceiveBy { get; set; }
        public int LifeSpan { get; set; }
        public bool IsUsed { get; set; } = false;

        public string Note { get; set; }

        public static implicit operator ReceiveViewModel(Receive receive)
        {
            return new ReceiveViewModel
            {
                Id = receive.Id,
                Name = $"{receive.Product.Prefix?.Name} {receive.Product.Name} " +
                        $"{receive.Product.Suffix?.Name}",
                ReceiveDate = receive.ReceiveDate.ToShortDateString(),
                Qty = receive.Quantity,
                ReceiveBy = receive.ReceiveBy,
                LifeSpan = receive.ExpiryDate.Subtract(DateTime.Now.Date).Days,
                IsUsed = receive.IsUse,
                Note = receive.Note,
            };
        }

        public static implicit operator Receive(ReceiveViewModel vm)
        {
            return new Receive
            {
                Id = vm.Id
            };
        }

    }
}
