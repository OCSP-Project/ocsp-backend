using Microsoft.EntityFrameworkCore;
using OCSP.Infrastructure.Data;
using OCSP.Infrastructure.Repositories.Interfaces;

namespace OCSP.Infrastructure.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public GenericRepository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public virtual async Task<T?> GetByIdAsync(Guid id) => await _dbSet.FindAsync(id);
        public virtual async Task<IEnumerable<T>> GetAllAsync() => await _dbSet.ToListAsync();
        public virtual async Task<T> AddAsync(T entity)
        {
            var e = await _dbSet.AddAsync(entity);
            return e.Entity;
        }
        public virtual async Task<T> UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            return await Task.FromResult(entity);
        }
        public virtual async Task DeleteAsync(T entity)
        {
            _dbSet.Remove(entity);
            await Task.CompletedTask;
        }
        public virtual async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();
    }
}