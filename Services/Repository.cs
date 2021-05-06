using InventoryApp.Helpers;
using InventoryApp.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace InventoryApp.Serviceses
{
    public class Repository<TEntity, TDataContext> : IRepository<TEntity>
      where TEntity : class
      where TDataContext : DbContext
    {
        protected readonly TDataContext _context;
        internal DbSet<TEntity> dbSet;

        public Repository(TDataContext context)
        {
            _context = context;
            dbSet = context.Set<TEntity>();
        }

        public async Task<TEntity> CreateAsync(TEntity entity)
        {
            await dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> DeleteAsync(TEntity entity)
        {
            if (_context.Entry(entity).State == EntityState.Detached)
            {
                dbSet.Attach(entity);
            }
            dbSet.Remove(entity);
            _context.Entry(entity).State = EntityState.Deleted;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(object id)
        {
            TEntity entity = await dbSet.FindAsync(id);
            return await DeleteAsync(entity);
        }

        public async Task<TEntity> GetByIdAsync(object id)
        {
            return await dbSet.FindAsync(id);
        }

        public async Task<IEnumerable<TEntity>> GetItemsAsync(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            string includeProperties = "")
        {
            try
            {
                // Get the dbSet from the Entity passed in                
                IQueryable<TEntity> query = dbSet;

                // Apply the filter
                if (filter != null)
                {
                    query = query.Where(filter);
                }

                // Include the specified properties
                foreach (var includeProperty in includeProperties.Split
                    (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProperty);
                }

                // Sort
                if (orderBy != null)
                {
                    return orderBy(query).ToList();
                }
                else
                {
                    var list = await query.ToListAsync();
                    return list;
                }
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
                return null;
            }
        }

        public async IAsyncEnumerable<TEntity> StreamListAsync(
            int pageNumber,
            int pageSize,
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            string includeProperties = "")
        {
            // Get the dbSet from the Entity passed in                
            IQueryable<TEntity> query = dbSet;

            try
            {
                // Apply the filter
                if (filter != null)
                {
                    query = query.Where(filter);
                }

                // Include the specified properties
                foreach (var includeProperty in includeProperties.Split
                    (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProperty);
                }

                // Sort
                if (orderBy != null)
                    query = orderBy(query);

            }
            catch (Exception ex)
            {
                var msg = ex.Message;
                query = null;
            }

            var pagedList = await PaginatedList<TEntity>.CreateAsync(query, pageNumber, pageSize);

            foreach (var item in pagedList)
            {
                yield return item;
            }
        }

        public async Task<TEntity> UpdateAsync(TEntity entity)
        {
            var dbSet = _context.Set<TEntity>();
            dbSet.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return entity;
        }

        public int PageCount()
        {
            return PaginatedList<TEntity>.TotalPage();
        }


    }
}