using MassTransit;

namespace BubbleTea.Services.Cart.API.Features.Carts.CheckOutCartSaga;

public class CheckOutCartState : SagaStateMachineInstance, ISagaVersion
{
    public Guid CorrelationId { get; set; }
    public int Version { get; set; }
    public string CurrentState { get; set; }
    public int PaymentFinishedStatus { get; set; }
    public int CheckOutCompletedStatus { get; set; }
}
