using System.Diagnostics;
using Galaxy.Lol.Domain.Entities;
using Galaxy.Lol.Domain.Enums;
using Galaxy.Lol.Domain.Exceptions;
using Galaxy.Lol.Domain.Ports.Repositories;
using Galaxy.Lol.Domain.Ports.Services;
using Galaxy.Lol.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Galaxy.Lol.Application.Features.Masteries.Services
{

    public class SummonerMasteryLoader(
        IRiotApiPort riotApi,
        ISummonerRepositoryPort summonerRepository,
        IAnalyticsRepositoryPort analyticsRepository,
        ILogger<SummonerMasteryLoader> logger)
    {

        public async Task<Puuid> ResolveAsync(string gameName, string tagLine, string platform,
                                              CancellationToken cancellationToken = default)
        {
            var cuenta = await riotApi.GetAccountByRiotIdAsync(gameName, tagLine, platform, cancellationToken);

            if (cuenta is null)
                throw new RiotAccountNotFoundException(gameName, tagLine);

            return Puuid.Create(cuenta.Puuid);
        }

        public async Task<Summoner?> LoadAsync(Puuid puuid, string gameName, string tagLine, string platform,
                                               bool refresh, CancellationToken cancellationToken = default)
        {
            var summoner = await summonerRepository.GetByPuuidAsync(puuid, cancellationToken);

            if (summoner is not null)
            {

                if (summoner.GameName != gameName || summoner.TagLine != tagLine)
                {
                    summoner.ActualizarIdentidad(gameName, tagLine);
                    summonerRepository.Update(summoner);
                }

                if (!refresh) return summoner;
            }

            var cronometro = Stopwatch.StartNew();
            try
            {
                var maestrias = await riotApi.GetMasteriesAsync(puuid, platform, cancellationToken);

                var esNuevo = summoner is null;
                if (summoner is null)
                {
                    summoner = Summoner.Create(puuid, platform, gameName, tagLine);
                    await summonerRepository.AddAsync(summoner, cancellationToken);
                }

                foreach (var m in maestrias)
                {
                    summoner.RegistrarMaestria(
                        ChampionKey.Create(m.ChampionId),
                        MasteryScore.Create(m.ChampionPoints, m.ChampionLevel),
                        DesdeEpoch(m.LastPlayTime),
                        m.ChestGranted,
                        m.TokensEarned);
                }

                summoner.MarcarSincronizado();
                if (!esNuevo) summonerRepository.Update(summoner);

                cronometro.Stop();
                await RegistrarBitacoraAsync(SyncLog.Exito(SyncOrigin.Manual, "champion-mastery-v4", platform,
                    maestrias.Count, cronometro.ElapsedMilliseconds), cancellationToken);

                return summoner;
            }
            catch (Exception ex)
            {
                cronometro.Stop();

                logger.LogError(ex, "Fallo la sincronizacion de maestrias de {Puuid} en {Plataforma}.",
                    puuid.Masked, platform);

                await RegistrarBitacoraAsync(SyncLog.Fallo(SyncOrigin.Manual, "champion-mastery-v4", platform,
                    ex.Message, cronometro.ElapsedMilliseconds), cancellationToken);

                if (summoner is not null) return summoner;
                throw;
            }
        }

        private async Task RegistrarBitacoraAsync(SyncLog log, CancellationToken cancellationToken)
        {
            try
            {
                await analyticsRepository.RegisterSyncAsync(log, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "No se pudo registrar la bitacora de sincronizacion en bdanalitica.");
            }
        }

        private static DateTime? DesdeEpoch(long milisegundos) =>
            milisegundos <= 0 ? null : DateTimeOffset.FromUnixTimeMilliseconds(milisegundos).UtcDateTime;
    }
}
