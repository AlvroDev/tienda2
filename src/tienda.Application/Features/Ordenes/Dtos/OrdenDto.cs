using tienda.Domain.Entities;

namespace tienda.Application.Features.Ordenes.Dtos;

public class OrdenDto
{
    public Guid Id { get; set; }
    public DateTime Fecha { get; set; }
    public string Estado { get; set; } = null!;
    public decimal Total { get; set; }
    public string Moneda { get; set; } = null!;
    public string? TransaccionId { get; set; }
    public List<OrdenItemDto> Items { get; set; } = new();

    // Mapeo entidad → DTO en un solo lugar (lo usan las dos queries)
    public static OrdenDto DesdeEntidad(Order order) => new()
    {
        Id = order.Id,
        Fecha = order.CreatedAt,
        Estado = order.Status.ToString(),
        Total = order.Total.Amount,
        Moneda = order.Total.Currency,
        TransaccionId = order.PaymentTransactionId,
        Items = order.Items.Select(i => new OrdenItemDto
        {
            ProductoId = i.ProductId,
            NombreProducto = i.ProductName,
            PrecioUnitario = i.UnitPrice,
            Cantidad = i.Quantity,
            Subtotal = i.Subtotal
        }).ToList()
    };
}

public class OrdenItemDto
{
    public Guid ProductoId { get; set; }
    public string NombreProducto { get; set; } = null!;
    public decimal PrecioUnitario { get; set; }
    public int Cantidad { get; set; }
    public decimal Subtotal { get; set; }
}