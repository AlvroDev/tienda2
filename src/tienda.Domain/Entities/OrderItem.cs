namespace tienda.Domain.Entities;

public class OrderItem : BaseEntity
{
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; } = null!;
    public decimal UnitPrice { get; private set; }
    public int Quantity { get; private set; }
    public decimal Subtotal => UnitPrice * Quantity; // calculado, no se guarda

    private OrderItem() { } // para EF Core

    // internal: solo Order (mismo proyecto) puede crear ítems
    internal OrderItem(Guid orderId, Product product, int quantity)
    {
        OrderId = orderId;
        ProductId = product.Id;
        ProductName = product.Name;   // copia del nombre al momento de comprar
        UnitPrice = product.Price;    // copia del precio al momento de comprar
        Quantity = quantity;
    }
}