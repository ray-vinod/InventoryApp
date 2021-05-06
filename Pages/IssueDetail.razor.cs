using Blazored.Modal;
using Blazored.Modal.Services;
using InventoryApp.Components;
using InventoryApp.Models;
using InventoryApp.Models.Enums;
using InventoryApp.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace InventoryApp.Pages
{
    public partial class IssueDetail
    {
        public Issue issue;
        public int avQty = 0;
        public bool load = false;
        public string title = "Issue-detail";

        [Parameter]
        public Guid Id { get; set; }

        [CascadingParameter]
        private IModalService Modal { get; set; }
        [Inject]
        private AlertService AlertService { get; set; }
        [Inject]
        private ILogger<ReceiveDetail> Logger { get; set; }
        [Inject]
        private IssueService IssueService { get; set; }
        [Inject]
        private NavigationManager NavigationManager { get; set; }
        [Inject]
        private UpdateService<UpdateModel> UpdateService { get; set; }



        protected override async Task OnInitializedAsync()
        {
            issue = new Issue();

            if (Id != Guid.Empty)
            {
                issue = await IssueService.GetByIdAsync(Id);
                load = true;

                if (issue.IsDelete != true && issue.IsUse != true)
                {
                    avQty = issue.Quantity - issue.ReturnQty;
                }

                NavigationManager.NavigateTo("/issue/detail", false);
            }
            else
            {
                load = true;
            }
        }

        private async Task CancellationRequest()
        {
            var parameters = new ModalParameters();

            parameters.Add(nameof(DeleteConfirmModal.Message), $"{issue.Product.Prefix?.Name } " +
                                                    $"{issue.Product.Name}" +
                                                    $"{issue.Product.Suffix?.Name}");
            parameters.Add(nameof(DeleteConfirmModal.ShowInput), true);

            var options = new ModalOptions
            {
                HideCloseButton = true,
                DisableBackgroundCancel = true,
            };

            var fromMessage = Modal.Show<DeleteConfirmModal>("Do you want to remove this item?",
                parameters, options);

            var result = await fromMessage.Result;
            if (!result.Cancelled)
            {
                issue.Note = result.Data.ToString().ToUpper();
                var isUpdateEntity = await IssueService.UpdateAsync(issue);

                if (isUpdateEntity != null)
                {
                    Logger.LogInformation("{0} request for cancel requested", issue.Product.Name);

                    AlertService.AddMessage(new Alert("Request for cancel entry has been sent!",
                        AlertType.Info));

                    UpdateService.UpdatePage();

                    NavigationManager.NavigateTo("/issue/index", true);
                }
            }
        }
    }
}