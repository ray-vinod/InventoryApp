using InventoryApp.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.ProtectedBrowserStorage;
using System.Threading.Tasks;


namespace InventoryApp.Shared
{
    public partial class MainLoginDisplay
    {
        public string name, imgSrc;
        [Inject] private UserStateService UserState { get; set; }
        [Inject] public ProtectedSessionStorage Storage { get; set; }



        protected override async Task OnInitializedAsync()
        {
            if (UserState.Name == null)
            {
                var readName = await Storage.GetAsync<string>("name");
                var readImagePath = await Storage.GetAsync<string>("imgSrc");

                if (readImagePath.Success && readName.Success)
                {
                    UserState.Name = readName.Value;
                    UserState.ImagePath = readImagePath.Value;
                }
            }

            name = UserState.Name;
            imgSrc = UserState.ImagePath;

        }

    }
}