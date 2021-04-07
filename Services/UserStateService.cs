using System;

namespace InventoryApp.Services
{
    public class UserStateService
    {
        public event Action OnRefreshRequeste;

        public string Name { get; set; }
        public string ImagePath { get; set; }
        public string Email { get; set; }


        public void CallPageRefresh()
        {
            OnRefreshRequeste?.Invoke();
        }
    }
}
