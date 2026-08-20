using Galaxy.Lol.Application.Features.Rotations.DTO;
using Galaxy.Lol.Application.Features.Rotations.Ports;
using Galaxy.Lol.Application.Results;
using Galaxy.Lol.Domain.Ports.Repositories;

namespace Galaxy.Lol.Application.Features.Rotations.UseCases
{

    public class GetRotationHistoryUseCase(
        IFreeRotationRepositoryPort rotationRepository,
        IChampionRepositoryPort championRepository) : IGetRotationHistoryUseCase
    {
        public async Task<Result<IReadOnlyCollection<FreeRotationResponse>>> ExecuteAsync(
            string platform, int take, CancellationToken cancellationToken = default)
        {
            var rotaciones = await rotationRepository.GetHistoryAsync(platform, take <= 0 ? 10 : take, cancellationToken);

            var respuestas = new List<FreeRotationResponse>();
            foreach (var rotacion in rotaciones)
                respuestas.Add(await GetFreeRotationUseCase.ConstruirRespuestaAsync(rotacion, championRepository, cancellationToken));

            return Result<IReadOnlyCollection<FreeRotationResponse>>.Success(respuestas.AsReadOnly());
        }
    }
}
