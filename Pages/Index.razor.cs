using InventoryApp.Models;
using InventoryApp.RefreshServices;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace InventoryApp.Pages
{
    public partial class Index : IDisposable
    {
        private List<CarouselItem> items;
        [Inject] private IWebHostEnvironment WebHostEnvironment { get; set; }
        [Inject] private ILogger<Index> Logger { get; set; }
        [Inject] public IndexRefreshService IndexRefreshService { get; set; }


        protected override async Task OnInitializedAsync()
        {
            await Task.Delay(0);
            IndexRefreshService.OnRefreshRequested += PageRefreshHandler;
            items = new List<CarouselItem>();
            CarouselItems();
        }

        private async void PageRefreshHandler()
        {
            await InvokeAsync(() =>
            {
                CarouselItems();
                StateHasChanged();
            });
        }

        private void CarouselItems()
        {
            items.Clear();

            var path = Path.Combine(WebHostEnvironment.WebRootPath, "Images", "CarouselImages");
            string[] files = Directory.GetFiles(path);

            if (files.Length != 0)
            {
                foreach (var file in files)
                {
                    string name = Path.GetFileNameWithoutExtension(file);
                    var src = Path.Combine("Images", "CarouselImages", Path.GetFileName(file));
                    var alt = string.Concat(name.ToUpper(), " SERVICE");
                    items.Add(new CarouselItem { Source = src, Caption = alt });
                }
            }

            Logger.LogInformation("Carousel file read completed!");
        }

        public void Dispose()
        {
            IndexRefreshService.OnRefreshRequested -= PageRefreshHandler;
        }
    }
}