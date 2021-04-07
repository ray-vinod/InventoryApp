using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using System.Net.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.JSInterop;
using InventoryApp;
using InventoryApp.Data;
using InventoryApp.Components;
using InventoryApp.Models;
using InventoryApp.Models.Enums;
using InventoryApp.Services;
using InventoryApp.Shared;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using System.IO;

namespace InventoryApp.Pages
{
    public partial class Index
    {
        private List<CarouselItem> items;
        [Inject] private IWebHostEnvironment WebHostEnvironment { get; set; }
        [Inject] private ILogger<Index> Logger { get; set; }



        protected override async Task OnInitializedAsync()
        {
            await Task.Delay(0);

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

    }
}