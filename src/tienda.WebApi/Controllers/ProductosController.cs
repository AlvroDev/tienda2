using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using tienda.Application.Features.Productos.Commands.CrearProducto;
using tienda.Application.Features.Productos.Queries.GetProductoPorId;
using tienda.Application.Features.Productos.Queries.GetProductos;
using tienda.WebApi.Controllers.Requests;

namespace tienda.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductosController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductosController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Lista todos los productos.
    /// Rol requerido: Admin o Customer.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ObtenerTodos(CancellationToken ct)
    {
        var query = new ObtenerProductosQuery();
        var productos = await _mediator.Send(query, ct);
        return Ok(productos);
    }

    /// <summary>
    /// Obtiene un producto por su ID.
    /// Rol requerido: Admin o Customer.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> ObtenerPorId(Guid id, CancellationToken ct)
    {
        var query = new ObtenerProductoPorIdQuery(id);
        var producto = await _mediator.Send(query, ct);

        return Ok(producto);
    }

    /// <summary>
    /// Crea un nuevo producto.
    /// Rol requerido: solo Admin.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Crear([FromBody] CrearProductoRequest request, CancellationToken ct)
    {
        // El controller mapea el Request (capa HTTP) al Command (capa Application)
        var command = new CrearProductoCommand(
            request.Nombre,
            request.Descripcion,
            request.Precio,
            request.Stock);

        var id = await _mediator.Send(command, ct);

        return CreatedAtAction(nameof(ObtenerPorId), new { id }, new { id });
    }
}
