using InventoryApp.Data;
using InventoryApp.Models;
using InventoryApp.Models.ViewModels;
using InventoryApp.Serviceses;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InventoryApp.Services
{
    public class PurchaseReturnService : Repository<PurchaseReturn, ApplicationDbContext>
    {
        ApplicationDbContext ApplicationDbContext
        {
            get { return _context as ApplicationDbContext; }
        }

        public PurchaseReturnService(ApplicationDbContext context) : base(context)
        {

        }

        public async Task<List<PurchaseReturnViewModel>> GetReport()
        {
            var entities = await ApplicationDbContext.PurchaseReturns
                .Include(x => x.Product)
                .Include(x => x.Product.Prefix)
                .Include(x => x.Product.Suffix)
                .OrderByDescending(x => x.ReturnDate)
                    .ThenBy(x => x.Product.Prefix.Name)
                    .ThenBy(x => x.Product.Name)
                .Select(x => new PurchaseReturnViewModel
                {
                    Return_Date = x.ReturnDate.ToShortDateString(),
                    Name = string.Concat((x.Product.Prefix != null) ? x.Product.Prefix.Name : null, " ",
                                          x.Product.Name, " ",
                                         (x.Product.Suffix != null) ? x.Product.Suffix.Name : null),
                    Qty = x.Quantity,
                    Retrun_By = x.ReturnBy,
                    Remarks = x.Remarks,
                }).ToListAsync();

            return entities;
        }


    }
}
