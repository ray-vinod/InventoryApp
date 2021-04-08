using Microsoft.AspNetCore.Components;

namespace InventoryApp.Components
{
    public partial class Card
    {
        [Parameter] public RenderFragment ChildContent { get; set; }
        [Parameter] public string Title { get; set; }
        [Parameter] public string BgClass { get; set; } = string.Empty;
        [Parameter] public string CssClass { get; set; } = string.Empty;
        [Parameter] public string Style { get; set; } = string.Empty;
    }
}