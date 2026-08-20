using Galaxy.Lol.Infraestructure.Configuration.Settings;
using Microsoft.Extensions.Options;

namespace Galaxy.Lol.Infraestructure.Adapters.Services.Handlers
{

    public class RiotApiKeyHandler(IOptions<RiotApiSettings> options) : DelegatingHandler
    {
        private readonly RiotApiSettings _settings = options.Value;

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(_settings.ApiKey))
                throw new InvalidOperationException(
                    "No hay Riot API Key configurada. Defina la variable de entorno RIOT_API_KEY " +
                    "antes de levantar la aplicacion.");

            request.Headers.Remove(_settings.ApiKeyHeader);
            request.Headers.Add(_settings.ApiKeyHeader, _settings.ApiKey);

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
