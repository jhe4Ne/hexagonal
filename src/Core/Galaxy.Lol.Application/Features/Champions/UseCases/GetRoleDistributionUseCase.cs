using Galaxy.Lol.Application.Features.Champions.DTO;
using Galaxy.Lol.Application.Features.Champions.Ports;
using Galaxy.Lol.Application.Results;
using Galaxy.Lol.Domain.Ports.Repositories;

namespace Galaxy.Lol.Application.Features.Champions.UseCases
{

    public class GetRoleDistributionUseCase(
        IChampionRepositoryPort championRepository,
        IFreeRotationRepositoryPort rotationRepository) : IGetRoleDistributionUseCase
    {
        public async Task<Result<IReadOnlyCollection<RoleDistributionResponse>>> ExecuteAsync(
            string platform, CancellationToken cancellationToken = default)
        {
            var rotacion = await rotationRepository.GetLatestAsync(platform, cancellationToken);
            var claves = rotacion?.ClavesGenerales ?? [];

            var distribucion = await championRepository.GetRoleDistributionAsync(claves, cancellationToken);

            var respuesta = distribucion
                .Select(d => new RoleDistributionResponse(d.Role, d.Total, Math.Round(d.AverageDifficulty, 2), d.InFreeRotation))
                .ToList();

            return Result<IReadOnlyCollection<RoleDistributionResponse>>.Success(respuesta);
        }
    }
}
