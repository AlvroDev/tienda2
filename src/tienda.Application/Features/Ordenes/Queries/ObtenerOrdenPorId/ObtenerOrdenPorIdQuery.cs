using MediatR;
using tienda.Application.Features.Ordenes.Dtos;

namespace tienda.Application.Features.Ordenes.Queries.ObtenerOrdenPorId;

public record ObtenerOrdenPorIdQuery(Guid OrderId, Guid UserId) : IRequest<OrdenDto>;