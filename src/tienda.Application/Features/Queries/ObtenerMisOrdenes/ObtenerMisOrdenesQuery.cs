using MediatR;
using tienda.Application.Features.Ordenes.Dtos;

namespace tienda.Application.Features.Ordenes.Queries.ObtenerMisOrdenes;

public record ObtenerMisOrdenesQuery(Guid UserId) : IRequest<List<OrdenDto>>;