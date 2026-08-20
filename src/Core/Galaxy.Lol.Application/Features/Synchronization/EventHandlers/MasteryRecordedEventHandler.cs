using Galaxy.Lol.Domain.Events.Domain;
using Galaxy.Lol.Domain.Events.Interfaces;
using Microsoft.Extensions.Logging;

namespace Galaxy.Lol.Application.Features.Synchronization.EventHandlers
{
    public class MasteryRecordedEventHandler(ILogger<MasteryRecordedEventHandler> logger)
        : IDomainEventHandler<MasteryRecordedDomainEvent>
    {
        public Task HandleAsync(MasteryRecordedDomainEvent domainEvent, CancellationToken cancellationToken = default)
        {

            logger.LogDebug("Maestria registrada: invocador {SummonerId}, campeon {ChampionKey}, {Puntos} puntos.",
                domainEvent.SummonerId, domainEvent.ChampionKey, domainEvent.Puntos);

            return Task.CompletedTask;
        }
    }
}
