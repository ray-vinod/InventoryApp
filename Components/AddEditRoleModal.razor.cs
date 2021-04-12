using Blazored.Modal;
using Blazored.Modal.Services;
using InventoryApp.Models.ViewModels;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InventoryApp.Components
{
    public partial class AddEditRoleModal
    {
        [CascadingParameter]
        BlazoredModalInstance BlazoredModal { get; set; }

        [Parameter]
        public string UserName { get; set; } = string.Empty;
        [Parameter]
        public List<UserRoleViewModel> Roles { get; set; }


        protected override void OnInitialized()
        {

        }

        async Task Yes()
        {
            await BlazoredModal.CloseAsync(ModalResult.Ok(Roles));
        }

        async Task No()
        {
            await BlazoredModal.CancelAsync();
        }

        private void Checked(string id)
        {
            var role = Roles.Find(x => x.RoleId == id);
            if (role.IsSelected)
            {
                role.IsSelected = false;
            }
            else
            {
                role.IsSelected = true;
            }

        }
    }
}