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
    public class SaleReturnService : Repository<SaleReturn, ApplicationDbContext>
    {
        ApplicationDbContext ApplicationDbContext
        {
            get { return _context as ApplicationDbContext; }
        }

        public SaleReturnService(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<SaleReturnViewModel>> GetReport()
        {
            var entities = await ApplicationDbContext.SaleReturns
                .Include(p => p.Product)
                .Include(px => px.Product.Prefix)
                .Include(sx => sx.Product.Suffix)
                .OrderByDescending(x => x.ReturnDate)
                   .ThenBy(x => x.Product.Prefix.Name)
                   .ThenBy(x => x.Product.Name)
                .AsNoTracking()
                .Select(x => new SaleReturnViewModel
                {
                    Return_Date = x.ReturnDate.ToShortDateString(),
                    Name = string.Concat((x.Product.Prefix != null) ? x.Product.Prefix.Name : null, " ",
                                                    x.Product.Name, " ",
                                               (x.Product.Suffix != null) ? x.Product.Suffix.Name : null),
                    Qty = x.Quantity,
                    Retrun_By = x.ReturneBy,
                    Remarks = x.Remarks,
                }).ToListAsync();

            return entities;
        }

    }
}
