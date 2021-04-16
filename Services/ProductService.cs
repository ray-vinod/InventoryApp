using InventoryApp.Data;
using InventoryApp.Models;
using InventoryApp.Serviceses;

namespace InventoryApp.Services
{
    public class ProductService : Repository<Product, ApplicationDbContext>
    {
        public ProductService(ApplicationDbContext context) : base(context)
        {
        }

    }
}
