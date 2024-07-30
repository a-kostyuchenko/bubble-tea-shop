namespace ServiceDefaults.Domain;

public interface IEntity
{
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }
    void ClearDomainEvents();
    void Raise(IDomainEvent domainEvent);
}
