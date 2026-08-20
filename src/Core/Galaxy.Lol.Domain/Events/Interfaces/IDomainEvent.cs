namespace Galaxy.Lol.Domain.Events.Interfaces
{

    public interface IDomainEvent
    {
        Guid EventId { get; }
        DateTime OccurredOn { get; }
    }
}
