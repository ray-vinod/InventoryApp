using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace InventoryApp.Pages
{
    public partial class Admin
    {
        [CascadingParameter]
        protected Task<AuthenticationState> AuthenticationState { get; set; }

        [Inject] private NavigationManager PageNavigationManager { get; set; }
        [Inject] private ProtectedLocalStorage LocalStorage { get; set; }
        [Inject] private ILogger<Admin> Logger { get; set; }

        public bool auth = false;


        protected override async Task OnInitializedAsync()
        {
            var user = (await AuthenticationState).User;
            if (!user.Identity.IsAuthenticated)
            {
                auth = true;
                await Task.Delay(3000);
                await LocalStorage.SetAsync("returnUrlSecure", "/" + "dashboard");
                PageNavigationManager.NavigateTo("Identity/Account/Login", true);
                Logger.LogInformation("You are not loggedin, so redirected to login page!");
            }
            else
            {
                auth = false;
                PageNavigationManager.NavigateTo("/dashboard", true);
                Logger.LogInformation("You are welcome!");
            }
        }
    }
}