using Blazored.Modal;
using Blazored.Modal.Services;
using InventoryApp.Components;
using InventoryApp.Helpers;
using InventoryApp.Models;
using InventoryApp.Models.Enums;
using InventoryApp.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InventoryApp.Pages
{
    public partial class PrefixIndex
    {
        //Local varialbels
        public List<Prefix> prefixes;
        public bool spinnerOnOff = true;
        List<PageUrl> urls = new List<PageUrl>();

        [CascadingParameter] IModalService Modal { get; set; }
        [Inject] public PrefixService PrefixService { get; set; }

        //Refresh Product Create and Index page
        [Inject] public AlertService AlertService { get; set; }
        [Inject] public ILogger<PrefixIndex> Logger { get; set; }
        [Inject] public NavigationManager NavigationManager { get; set; }


        protected override async Task OnInitializedAsync()
        {
            prefixes = new List<Prefix>();

            urls.Add(new PageUrl("prefix/edit/", "Edit", "oi-pencil", "btn-outline-info ml-2"));
            urls.Add(new PageUrl(null, "Delete", "oi-trash", "btn-outline-danger ml-2"));

            await LoadData(PagingParameter.CurrentPage, null);
        }

        private async Task LoadData(int page, string searchText)
        {
            await CallData(page, searchText);

            PagingParameter.TotalPages = PrefixService.PageCount();

            if (PagingParameter.TotalPages == 0)
            {
                spinnerOnOff = false;
                if (searchText != null)
                {
                    PagingParameter.TotalPages = 1;
                    AlertService.AddMessage(new Alert("Record Notfound!", AlertType.Error), 2);
                }
            }

            if (prefixes.Count == 0 && PagingParameter.CurrentPage != 1)
            {
                PagingParameter.CurrentPage -= 1;
                await CallData(PagingParameter.CurrentPage, null);
            }
        }

        //EventCallback for paging navigation link
        public async Task SelectedPage(int page)
        {
            PagingParameter.CurrentPage = page;
            await LoadData(PagingParameter.CurrentPage, null);
        }

        private async Task OnSearchTexChanged(ChangeEventArgs e)
        {
            string searchText = e.Value.ToString().ToUpper();
            await LoadData(PagingParameter.CurrentPage, searchText);
        }

        public async Task Delete(Guid id)
        {
            var prefix = await PrefixService.GetByIdAsync(id);

            var parameters = new ModalParameters();
            parameters.Add(nameof(DeleteConfirmModal.Message), prefix.Name);

            var options = new ModalOptions { HideCloseButton = true, DisableBackgroundCancel = true };

            var messageFrom = Modal.Show<DeleteConfirmModal>("Do you want to delete this?",
                parameters, options);
            var result = await messageFrom.Result;

            if (!result.Cancelled)
            {
                bool isDeleted = await PrefixService.DeleteAsync(id);

                if (isDeleted)
                {
                    Logger.LogInformation("{0} is deleted successfully!", prefix.Name);
                    AlertService.AddMessage(new Alert(prefix.Name + AlertMessage.DeleteInfo,
                        AlertType.Error));

                    await LoadData(PagingParameter.CurrentPage,null);

                }
            }
        }

        private async Task CallData(int page, string searchText)
        {
            prefixes = new List<Prefix>();
            Logger.LogInformation("Prefix list is loading!");

            if (searchText != null)
            {
                await foreach (var item in PrefixService.StreamListAsync(
                    page,
                    PagingParameter.PageSize,
                    filter: x => x.Name.Contains(searchText),
                    orderBy: x => x.OrderBy(x => x.Name)))
                {
                    prefixes.Add(item);
                    spinnerOnOff = false;
                    StateHasChanged();
                }
            }
            else
            {
                await foreach (var item in PrefixService.StreamListAsync(
                    page,
                    PagingParameter.PageSize,
                    orderBy: x => x.OrderBy(x => x.Name)))
                {
                    prefixes.Add(item);
                    spinnerOnOff = false;
                    StateHasChanged();
                }
            }
        }
    }
}