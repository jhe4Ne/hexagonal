using Galaxy.Lol.Domain.Entities;
using Galaxy.Lol.Domain.Model.ReadModels;
using Galaxy.Lol.Domain.ValueObjects;

namespace Galaxy.Lol.Domain.Ports.Repositories
{
    public interface ISummonerRepositoryPort : IBaseRepository<Summoner>
    {

        Task<Summoner?> GetByPuuidAsync(Puuid puuid, CancellationToken cancellationToken = default);

        Task<(IReadOnlyCollection<ChampionMastery> Items, int TotalRows)> ListMasteriesAsync(
            Puuid puuid, int page, int rows, CancellationToken cancellationToken = default);

        Task<IReadOnlyCollection<ChampionMastery>> GetTopMasteriesAsync(
            Puuid puuid, int count, CancellationToken cancellationToken = default);

        Task<IReadOnlyCollection<MasteryByRoleReadModel>> GetMasteryByRoleAsync(
            Puuid puuid, CancellationToken cancellationToken = default);
    }
}
