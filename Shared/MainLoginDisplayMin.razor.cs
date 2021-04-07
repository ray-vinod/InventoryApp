using InventoryApp.Services;
using Microsoft.AspNetCore.Components;
using System;

namespace InventoryApp.Shared
{
    public partial class MainLoginDisplayMin : IDisposable
    {
        public string name;
        [Inject] private UserStateService UserState { get; set; }


        protected override void OnInitialized()
        {
            UserState.OnRefreshRequeste += PageRefreshHandler;
            name = UserState.Name;
        }

        private async void PageRefreshHandler()
        {
            await InvokeAsync(() =>
            {
                name = UserState.Name;
                StateHasChanged();
            });
        }

        public void Dispose()
        {
            UserState.OnRefreshRequeste -= PageRefreshHandler;
        }

    }
}