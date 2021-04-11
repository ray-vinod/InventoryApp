using Blazored.Modal;
using Blazored.Modal.Services;
using InventoryApp.Components;
using InventoryApp.Models;
using InventoryApp.Models.Enums;
using InventoryApp.Models.ViewModels;
using InventoryApp.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InventoryApp.Pages
{
    public partial class UserIndex
    {
        private List<UserViewModel> users;
        private bool spinner = true;

        [Inject] private UserManager<ApplicationUser> UserManager { get; set; }
        [Inject] private RoleManager<ApplicationRole> RoleManager { get; set; }
        [Inject] private AlertService AlertService { get; set; }
        [Inject] private ILogger<UserIndex> Logger { get; set; }
        [CascadingParameter] IModalService Modal { get; set; }



        protected override async Task OnInitializedAsync()
        {
            await Task.Delay(0);
            users = new List<UserViewModel>();

            await LoadUser();

        }

        private async Task LoadUser()
        {
            foreach (var user in UserManager.Users)
            {
                //create user
                var model = new UserViewModel
                {
                    Id = user.Id,
                    FullName = $"{user.FName} {(user.MName ?? null)} {user.LName}",
                    Email = user.UserName,
                    ProfileImage = $"Images/{user.ImagePath}",
                    Roles = new List<UserRoleViewModel>(),
                };

                //create roles belongs to user => IsSelected =true
                foreach (var role in RoleManager.Roles)
                {
                    var userRole = new UserRoleViewModel
                    {
                        RoleId = role.Id,
                        RoleName = role.Name,
                    };

                    if (await UserManager.IsInRoleAsync(user, role.Name))
                    {
                        userRole.IsSelected = true;
                    }

                    model.Roles.Add(userRole);
                }

                users.Add(model);
            }

            spinner = false;

            Logger.LogInformation("User profile loading is completed!");
        }

        //final update made changes in db
        private async Task UpdateRole(UserViewModel user)
        {
            var selectedUser = await UserManager.FindByIdAsync(user.Id);
            var getRoles = await UserManager.GetRolesAsync(selectedUser);

            var result = await UserManager.RemoveFromRolesAsync(selectedUser, getRoles);
            if (!result.Succeeded)
            {
                //Cannot remove role from user.
                Logger.LogInformation("Cannot remove role from {}", selectedUser.Email);
                return;
            }

            result = await UserManager.AddToRolesAsync(selectedUser, user.Roles.
                Where(x => x.IsSelected).Select(r => r.RoleName));

            if (!result.Succeeded)
            {
                //Cannot add role to user.
                Logger.LogInformation("Cannot add role to {}", selectedUser.Email);
                return;
            }

            AlertService.AddMessage(new Alert("User role is updated!", AlertType.Success));
            await InvokeAsync(() => StateHasChanged());

        }

        //fuction for change value on check/uncheck checkbox
        private void Checked(string userId, string roleId)
        {
            var selectedUser = users.Find(x => x.Id == userId);
            var selectedRole = selectedUser.Roles.Find(x => x.RoleId == roleId);

            if (selectedRole.IsSelected)
            {
                selectedRole.IsSelected = false;
            }
            else
            {
                selectedRole.IsSelected = true;
            }
        }


        private async Task AddRemoveRole(UserViewModel user)
        {
            var parameter = new ModalParameters();
            parameter.Add(nameof(RoleModal.UserName), user.FullName);
            parameter.Add(nameof(RoleModal.Roles), user.Roles);

            var options = new ModalOptions
            {
                HideCloseButton = true,
                DisableBackgroundCancel = true,

            };

            var dialog = Modal.Show<RoleModal>("", parameter, options);
            var result = await dialog.Result;
            if (!result.Cancelled)
            {
                user.Roles = (List<UserRoleViewModel>)result.Data;

                await UpdateRole(user);
            }
        }
    }
}