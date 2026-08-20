using Galaxy.Lol.Domain.Entities;
using Galaxy.Lol.Domain.Model.ReadModels;
using Galaxy.Lol.Domain.ValueObjects;

namespace Galaxy.Lol.Domain.Ports.Repositories
{
    public interface IChampionRepositoryPort : IBaseRepository<ChampionProfile>
    {
        Task<ChampionProfile?> GetByChampionIdAsync(string championId, CancellationToken cancellationToken = default);

        Task<ChampionProfile?> GetByKeyAsync(ChampionKey key, CancellationToken cancellationToken = default);

        Task<ChampionProfile?> GetDetailAsync(string championId, CancellationToken cancellationToken = default);

        Task<IReadOnlyCollection<ChampionProfile>> GetAllAsync(CancellationToken cancellationToken = default);

        Task<(IReadOnlyCollection<ChampionProfile> Items, int TotalRows)> SearchAsync(
            string? filter, string? role, int? minDifficulty, int? maxDifficulty,
            IReadOnlyCollection<int>? onlyKeys, int page, int rows,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyCollection<ChampionProfile>> GetByKeysAsync(
            IReadOnlyCollection<int> keys, CancellationToken cancellationToken = default);

        Task<IReadOnlyCollection<RoleDistributionReadModel>> GetRoleDistributionAsync(
            IReadOnlyCollection<int> rotationKeys, CancellationToken cancellationToken = default);
    }
}
