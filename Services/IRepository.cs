using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace InventoryApp.Services
{
    public interface IRepository<TEntity> where TEntity : class
    {
        Task<bool> DeleteAsync(TEntity entity);
        Task<bool> DeleteAsync(object id);

        Task<TEntity> CreateAsync(TEntity entity);
        Task<TEntity> UpdateAsync(TEntity entity);

        Task<TEntity> GetByIdAsync(object id);
        Task<IEnumerable<TEntity>> GetItemsAsync(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            string includeProperties = "");

        IAsyncEnumerable<TEntity> StreamListAsync(
            int pageNumber,
            int pageSize,
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            string includeProperties = "");
        int PageCount();
    }
}