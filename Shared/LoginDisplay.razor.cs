using InventoryApp.Models;
using InventoryApp.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
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
                        UserState.Name = name;
                        UserState.ImagePath = imgSrc;

                        UserState.CallPageRefresh();
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