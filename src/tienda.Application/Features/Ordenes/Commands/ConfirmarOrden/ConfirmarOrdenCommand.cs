using MediatR;
using tienda.Application.Features.Ordenes.Dtos;

namespace tienda.Application.Features.Ordenes.Commands.ConfirmarOrden;

public record ConfirmarOrdenCommand(Guid OrderId, Guid UserId) : IRequest<OrdenDto>;