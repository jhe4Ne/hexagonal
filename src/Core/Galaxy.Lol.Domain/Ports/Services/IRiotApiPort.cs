using Galaxy.Lol.Domain.Model.External;
using Galaxy.Lol.Domain.ValueObjects;

namespace Galaxy.Lol.Domain.Ports.Services
{

    public interface IRiotApiPort
    {
        Task<RiotFreeRotation> GetFreeRotationAsync(string platform, CancellationToken cancellationToken = default);

        Task<IReadOnlyCollection<RiotChampionMastery>> GetMasteriesAsync(
            Puuid puuid, string platform, CancellationToken cancellationToken = default);

        Task<IReadOnlyCollection<RiotChampionMastery>> GetTopMasteriesAsync(
            Puuid puuid, string platform, int count, CancellationToken cancellationToken = default);

        Task<RiotAccount?> GetAccountByRiotIdAsync(
            string gameName, string tagLine, string platform, CancellationToken cancellationToken = default);
    }
}
