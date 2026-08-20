using Galaxy.Lol.Domain.Events.Domain;
using Galaxy.Lol.Domain.Events.Interfaces;
using Microsoft.Extensions.Logging;

namespace Galaxy.Lol.Application.Features.Synchronization.EventHandlers
{

    public class FreeRotationChangedEventHandler(ILogger<FreeRotationChangedEventHandler> logger)
        : IDomainEventHandler<FreeRotationChangedDomainEvent>
    {
        public Task HandleAsync(FreeRotationChangedDomainEvent domainEvent, CancellationToken cancellationToken = default)
        {
            logger.LogInformation(
                "Rotacion gratuita de {Plataforma} actualizada con {Total} campeones ({Evento}).",
                domainEvent.Plataforma, domainEvent.ChampionKeys.Count, domainEvent.EventId);

            return Task.CompletedTask;
        }
    }
}
