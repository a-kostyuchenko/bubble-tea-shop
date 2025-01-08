using MassTransit;

namespace BubbleTea.Services.Orders.API.Features.Orders.CancelOrderSaga;

public sealed class CancelOrderState : SagaStateMachineInstance, ISagaVersion
{
    public Guid CorrelationId { get; set; }
    public int Version { get; set; }
    public string CurrentState { get; set; }
    public int CancellationCompletedStatus { get; set; }
}
