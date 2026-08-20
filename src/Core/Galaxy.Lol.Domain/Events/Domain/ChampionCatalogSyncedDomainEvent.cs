using Galaxy.Lol.Domain.Events.Interfaces;

namespace Galaxy.Lol.Domain.Events.Domain
{
    public class ChampionCatalogSyncedDomainEvent(string version, int totalCampeones) : IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
        public string Version { get; } = version;
        public int TotalCampeones { get; } = totalCampeones;
    }
}
