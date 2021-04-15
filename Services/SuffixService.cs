using InventoryApp.Data;
using InventoryApp.Models;
using InventoryApp.Serviceses;

namespace InventoryApp.Services
{
    public class SuffixService : Repository<Suffix, ApplicationDbContext>
    {
        public SuffixService(ApplicationDbContext context) : base(context)
        {
        }
    }
}
