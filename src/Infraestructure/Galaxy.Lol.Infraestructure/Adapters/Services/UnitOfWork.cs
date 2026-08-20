using Galaxy.Lol.Domain.Events.Interfaces;
using Galaxy.Lol.Domain.Ports.Repositories;
using Galaxy.Lol.Infraestructure.Configuration.Repositories.Context;
using Microsoft.EntityFrameworkCore.Storage;

namespace Galaxy.Lol.Infraestructure.Adapters.Services
{

    public class UnitOfWork(ChampionsDbContext context, IDomainEventDispatcher dispatcher) : IUnitOfWork
    {
        private IDbContextTransaction? _transaction;

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var eventos = DomainEventDispatcher.Recolectar(context);
            if (eventos.Count != 0)
                await dispatcher.DispatchAsync(eventos, cancellationToken);

            return await context.SaveChangesAsync(cancellationToken);
        }

        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default) =>
            _transaction ??= await context.Database.BeginTransactionAsync(cancellationToken);

        public async Task CommitAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction is null) return;

            await SaveChangesAsync(cancellationToken);
            await _transaction.CommitAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }

        public async Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction is null) return;

            await _transaction.RollbackAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }
}
