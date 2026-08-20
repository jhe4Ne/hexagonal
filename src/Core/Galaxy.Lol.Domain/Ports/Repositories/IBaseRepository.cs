using System.Linq.Expressions;
using Galaxy.Lol.Domain.Entities;

namespace Galaxy.Lol.Domain.Ports.Repositories
{

    public interface IBaseRepository<TEntity> where TEntity : BaseEntity
    {
        Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);

        Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

        void Update(TEntity entity);

        Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        Task<TEntity?> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

        Task<(ICollection<TResult> Result, int TotalRows)> ListAsync<TResult>(
            Expression<Func<TEntity, bool>> predicate,
            Expression<Func<TEntity, TResult>> selector,
            int pageNumber = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default);

        async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default) =>
            await GetByIdAsync(id, cancellationToken) is not null;
    }
}
