using Galaxy.Lol.Application.Features.Synchronization.DTO;
using Galaxy.Lol.Application.Results;

namespace Galaxy.Lol.Application.Features.Synchronization.Ports
{
    public interface ISyncChampionCatalogUseCase
    {
        Task<Result<SyncResultResponse>> ExecuteAsync(
            SyncCatalogRequest request, CancellationToken cancellationToken = default);
    }

    public interface ISyncFreeRotationUseCase
    {
        Task<Result<SyncResultResponse>> ExecuteAsync(
            SyncRotationRequest request, CancellationToken cancellationToken = default);
    }

    public interface IGetSyncHistoryUseCase
    {
        Task<Result<IReadOnlyCollection<SyncLogResponse>>> ExecuteAsync(
            int take, CancellationToken cancellationToken = default);
    }

    public record SyncUseCases(
        ISyncChampionCatalogUseCase Catalog,
        ISyncFreeRotationUseCase Rotation,
        IGetSyncHistoryUseCase History);
}
