using Galaxy.Lol.Domain.Events.Interfaces;

namespace Galaxy.Lol.Domain.Events.Domain
{
    public class MasteryRecordedDomainEvent(Guid summonerId, int championKey, long puntos) : IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
        public Guid SummonerId { get; } = summonerId;
        public int ChampionKey { get; } = championKey;
        public long Puntos { get; } = puntos;
    }
}
