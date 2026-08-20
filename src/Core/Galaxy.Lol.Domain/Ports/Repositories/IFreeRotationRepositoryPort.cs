using Galaxy.Lol.Domain.Entities;

namespace Galaxy.Lol.Domain.Ports.Repositories
{
    public interface IFreeRotationRepositoryPort : IBaseRepository<FreeRotation>
    {

        Task<FreeRotation?> GetLatestAsync(string platform, CancellationToken cancellationToken = default);

        Task<IReadOnlyCollection<FreeRotation>> GetHistoryAsync(
            string platform, int take, CancellationToken cancellationToken = default);
    }
}
