using Galaxy.Lol.Domain.Model.External;

namespace Galaxy.Lol.Domain.Ports.Cache
{

    public interface IChampionRawCachePort
    {
        Task SaveCatalogAsync(string version, string locale, string rawJson,
                              CancellationToken cancellationToken = default);

        Task<string?> GetCatalogAsync(string version, string locale,
                                      CancellationToken cancellationToken = default);

        Task SaveChampionDetailAsync(string version, string locale, string championId, string rawJson,
                                     CancellationToken cancellationToken = default);

        Task<string?> GetChampionDetailAsync(string version, string locale, string championId,
                                             CancellationToken cancellationToken = default);

        Task<IReadOnlyCollection<string>> GetCachedVersionsAsync(CancellationToken cancellationToken = default);
    }
}
