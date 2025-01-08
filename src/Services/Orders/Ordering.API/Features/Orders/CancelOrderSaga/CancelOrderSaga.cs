using BubbleTeaShop.Contracts;
using MassTransit;

namespace Ordering.API.Features.Orders.CancelOrderSaga;

public sealed class CancelOrderSaga : MassTransitStateMachine<CancelOrderState>
{
    public State CancellationStarted { get; private set; }
    public State PaymentRefunded { get; private set; }
    public State NotificationSent { get; private set; }
    
    public Event<OrderCancelledEvent> OrderCancelled { get; private set; }
    public Event<OrderPaymentRefundedEvent> OrderPaymentRefunded { get; private set; }
    public Event<OrderCancelledNotificationSentEvent> OrderCancelledNotificationSent { get; private set; }

    public Event OrderCancellationCompleted { get; private set; }

    public CancelOrderSaga()
    {
        Event(() => OrderCancelled, c => c.CorrelateById(m => m.Message.OrderId));
        Event(() => OrderPaymentRefunded, c => c.CorrelateById(m => m.Message.OrderId));
        Event(() => OrderCancelledNotificationSent, c => c.CorrelateById(m => m.Message.OrderId));
        
        InstanceState(s => s.CurrentState);
        
        Initially(
            When(OrderCancelled)
                .Publish(context => new OrderCancellationStartedEvent(
                    context.Message.Id,
                    context.Message.OccurredOnUtc,
                    context.Message.OrderId))
                .TransitionTo(CancellationStarted));
        
        During(CancellationStarted,
            When(OrderPaymentRefunded)
                .TransitionTo(PaymentRefunded),
            When(OrderCancelledNotificationSent)
                .TransitionTo(NotificationSent));
        
        During(PaymentRefunded,
            When(OrderCancelledNotificationSent)
                .TransitionTo(NotificationSent));
        
        During(NotificationSent,
            When(OrderPaymentRefunded)
                .TransitionTo(PaymentRefunded));
        
        CompositeEvent(
            () => OrderCancellationCompleted,
            state => state.CancellationCompletedStatus,
            OrderPaymentRefunded, OrderCancelledNotificationSent);
        
        DuringAny(
            When(OrderCancellationCompleted)
                .Publish(context => new OrderCancellationCompletedEvent(
                    Guid.NewGuid(),
                    DateTime.UtcNow,
                    context.Saga.CorrelationId))
                .Finalize());
    }
}
