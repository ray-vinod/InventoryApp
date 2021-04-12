using InventoryApp.Helpers;
using InventoryApp.Models;
using InventoryApp.Models.Enums;
using InventoryApp.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InventoryApp.Pages
{
    public partial class RoleIndex
    {
        public bool spinnerOnOff = true;
        public List<RoleViewModel> roleModels;

        List<PageUrl> urls = new List<PageUrl>();
        [Inject] private RoleManager<ApplicationRole> RoleManager { get; set; }
        [Inject] private AlertService AlertService { get; set; }
        [Inject] private ILogger<RoleIndex> Logger { get; set; }


        protected override async Task OnInitializedAsync()
        {
            roleModels = new List<RoleViewModel>();

            urls.Add(new PageUrl("role/create/", "Edit", "oi-pencil", "btn-outline-info ml-2"));
            urls.Add(new PageUrl(null, "Delete", "oi-trash", "btn-outline-danger ml-2"));

            await LoadDataAsync(PagingParameter.CurrentPage, null);
        }

        //EventCallback for paging navigation link
        public async Task SelectedPage(int page)
        {
            PagingParameter.CurrentPage = page;
            await LoadDataAsync(PagingParameter.CurrentPage, null);
        }

        private async Task OnSearchTexChanged(ChangeEventArgs e)
        {
            string searchText = e.Value.ToString();
            await LoadDataAsync(PagingParameter.CurrentPage, searchText);
        }

        private async Task LoadDataAsync(int page, string searchText)
        {
            await CallDataAsync(page, searchText);

            PagingParameter.TotalPages = PaginatedList<ApplicationRole>.TotalPage();

            if (PagingParameter.TotalPages == 0)
            {
                spinnerOnOff = false;
                if (searchText != null)
                {
                    PagingParameter.TotalPages = 1;
                    AlertService.AddMessage(new Alert("Record Notfound!", AlertType.Error), 2);
                }
            }

            if (roleModels.Count == 0 && PagingParameter.CurrentPage != 1)
            {
                PagingParameter.CurrentPage -= 1;
                await CallDataAsync(PagingParameter.CurrentPage, null);
            }
        }

        private async Task CallDataAsync(int page, string searchText)
        {
            roleModels.Clear();
            var roles = RoleManager.Roles;

            if (searchText != null)
            {
                roles = RoleManager.Roles.Where(x => x.Name.Contains(searchText));
            }

            roles.OrderBy(x => x.Name);

            var pagedList = await PaginatedList<ApplicationRole>.CreateAsync(roles, page,
                PagingParameter.PageSize);

            foreach (var role in pagedList)
            {
                roleModels.Add(new RoleViewModel
                {
                    Id = role.Id,
                    Name = role.Name,
                    Description = role.Description,
                });

            }

            spinnerOnOff = false;
            PagingParameter.TotalPages = roleModels.Count;
        }

        private async Task DeleteAsync(Guid id)
        {
            var role = await RoleManager.FindByIdAsync(id.ToString());

            if (role != null)
            {
                var result = await RoleManager.DeleteAsync(role);

                if (result.Succeeded)
                {
                    Logger.LogInformation("Role deleted successfully!");
                    AlertService.AddMessage(new Alert("Role deleted successfully!", AlertType.Error));
                    await LoadDataAsync(PagingParameter.CurrentPage, null);
                }
                else
                {
                    Logger.LogInformation("Role cannot deleted!");
                    AlertService.AddMessage(new Alert("Role cannot deleted!", AlertType.Error));
                }
            }
            else
            {
                Logger.LogInformation("Role Not Found!");
                AlertService.AddMessage(new Alert("Role Not Found!", AlertType.Error));
            }

        }

    }
}