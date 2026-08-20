using Galaxy.Lol.Application.Features.Masteries.DTO;
using Galaxy.Lol.Application.Results;

namespace Galaxy.Lol.Application.Features.Masteries.Ports
{
    public interface IGetPlayerMasteryUseCase
    {
        Task<Result<PlayerMasteryResponse>> ExecuteAsync(
            GetPlayerMasteryRequest request, CancellationToken cancellationToken = default);
    }

    public interface IGetTopMasteryUseCase
    {
        Task<Result<TopMasteryResponse>> ExecuteAsync(
            GetTopMasteryRequest request, CancellationToken cancellationToken = default);
    }

    public interface IRecommendChampionsUseCase
    {
        Task<Result<IReadOnlyCollection<RecommendationResponse>>> ExecuteAsync(
            RecommendChampionsRequest request, CancellationToken cancellationToken = default);
    }

    public interface IGetMasteryByRoleUseCase
    {
        Task<Result<IReadOnlyCollection<MasteryByRoleResponse>>> ExecuteAsync(
            string gameName, string tagLine, string platform, CancellationToken cancellationToken = default);
    }

    public record MasteryUseCases(
        IGetPlayerMasteryUseCase Player,
        IGetTopMasteryUseCase Top,
        IRecommendChampionsUseCase Recommend,
        IGetMasteryByRoleUseCase ByRole);
}
