using Galaxy.Lol.Application.Features.Synchronization.DTO;
using Galaxy.Lol.Application.Features.Synchronization.Ports;
using Galaxy.Lol.Application.Results;
using Galaxy.Lol.Domain.Ports.Repositories;

namespace Galaxy.Lol.Application.Features.Synchronization.UseCases
{

    public class GetSyncHistoryUseCase(IAnalyticsRepositoryPort analyticsRepository) : IGetSyncHistoryUseCase
    {
        public async Task<Result<IReadOnlyCollection<SyncLogResponse>>> ExecuteAsync(
            int take, CancellationToken cancellationToken = default)
        {
            var logs = await analyticsRepository.GetRecentSyncsAsync(take <= 0 ? 50 : take, cancellationToken);

            var respuesta = logs
                .Select(l => new SyncLogResponse(l.Origin.ToString(), l.Endpoint, l.Platform, l.Successful,
                    l.Message, l.ProcessedRecords, l.ElapsedMilliseconds, l.ExecutedAt))
                .ToList();

            return Result<IReadOnlyCollection<SyncLogResponse>>.Success(respuesta.AsReadOnly());
        }
    }
}
