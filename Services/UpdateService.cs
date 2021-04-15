using System;

namespace InventoryApp.Services
{
    public class UpdateService
    {
        public event Action<string, bool> OnUpdateRequested;

        public void UpdatePage(string property, bool isUpdate)
        {
            OnUpdateRequested?.Invoke(property, isUpdate);
        }
    }
}
