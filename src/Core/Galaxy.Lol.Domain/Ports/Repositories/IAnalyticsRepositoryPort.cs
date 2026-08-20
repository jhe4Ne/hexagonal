using Galaxy.Lol.Domain.Entities;

namespace Galaxy.Lol.Domain.Ports.Repositories
{

    public interface IAnalyticsRepositoryPort
    {
        Task RegisterSyncAsync(SyncLog log, CancellationToken cancellationToken = default);

        Task SaveSnapshotsAsync(IEnumerable<MasterySnapshot> snapshots, CancellationToken cancellationToken = default);

        Task<IReadOnlyCollection<SyncLog>> GetRecentSyncsAsync(int take, CancellationToken cancellationToken = default);

        Task<IReadOnlyCollection<MasterySnapshot>> GetSnapshotHistoryAsync(
            string maskedPuuid, int championKey, CancellationToken cancellationToken = default);
    }
}
