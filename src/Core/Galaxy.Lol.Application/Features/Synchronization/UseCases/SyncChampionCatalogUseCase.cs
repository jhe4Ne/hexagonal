using System.Diagnostics;
using Galaxy.Lol.Application.Features.Synchronization.DTO;
using Galaxy.Lol.Application.Features.Synchronization.Ports;
using Galaxy.Lol.Application.Results;
using Galaxy.Lol.Domain.Entities;
using Galaxy.Lol.Domain.Enums;
using Galaxy.Lol.Domain.Ports.Repositories;
using Galaxy.Lol.Domain.Ports.Services;
using Galaxy.Lol.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Galaxy.Lol.Application.Features.Synchronization.UseCases
{

    public class SyncChampionCatalogUseCase(
        IDataDragonPort dataDragon,
        IChampionRepositoryPort championRepository,
        IAnalyticsRepositoryPort analyticsRepository,
        INotificationPort notificationPort,
        ILogger<SyncChampionCatalogUseCase> logger) : ISyncChampionCatalogUseCase
    {
        public async Task<Result<SyncResultResponse>> ExecuteAsync(
            SyncCatalogRequest request, CancellationToken cancellationToken = default)
        {
            var cronometro = Stopwatch.StartNew();
            var creados = 0;
            var actualizados = 0;

            try
            {
                var catalogo = await dataDragon.GetChampionCatalogAsync(request.Version, request.Locale, cancellationToken);
                var existentes = (await championRepository.GetAllAsync(cancellationToken))
                    .ToDictionary(c => c.ChampionId, StringComparer.OrdinalIgnoreCase);

                foreach (var resumen in catalogo.Champions)
                {
                    var dificultad = DifficultyLevel.Create(resumen.Difficulty);
                    var stats = ChampionStats.Create(resumen.Stats.Hp, resumen.Stats.Mp, resumen.Stats.Armor,
                        resumen.Stats.SpellBlock, resumen.Stats.AttackDamage, resumen.Stats.AttackSpeed,
                        resumen.Stats.MoveSpeed);
                    var imagen = string.IsNullOrWhiteSpace(resumen.ImageFile)
                        ? null
                        : dataDragon.BuildChampionImageUrl(catalogo.Version, resumen.ImageFile!);

                    var roles = resumen.Tags
                        .Where(ChampionRole.EsValido)
                        .Select(ChampionRole.Create)
                        .ToList();

                    if (existentes.TryGetValue(resumen.ChampionId, out var champion))
                    {
                        champion.ActualizarFicha(resumen.Name, resumen.Title, resumen.Blurb, imagen,
                            catalogo.Version, dificultad, stats);
                        champion.DefinirRoles(roles);
                        championRepository.Update(champion);
                        actualizados++;
                    }
                    else
                    {
                        champion = ChampionProfile.Create(ChampionKey.Create(resumen.Key), resumen.ChampionId,
                            resumen.Name, resumen.Title, resumen.Blurb, imagen, catalogo.Version, dificultad, stats);
                        champion.DefinirRoles(roles);
                        await championRepository.AddAsync(champion, cancellationToken);
                        existentes[resumen.ChampionId] = champion;
                        creados++;
                    }

                    if (request.IncludeDetails)
                        await AplicarDetalleAsync(champion, catalogo.Version, request.Locale, cancellationToken);
                }

                cronometro.Stop();

                await analyticsRepository.RegisterSyncAsync(
                    SyncLog.Exito(SyncOrigin.Manual, "datadragon/champion.json", null,
                        catalogo.Champions.Count, cronometro.ElapsedMilliseconds), cancellationToken);

                await notificationPort.NotifyCatalogSyncedAsync(catalogo.Version, catalogo.Champions.Count, cancellationToken);

                logger.LogInformation("Catalogo sincronizado: version {Version}, {Creados} altas y {Actualizados} actualizaciones.",
                    catalogo.Version, creados, actualizados);

                return Result<SyncResultResponse>.Success(new SyncResultResponse(
                    "datadragon/champion.json", catalogo.Version, null, catalogo.Champions.Count,
                    creados, actualizados, creados > 0 || actualizados > 0,
                    cronometro.ElapsedMilliseconds, DateTime.UtcNow));
            }
            catch (Exception ex)
            {
                cronometro.Stop();
                logger.LogError(ex, "Fallo la sincronizacion del catalogo de Data Dragon.");

                await analyticsRepository.RegisterSyncAsync(
                    SyncLog.Fallo(SyncOrigin.Manual, "datadragon/champion.json", null,
                        ex.Message, cronometro.ElapsedMilliseconds), cancellationToken);

                return Result<SyncResultResponse>.Failure($"No se pudo sincronizar el catalogo: {ex.Message}", 502);
            }
        }

        private async Task AplicarDetalleAsync(ChampionProfile champion, string version, string? locale,
                                               CancellationToken cancellationToken)
        {
            try
            {
                var detalle = await dataDragon.GetChampionDetailAsync(champion.ChampionId, version, locale, cancellationToken);

                champion.ReemplazarHabilidades(detalle.Abilities.Select(a => (
                    Slot: (AbilitySlot)Math.Clamp(a.SlotIndex, 0, 4),
                    a.Name,
                    a.Description,
                    ImageUrl: string.IsNullOrWhiteSpace(a.ImageFile) ? null : a.ImageFile,
                    a.Cooldown)));

                championRepository.Update(champion);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "No se pudo traer el detalle de {Campeon}; se conserva la ficha resumida.",
                    champion.ChampionId);
            }
        }
    }
}
