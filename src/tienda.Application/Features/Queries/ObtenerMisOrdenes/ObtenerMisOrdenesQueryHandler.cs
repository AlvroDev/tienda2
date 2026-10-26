using MediatR;
using tienda.Application.Contracts.Persistence;
using tienda.Application.Features.Ordenes.Dtos;

namespace tienda.Application.Features.Ordenes.Queries.ObtenerMisOrdenes;

public class ObtenerMisOrdenesQueryHandler : IRequestHandler<ObtenerMisOrdenesQuery, List<OrdenDto>>
{
    private readonly IOrderRepository _repository;

    public ObtenerMisOrdenesQueryHandler(IOrderRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<OrdenDto>> Handle(ObtenerMisOrdenesQuery request, CancellationToken cancellationToken)
    {
        var orders = await _repository.ObtenerPorUsuarioAsync(request.UserId, cancellationToken);
        return orders.Select(OrdenDto.DesdeEntidad).ToList();
    }
}