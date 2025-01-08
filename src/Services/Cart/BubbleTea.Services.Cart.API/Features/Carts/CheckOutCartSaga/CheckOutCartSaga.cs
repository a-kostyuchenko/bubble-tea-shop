using BubbleTea.Contracts;
using MassTransit;

namespace BubbleTea.Services.Cart.API.Features.Carts.CheckOutCartSaga;

public class CheckOutCartSaga : MassTransitStateMachine<CheckOutCartState>
{
    public State CheckOutStarted { get; set; }
    public State OrderCreated { get; set; }
    public State PaymentProcessed { get; set; }
    public State InvoiceFormed { get; set; }
    public State OrderPaid { get; set; }
    public State PaymentFailed { get; set; }
    
    public Event<CartCheckedOutEvent> CheckOutCartEvent { get; set; }
    public Event<OrderCreatedEvent> OrderCreatedEvent { get; set; }
    public Event<PaymentProcessedEvent> PaymentProcessedEvent { get; set; }
    public Event<InvoiceFormedEvent> InvoiceFormedEvent { get; set; }
    public Event<OrderPaidEvent> OrderPaidEvent { get; set; }
    public Event<PaymentFailedEvent> PaymentFailedEvent { get; set; }
    public Event PaymentFinished { get; set; }
    public Event OrderCheckOutCompleted { get; set; }

    public CheckOutCartSaga()
    {
        Event(() => CheckOutCartEvent, c => c.CorrelateById(m => m.Message.CartId));
        Event(() => OrderCreatedEvent, c => c.CorrelateById(m => m.Message.OrderId));
        Event(() => PaymentProcessedEvent, c => c.CorrelateById(m => m.Message.OrderId));
        Event(() => InvoiceFormedEvent, c => c.CorrelateById(m => m.Message.OrderId));
        Event(() => OrderPaidEvent, c => c.CorrelateById(m => m.Message.OrderId));
        Event(() => PaymentFailedEvent, c => c.CorrelateById(m => m.Message.OrderId));
        
        InstanceState(s => s.CurrentState);
        
        Initially(
            When(CheckOutCartEvent)
                .Publish(context => new CheckOutCartStartedEvent(
                    context.Message.Id,
                    context.Message.OccurredOnUtc,
                    context.Message.CartId,
                    context.Message.Customer,
                    context.Message.Note,
                    context.Message.CardNumber,
                    context.Message.ExpiryMonth,
                    context.Message.ExpiryYear,
                    context.Message.CVV,
                    context.Message.CardHolderName,
                    context.Message.TotalAmount,
                    context.Message.Currency,
                    context.Message.Items))
                .TransitionTo(CheckOutStarted));

        During(CheckOutStarted,
            When(OrderCreatedEvent)
                .TransitionTo(OrderCreated),
            When(PaymentProcessedEvent)
                .TransitionTo(PaymentProcessed));
        
        DuringAny(
            When(PaymentFailedEvent)
                .TransitionTo(PaymentFailed)
                .Finalize());
        
        During(OrderCreated,
            When(PaymentProcessedEvent)
                .TransitionTo(PaymentProcessed));

        During(PaymentProcessed,
            When(OrderCreatedEvent)
                .TransitionTo(OrderCreated));
        
        During(PaymentProcessed,
            When(InvoiceFormedEvent)
                .TransitionTo(InvoiceFormed));
        
        During(OrderCreated,
            When(InvoiceFormedEvent)
                .TransitionTo(InvoiceFormed));
        
        CompositeEvent(
            () => PaymentFinished,
            state => state.PaymentFinishedStatus,
            OrderCreatedEvent, PaymentProcessedEvent, InvoiceFormedEvent);
        
        DuringAny(
            When(PaymentFinished)
                .Publish(context =>
                    new PaymentFinishedEvent(
                        context.MessageId ?? Guid.NewGuid(),
                        DateTime.UtcNow,
                        context.Saga.CorrelationId)));
        
        DuringAny(
            When(OrderPaidEvent)
                .TransitionTo(OrderPaid));
        
        CompositeEvent(
            () => OrderCheckOutCompleted,
            state => state.CheckOutCompletedStatus,
            OrderCreatedEvent, PaymentProcessedEvent, OrderPaidEvent);
        
        DuringAny(
            When(OrderCheckOutCompleted)
                .Publish(context =>
                    new CartCheckOutCompletedEvent(
                        Guid.NewGuid(),
                        DateTime.UtcNow,
                        context.Saga.CorrelationId))
                .Finalize());
        
        SetCompletedWhenFinalized();
    }
}
