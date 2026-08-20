using Galaxy.Lol.Application.Features.Rotations.DTO;
using Galaxy.Lol.Application.Results;

namespace Galaxy.Lol.Application.Features.Rotations.Ports
{
    public interface IGetFreeRotationUseCase
    {
        Task<Result<FreeRotationResponse>> ExecuteAsync(string platform, CancellationToken cancellationToken = default);
    }

    public interface IGetRotationHistoryUseCase
    {
        Task<Result<IReadOnlyCollection<FreeRotationResponse>>> ExecuteAsync(
            string platform, int take, CancellationToken cancellationToken = default);
    }

    public record RotationUseCases(
        IGetFreeRotationUseCase Current,
        IGetRotationHistoryUseCase History);
}
