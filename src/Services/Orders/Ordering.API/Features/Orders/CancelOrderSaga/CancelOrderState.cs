using MassTransit;

namespace Ordering.API.Features.Orders.CancelOrderSaga;

public sealed class CancelOrderState : SagaStateMachineInstance, ISagaVersion
{
    public Guid CorrelationId { get; set; }
    public int Version { get; set; }
    public string CurrentState { get; set; }
    public int CancellationCompletedStatus { get; set; }
}
