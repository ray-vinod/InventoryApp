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
    public class IssueService : Repository<Issue, ApplicationDbContext>
    {
        ApplicationDbContext ApplicationDbContext
        {
            get { return _context as ApplicationDbContext; }
        }

        public IssueService(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<IssueReportViewModel>> GetReport()
        {
            var issues = await ApplicationDbContext.Issues
                .Include(x => x.Product)
                .Include(x => x.Product.Prefix)
                .Include(x => x.Product.Suffix)
                .OrderBy(x => x.Product.Prefix.Name)
                     .ThenBy(x => x.Product.Name)
                     .ThenByDescending(x => x.IssueDate)
                .AsNoTracking()
                .Select(x => new IssueReportViewModel
                {
                    Issue_Date = x.IssueDate.ToShortDateString(),
                    Name = string.Concat((x.Product.Prefix != null) ? x.Product.Prefix.Name : null, " ",
                                          x.Product.Name, " ",
                                         (x.Product.Suffix != null) ? x.Product.Suffix.Name : null),
                    Qty = x.Quantity,
                    Issue_By = x.IssueBy,
                    Remarks = x.Remarks,
                }).ToListAsync();

            return issues;
        }


    }
}
