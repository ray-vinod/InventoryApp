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
    public partial class ReceiveDetail : ComponentBase
    {

        public Receive receive;
        public bool load = false;
        int lifeSpan = 0;
        int avQty = 0;

        [Parameter] public Guid Id { get; set; }

        [Inject] public ReceiveService ReceiveService { get; set; }
        [Inject] public NavigationManager NavigationManager { get; set; }
        [Inject] public AlertService AlertService { get; set; }
        [Inject] private ILogger<ReceiveDetail> Logger { get; set; }
        [Inject] public UpdateService<Receive> UpdateServiceIssue { get; set; }

        [CascadingParameter] private IModalService Modal { get; set; }



        protected override void OnInitialized()
        {
            receive = new Receive();

        }

        protected override async Task OnParametersSetAsync()
        {
            if (Id != Guid.Empty)
            {
                receive = await ReceiveService.GetByIdAsync(Id);

                lifeSpan = receive.ExpiryDate.Subtract(DateTime.Now.Date).Days;
                if (receive.IsUse != true && receive.IsDelete != true)
                {
                    avQty = receive.Quantity - receive.UseQuantity;
                }

                load = true;
                NavigationManager.NavigateTo("/receive/detail", false);
            }
            else
            {
                load = true;
                receive.IsUse = true;
            }
        }

        //Request for remove from list which is aproved by the authorize person
        private async Task CancellationRequest()
        {
            var parameters = new ModalParameters();

            parameters.Add(nameof(DeleteConfirmModal.Message),
                $"{((receive.Product.Prefix != null) ? receive.Product.Prefix.Name : "")} " +
                $"{receive.Product.Name} " +
                $"{((receive.Product.Suffix != null) ? receive.Product.Suffix.Name : "")}");

            parameters.Add(nameof(DeleteConfirmModal.ShowInput), true);

            var options = new ModalOptions
            {
                HideCloseButton = true,
                DisableBackgroundCancel = true,
            };

            var fromMessage = Modal.Show<DeleteConfirmModal>("Do you want to remove this item?", parameters, options);
            var result = await fromMessage.Result;
            if (!result.Cancelled)
            {
                //Update Note Field for receive item and send to admin for the action
                receive.Note = result.Data.ToString().ToUpper();
                var isUpdateEntity = await ReceiveService.UpdateAsync(receive);

                if (isUpdateEntity != null)
                {
                    Logger.LogInformation("{0} request for cancel requested", receive.Product.Name);

                    AlertService.AddMessage(new Alert("Request for cancel entry has been sent!",
                        AlertType.Info));

                    UpdateServiceIssue.UpdatePage(isUpdateEntity);
                    NavigationManager.NavigateTo("/receive/index", false);
                }
            }
        }
    }
}