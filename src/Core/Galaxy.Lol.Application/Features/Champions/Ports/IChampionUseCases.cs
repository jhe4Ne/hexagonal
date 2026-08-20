using Galaxy.Lol.Application.Common.DTO;
using Galaxy.Lol.Application.Features.Champions.DTO;
using Galaxy.Lol.Application.Results;

namespace Galaxy.Lol.Application.Features.Champions.Ports
{

    public interface IGetChampionCatalogUseCase
    {
        Task<Result<PagedResult<ChampionListItemResponse>>> ExecuteAsync(
            SearchChampionsRequest request, CancellationToken cancellationToken = default);
    }

    public interface IGetChampionDetailUseCase
    {
        Task<Result<ChampionDetailResponse>> ExecuteAsync(
            string championId, string platform, CancellationToken cancellationToken = default);
    }

    public interface IGetRoleDistributionUseCase
    {
        Task<Result<IReadOnlyCollection<RoleDistributionResponse>>> ExecuteAsync(
            string platform, CancellationToken cancellationToken = default);
    }

    public record ChampionUseCases(
        IGetChampionCatalogUseCase Catalog,
        IGetChampionDetailUseCase Detail,
        IGetRoleDistributionUseCase RoleDistribution);
}
