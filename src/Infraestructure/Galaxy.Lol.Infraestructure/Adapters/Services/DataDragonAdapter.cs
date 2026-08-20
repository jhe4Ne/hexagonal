using System.Text.Json;
using Galaxy.Lol.Domain.Model.External;
using Galaxy.Lol.Domain.Ports.Cache;
using Galaxy.Lol.Domain.Ports.Services;
using Galaxy.Lol.Infraestructure.Configuration.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Galaxy.Lol.Infraestructure.Adapters.Services
{

    public class DataDragonAdapter(
        IHttpClientFactory httpClientFactory,
        IChampionRawCachePort rawCache,
        IOptions<DataDragonSettings> options,
        ILogger<DataDragonAdapter> logger) : IDataDragonPort
    {
        public const string HttpClientName = "data-dragon";

        private readonly DataDragonSettings _settings = options.Value;
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        public async Task<string> GetLatestVersionAsync(CancellationToken cancellationToken = default)
        {
            if (!string.IsNullOrWhiteSpace(_settings.PinnedVersion))
                return _settings.PinnedVersion!;

            var client = httpClientFactory.CreateClient(HttpClientName);
            var versiones = await client.GetFromJsonSafeAsync<List<string>>("/api/versions.json", JsonOptions, cancellationToken);

            return versiones?.FirstOrDefault()
                   ?? throw new InvalidOperationException("Data Dragon no devolvio ninguna version en versions.json.");
        }

        public async Task<DataDragonCatalog> GetChampionCatalogAsync(
            string? version = null, string? locale = null, CancellationToken cancellationToken = default)
        {
            var v = string.IsNullOrWhiteSpace(version) ? await GetLatestVersionAsync(cancellationToken) : version!;
            var l = string.IsNullOrWhiteSpace(locale) ? _settings.DefaultLocale : locale!;

            var json = await rawCache.GetCatalogAsync(v, l, cancellationToken);
            if (json is null)
            {
                var client = httpClientFactory.CreateClient(HttpClientName);
                json = await client.GetStringAsync($"/cdn/{v}/data/{l}/champion.json", cancellationToken);
                await rawCache.SaveCatalogAsync(v, l, json, cancellationToken);
                logger.LogInformation("Catalogo {Version}/{Locale} descargado del CDN y cacheado.", v, l);
            }

            var payload = JsonSerializer.Deserialize<CatalogPayload>(json, JsonOptions)
                          ?? throw new InvalidOperationException("El champion.json de Data Dragon llego vacio.");

            var campeones = payload.Data.Values.Select(MapearResumen).ToList();

            return new DataDragonCatalog(payload.Version ?? v, l, campeones);
        }

        public string BuildChampionImageUrl(string version, string imageFile) =>
            $"{_settings.BaseUrl.TrimEnd('/')}/cdn/{version}/img/champion/{imageFile}";

        public async Task<DataDragonChampionDetail> GetChampionDetailAsync(
            string championId, string? version = null, string? locale = null,
            CancellationToken cancellationToken = default)
        {
            var v = string.IsNullOrWhiteSpace(version) ? await GetLatestVersionAsync(cancellationToken) : version!;
            var l = string.IsNullOrWhiteSpace(locale) ? _settings.DefaultLocale : locale!;

            var json = await rawCache.GetChampionDetailAsync(v, l, championId, cancellationToken);
            if (json is null)
            {
                var client = httpClientFactory.CreateClient(HttpClientName);
                json = await client.GetStringAsync($"/cdn/{v}/data/{l}/champion/{championId}.json", cancellationToken);
                await rawCache.SaveChampionDetailAsync(v, l, championId, json, cancellationToken);
            }

            var payload = JsonSerializer.Deserialize<CatalogPayload>(json, JsonOptions)
                          ?? throw new InvalidOperationException($"El detalle de {championId} llego vacio.");

            var crudo = payload.Data.Values.First();
            var habilidades = new List<DataDragonAbility>();

            if (crudo.Passive is not null)
                habilidades.Add(new DataDragonAbility(0, crudo.Passive.Name ?? "Pasiva",
                    crudo.Passive.Description, crudo.Passive.Image?.Full, null));

            for (var i = 0; i < (crudo.Spells?.Count ?? 0); i++)
            {
                var spell = crudo.Spells![i];
                habilidades.Add(new DataDragonAbility(i + 1, spell.Name ?? $"Habilidad {i + 1}",
                    spell.Description, spell.Image?.Full,
                    spell.Cooldown is { Count: > 0 } ? (int)spell.Cooldown[0] : null));
            }

            return new DataDragonChampionDetail(MapearResumen(crudo), crudo.Lore, habilidades);
        }

        private static DataDragonChampionSummary MapearResumen(ChampionPayload c) =>
            new(c.Id ?? string.Empty,
                int.TryParse(c.Key, out var key) ? key : 0,
                c.Name ?? string.Empty,
                c.Title ?? string.Empty,
                c.Blurb,
                c.Image?.Full,
                c.Tags ?? [],
                c.Info?.Difficulty ?? 0,
                new DataDragonStats(
                    c.Stats?.Hp ?? 0, c.Stats?.Mp ?? 0, c.Stats?.Armor ?? 0, c.Stats?.SpellBlock ?? 0,
                    c.Stats?.AttackDamage ?? 0, c.Stats?.AttackSpeed ?? 0, c.Stats?.MoveSpeed ?? 0));

        private record CatalogPayload(string? Version, Dictionary<string, ChampionPayload> Data);

        private record ChampionPayload(
            string? Id, string? Key, string? Name, string? Title, string? Blurb, string? Lore,
            ImagePayload? Image, List<string>? Tags, InfoPayload? Info, StatsPayload? Stats,
            List<SpellPayload>? Spells, PassivePayload? Passive);

        private record ImagePayload(string? Full);
        private record InfoPayload(int Attack, int Defense, int Magic, int Difficulty);
        private record SpellPayload(string? Id, string? Name, string? Description, ImagePayload? Image, List<double>? Cooldown);
        private record PassivePayload(string? Name, string? Description, ImagePayload? Image);

        private record StatsPayload(
            double Hp, double Mp, double Armor, double SpellBlock,
            double AttackDamage, double AttackSpeed, double MoveSpeed);
    }
}
