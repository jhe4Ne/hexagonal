using Galaxy.Lol.Application.Common.DTO;
using Galaxy.Lol.Application.Features.Champions.DTO;
using Galaxy.Lol.Application.Features.Champions.Mappers;
using Galaxy.Lol.Application.Features.Champions.Ports;
using Galaxy.Lol.Application.Results;
using Galaxy.Lol.Domain.Ports.Repositories;
using Microsoft.Extensions.Logging;

namespace Galaxy.Lol.Application.Features.Champions.UseCases
{

    public class GetChampionCatalogUseCase(
        IChampionRepositoryPort championRepository,
        IFreeRotationRepositoryPort rotationRepository,
        ILogger<GetChampionCatalogUseCase> logger) : IGetChampionCatalogUseCase
    {
        public async Task<Result<PagedResult<ChampionListItemResponse>>> ExecuteAsync(
            SearchChampionsRequest request, CancellationToken cancellationToken = default)
        {
            var rotacion = await rotationRepository.GetLatestAsync(request.Platform, cancellationToken);
            var clavesRotacion = rotacion?.ClavesGenerales.ToHashSet() ?? [];

            if (request.OnlyFreeRotation && clavesRotacion.Count == 0)
            {
                logger.LogWarning("Se pidio filtrar por rotacion en {Plataforma} pero no hay ninguna sincronizada.",
                    request.Platform);
                return Result<PagedResult<ChampionListItemResponse>>.Failure(
                    "Todavia no se ha sincronizado la rotacion gratuita de esta plataforma.", 404);
            }

            var (items, total) = await championRepository.SearchAsync(
                request.Filter,
                request.Role,
                request.MinDifficulty,
                request.MaxDifficulty,
                request.OnlyFreeRotation ? clavesRotacion.ToList() : null,
                request.Page,
                request.Rows,
                cancellationToken);

            var respuesta = items.Select(c => ChampionMapper.ToListItem(c, clavesRotacion)).ToList();

            return Result<PagedResult<ChampionListItemResponse>>.Success(
                PagedResult<ChampionListItemResponse>.Create(respuesta, request.Page, request.Rows, total));
        }
    }
}
