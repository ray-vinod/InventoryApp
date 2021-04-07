using InventoryApp.Models;
using InventoryApp.Services;
using Microsoft.AspNetCore.Components;

namespace InventoryApp.Components
{
    public partial class ToastNotification
    {
        [Inject]
        public AlertService AlertService { get; set; }

        [Parameter]
        public string Possition { get; set; } = "top";



        protected override void OnInitialized()
        {
            AlertService.OnRefreshRequested += RefreshHandler;
        }

        protected async void RefreshHandler()
        {
            await InvokeAsync(() =>
            {
                StateHasChanged();
            });
        }

        public string GetAlertClass(Alert alert)
        {
            return string.Format("alert-{0}", alert.AlertType.ToString().ToLower());
        }

        public void Dispose()
        {
            AlertService.OnRefreshRequested -= RefreshHandler;
        }
    }
}