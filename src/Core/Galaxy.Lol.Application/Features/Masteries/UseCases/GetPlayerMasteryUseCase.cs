using Galaxy.Lol.Application.Common.DTO;
using Galaxy.Lol.Application.Features.Masteries.DTO;
using Galaxy.Lol.Application.Features.Masteries.Mappers;
using Galaxy.Lol.Application.Features.Masteries.Ports;
using Galaxy.Lol.Application.Features.Masteries.Services;
using Galaxy.Lol.Application.Results;
using Galaxy.Lol.Domain.Entities;
using Galaxy.Lol.Domain.Exceptions;
using Galaxy.Lol.Domain.Ports.Repositories;
using Galaxy.Lol.Domain.Services;
using Galaxy.Lol.Domain.ValueObjects;

namespace Galaxy.Lol.Application.Features.Masteries.UseCases
{

    public class GetPlayerMasteryUseCase(
        SummonerMasteryLoader loader,
        IChampionRepositoryPort championRepository,
        IFreeRotationRepositoryPort rotationRepository,
        IAnalyticsRepositoryPort analyticsRepository,
        IDominanceIndexCalculator calculator) : IGetPlayerMasteryUseCase
    {
        public async Task<Result<PlayerMasteryResponse>> ExecuteAsync(
            GetPlayerMasteryRequest request, CancellationToken cancellationToken = default)
        {
            Puuid puuid;
            try
            {
                puuid = await loader.ResolveAsync(request.GameName, request.TagLine, request.Platform, cancellationToken);
            }
            catch (RiotAccountNotFoundException ex)
            {
                return Result<PlayerMasteryResponse>.Failure(ex.Message, 404);
            }

            var summoner = await loader.LoadAsync(
                puuid, request.GameName, request.TagLine, request.Platform, request.Refresh, cancellationToken);
            if (summoner is null || summoner.Masteries.Count == 0)
                return Result<PlayerMasteryResponse>.Failure(
                    "No hay maestrias registradas para este invocador. Ejecute la consulta con refresh=true.", 404);

            var ordenadas = summoner.Masteries.OrderByDescending(m => m.Score.Points).ToList();
            var pagina = ordenadas.Skip((request.Page - 1) * request.Rows).Take(request.Rows).ToList();

            var catalogo = await ObtenerCatalogoAsync(pagina, cancellationToken);
            var clavesRotacion = await ObtenerRotacionAsync(request.Platform, cancellationToken);

            var maximo = summoner.PuntajeMaximo.Points;
            var ahora = DateTime.UtcNow;

            var items = pagina
                .Select(m => MasteryMapper.ToItem(m, catalogo, clavesRotacion, calculator, maximo, ahora))
                .ToList();

            await GuardarSnapshotsAsync(summoner, items, cancellationToken);

            var respuesta = new PlayerMasteryResponse(
                summoner.Puuid.Masked,
                summoner.Platform,
                summoner.Masteries.Count,
                summoner.Masteries.Sum(m => m.Score.Points),
                summoner.Masteries.Max(m => m.Score.Level),
                calculator.Nombre,
                summoner.LastSyncAt,
                PagedResult<MasteryItemResponse>.Create(items, request.Page, request.Rows, ordenadas.Count));

            return Result<PlayerMasteryResponse>.Success(respuesta);
        }

        private async Task<IReadOnlyDictionary<int, ChampionProfile>> ObtenerCatalogoAsync(
            IReadOnlyCollection<ChampionMastery> maestrias, CancellationToken cancellationToken)
        {
            var claves = maestrias.Select(m => m.Key.Value).Distinct().ToList();
            var campeones = await championRepository.GetByKeysAsync(claves, cancellationToken);
            return campeones.ToDictionary(c => c.Key.Value);
        }

        private async Task<IReadOnlySet<int>> ObtenerRotacionAsync(string platform, CancellationToken cancellationToken)
        {
            var rotacion = await rotationRepository.GetLatestAsync(platform, cancellationToken);
            return rotacion?.ClavesGenerales.ToHashSet() ?? [];
        }

        private async Task GuardarSnapshotsAsync(Summoner summoner, IReadOnlyCollection<MasteryItemResponse> items,
                                                 CancellationToken cancellationToken)
        {
            try
            {
                var snapshots = items.Select(i => MasterySnapshot.Create(
                    summoner.Puuid.Masked, i.ChampionKey, i.ChampionName, i.Points, i.Level, i.DominanceIndex));

                await analyticsRepository.SaveSnapshotsAsync(snapshots, cancellationToken);
            }
            catch
            {

            }
        }
    }
}
