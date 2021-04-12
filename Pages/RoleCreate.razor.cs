using InventoryApp.Models;
using InventoryApp.Models.Enums;
using InventoryApp.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace InventoryApp.Pages
{
    public partial class RoleCreate
    {
        public RoleViewModel model;
        private ApplicationRole role;
        public string title = "Role-create";

        [Parameter] public string Id { get; set; }
        [Inject] private RoleManager<ApplicationRole> RoleManager { get; set; }
        [Inject] private AlertService AlertService { get; set; }
        [Inject] public NavigationManager NavigationManager { get; set; }
        [Inject] private ILogger<RoleIndex> Logger { get; set; }



        protected override void OnInitialized()
        {
            model = new RoleViewModel();
            role = new ApplicationRole();

        }

        protected override async Task OnParametersSetAsync()
        {
            if (Id != null)
            {
                await Edit();
            }
        }

        private async Task AddEdit()
        {
            //add new role
            if (model.Id == null)
            {
                //Find role
                if (!await IsExist(model.Name))
                {
                    role.Name = model.Name;
                    role.Description = model.Description;

                    var result = await RoleManager.CreateAsync(role);

                    if (result.Succeeded)
                    {
                        Logger.LogInformation("{0} {1}", role.Name, AlertMessage.AddInfo);
                        AlertService.AddMessage(new Alert(role.Name + AlertMessage.AddInfo,
                            AlertType.Success));
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            Logger.LogError("", error.Description);
                            AlertService.AddMessage(new Alert(error.Description, AlertType.Error));
                        }

                        return;
                    }

                    role = new ApplicationRole();
                }
                else
                {
                    Logger.LogInformation("The role is already exist!");
                    AlertService.AddMessage(new Alert(role.Name + AlertMessage.ExistInfo, AlertType.Warning));
                    return;
                }

                model = new RoleViewModel();

            }
            else
            {
                //update role
                if (!await IsExist(model.Name))
                {
                    //Edit Role
                    role.Name = model.Name;
                    role.Description = model.Description;

                    var result = await RoleManager.UpdateAsync(role);

                    if (result.Succeeded)
                    {
                        Logger.LogInformation("{0} {1}", role.Name, AlertMessage.UpdateInfo);
                        AlertService.AddMessage(new Alert(role.Name + AlertMessage.UpdateInfo, AlertType.Success));
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            Logger.LogError("", error.Description);
                            AlertService.AddMessage(new Alert(error.Description, AlertType.Error));
                        }

                        return;
                    }

                    role = new ApplicationRole();
                }
                else
                {
                    Logger.LogInformation("The role is already exist!");
                    AlertService.AddMessage(new Alert(model.Name + AlertMessage.ExistInfo, AlertType.Warning));
                    return;
                }

                model = new RoleViewModel();
            }
        }

        private async Task Edit()
        {
            title = "Role-edit";

            role = await RoleManager.FindByIdAsync(Id.ToString());

            model.Id = role.Id;
            model.Name = role.Name;
            model.Description = role.Description;
        }

        private async Task<bool> IsExist(string name)
        {
            bool isExist = false;
            var result = await RoleManager.FindByNameAsync(name);

            if (result != null)
            {
                isExist = true;
            }

            return isExist;
        }


    }
}