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
    public class StockService : Repository<Stock, ApplicationDbContext>
    {
        ApplicationDbContext ApplicationDbContext
        {
            get { return _context as ApplicationDbContext; }
        }

        public StockService(ApplicationDbContext context) : base(context)
        {
        }


        public async Task<List<StockViewModel>> GetStockReport()
        {
            var entities = await ApplicationDbContext.Stocks
                .Include(x => x.Product)
                .Include(x => x.Product.Prefix)
                .Include(x => x.Product.Suffix)
                .OrderBy(x => x.Product.Prefix.Name)
                    .ThenBy(x => x.Product.Name)
                .AsNoTracking()
                .Select(x => new StockViewModel
                {
                    Name = $"{((x.Product.Prefix != null) ? x.Product.Prefix.Name : "")} {x.Product.Name} " +
                           $"{((x.Product.Suffix != null) ? x.Product.Suffix.Name : "")}",
                    Total_Receive = x.TotalReceive,
                    Total_Receive_Return = x.TotalReceiveReturn,
                    Total_Issue = x.TotalIssue,
                    Total_Issue_Return = x.TotalIssueReturn,
                    In_Stock = x.InStock
                }).ToListAsync();

            return entities;
        }

        //Create and Update Stock row, when Item received in
        public async Task ReceiveItem(Receive receive)
        {
            var stock = await ApplicationDbContext.Stocks.FindAsync(receive.ProductId);

            if (stock != null)
            {
                stock.TotalReceive += receive.Quantity;
                stock.InStock = (stock.TotalReceive - stock.TotalReceiveReturn) - (stock.TotalIssue - stock.TotalIssueReturn);
                ApplicationDbContext.Stocks.Update(stock);
                Console.WriteLine("Message : ==> Stock updated!");
            }
            else
            {
                stock = new Stock
                {
                    Id = receive.ProductId,
                    ProductId = receive.ProductId,
                    TotalReceive = receive.Quantity,
                    TotalIssue = 0,
                    TotalIssueReturn = 0,
                    InStock = receive.Quantity,
                };

                ApplicationDbContext.Stocks.Add(stock);
                Console.WriteLine("Message : ==> Stock Added!");
            }

            await ApplicationDbContext.SaveChangesAsync();
        }


        //Issue Item from Stock as FIFO style
        public async Task<bool> IssueItem(Issue entity)
        {
            List<Receive> receives = new List<Receive>();
            List<Issue> issues = new List<Issue>();

            var stock = await ApplicationDbContext.Stocks.FindAsync(entity.ProductId);

            //find early exipry date item and then issue form that
            var earlyExpiryRecieveItems = await ApplicationDbContext.Receives
                .Where(x => x.IsDelete == false && x.IsUse == false && x.ProductId == entity.ProductId)
                .OrderBy(x => x.ExpiryDate)
                .ToListAsync();

            foreach (var rItem in earlyExpiryRecieveItems)
            {
                if (entity.Quantity == 0)
                {
                    break;
                }

                //Create issue item without quantity
                var issue = new Issue
                {
                    ProductId = entity.ProductId,
                    IssueDate = entity.IssueDate,
                    Remarks = entity.Remarks,
                    IssueBy = entity.IssueBy,
                    Note = entity.Note,
                    PurchaseId = rItem.Id,
                    Quantity = 0,
                };

                int availableQty = rItem.Quantity - rItem.UseQuantity;

                if (entity.Quantity >= availableQty)
                {
                    entity.Quantity -= availableQty;

                    rItem.UseQuantity = 0;
                    rItem.IsUse = true;

                    stock.TotalIssue += availableQty;
                    stock.InStock = (stock.TotalReceive - stock.TotalReceiveReturn) - (stock.TotalIssue - stock.TotalIssueReturn);

                    issue.Quantity += availableQty;
                }
                else
                {
                    rItem.UseQuantity += entity.Quantity;

                    stock.TotalIssue += entity.Quantity;
                    stock.InStock = (stock.TotalReceive - stock.TotalReceiveReturn) - (stock.TotalIssue - stock.TotalIssueReturn);

                    issue.Quantity += entity.Quantity;

                    entity.Quantity = 0;
                }

                //Create purchase item
                receives.Add(rItem);
                issues.Add(issue);
            }

            var tasks = new List<Task>();

            var rtask = Task.Run(() => ApplicationDbContext.Receives.UpdateRange(receives));
            tasks.Add(rtask);

            var itask = Task.Run(() => ApplicationDbContext.Issues.UpdateRange(issues));
            tasks.Add(itask);

            var stask = Task.Run(() => ApplicationDbContext.Stocks.Update(stock));
            tasks.Add(stask);

            await Task.WhenAll(tasks);

            await Task.Run(() => ApplicationDbContext.SaveChangesAsync());

            Console.WriteLine("Message : ==> Issued Item and managed stock!");

            return true;
        }


    }
}
