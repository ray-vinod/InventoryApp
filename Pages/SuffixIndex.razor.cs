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
    public partial class SuffixIndex : IDisposable
    {
        //Local varialbels
        public List<Suffix> suffixes;
        public bool spinnerOnOff = true;
        List<PageUrl> urls = new List<PageUrl>();
        private bool isLock = false;

        [CascadingParameter] IModalService Modal { get; set; }
        [Inject] public SuffixService SuffixService { get; set; }

        //Refresh Product Create and Index page
        [Inject] public AlertService AlertService { get; set; }
        [Inject] public ILogger<SuffixIndex> Logger { get; set; }
        [Inject] public NavigationManager NavigationManager { get; set; }
        [Inject] public UpdateService<Suffix> UpdateService { get; set; }



        protected override async Task OnInitializedAsync()
        {
            suffixes = new List<Suffix>();

            urls.Add(new PageUrl("suffix/edit/", "Edit", "oi-pencil", "btn-outline-info ml-2"));
            urls.Add(new PageUrl(null, "Delete", "oi-trash", "btn-outline-danger ml-2"));

            UpdateService.OnUpdateRequested += PageUpdateHandler;

            await LoadData(PagingParameter.CurrentPage, null);
        }

        public async void PageUpdateHandler(Suffix suffix)
        {
            await InvokeAsync(async () =>
            {
                if (suffix != null)
                {
                    //find index and remove
                    int index = suffixes.FindIndex(x => x.Id == suffix.Id);
                    suffixes.RemoveAt(index);

                    //find new update entity from db
                    suffixes.Insert(index, suffix);

                    NavigationManager.NavigateTo("/suffix/index", true);
                }
                else
                {
                    if (!isLock)
                    {
                        while (isLock)
                        {
                            Logger.LogInformation("System is busy ...");
                            await Task.Delay(100);
                        }

                        await LoadData(PagingParameter.CurrentPage, null);
                    }
                }

                StateHasChanged();
            });
        }

        private async Task LoadData(int page, string searchText)
        {
            await CallData(page, searchText);

            PagingParameter.TotalPages = SuffixService.PageCount();

            if (PagingParameter.TotalPages == 0)
            {
                spinnerOnOff = false;
                if (searchText != null)
                {
                    PagingParameter.TotalPages = 1;
                    AlertService.AddMessage(new Alert("Record Notfound!", AlertType.Error), 2);
                }
            }

            if (suffixes.Count == 0 && PagingParameter.CurrentPage != 1)
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
            var suffix = await SuffixService.GetByIdAsync(id);

            var parameters = new ModalParameters();
            parameters.Add(nameof(DeleteConfirmModal.Message), suffix.Name);

            var options = new ModalOptions { HideCloseButton = true, DisableBackgroundCancel = true };

            var messageFrom = Modal.Show<DeleteConfirmModal>("Do you want to delete this?",
                parameters, options);
            var result = await messageFrom.Result;

            if (!result.Cancelled)
            {
                bool isDeleted = await SuffixService.DeleteAsync(id);

                if (isDeleted)
                {
                    Logger.LogInformation("{0} is deleted successfully!", suffix.Name);
                    AlertService.AddMessage(new Alert(suffix.Name + AlertMessage.DeleteInfo,
                        AlertType.Error));

                    UpdateService.UpdatePage();
                }
            }
        }

        private async Task CallData(int page, string searchText)
        {
            isLock = true;
            suffixes.Clear();
            Logger.LogInformation("Prefix list is loading!");

            if (searchText != null)
            {
                await foreach (var item in SuffixService.StreamListAsync(
                    page,
                    PagingParameter.PageSize,
                    filter: x => x.Name.Contains(searchText),
                    orderBy: x => x.OrderBy(x => x.Name)))
                {
                    suffixes.Add(item);
                    spinnerOnOff = false;
                    StateHasChanged();
                }
            }
            else
            {
                await foreach (var item in SuffixService.StreamListAsync(
                    page,
                    PagingParameter.PageSize,
                    orderBy: x => x.OrderBy(x => x.Name)))
                {
                    suffixes.Add(item);
                    spinnerOnOff = false;
                    StateHasChanged();
                }
            }

            isLock = false;
        }

        public void Dispose()
        {
            UpdateService.OnUpdateRequested -= PageUpdateHandler;
        }
    }
}