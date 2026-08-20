using Galaxy.Lol.Application.Features.Champions.DTO;
using Galaxy.Lol.Application.Features.Champions.Mappers;
using Galaxy.Lol.Application.Features.Champions.Ports;
using Galaxy.Lol.Application.Results;
using Galaxy.Lol.Domain.Ports.Repositories;

namespace Galaxy.Lol.Application.Features.Champions.UseCases
{

    public class GetChampionDetailUseCase(
        IChampionRepositoryPort championRepository,
        IFreeRotationRepositoryPort rotationRepository) : IGetChampionDetailUseCase
    {
        public async Task<Result<ChampionDetailResponse>> ExecuteAsync(
            string championId, string platform, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(championId))
                return Result<ChampionDetailResponse>.Failure("El identificador del campeon es obligatorio.", 400);

            var champion = await championRepository.GetDetailAsync(championId, cancellationToken);
            if (champion is null)
                return Result<ChampionDetailResponse>.Failure($"No existe el campeon '{championId}'.", 404);

            var rotacion = await rotationRepository.GetLatestAsync(platform, cancellationToken);
            var clavesRotacion = rotacion?.ClavesGenerales.ToHashSet() ?? [];

            return Result<ChampionDetailResponse>.Success(ChampionMapper.ToDetail(champion, clavesRotacion));
        }
    }
}
