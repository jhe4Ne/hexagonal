using Galaxy.Lol.Application.Features.Masteries.DTO;
using Galaxy.Lol.Application.Features.Masteries.Ports;
using Galaxy.Lol.Application.Features.Masteries.Services;
using Galaxy.Lol.Application.Results;
using Galaxy.Lol.Domain.Exceptions;
using Galaxy.Lol.Domain.Ports.Repositories;
using Galaxy.Lol.Domain.Services;
using Galaxy.Lol.Domain.ValueObjects;

namespace Galaxy.Lol.Application.Features.Masteries.UseCases
{

    public class RecommendChampionsUseCase(
        SummonerMasteryLoader loader,
        IChampionRepositoryPort championRepository,
        IFreeRotationRepositoryPort rotationRepository,
        ISummonerRepositoryPort summonerRepository,
        ChampionRecommendationService recommendationService) : IRecommendChampionsUseCase
    {
        public async Task<Result<IReadOnlyCollection<RecommendationResponse>>> ExecuteAsync(
            RecommendChampionsRequest request, CancellationToken cancellationToken = default)
        {
            Puuid? puuid = null;
            if (!string.IsNullOrWhiteSpace(request.GameName) && !string.IsNullOrWhiteSpace(request.TagLine))
            {
                try
                {
                    puuid = await loader.ResolveAsync(request.GameName, request.TagLine, request.Platform, cancellationToken);
                }
                catch (RiotAccountNotFoundException ex)
                {
                    return Result<IReadOnlyCollection<RecommendationResponse>>.Failure(ex.Message, 404);
                }
            }

            var catalogo = await championRepository.GetAllAsync(cancellationToken);
            if (catalogo.Count == 0)
                return Result<IReadOnlyCollection<RecommendationResponse>>.Failure(
                    "El catalogo de campeones esta vacio. Sincronice Data Dragon primero.", 409);

            var rotacion = await rotationRepository.GetLatestAsync(request.Platform, cancellationToken);

            var summoner = puuid is null
                ? null
                : await summonerRepository.GetByPuuidAsync(puuid, cancellationToken);

            var recomendaciones = recommendationService.Recomendar(
                catalogo, summoner, rotacion, request.OnlyBeginnerFriendly, request.Count);

            var clavesRotacion = rotacion?.ClavesGenerales.ToHashSet() ?? [];

            var respuesta = recomendaciones
                .Select(r => new RecommendationResponse(
                    r.Champion.Key.Value,
                    r.Champion.ChampionId,
                    r.Champion.Name,
                    r.Champion.ImageUrl,
                    r.Champion.RolPrincipal,
                    r.Champion.Difficulty.Value,
                    r.Champion.Difficulty.Category,
                    clavesRotacion.Contains(r.Champion.Key.Value),
                    r.Reason,
                    r.Score))
                .ToList();

            return Result<IReadOnlyCollection<RecommendationResponse>>.Success(respuesta.AsReadOnly());
        }
    }
}
