using InventoryApp.Models.Enums;
using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;

namespace InventoryApp.Models
{
    public class ApplicationUser : IdentityUser
    {
        public ApplicationUser() : base()
        {
        }

        [Required]
        [PersonalData]
        [Display(Name = "First Name")]
        public string FName { get; set; }

        [PersonalData]
        [Display(Name = "Middle Name")]
        public string MName { get; set; }

        [Required]
        [PersonalData]
        [Display(Name = "Last Name")]
        public string LName { get; set; }

        [PersonalData]
        public string City { get; set; }
        [PersonalData]
        public int CountryId { get; set; }
        [PersonalData]
        public int ProvinceId { get; set; }
        [PersonalData]
        public Gender Gender { get; set; }
        [PersonalData]
        public DateTime CreatedDate { get; internal set; } = DateTime.Now;
        [PersonalData]
        public string ImagePath { get; internal set; }

        //Navigation Properties
        public Country Country { get; set; }


    }
}
