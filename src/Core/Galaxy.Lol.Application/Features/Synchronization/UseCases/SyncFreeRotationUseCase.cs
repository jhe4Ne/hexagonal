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

    public class SyncFreeRotationUseCase(
        IRiotApiPort riotApi,
        IFreeRotationRepositoryPort rotationRepository,
        IChampionRepositoryPort championRepository,
        IAnalyticsRepositoryPort analyticsRepository,
        INotificationPort notificationPort,
        ILogger<SyncFreeRotationUseCase> logger) : ISyncFreeRotationUseCase
    {
        public async Task<Result<SyncResultResponse>> ExecuteAsync(
            SyncRotationRequest request, CancellationToken cancellationToken = default)
        {
            var cronometro = Stopwatch.StartNew();

            try
            {
                var remota = await riotApi.GetFreeRotationAsync(request.Platform, cancellationToken);

                var nueva = FreeRotation.Create(
                    request.Platform,
                    RotationPeriod.SemanaDe(DateTime.UtcNow),
                    remota.MaxNewPlayerLevel,
                    remota.FreeChampionIds,
                    remota.FreeChampionIdsForNewPlayers);

                var ultima = await rotationRepository.GetLatestAsync(request.Platform, cancellationToken);

                if (nueva.TieneMismoContenidoQue(ultima))
                {
                    cronometro.Stop();
                    logger.LogInformation("La rotacion de {Plataforma} no cambio (hash {Hash}).",
                        request.Platform, nueva.Hash[..8]);

                    return Result<SyncResultResponse>.Success(new SyncResultResponse(
                        "lol/platform/v3/champion-rotations", null, request.Platform,
                        remota.FreeChampionIds.Count, 0, 0, false,
                        cronometro.ElapsedMilliseconds, DateTime.UtcNow),
                        "La rotacion no ha cambiado desde la ultima sincronizacion.");
                }

                await rotationRepository.AddAsync(nueva, cancellationToken);
                cronometro.Stop();

                await analyticsRepository.RegisterSyncAsync(
                    SyncLog.Exito(SyncOrigin.Programada, "lol/platform/v3/champion-rotations", request.Platform,
                        remota.FreeChampionIds.Count, cronometro.ElapsedMilliseconds), cancellationToken);

                await AvisarCambioAsync(nueva, cancellationToken);

                return Result<SyncResultResponse>.Success(new SyncResultResponse(
                    "lol/platform/v3/champion-rotations", null, request.Platform,
                    remota.FreeChampionIds.Count, 1, 0, true,
                    cronometro.ElapsedMilliseconds, DateTime.UtcNow));
            }
            catch (Exception ex)
            {
                cronometro.Stop();
                logger.LogError(ex, "Fallo la sincronizacion de la rotacion de {Plataforma}.", request.Platform);

                await analyticsRepository.RegisterSyncAsync(
                    SyncLog.Fallo(SyncOrigin.Programada, "lol/platform/v3/champion-rotations", request.Platform,
                        ex.Message, cronometro.ElapsedMilliseconds), cancellationToken);

                return Result<SyncResultResponse>.Failure($"No se pudo sincronizar la rotacion: {ex.Message}", 502);
            }
        }

        private async Task AvisarCambioAsync(FreeRotation rotacion, CancellationToken cancellationToken)
        {
            try
            {
                var campeones = await championRepository.GetByKeysAsync(rotacion.ClavesGenerales.ToList(), cancellationToken);
                var nombres = campeones.Select(c => c.Name).OrderBy(n => n).ToList();

                await notificationPort.NotifyRotationChangedAsync(rotacion.Platform, nombres, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "La rotacion se guardo pero no se pudo enviar el aviso.");
            }
        }
    }
}
