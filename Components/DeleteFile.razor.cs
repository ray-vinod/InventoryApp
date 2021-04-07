using InventoryApp.RefreshServices;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using System;
using System.IO;
using System.Threading.Tasks;

namespace InventoryApp.Components
{
    public partial class DeleteFile : IDisposable
    {
        private string[] files;
        [Inject]
        private IWebHostEnvironment Env { get; set; }

        [Inject]
        public IndexRefreshService IndexRefreshService { get; set; }


        protected override void OnInitialized()
        {
            IndexRefreshService.OnRefreshRequested += PageRefreshHandler;

            GetFiles();
        }

        private async void PageRefreshHandler()
        {
            await InvokeAsync(() =>
            {
                GetFiles();
                StateHasChanged();
            });
        }

        private async Task Delete(string file)
        {
            File.Delete(file);

            IndexRefreshService.CallPageRefresh();
            await InvokeAsync(StateHasChanged);
        }

        private void GetFiles()
        {
            var path = $"{Env.WebRootPath}\\Images\\CarouselImages\\";
            files = Directory.GetFiles(path);
        }

        public void Dispose()
        {
            IndexRefreshService.OnRefreshRequested -= PageRefreshHandler;
        }

    }
}