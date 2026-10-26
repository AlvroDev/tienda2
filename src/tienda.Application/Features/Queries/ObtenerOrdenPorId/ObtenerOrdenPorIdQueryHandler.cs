using MediatR;
using tienda.Application.Contracts.Persistence;
using tienda.Application.Features.Ordenes.Dtos;
using tienda.Domain.Exceptions;

namespace tienda.Application.Features.Ordenes.Queries.ObtenerOrdenPorId;

public class ObtenerOrdenPorIdQueryHandler : IRequestHandler<ObtenerOrdenPorIdQuery, OrdenDto>
{
    private readonly IOrderRepository _repository;

    public ObtenerOrdenPorIdQueryHandler(IOrderRepository repository)
    {
        _repository = repository;
    }

    public async Task<OrdenDto> Handle(ObtenerOrdenPorIdQuery request, CancellationToken cancellationToken)
    {
        var order = await _repository.ObtenerPorIdConItemsAsync(request.OrderId, cancellationToken);

        // Si no existe O es de otro usuario: 404 (no revelamos que la orden existe)
        if (order is null || order.UserId != request.UserId)
            throw new NotFoundException("Orden", request.OrderId);

        return OrdenDto.DesdeEntidad(order);
    }
}