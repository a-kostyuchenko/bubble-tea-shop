using ServiceDefaults.Domain;

namespace Payment.Domain.Payments;

public sealed class Payment : Entity
{
    public Guid OrderId { get; private set; }
    public Guid TransactionId { get; private set; }
    public Money Amount { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public PaymentInfo PaymentInfo { get; set; }
    
    private Payment()
    {
    }
    
    public static Payment Process(
        Guid orderId,
        Guid transactionId,
        Money amount,
        PaymentInfo paymentInfo)
    {
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            TransactionId = transactionId,
            Amount = amount,
            CreatedAtUtc = DateTime.UtcNow,
            PaymentInfo = paymentInfo
        };
        
        payment.Raise(new PaymentProcessedDomainEvent(payment.Id, payment.OrderId));
        
        return payment;
    }
}
