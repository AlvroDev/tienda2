using tienda.Domain.Enums;
using tienda.Domain.Exceptions;
using tienda.Domain.ValueObjects;

namespace tienda.Domain.Entities;

public class Order : BaseEntity
{
    public Guid UserId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public OrderStatus Status { get; private set; }
    public Money Total { get; private set; } = null!;
    public string? PaymentTransactionId { get; private set; }

    private readonly List<OrderItem> _items = new();
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    private Order() { } // para EF Core

    public static Order Create(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new DomainException("La orden debe pertenecer a un usuario.");

        return new Order
        {
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            Status = OrderStatus.Pending,
            Total = Money.Create(0)
        };
    }

    public void AddItem(Product product, int quantity)
    {
        EnsurePending();

        if (!product.IsActive)
            throw new DomainException($"El producto '{product.Name}' no está disponible.");

        if (_items.Any(i => i.ProductId == product.Id))
            throw new DomainException($"El producto '{product.Name}' ya está en la orden.");

        // Regla de negocio en el dominio: valida cantidad > 0 y stock suficiente
        product.RemoveStock(quantity);

        var item = new OrderItem(Id, product, quantity);
        _items.Add(item);
        Total = Total.Add(Money.Create(item.Subtotal));
    }

    public void MarkAsPaid(string transactionId)
    {
        EnsurePending();

        if (string.IsNullOrWhiteSpace(transactionId))
            throw new DomainException("Un pago aprobado debe tener número de transacción.");

        Status = OrderStatus.Paid;
        PaymentTransactionId = transactionId;
    }

    public void RejectPayment()
    {
        EnsurePending();
        Status = OrderStatus.PaymentRejected;
    }

    private void EnsurePending()
    {
        if (Status != OrderStatus.Pending)
            throw new DomainException($"La orden ya fue procesada (estado: {Status}).");
    }
}