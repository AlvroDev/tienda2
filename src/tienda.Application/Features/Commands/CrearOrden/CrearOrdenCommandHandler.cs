using MediatR;
using tienda.Application.Contracts.Persistence;
using tienda.Domain.Entities;
using tienda.Domain.Exceptions;

namespace tienda.Application.Features.Ordenes.Commands.CrearOrden;

public class CrearOrdenCommandHandler : IRequestHandler<CrearOrdenCommand, Guid>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;

    public CrearOrdenCommandHandler(IOrderRepository orderRepository, IProductRepository productRepository)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
    }

    public async Task<Guid> Handle(CrearOrdenCommand request, CancellationToken cancellationToken)
    {
        var order = Order.Create(request.UserId);

        foreach (var item in request.Items)
        {
            var product = await _productRepository.ObtenerPorIdAsync(item.ProductId, cancellationToken)
                ?? throw new NotFoundException("Producto", item.ProductId);

            // La regla de negocio (stock, producto activo, duplicados) la valida el dominio
            order.AddItem(product, item.Cantidad);
        }

        // Un solo SaveChanges guarda la orden, sus ítems y el stock descontado de cada producto
        await _orderRepository.AgregarAsync(order, cancellationToken);

        return order.Id;
    }
}