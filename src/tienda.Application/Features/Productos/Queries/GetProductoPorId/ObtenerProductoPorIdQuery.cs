using MediatR;
using tienda.Application.Features.Productos.Dtos;

namespace tienda.Application.Features.Productos.Queries.GetProductoPorId;

public record ObtenerProductoPorIdQuery(Guid Id) : IRequest<ProductoDto>;
