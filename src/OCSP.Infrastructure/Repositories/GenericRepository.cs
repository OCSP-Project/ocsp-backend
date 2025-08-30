using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using OCSP.Infrastructure.Data;
using OCSP.Infrastructure.Repositories.Interfaces;

namespace OCSP.Infrastructure.Repositories
{
    // Namespace như bạn đưa: OCSP.Infrastructure.Repositories
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        /// <summary>Cho phép repo con truy cập trực tiếp khi cần.</summary>
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<T> _dbSet;

        // ✅ Constructor 1 tham số để các repo con có thể ": base(context)"
        public GenericRepository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            // EF Core FindAsync dùng key value; phù hợp khi entity có key là Guid tên "Id"
            return await _dbSet.FindAsync([id], ct);
        }

        public virtual async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default)
        {
            return await _dbSet.AsNoTracking().ToListAsync(ct);
        }

        public virtual async Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
        {
            return await _dbSet.AsNoTracking().Where(predicate).ToListAsync(ct);
        }

        public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
        {
            return await _dbSet.AnyAsync(predicate, ct);
        }

        public virtual async Task AddAsync(T entity, CancellationToken ct = default)
        {
            await _dbSet.AddAsync(entity, ct);
            await _context.SaveChangesAsync(ct);
        }

        public virtual async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default)
        {
            await _dbSet.AddRangeAsync(entities, ct);
            await _context.SaveChangesAsync(ct);
        }

        public virtual async Task UpdateAsync(T entity, CancellationToken ct = default)
        {
            _dbSet.Update(entity);
            await _context.SaveChangesAsync(ct);
        }

        public virtual async Task UpdateRangeAsync(IEnumerable<T> entities, CancellationToken ct = default)
        {
            _dbSet.UpdateRange(entities);
            await _context.SaveChangesAsync(ct);
        }

        public virtual async Task DeleteAsync(T entity, CancellationToken ct = default)
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync(ct);
        }

        public virtual async Task DeleteByIdAsync(Guid id, CancellationToken ct = default)
        {
            var entity = await GetByIdAsync(id, ct);
            if (entity is not null)
            {
                _dbSet.Remove(entity);
                await _context.SaveChangesAsync(ct);
            }
        }

        public virtual async Task DeleteRangeAsync(IEnumerable<T> entities, CancellationToken ct = default)
        {
            _dbSet.RemoveRange(entities);
            await _context.SaveChangesAsync(ct);
        }

        public virtual IQueryable<T> Query() => _dbSet.AsQueryable();

        public virtual Task<int> SaveChangesAsync(CancellationToken ct = default)
            => _context.SaveChangesAsync(ct);
    }
}
