using Galaxy.Lol.Domain.Ports.Cache;
using Galaxy.Lol.Infraestructure.Configuration.Mongo;
using Galaxy.Lol.Infraestructure.Configuration.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Galaxy.Lol.Infraestructure.Adapters.Cache
{

    public class MongoChampionRawCacheAdapter : IChampionRawCachePort
    {
        private readonly IMongoCollection<RawPayloadDocument> _catalogo;
        private readonly IMongoCollection<RawPayloadDocument> _detalle;
        private readonly ILogger<MongoChampionRawCacheAdapter> _logger;

        public MongoChampionRawCacheAdapter(IMongoDatabase database, IOptions<MongoSettings> options,
                                            ILogger<MongoChampionRawCacheAdapter> logger)
        {
            var settings = options.Value;
            _catalogo = database.GetCollection<RawPayloadDocument>(settings.CatalogCollection);
            _detalle = database.GetCollection<RawPayloadDocument>(settings.DetailCollection);
            _logger = logger;
        }

        public async Task SaveCatalogAsync(string version, string locale, string rawJson,
                                           CancellationToken cancellationToken = default) =>
            await GuardarAsync(_catalogo, FiltroCatalogo(version, locale),
                new RawPayloadDocument { Version = version, Locale = locale, Payload = rawJson },
                cancellationToken);

        public async Task<string?> GetCatalogAsync(string version, string locale,
                                                   CancellationToken cancellationToken = default) =>
            await LeerAsync(_catalogo, FiltroCatalogo(version, locale), cancellationToken);

        public async Task SaveChampionDetailAsync(string version, string locale, string championId, string rawJson,
                                                  CancellationToken cancellationToken = default) =>
            await GuardarAsync(_detalle, FiltroDetalle(version, locale, championId),
                new RawPayloadDocument { Version = version, Locale = locale, ChampionId = championId, Payload = rawJson },
                cancellationToken);

        public async Task<string?> GetChampionDetailAsync(string version, string locale, string championId,
                                                          CancellationToken cancellationToken = default) =>
            await LeerAsync(_detalle, FiltroDetalle(version, locale, championId), cancellationToken);

        public async Task<IReadOnlyCollection<string>> GetCachedVersionsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var versiones = await _catalogo.Distinct<string>("version", FilterDefinition<RawPayloadDocument>.Empty,
                    cancellationToken: cancellationToken).ToListAsync(cancellationToken);

                return versiones.OrderByDescending(v => v).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudieron listar las versiones cacheadas en Mongo.");
                return [];
            }
        }

        private static FilterDefinition<RawPayloadDocument> FiltroCatalogo(string version, string locale) =>
            Builders<RawPayloadDocument>.Filter.And(
                Builders<RawPayloadDocument>.Filter.Eq(d => d.Version, version),
                Builders<RawPayloadDocument>.Filter.Eq(d => d.Locale, locale));

        private static FilterDefinition<RawPayloadDocument> FiltroDetalle(string version, string locale, string championId) =>
            Builders<RawPayloadDocument>.Filter.And(
                FiltroCatalogo(version, locale),
                Builders<RawPayloadDocument>.Filter.Eq(d => d.ChampionId, championId));

        private async Task GuardarAsync(IMongoCollection<RawPayloadDocument> coleccion,
                                        FilterDefinition<RawPayloadDocument> filtro,
                                        RawPayloadDocument documento,
                                        CancellationToken cancellationToken)
        {
            try
            {
                await coleccion.ReplaceOneAsync(filtro, documento,
                    new ReplaceOptions { IsUpsert = true }, cancellationToken);
            }
            catch (Exception ex)
            {

                _logger.LogWarning(ex, "No se pudo escribir el cache en Mongo.");
            }
        }

        private async Task<string?> LeerAsync(IMongoCollection<RawPayloadDocument> coleccion,
                                              FilterDefinition<RawPayloadDocument> filtro,
                                              CancellationToken cancellationToken)
        {
            try
            {
                var documento = await coleccion.Find(filtro).FirstOrDefaultAsync(cancellationToken);
                return documento?.Payload;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo leer el cache de Mongo; se ira al CDN.");
                return null;
            }
        }
    }
}
