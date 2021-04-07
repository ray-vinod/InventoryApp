using InventoryApp.Components;
using Microsoft.AspNetCore.Components;
using System;
using System.Linq;

namespace InventoryApp.Pages
{
    public partial class Upload
    {
        int selected;
        ComponentBase[] components = { new UploadFile(), new DeleteFile() };
        Type[] types => components.Select(c => c.GetType()).ToArray();

        RenderFragment GetRenderFragment(Type type)
        {
            return BuildRenderTree =>
            {
                BuildRenderTree.OpenComponent(0, type);
                BuildRenderTree.CloseComponent();
            };
        }

    }
}