using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace InventoryApp.Models.ViewModels
{
    public class IssueViewModel
    {
        public Guid Id { get; set; }
        [Display(Name = "Issue Date")]
        public string IssueDate { get; set; }
        public string Name { get; set; }
        public int Qty { get; set; }
        [Display(Name = "Issue By")]
        public string IssueBy { get; set; }
        public string Note { get; set; }


        public static implicit operator IssueViewModel(Issue issue)
        {
            return new IssueViewModel
            {
                Id = issue.Id,
                IssueDate = issue.IssueDate.ToShortDateString(),

                Name = string.Concat((issue.Product.Prefix != null) ? issue.Product.Prefix.Name : null, " ",
                                      issue.Product.Name, " ",
                                     (issue.Product.Suffix != null) ? issue.Product.Suffix.Name : null),

                Qty = issue.Quantity,
                IssueBy = issue.IssueBy,
                Note = issue.Note,
            };
        }

        public static implicit operator Issue(IssueViewModel vm)
        {
            return new Issue
            {
                Id = vm.Id,
            };
        }

    }
}
