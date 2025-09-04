using System.Linq.Expressions;

namespace OCSP.Infrastructure.Repositories.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {

        Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default);
        Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);

        Task AddAsync(T entity, CancellationToken ct = default);
        Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default);

        Task UpdateAsync(T entity, CancellationToken ct = default);
        Task UpdateRangeAsync(IEnumerable<T> entities, CancellationToken ct = default);

        Task DeleteAsync(T entity, CancellationToken ct = default);
        Task DeleteByIdAsync(Guid id, CancellationToken ct = default);
        Task DeleteRangeAsync(IEnumerable<T> entities, CancellationToken ct = default);

        /// <summary>Trả về IQueryable để repo con có thể build query nâng cao (Include, OrderBy…)</summary>
        IQueryable<T> Query();

        /// <summary>Lưu thay đổi (nếu repo con cần gọi thủ công).</summary>
        Task<int> SaveChangesAsync(CancellationToken ct = default);
    }
}

