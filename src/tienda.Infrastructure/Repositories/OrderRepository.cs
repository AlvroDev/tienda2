using Microsoft.EntityFrameworkCore;
using tienda.Application.Contracts.Persistence;
using tienda.Domain.Entities;
using tienda.Infrastructure.Persistence;

namespace tienda.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _context;

    public OrderRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Order?> ObtenerPorIdConItemsAsync(Guid id, CancellationToken ct = default)
    {
        // Eager loading: trae la orden con sus ítems (JOIN con OrderItems)
        return await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id, ct);
    }

    public async Task<IReadOnlyList<Order>> ObtenerPorUsuarioAsync(Guid userId, CancellationToken ct = default)
    {
        // AsNoTracking: solo lectura, EF no necesita seguir los cambios
        return await _context.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task AgregarAsync(Order order, CancellationToken ct = default)
    {
        await _context.Orders.AddAsync(order, ct);
        await _context.SaveChangesAsync(ct);
    }
    public async Task GuardarCambiosAsync(CancellationToken ct = default)
    {
        await _context.SaveChangesAsync(ct);
    }
}