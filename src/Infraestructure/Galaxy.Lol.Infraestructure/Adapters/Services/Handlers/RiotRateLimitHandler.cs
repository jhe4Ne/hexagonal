using System.Net;
using System.Threading.RateLimiting;
using Galaxy.Lol.Infraestructure.Configuration.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Galaxy.Lol.Infraestructure.Adapters.Services.Handlers
{

    public class RiotRateLimitHandler : DelegatingHandler
    {
        private readonly SlidingWindowRateLimiter _limiter;
        private readonly ILogger<RiotRateLimitHandler> _logger;
        private readonly int _maxRetries;

        public RiotRateLimitHandler(IOptions<RiotApiSettings> options, ILogger<RiotRateLimitHandler> logger)
        {
            var settings = options.Value;
            _logger = logger;
            _maxRetries = settings.MaxRetries;

            _limiter = new SlidingWindowRateLimiter(new SlidingWindowRateLimiterOptions
            {
                PermitLimit = settings.RequestsPerSecond,
                Window = TimeSpan.FromSeconds(1),
                SegmentsPerWindow = 4,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 200
            });
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            for (var intento = 0; ; intento++)
            {
                using var lease = await _limiter.AcquireAsync(1, cancellationToken);

                var response = await base.SendAsync(request, cancellationToken);

                var esTransitorio = response.StatusCode is HttpStatusCode.TooManyRequests
                    or HttpStatusCode.BadGateway
                    or HttpStatusCode.ServiceUnavailable
                    or HttpStatusCode.GatewayTimeout;

                if (!esTransitorio || intento >= _maxRetries)
                    return response;

                var espera = response.Headers.RetryAfter?.Delta ?? TimeSpan.FromSeconds(1);
                _logger.LogWarning("Riot devolvio {Codigo}; reintentando en {Segundos}s (intento {Intento}).",
                    (int)response.StatusCode, espera.TotalSeconds, intento + 1);

                response.Dispose();
                await Task.Delay(espera, cancellationToken);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _limiter.Dispose();
            base.Dispose(disposing);
        }
    }
}
