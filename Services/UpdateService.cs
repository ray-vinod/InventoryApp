using System;

namespace InventoryApp.Services
{
    public class UpdateService<TEntity> where TEntity:class
    {
        public event Action<string, TEntity> OnUpdateRequested;

        public void UpdatePage(string property,TEntity entity)
        {
            OnUpdateRequested?.Invoke(property, entity);
        }
    }
}
