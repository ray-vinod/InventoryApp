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
    public partial class SuffixCreate
    {
        //Local variables
        public Suffix suffix;
        public ElementReference firstInput;
        public bool getFocus = false;
        public string title = "Suffix-create";

        [Parameter] public Guid Id { get; set; }

        //Refresh to Prfix and suffix list in product create page
        [Inject] public NavigationManager NavigationManager { get; set; }
        [Inject] public AlertService AlertService { get; set; }
        [Inject] public SuffixService SuffixService { get; set; }
        [Inject] public ILogger<SuffixCreate> Logger { get; set; }
        [Inject] public UpdateService<Suffix> UpdateService { get; set; }


        protected override void OnInitialized()
        {
            suffix = new Suffix();

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
            suffix.Name = suffix.Name.ToUpper();

            //Find the records by Name
            var result = await SuffixService.GetItemsAsync(x => x.Name == suffix.Name);
            var isExist = result.FirstOrDefault();

            //Prevent duplication item
            if (isExist != null && suffix.Id != isExist.Id)
            {
                Logger.LogWarning("{0} is already exist!", suffix.Name);
                AlertService.AddMessage(new Alert(suffix.Name + AlertMessage.ExistInfo,
                    AlertType.Warning));

                getFocus = true;
                return;
            }

            if (suffix.Id == Guid.Empty)
            {
                //Create New Record
                var createEntity = await SuffixService.CreateAsync(suffix);

                if (createEntity != null)
                {
                    Logger.LogInformation("{0} is created successfully!", suffix.Name);

                    AlertService.AddMessage(new Alert(suffix.Name + AlertMessage.AddInfo,
                        AlertType.Success));

                    UpdateService.UpdatePage("suffix/index", null);
                }

                getFocus = true;
                suffix = new Suffix();
            }
            else
            {
                //No Change so do not perform db action
                if (suffix == isExist)
                {
                    AlertService.AddMessage(new Alert("There is no changes.", AlertType.Warning));
                    return;
                }

                //update record
                var isUpdate = await SuffixService.UpdateAsync(suffix);

                if (isUpdate != null)
                {
                    Logger.LogInformation("{0} is updated successfully!", suffix.Name);

                    AlertService.AddMessage(new Alert(suffix.Name + AlertMessage.UpdateInfo,
                        AlertType.Success));

                    UpdateService.UpdatePage("suffix/update", isUpdate);

                    NavigationManager.NavigateTo("/suffix/index", false);
                }
            }
        }

        private async Task FindEditAsync()
        {
            suffix = await SuffixService.GetByIdAsync(Id);
            NavigationManager.NavigateTo("/suffix/edit", false);
            title = "suffix-edit";
        }
    }
}