using InventoryApp.Models;
using InventoryApp.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.ProtectedBrowserStorage;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace InventoryApp.Shared
{
    public partial class LoginDisplay
    {
        public string name, imgSrc;

        [CascadingParameter]
        private Task<AuthenticationState> AuthenticationState { get; set; }
        [Inject]
        private UserManager<ApplicationUser> UserManager { get; set; }
        [Inject]
        private UserStateService UserState { get; set; }
        [Inject]
        public ProtectedSessionStorage Storage { get; set; }




        protected override async Task OnInitializedAsync()
        {
            if (UserState.Name == null)
            {
                var state = await AuthenticationState;
                if (state != null)
                {
                    var email = state.User.Identity.Name;
                    if (state.User.Identity.IsAuthenticated)
                    {
                        var appUser = await UserManager.FindByEmailAsync(email);

                        name = $"{appUser.FName} {appUser.LName}";
                        imgSrc = "/Images/" + appUser.ImagePath;

                        //For the loacal session storage for the use on hot reload page on Mainlogin display
                        await Storage.SetAsync("name", name);
                        await Storage.SetAsync("imgSrc", imgSrc);

                        //For the Mainlogin display and mainlogin displaymin
                        UserState.Name = name;
                        UserState.ImagePath = imgSrc;
                    }
                }
            }
            else
            {
                name = UserState.Name;
                imgSrc = UserState.ImagePath;
            }
        }



    }
}