using System;

namespace InventoryApp.Models
{
    public class Stock : BaseEntity
    {
        public Stock()
        {
        }

        public int TotalReceive { get; set; }
        public int TotalReceiveReturn { get; set; }
        public int TotalIssue { get; set; }
        public int TotalIssueReturn { get; set; }
        public int InStock { get; set; }

        public Guid ProductId { get; set; }
        public virtual Product Product { get; set; }
    }
}
