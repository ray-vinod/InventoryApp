using System.Collections.Generic;

namespace InventoryApp.Models.ViewModels
{
    public class UserViewModel
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string ProfileImage { get; set; }

        public List<UserRoleViewModel> Roles { get; set; }

    }
}
