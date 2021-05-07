using InventoryApp.Data;
using InventoryApp.Models;
using InventoryApp.Models.ViewModels;
using InventoryApp.Serviceses;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InventoryApp.Services
{
    public class ReceiveService : Repository<Receive, ApplicationDbContext>
    {
        ApplicationDbContext ApplicationDbContext
        {
            get { return _context as ApplicationDbContext; }
        }

        public ReceiveService(ApplicationDbContext context) : base(context)
        {

        }

        public async Task<List<ReceiveReportViewModel>> GetReportData()
        {
            var entities = await ApplicationDbContext.Receives
                .Where(x => x.IsDelete != true)
                .AsNoTracking()
                .Include(x => x.Product)
                .Include(x => x.Product.Prefix)
                .Include(x => x.Product.Suffix)
                .OrderByDescending(x => x.ReceiveDate)
                    .ThenBy(x => x.Product.Prefix.Name)
                    .ThenBy(x => x.Product.Name)
                .AsNoTracking()
                .Select(x => new ReceiveReportViewModel
                {
                    Receive_Date = x.ReceiveDate,
                    Name = $"{((x.Product.Prefix != null) ? x.Product.Prefix.Name : "")} {x.Product.Name} " +
                           $"{((x.Product.Suffix != null) ? x.Product.Suffix.Name : "")}",
                    Batch = x.Batch,
                    Qty = x.Quantity,
                    Group = x.Product.GroupName,
                    Received_By = x.ReceiveBy,
                    Manufacture_Date = x.ManufactureDate,
                    Expiry_Date = x.ExpiryDate,
                    Remarks = x.Remarks
                }).ToListAsync();

            return entities;

        }

        public async Task<int> GetBatch(Guid producId)
        {
            var receives = ApplicationDbContext.Receives
                                      .Where(x => x.ProductId == producId)
                                      .AsQueryable();
            int batch = 0;

            if (receives.Any())
            {
                batch = await receives.MaxAsync(x => x.Batch);
            }

            return batch;
        }


    }
}
