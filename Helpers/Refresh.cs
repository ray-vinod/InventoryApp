using System;

namespace InventoryApp.Helpers
{
    public class Refresh
    {
        public event Action OnRefreshRequested;

        public void CallPageRefresh()
        {
            OnRefreshRequested?.Invoke();
        }
    }
}
