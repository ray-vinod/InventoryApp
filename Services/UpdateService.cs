using System;

namespace InventoryApp.Services
{
    public class UpdateService<TEntity> where TEntity:class
    {
        public event Action<TEntity> OnUpdateRequested;

        public void UpdatePage(TEntity entity=null)
        {
            OnUpdateRequested?.Invoke(entity);
        }
    }
}
