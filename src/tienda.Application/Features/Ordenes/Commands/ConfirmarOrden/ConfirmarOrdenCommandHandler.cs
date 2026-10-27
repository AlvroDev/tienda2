using MediatR;
using tienda.Application.Contracts.Payments;
using tienda.Application.Contracts.Persistence;
using tienda.Application.Features.Ordenes.Dtos;
using tienda.Domain.Exceptions;

namespace tienda.Application.Features.Ordenes.Commands.ConfirmarOrden;

public class ConfirmarOrdenCommandHandler : IRequestHandler<ConfirmarOrdenCommand, OrdenDto>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IPaymentClient _paymentClient;

    public ConfirmarOrdenCommandHandler(
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        IPaymentClient paymentClient)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _paymentClient = paymentClient;
    }

    public async Task<OrdenDto> Handle(ConfirmarOrdenCommand request, CancellationToken cancellationToken)
    {
        // 1. Buscar la orden (si no existe o es de otro usuario → 404)
        var order = await _orderRepository.ObtenerPorIdConItemsAsync(request.OrderId, cancellationToken);
        if (order is null || order.UserId != request.UserId)
            throw new NotFoundException("Orden", request.OrderId);

        // 2. Regla del dominio: solo se pagan órdenes pendientes (si no → 422, sin llamar al servicio)
        order.EnsurePending();

        // 3. Llamar al PaymentService. Si está caído, IPaymentClient lanza
        //    ServiceUnavailableException y la orden queda Pending, sin cambios
        var payment = await _paymentClient.ProcessPaymentAsync(order.Id, order.Total.Amount, cancellationToken);

        // 4. Actuar según la respuesta. El cambio de estado lo hace el dominio
        if (payment.IsApproved)
        {
            order.MarkAsPaid(payment.TransactionId);
        }
        else
        {
            order.RejectPayment();

            // Pago rechazado: se devuelve el stock que la orden había reservado
            foreach (var item in order.Items)
            {
                var product = await _productRepository.ObtenerPorIdAsync(item.ProductId, cancellationToken);
                product?.AddStock(item.Quantity);
            }
        }

        // 5. Un solo SaveChanges: estado de la orden + stock devuelto
        await _orderRepository.GuardarCambiosAsync(cancellationToken);

        return OrdenDto.DesdeEntidad(order);
    }
}