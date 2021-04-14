using InventoryApp.Helpers;
using InventoryApp.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace InventoryApp.Models
{
    public class Product : BaseEntity
    {
        [Required]
        [StringLength(maximumLength: 60, MinimumLength = 3)]
        public string Name { get; set; }

        [Required]
        public EntityGroup Group { get; set; }

        public string GroupName => EnumNameHelper.GetDisplayName(Group);


        //Navigation property
        public Guid? PrefixId { get; set; }
        public virtual Prefix Prefix { get; set; }

        public Guid? SuffixId { get; set; }
        public virtual Suffix Suffix { get; set; }

        public virtual List<Receive> Receives { get; set; }
        public virtual List<Issue> Issues { get; set; }
        public virtual Stock Stock { get; set; }
        public virtual List<PurchaseReturn> PurchaseReturns { get; set; }
        public virtual List<SaleReturn> SaleReturns { get; set; }


    }
}