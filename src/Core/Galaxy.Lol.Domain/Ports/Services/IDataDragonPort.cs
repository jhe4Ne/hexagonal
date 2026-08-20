using Galaxy.Lol.Domain.Model.External;

namespace Galaxy.Lol.Domain.Ports.Services
{

    public interface IDataDragonPort
    {
        Task<string> GetLatestVersionAsync(CancellationToken cancellationToken = default);

        Task<DataDragonCatalog> GetChampionCatalogAsync(
            string? version = null, string? locale = null, CancellationToken cancellationToken = default);

        Task<DataDragonChampionDetail> GetChampionDetailAsync(
            string championId, string? version = null, string? locale = null,
            CancellationToken cancellationToken = default);

        string BuildChampionImageUrl(string version, string imageFile);
    }
}
