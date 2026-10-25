using MediatR;
using tienda.Application.Contracts.Persistence;
using tienda.Application.Features.Productos.Dtos;
using tienda.Domain.Exceptions;

namespace tienda.Application.Features.Productos.Queries.GetProductoPorId;

public class ObtenerProductoPorIdQueryHandler : IRequestHandler<ObtenerProductoPorIdQuery, ProductoDto>
{
    private readonly IProductRepository _repository;

    public ObtenerProductoPorIdQueryHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<ProductoDto> Handle(ObtenerProductoPorIdQuery request, CancellationToken cancellationToken)
    {
        var producto = await _repository.ObtenerPorIdAsync(request.Id, cancellationToken);

        // Si no existe, lanzamos la excepción y el GlobalExceptionHandler responde 404
        if (producto is null)
            throw new NotFoundException("Producto", request.Id);

        // El handler mapea la entidad de dominio a DTO antes de devolverla
        return new ProductoDto
        {
            Id = producto.Id,
            Nombre = producto.Name,
            Descripcion = producto.Description,
            Precio = producto.Price,
            Stock = producto.Stock,
            EstaActivo = producto.IsActive
        };
    }
}