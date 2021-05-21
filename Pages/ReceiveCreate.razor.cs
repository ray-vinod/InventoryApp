using InventoryApp.Models;
using InventoryApp.Models.Enums;
using InventoryApp.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InventoryApp.Pages
{
    public partial class ReceiveCreate : ComponentBase, IDisposable
    {
        public List<Product> products;
        public Receive receive;
        public int avQty;
        public string oldRemarks;
        public string oldUser;
        public int oldQuantity;
        public bool IsDisable = false;
        public bool IsEdit = false;
        public bool IsTrue = false;
        public string title = "Receive-create";
        private bool isLock = false;

        [Parameter]
        public Guid Id { get; set; }

        [Inject]
        private UpdateService<UpdateModel> UpdateService { get; set; }
        [Inject]
        private NavigationManager NavigationManager { get; set; }
        [Inject]
        private AlertService AlertService { get; set; }
        [Inject]
        private ReceiveService ReceiveService { get; set; }
        [Inject]
        public ProductService ProductService { get; set; }
        [Inject]
        public UserStateService UserStateService { get; set; }
        [Inject]
        public StockService StockService { get; set; }
        [Inject]
        public PurchaseReturnService PurchaseReturnService { get; set; }
        [Inject]
        public ILogger<ReceiveCreate> Logger { get; set; }



        Product ProductSelection
        {
            get => receive.Product;
            set
            {
                receive.Product = value;
                if (value != null)
                    ChangeHandler(value).ConfigureAwait(false);

                StateHasChanged();
            }
        }

        protected override async Task OnInitializedAsync()
        {
            receive = new Receive();

            UpdateService.OnUpdateRequested += PageUpdateHandler;

            await LoadProduct();

            ReadUserName();
        }

        protected override async Task OnParametersSetAsync()
        {
            if (Id != Guid.Empty)
            {
                string qery = GetQueryParam("q");

                if (qery != null)
                {
                    await Edit();
                    IsEdit = true;
                    IsTrue = IsEdit;
                    title = "Receive-edit";
                }
                else
                {
                    await Return();
                    IsDisable = true;
                    IsTrue = IsDisable;
                    title = "Receive-return";
                }
            }
        }

        private async Task LoadProduct()
        {
            isLock = true;

            products = (List<Product>)await ProductService.GetItemsAsync(
                orderBy: o => o.OrderBy(x => x.Name),
                includeProperties: "Prefix,Suffix");

            isLock = false;
        }

        //Product list auto refresh
        private async void PageUpdateHandler(string property, UpdateModel model)
        {
            await InvokeAsync(async () =>
            {
                if (model != null)
                {
                    if (model.Prefix != null)
                        NavigationManager.NavigateTo("/receive/create", true);

                    if (model.Suffix != null)
                        NavigationManager.NavigateTo("/receive/create", true);

                    if (model.Product != null)
                    {
                        int index = products.FindIndex(x => x.Id == model.Product.Id);
                        products.RemoveAt(index);
                        products.Insert(index, model.Product);
                    }
                }
                else
                {
                    if (property != null)
                    {
                        foreach (var load in property.Split(new char[] { ',' },
                            StringSplitOptions.RemoveEmptyEntries))
                        {
                            if (!isLock)
                            {
                                while (isLock)
                                {
                                    Logger.LogInformation("System is busy ...");
                                    await Task.Delay(100);
                                }

                                if (load == "product/index")
                                    await LoadProduct();
                            }
                        }
                    }
                }

                StateHasChanged();
            });
        }

        private void ReadUserName()
        {
            var result = UserStateService.Name;
            {
                receive.ReceiveBy = result;
            }

            if (receive.Remarks == null)
            {
                receive.Remarks = "From Pharmacy";
            }
        }

        private async Task AddEditReturn()
        {
            if (IsDisable)
            {
                //Return Purchase Item
                if (avQty >= receive.Quantity)
                {
                    //Stock
                    var stock = await StockService.GetByIdAsync(receive.ProductId);

                    //Purchase Return
                    var purchaseReturn = new PurchaseReturn
                    {
                        ProductId = receive.ProductId,
                        Quantity = receive.Quantity,
                        Remarks = receive.Remarks.ToUpper(),
                        ReturnBy = receive.ReceiveBy,
                        PurchaseId = receive.Id,
                    };

                    //Create Purchase return
                    await PurchaseReturnService.CreateAsync(purchaseReturn);
                    Logger.LogInformation("Purchase retrun inserted");

                    //Stock update
                    stock.TotalReceiveReturn += receive.Quantity;
                    int receiveQty = stock.TotalReceive - stock.TotalReceiveReturn;
                    int issueQty = stock.TotalIssue - stock.TotalIssueReturn;
                    stock.InStock = receiveQty - issueQty;
                    await StockService.UpdateAsync(stock);
                    Logger.LogInformation("Stock updated");

                    //Receive --> from old values
                    receive.UseQuantity += receive.Quantity;
                    receive.Quantity = oldQuantity;
                    receive.Remarks = oldRemarks;
                    receive.ReceiveBy = oldUser;

                    if (receive.Quantity == receive.UseQuantity)
                    {
                        receive.IsUse = true;
                        receive.UseQuantity = 0;
                    }

                    await ReceiveService.UpdateAsync(receive);
                    Logger.LogInformation("Receive table updated");
                    Logger.LogInformation("Purchase Return task completed!");

                    UpdateService.UpdatePage(entity: new UpdateModel { PurchaseReturn = purchaseReturn });

                    AlertService.AddMessage(new Alert($"{receive.Product.Name} add to purchase return list", AlertType.Success));

                    NavigationManager.NavigateTo("/receive/index", false);
                }
                else
                {
                    Logger.LogInformation("Quantity is not available as you want!");
                    AlertService.AddMessage(new Alert("Quantity is not available as you want!", AlertType.Error));
                    return;
                }

                IsDisable = false;
                receive = new Receive();
                ReadUserName();
            }
            else
            {
                if (receive.Id == Guid.Empty)
                {
                    //Create Receive Item
                    if (receive.Product == null && receive.Quantity == 0)
                    {
                        return;
                    }

                    receive.Remarks = receive.Remarks.ToUpper();
                    receive.ProductId = receive.Product.Id;
                    receive.Product = null;

                    await ReceiveService.CreateAsync(receive);

                    await StockService.ReceiveItem(receive);

                    Logger.LogInformation("Item received and updated stock!");

                    //Refresh page list
                    UpdateService.UpdatePage("create", new UpdateModel { Receive = receive });

                    AlertService.AddMessage(new Alert(receive.Product.Name + AlertMessage.AddInfo, AlertType.Success));

                    receive = new Receive();
                    ReadUserName();
                }
                else
                {
                    receive.ReceiveDate = receive.ReceiveDate;
                    receive.ManufactureDate = receive.ManufactureDate;
                    receive.ExpiryDate = receive.ExpiryDate;

                    var isUpdate = await ReceiveService.UpdateAsync(receive);

                    Logger.LogInformation($"{isUpdate.Product.Name} is updated!");

                    AlertService.AddMessage(new Alert(isUpdate.Product.Name + AlertMessage.UpdateInfo, AlertType.Success));

                    UpdateService.UpdatePage(entity: new UpdateModel { Receive = isUpdate });

                    IsEdit = false;
                    receive = new Receive();
                    ReadUserName();
                }
            }
        }

        //Search function
        public async Task<IEnumerable<Product>> SearchAsync(string searchText)
        {
            var p = await Task.FromResult(products.Where(p => p.Name.Contains(searchText.ToUpper())));
            return p;
        }

        private async Task ChangeHandler(Product product)
        {
            int getbatch = await ReceiveService.GetBatch(product.Id);
            receive.Batch = getbatch + 1;

            StateHasChanged();
        }

        private async Task Return()
        {
            receive = await ReceiveService.GetByIdAsync(Id);

            avQty = receive.Quantity - receive.UseQuantity;

            oldRemarks = receive.Remarks;
            oldUser = receive.ReceiveBy;
            oldQuantity = receive.Quantity;

            receive.Remarks = "To Pharmacy";
            receive.Quantity = avQty;

            NavigationManager.NavigateTo("/receive/return", false);
        }

        private async Task Edit()
        {
            receive = await ReceiveService.GetByIdAsync(Id);

            avQty = receive.Quantity - receive.UseQuantity;

            oldRemarks = receive.Remarks;
            oldUser = receive.ReceiveBy;
            oldQuantity = receive.Quantity;

            receive.Remarks = "To Pharmacy";
            receive.Quantity = avQty;

            NavigationManager.NavigateTo("/receive/edit", false);
        }

        private void Reset()
        {
            IsDisable = false;
            IsEdit = false;
            IsTrue = false;

            receive.Quantity = oldQuantity;
            receive = new Receive();
            ReadUserName();
            NavigationManager.NavigateTo("/receive/create", false);
        }

        private string GetQueryParam(string paramName)
        {
            var urlBuilder = new UriBuilder(NavigationManager.Uri);
            var q = System.Web.HttpUtility.ParseQueryString(urlBuilder.Query);

            return q[paramName] ?? null;
        }

        public void Dispose()
        {
            UpdateService.OnUpdateRequested -= PageUpdateHandler;
        }
    }
}