using InventoryApp.Data;
using InventoryApp.Models;
using InventoryApp.Serviceses;

namespace InventoryApp.Services
{
    public class PrefixService : Repository<Prefix, ApplicationDbContext>
    {
        public PrefixService(ApplicationDbContext context) : base(context)
        {
        }

    }
}
