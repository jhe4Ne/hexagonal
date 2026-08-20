using Galaxy.Lol.Application.Features.Masteries.DTO;
using Galaxy.Lol.Application.Features.Masteries.Mappers;
using Galaxy.Lol.Application.Features.Masteries.Ports;
using Galaxy.Lol.Application.Features.Masteries.Services;
using Galaxy.Lol.Application.Results;
using Galaxy.Lol.Domain.Exceptions;
using Galaxy.Lol.Domain.Ports.Repositories;
using Galaxy.Lol.Domain.Services;
using Galaxy.Lol.Domain.ValueObjects;

namespace Galaxy.Lol.Application.Features.Masteries.UseCases
{

    public class GetTopMasteryUseCase(
        SummonerMasteryLoader loader,
        IChampionRepositoryPort championRepository,
        IFreeRotationRepositoryPort rotationRepository,
        IDominanceIndexCalculator calculator) : IGetTopMasteryUseCase
    {
        public async Task<Result<TopMasteryResponse>> ExecuteAsync(
            GetTopMasteryRequest request, CancellationToken cancellationToken = default)
        {
            Puuid puuid;
            try
            {
                puuid = await loader.ResolveAsync(request.GameName, request.TagLine, request.Platform, cancellationToken);
            }
            catch (RiotAccountNotFoundException ex)
            {
                return Result<TopMasteryResponse>.Failure(ex.Message, 404);
            }

            var cantidad = request.Count is <= 0 or > 50 ? 5 : request.Count;

            var summoner = await loader.LoadAsync(
                puuid, request.GameName, request.TagLine, request.Platform, request.Refresh, cancellationToken);
            if (summoner is null || summoner.Masteries.Count == 0)
                return Result<TopMasteryResponse>.Failure(
                    "No hay maestrias registradas para este invocador. Ejecute la consulta con refresh=true.", 404);

            var top = summoner.Masteries
                .OrderByDescending(m => m.Score.Points)
                .Take(cantidad)
                .ToList();

            var campeones = await championRepository.GetByKeysAsync(
                top.Select(m => m.Key.Value).ToList(), cancellationToken);
            var catalogo = campeones.ToDictionary(c => c.Key.Value);

            var rotacion = await rotationRepository.GetLatestAsync(request.Platform, cancellationToken);
            var clavesRotacion = rotacion?.ClavesGenerales.ToHashSet() ?? [];

            var maximo = summoner.PuntajeMaximo.Points;
            var ahora = DateTime.UtcNow;

            var items = top
                .Select(m => MasteryMapper.ToItem(m, catalogo, clavesRotacion, calculator, maximo, ahora))
                .ToList();

            return Result<TopMasteryResponse>.Success(
                new TopMasteryResponse(summoner.Puuid.Masked, summoner.Platform, items));
        }
    }
}
