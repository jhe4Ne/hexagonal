using Galaxy.Lol.Domain.Events.Interfaces;

namespace Galaxy.Lol.Domain.Events.Domain
{

    public class FreeRotationChangedDomainEvent(string plataforma, IReadOnlyCollection<int> championKeys) : IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
        public string Plataforma { get; } = plataforma;
        public IReadOnlyCollection<int> ChampionKeys { get; } = championKeys;
    }
}
