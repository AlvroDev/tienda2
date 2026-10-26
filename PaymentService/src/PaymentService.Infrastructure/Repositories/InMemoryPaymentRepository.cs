using System.Collections.Concurrent;
using PaymentService.Application.Contracts;
using PaymentService.Domain.Entities;

namespace PaymentService.Infrastructure.Repositories;

// Almacenamiento en memoria: los pagos se pierden al reiniciar el servicio.
// La consigna lo permite para un servicio simple (aclarado en el README).
public class InMemoryPaymentRepository : IPaymentRepository
{
    // ConcurrentDictionary: seguro si llegan varias requests al mismo tiempo
    private readonly ConcurrentDictionary<Guid, Payment> _paymentsByOrderId = new();

    public Task<Payment?> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default)
    {
        _paymentsByOrderId.TryGetValue(orderId, out var payment);
        return Task.FromResult(payment);
    }

    public Task AddAsync(Payment payment, CancellationToken ct = default)
    {
        _paymentsByOrderId.TryAdd(payment.OrderId, payment);
        return Task.CompletedTask;
    }
}