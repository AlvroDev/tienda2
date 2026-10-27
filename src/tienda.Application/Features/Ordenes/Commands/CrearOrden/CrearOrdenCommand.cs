using MediatR;

namespace tienda.Application.Features.Ordenes.Commands.CrearOrden;

public record CrearOrdenCommand(Guid UserId, List<CrearOrdenItem> Items) : IRequest<Guid>;

public record CrearOrdenItem(Guid ProductId, int Cantidad);