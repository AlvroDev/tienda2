using tienda.Domain.Entities;

namespace tienda.Application.Contracts.Persistence;

public interface IOrderRepository
{
    Task<Order?> ObtenerPorIdConItemsAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Order>> ObtenerPorUsuarioAsync(Guid userId, CancellationToken ct = default);
    Task AgregarAsync(Order order, CancellationToken ct = default);
}