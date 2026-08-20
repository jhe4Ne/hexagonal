using Galaxy.Lol.Domain.Entities;
using Galaxy.Lol.Domain.Events.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Galaxy.Lol.Infraestructure.Adapters.Services
{

    public class DomainEventDispatcher(IServiceProvider serviceProvider, ILogger<DomainEventDispatcher> logger)
        : IDomainEventDispatcher
    {
        public async Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents,
                                        CancellationToken cancellationToken = default)
        {
            foreach (var domainEvent in domainEvents)
            {
                var tipoManejador = typeof(IDomainEventHandler<>).MakeGenericType(domainEvent.GetType());
                var manejadores = serviceProvider.GetServices(tipoManejador);

                foreach (var manejador in manejadores)
                {
                    if (manejador is null) continue;

                    var metodo = tipoManejador.GetMethod(nameof(IDomainEventHandler<IDomainEvent>.HandleAsync));
                    if (metodo is null) continue;

                    try
                    {
                        await (Task)metodo.Invoke(manejador, [domainEvent, cancellationToken])!;
                    }
                    catch (Exception ex)
                    {

                        logger.LogError(ex, "Fallo el manejador {Manejador} del evento {Evento}.",
                            manejador.GetType().Name, domainEvent.GetType().Name);
                    }
                }
            }
        }

        public static IReadOnlyCollection<IDomainEvent> Recolectar(DbContext context)
        {
            var entidades = context.ChangeTracker
                .Entries<BaseEntity>()
                .Where(e => e.Entity.DomainEvents.Count != 0)
                .Select(e => e.Entity)
                .ToList();

            var eventos = entidades.SelectMany(e => e.DomainEvents).ToList();
            entidades.ForEach(e => e.ClearDomainEvents());

            return eventos;
        }
    }
}
