using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using tienda.Application.Features.Ordenes.Commands.CrearOrden;
using tienda.Application.Features.Ordenes.Queries.ObtenerMisOrdenes;
using tienda.Application.Features.Ordenes.Queries.ObtenerOrdenPorId;
using tienda.WebApi.Controllers.Requests;
using tienda.Application.Features.Ordenes.Commands.ConfirmarOrden;

namespace tienda.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // cualquier usuario logueado (Admin o Customer)
public class OrdenesController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrdenesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Crea una orden para el usuario logueado. Descuenta el stock de cada producto.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearOrdenRequest request, CancellationToken ct)
    {
        var items = (request.Items ?? new List<CrearOrdenItemRequest>())
            .Select(i => new CrearOrdenItem(i.ProductoId, i.Cantidad))
            .ToList();

        var command = new CrearOrdenCommand(ObtenerUserId(), items);
        var id = await _mediator.Send(command, ct);

        return CreatedAtAction(nameof(ObtenerPorId), new { id }, new { id });
    }

    /// <summary>
    /// Confirma una orden: la manda a pagar al PaymentService y actualiza su estado.
    /// </summary>
    [HttpPost("{id:guid}/confirmar")]
    public async Task<IActionResult> Confirmar(Guid id, CancellationToken ct)
    {
        var orden = await _mediator.Send(new ConfirmarOrdenCommand(id, ObtenerUserId()), ct);
        return Ok(orden);
    }

    /// <summary>
    /// Obtiene una orden del usuario logueado por su ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> ObtenerPorId(Guid id, CancellationToken ct)
    {
        var orden = await _mediator.Send(new ObtenerOrdenPorIdQuery(id, ObtenerUserId()), ct);
        return Ok(orden);
    }

    /// <summary>
    /// Lista las órdenes del usuario logueado, de la más nueva a la más vieja.
    /// </summary>
    [HttpGet("mis-ordenes")]
    public async Task<IActionResult> ObtenerMisOrdenes(CancellationToken ct)
    {
        var ordenes = await _mediator.Send(new ObtenerMisOrdenesQuery(ObtenerUserId()), ct);
        return Ok(ordenes);
    }

    // El id del usuario sale del token (claim NameIdentifier), nunca del body
    private Guid ObtenerUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}