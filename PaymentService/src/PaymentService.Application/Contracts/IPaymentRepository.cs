using PaymentService.Domain.Entities;

namespace PaymentService.Application.Contracts;

public interface IPaymentRepository
{
    Task<Payment?> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default);
    Task AddAsync(Payment payment, CancellationToken ct = default);
}