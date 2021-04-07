using InventoryApp.Services;
using Microsoft.AspNetCore.Components;
using System;

namespace InventoryApp.Shared
{
    public partial class MainLoginDisplay : IDisposable
    {
        public string name, imgSrc;
        [Inject] private UserStateService UserState { get; set; }

        protected override void OnInitialized()
        {
            UserState.OnRefreshRequeste += PageRefreshHandler;

            name = UserState.Name;
            imgSrc = UserState.ImagePath;
        }

        private async void PageRefreshHandler()
        {
            await InvokeAsync(() =>
            {
                name = UserState.Name;
                imgSrc = UserState.ImagePath;
                StateHasChanged();
            });
        }

        public void Dispose()
        {
            UserState.OnRefreshRequeste -= PageRefreshHandler;
        }

    }
}