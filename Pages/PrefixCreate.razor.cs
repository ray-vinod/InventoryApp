using InventoryApp.Models;
using InventoryApp.Models.Enums;
using InventoryApp.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace InventoryApp.Pages
{
    public partial class PrefixCreate
    {
        //Local variables
        public Prefix prefix;
        public ElementReference firstInput;
        public bool getFocus = false;
        public string title = "Prefix-create";

        [Parameter] public Guid Id { get; set; }

        //Refresh to Prfix and suffix list in product create page
        [Inject] public NavigationManager NavigationManager { get; set; }
        [Inject] public AlertService AlertService { get; set; }
        [Inject] public PrefixService PrefixService { get; set; }
        [Inject] public ILogger<PrefixCreate> Logger { get; set; }
        [Inject] public UpdateService<Prefix> UpdateService { get; set; }



        protected override void OnInitialized()
        {
            prefix = new Prefix();

        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender || getFocus)
            {
                await firstInput.FocusAsync();
            }
        }

        protected override async Task OnParametersSetAsync()
        {
            if (Id != Guid.Empty)
            {
                await FindEditAsync();
            }
        }

        private async Task AddEdit()
        {
            prefix.Name = prefix.Name.ToUpper();

            //Find the records by Name
            var result = await PrefixService.GetItemsAsync(x => x.Name == prefix.Name);
            var isExist = result.FirstOrDefault();

            //Prevent duplication item
            if (isExist != null && prefix.Id != isExist.Id) 
            {
                Logger.LogWarning("{0} is already exist!", prefix.Name);
                AlertService.AddMessage(new Alert(prefix.Name + AlertMessage.ExistInfo,
                    AlertType.Warning));

                getFocus = true;
                return;
            }

            if (prefix.Id == Guid.Empty)
            {
                //Create New Record
                var createEntity = await PrefixService.CreateAsync(prefix);

                if (createEntity != null)
                {
                    Logger.LogInformation("{0} is created successfully!", prefix.Name);

                    AlertService.AddMessage(new Alert(prefix.Name + AlertMessage.AddInfo,
                        AlertType.Success));

                    UpdateService.UpdatePage();
                }

                getFocus = true;
                prefix = new Prefix();
            }
            else
            {
                //No Change so do not perform db action
                if (prefix == isExist)
                {
                    AlertService.AddMessage(new Alert("There is no changes.", AlertType.Warning));
                    return;
                }

                //update record
                var isUpdate = await PrefixService.UpdateAsync(prefix);

                if (isUpdate != null)
                {
                    Logger.LogInformation("{0} is updated successfully!", prefix.Name);

                    AlertService.AddMessage(new Alert(prefix.Name + AlertMessage.UpdateInfo,
                        AlertType.Success));

                    //send id for the update page
                    UpdateService.UpdatePage(isUpdate);

                    NavigationManager.NavigateTo("/prefix/index",false);
                }
            }
        }

        private async Task FindEditAsync()
        {
            prefix = await PrefixService.GetByIdAsync(Id);
            NavigationManager.NavigateTo("/prefix/edit", false);
            title = "Prefix-edit";
        }

    }
}