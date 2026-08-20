using System.Linq.Expressions;
using Galaxy.Lol.Domain.Entities;
using Galaxy.Lol.Domain.Ports.Repositories;
using Galaxy.Lol.Infraestructure.Configuration.Repositories.Context;
using Microsoft.EntityFrameworkCore;

namespace Galaxy.Lol.Infraestructure.Adapters.Repositories
{

    public class BaseRepository<TEntity>(ChampionsDbContext context) : IBaseRepository<TEntity>
        where TEntity : BaseEntity
    {
        protected readonly ChampionsDbContext Context = context;
        protected DbSet<TEntity> Entities => Context.Set<TEntity>();

        public virtual async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            await Entities.AddAsync(entity, cancellationToken);
            return entity;
        }

        public virtual async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default) =>
            await Entities.AddRangeAsync(entities, cancellationToken);

        public virtual void Update(TEntity entity) => Entities.Update(entity);

        public virtual async Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            await Entities.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        public virtual async Task<TEntity?> FindAsync(Expression<Func<TEntity, bool>> predicate,
                                                      CancellationToken cancellationToken = default) =>
            await Entities.AsNoTracking().FirstOrDefaultAsync(predicate, cancellationToken);

        public virtual async Task<(ICollection<TResult> Result, int TotalRows)> ListAsync<TResult>(
            Expression<Func<TEntity, bool>> predicate,
            Expression<Func<TEntity, TResult>> selector,
            int pageNumber = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var consulta = Entities.AsNoTracking().Where(predicate);

            var total = await consulta.CountAsync(cancellationToken);

            var pagina = await consulta
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(selector)
                .ToListAsync(cancellationToken);

            return (pagina, total);
        }
    }
}
