using PaymentService.Domain.Entities;

namespace PaymentService.Application.Features.Payments.Dtos;

public record PaymentResponse(Guid OrderId, decimal Amount, string Status, string TransactionId)
{
    public static PaymentResponse FromEntity(Payment payment) => new(
        payment.OrderId,
        payment.Amount.Amount,
        payment.Status.ToString(),   // "Approved" o "Rejected"
        payment.TransactionId);
}