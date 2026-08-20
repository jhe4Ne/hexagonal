using Galaxy.Lol.Domain.Events.Interfaces;

namespace Galaxy.Lol.Domain.Entities
{

    public abstract class BaseEntity
    {
        public Guid Id { get; protected set; }
        public bool IsActive { get; protected set; }
        public DateTime CreatedAt { get; protected set; }
        public DateTime? UpdatedAt { get; protected set; }

        private readonly List<IDomainEvent> _domainEvents = [];
        public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        protected BaseEntity()
        {
            Id = Guid.NewGuid();
            IsActive = true;
            CreatedAt = DateTime.UtcNow;
        }

        protected void Touch() => UpdatedAt = DateTime.UtcNow;

        public void Delete()
        {
            IsActive = false;
            Touch();
        }

        public void Restore()
        {
            IsActive = true;
            Touch();
        }

        protected void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

        public void ClearDomainEvents() => _domainEvents.Clear();
    }
}
