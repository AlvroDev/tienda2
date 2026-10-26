using PaymentService.Domain.Enums;
using PaymentService.Domain.Exceptions;
using PaymentService.Domain.ValueObjects;

namespace PaymentService.Domain.Entities;

public class Payment
{
    // REGLA DE NEGOCIO: se aprueban los pagos de hasta 100.000 inclusive
    public const decimal MaxApprovedAmount = 100_000m;

    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public Money Amount { get; private set; } = null!;
    public PaymentStatus Status { get; private set; }
    public string TransactionId { get; private set; } = null!;
    public DateTime ProcessedAt { get; private set; }

    private Payment() { }

    // Factory method: la única forma de crear un pago es procesándolo con la regla
    public static Payment Process(Guid orderId, Money amount)
    {
        if (orderId == Guid.Empty)
            throw new DomainException("El pago debe estar asociado a una orden.");

        return new Payment
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            Amount = amount,
            Status = amount.Amount <= MaxApprovedAmount
                ? PaymentStatus.Approved
                : PaymentStatus.Rejected,
            TransactionId = $"TX-{Guid.NewGuid().ToString("N")[..8].ToUpper()}",
            ProcessedAt = DateTime.UtcNow
        };
    }
}