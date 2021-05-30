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
        public ApplicationDbContext Context
        {
            get { return _context as ApplicationDbContext; }
        }

        public StockService(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<StockViewModel>> GetStockReport()
        {
            var entities = await Context.Stocks
                .Include(x => x.Product)
                .Include(x => x.Product.Prefix)
                .Include(x => x.Product.Suffix)
                .OrderBy(x => x.Product.Prefix.Name)
                    .ThenBy(x => x.Product.Name)
                .AsNoTracking()
                .Select(x => new StockViewModel
                {
                    Name = $"{((x.Product.Prefix != null) ? x.Product.Prefix.Name : "")} " +
                           $"{x.Product.Name} " +
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
            var stock = await Context.Stocks.FindAsync(receive.ProductId);

            if (stock != null)
            {
                stock.TotalReceive += receive.Quantity;
                stock.InStock = (stock.TotalReceive - stock.TotalReceiveReturn) - (stock.TotalIssue - stock.TotalIssueReturn);
                Context.Stocks.Update(stock);
                Console.WriteLine("Message : ==> Item updated in stock!");
            }
            else
            {
                //New stock item
                stock = new Stock
                {
                    Id = receive.ProductId,
                    ProductId = receive.ProductId,
                    TotalReceive = receive.Quantity,
                    TotalIssue = 0,
                    TotalIssueReturn = 0,
                    InStock = receive.Quantity,
                };

                Context.Stocks.Add(stock);
                Console.WriteLine("Message : ==> Item added to the stock!");
            }

            await Context.SaveChangesAsync();
        }

        //Issue Item from Stock as FIFO style
        public async Task<bool> IssueItem(Issue entity)
        {
            var stock = await Context.Stocks.FindAsync(entity.ProductId);

            //find early exipry date item and then issue form that
            var allReceive = Context.Receives.AsQueryable();

            var earlyExpiryRecieveItems = allReceive
                .Where(x => x.IsDelete == false &&
                        x.IsUse == false &&
                        x.ProductId == entity.ProductId
                )
                .OrderBy(x => x.ExpiryDate)
                .ToList();

            foreach (var receive in earlyExpiryRecieveItems)
            {
                if (entity.Quantity == 0)
                    break;

                //Create issue item without quantity
                var issue = new Issue
                {
                    ProductId = entity.ProductId,
                    IssueDate = entity.IssueDate,
                    Remarks = entity.Remarks,
                    IssueBy = entity.IssueBy,
                    Note = entity.Note,
                    PurchaseId = receive.Id,
                    Quantity = 0,
                };

                int availableQty = receive.Quantity - receive.UseQuantity;

                if (entity.Quantity >= availableQty)
                {
                    //for the loop runs
                    entity.Quantity -= availableQty;

                    //update receive
                    receive.UseQuantity = 0;
                    receive.IsUse = true;

                    //update issue
                    issue.Quantity += availableQty;

                    //update stock
                    stock.TotalIssue += availableQty;

                }
                else
                {
                    //update receive
                    receive.UseQuantity += entity.Quantity;

                    //update issue
                    issue.Quantity += entity.Quantity;

                    //update stock
                    stock.TotalIssue += entity.Quantity;

                    //for the loop runs
                    entity.Quantity = 0;
                }

                int receiveQty = stock.TotalReceive - stock.TotalReceiveReturn;
                int issueQty = stock.TotalIssue - stock.TotalIssueReturn;
                stock.InStock = receiveQty - issueQty;

                Context.Receives.Update(receive);
                Context.Issues.Add(issue);
                Context.Stocks.Update(stock);
                Context.SaveChanges();
            }

            return true;
        }
    }
}
