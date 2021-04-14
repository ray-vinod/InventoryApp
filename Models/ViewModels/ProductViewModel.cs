using System;

namespace InventoryApp.Models.ViewModels
{
    public class ProductViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Group { get; set; }


        public static implicit operator ProductViewModel(Product product)
        {
            return new ProductViewModel
            {
                Id = product.Id,
                Name = product.Prefix?.Name + " " + product.Name + " " + product.Suffix?.Name,
                Group = $"{product.GroupName}",
            };
        }

        public static implicit operator Product(ProductViewModel vm)
        {
            return new Product
            {
                Id = vm.Id,
            };
        }

    }
}
