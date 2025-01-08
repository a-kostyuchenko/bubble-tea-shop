namespace BubbleTea.ServiceDefaults.Domain;

public interface IDomainEvent
{
    Guid Id { get; }
    DateTime OccurredOnUtc { get; }
}
