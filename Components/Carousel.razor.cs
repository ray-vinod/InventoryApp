using InventoryApp.Models;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace InventoryApp.Components
{
    public partial class Carousel
    {
        int currentPossition;
        CancellationTokenSource cts;
        CancellationToken ct;

        [Parameter]
        public bool Start { get; set; }
        [Parameter]
        public List<CarouselItem> Items { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await Task.Delay(0);
            cts = new CancellationTokenSource();
            ct = new CancellationToken();

            //await InitCarousel();
        }

        private async Task InitCarousel()
        {
            if (!Start)
            {
                cts.Cancel();
            }

            while (!cts.IsCancellationRequested)
            {
                await Task.Delay(3500, ct);
                currentPossition++;

                await InvokeAsync(() => StateHasChanged());
            }
        }

        private async Task Manually(bool backwards)
        {
            if (!cts.IsCancellationRequested)
            {
                cts.Cancel();
            }

            if (backwards)
            {
                currentPossition--;
            }
            else
            {
                currentPossition++;
            }

            await Task.Delay(0);
            cts = new CancellationTokenSource();
        }
    }
}