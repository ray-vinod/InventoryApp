using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;

namespace InventoryApp.Models
{
    public class ApplicationRole : IdentityRole
    {

        public ApplicationRole() : base()
        {
        }

        public ApplicationRole(string roleName) : base(roleName)
        {
        }

        public ApplicationRole(string roleName, string description) : base(roleName)
        {
            Description = description;
            CreateDate = DateTime.Today;
        }

        public string Description { get; set; }

        [DataType(DataType.Date)]
        public DateTime CreateDate { get; set; } = DateTime.Now;


    }
}
