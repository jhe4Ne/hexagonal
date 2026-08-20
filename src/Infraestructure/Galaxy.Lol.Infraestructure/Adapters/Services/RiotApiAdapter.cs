using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Galaxy.Lol.Domain.Model.External;
using Galaxy.Lol.Domain.Ports.Services;
using Galaxy.Lol.Domain.ValueObjects;
using Galaxy.Lol.Infraestructure.Configuration.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Galaxy.Lol.Infraestructure.Adapters.Services
{

    public class RiotApiAdapter(
        IHttpClientFactory httpClientFactory,
        IOptions<RiotApiSettings> options,
        ILogger<RiotApiAdapter> logger) : IRiotApiPort
    {
        public const string HttpClientName = "riot-api";

        private readonly RiotApiSettings _settings = options.Value;
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        public async Task<RiotFreeRotation> GetFreeRotationAsync(string platform, CancellationToken cancellationToken = default)
        {
            var url = $"{_settings.BuildBaseUrl(platform)}/lol/platform/v3/champion-rotations";
            var payload = await GetAsync<RotationPayload>(url, cancellationToken);

            return new RiotFreeRotation(
                payload?.FreeChampionIds ?? [],
                payload?.FreeChampionIdsForNewPlayers ?? [],
                payload?.MaxNewPlayerLevel ?? 0);
        }

        public async Task<IReadOnlyCollection<RiotChampionMastery>> GetMasteriesAsync(
            Puuid puuid, string platform, CancellationToken cancellationToken = default)
        {
            var url = $"{_settings.BuildBaseUrl(platform)}/lol/champion-mastery/v4/champion-masteries/by-puuid/{puuid.Value}";
            return await GetMasteryListAsync(url, puuid, cancellationToken);
        }

        public async Task<IReadOnlyCollection<RiotChampionMastery>> GetTopMasteriesAsync(
            Puuid puuid, string platform, int count, CancellationToken cancellationToken = default)
        {
            var url = $"{_settings.BuildBaseUrl(platform)}/lol/champion-mastery/v4/champion-masteries/by-puuid/{puuid.Value}/top?count={count}";
            return await GetMasteryListAsync(url, puuid, cancellationToken);
        }

        public async Task<RiotAccount?> GetAccountByRiotIdAsync(
            string gameName, string tagLine, string platform, CancellationToken cancellationToken = default)
        {
            var url = $"{_settings.BuildAccountBaseUrl(platform)}/riot/account/v1/accounts/by-riot-id/" +
                      $"{Uri.EscapeDataString(gameName)}/{Uri.EscapeDataString(tagLine)}";

            var payload = await GetAsync<AccountPayload>(url, cancellationToken);

            return payload is null ? null : new RiotAccount(payload.Puuid, payload.GameName, payload.TagLine);
        }

        private async Task<IReadOnlyCollection<RiotChampionMastery>> GetMasteryListAsync(
            string url, Puuid puuid, CancellationToken cancellationToken)
        {
            var payload = await GetAsync<List<MasteryPayload>>(url, cancellationToken, puuid);

            return payload?
                .Select(m => new RiotChampionMastery(m.ChampionId, m.ChampionLevel, m.ChampionPoints,
                    m.LastPlayTime, m.ChestGranted, m.TokensEarned))
                .ToList() ?? [];
        }

        private async Task<T?> GetAsync<T>(string url, CancellationToken cancellationToken, Puuid? puuid = null)
        {
            var client = httpClientFactory.CreateClient(HttpClientName);

            var urlSegura = puuid is null ? url : url.Replace(puuid.Value, puuid.Masked);
            logger.LogDebug("GET {Url}", urlSegura);

            var response = await client.GetAsync(url, cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                logger.LogInformation("Riot respondio 404 para {Url}.", urlSegura);
                return default;
            }

            if (response.StatusCode == HttpStatusCode.Forbidden)
                throw new HttpRequestException(
                    "Riot rechazo la peticion (403). La API Key de desarrollo caduca cada 24 horas: renuevela en RIOT_API_KEY.");

            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            return await JsonSerializer.DeserializeAsync<T>(stream, JsonOptions, cancellationToken);
        }

        private record RotationPayload(
            [property: JsonPropertyName("sr")] List<int> FreeChampionIds,
            [property: JsonPropertyName("newplayer")] List<int> FreeChampionIdsForNewPlayers,
            int MaxNewPlayerLevel);

        private record MasteryPayload(
            int ChampionId,
            int ChampionLevel,
            long ChampionPoints,
            long LastPlayTime,
            bool ChestGranted,
            int TokensEarned);

        private record AccountPayload(string Puuid, string GameName, string TagLine);
    }
}
