namespace BubbleTea.ServiceDefaults.Domain;

public abstract class Entity : IEntity
{
    private readonly List<IDomainEvent> _domainEvents = [];

    protected Entity(Ulid id) : this()
    {
        Id = id.ToGuid();
    }

    protected Entity()
    {
    }

    public Guid Id { get; protected init; } = Ulid.NewUlid().ToGuid();

    public IReadOnlyCollection<IDomainEvent> DomainEvents => [.._domainEvents];

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    public void Raise(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }
}
