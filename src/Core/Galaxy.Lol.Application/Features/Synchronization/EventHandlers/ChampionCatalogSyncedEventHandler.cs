using Galaxy.Lol.Domain.Events.Domain;
using Galaxy.Lol.Domain.Events.Interfaces;
using Microsoft.Extensions.Logging;

namespace Galaxy.Lol.Application.Features.Synchronization.EventHandlers
{
    public class ChampionCatalogSyncedEventHandler(ILogger<ChampionCatalogSyncedEventHandler> logger)
        : IDomainEventHandler<ChampionCatalogSyncedDomainEvent>
    {
        public Task HandleAsync(ChampionCatalogSyncedDomainEvent domainEvent, CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Catalogo sincronizado con la version {Version} ({Total} campeones).",
                domainEvent.Version, domainEvent.TotalCampeones);

            return Task.CompletedTask;
        }
    }
}
