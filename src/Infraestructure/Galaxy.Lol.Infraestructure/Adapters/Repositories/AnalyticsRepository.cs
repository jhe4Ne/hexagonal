using Galaxy.Lol.Domain.Entities;
using Galaxy.Lol.Domain.Ports.Repositories;
using Galaxy.Lol.Infraestructure.Configuration.Repositories.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Galaxy.Lol.Infraestructure.Adapters.Repositories
{

    public class AnalyticsRepository(AnalyticsDbContext context, ILogger<AnalyticsRepository> logger)
        : IAnalyticsRepositoryPort
    {
        public async Task RegisterSyncAsync(SyncLog log, CancellationToken cancellationToken = default)
        {
            await context.SyncLogs.AddAsync(log, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }

        public async Task SaveSnapshotsAsync(IEnumerable<MasterySnapshot> snapshots,
                                             CancellationToken cancellationToken = default)
        {
            var lista = snapshots.ToList();
            if (lista.Count == 0) return;

            await context.MasterySnapshots.AddRangeAsync(lista, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            logger.LogDebug("Guardadas {Total} fotos de maestria en bdanalitica.", lista.Count);
        }

        public async Task<IReadOnlyCollection<SyncLog>> GetRecentSyncsAsync(
            int take, CancellationToken cancellationToken = default) =>
            await context.SyncLogs
                .AsNoTracking()
                .OrderByDescending(l => l.ExecutedAt)
                .Take(take)
                .ToListAsync(cancellationToken);

        public async Task<IReadOnlyCollection<MasterySnapshot>> GetSnapshotHistoryAsync(
            string maskedPuuid, int championKey, CancellationToken cancellationToken = default) =>
            await context.MasterySnapshots
                .AsNoTracking()
                .Where(s => s.MaskedPuuid == maskedPuuid && s.ChampionKey == championKey)
                .OrderBy(s => s.TakenAt)
                .ToListAsync(cancellationToken);
    }
}
