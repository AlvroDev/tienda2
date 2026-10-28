using MediatR;
using Microsoft.AspNetCore.Mvc;
using PaymentService.Api.Controllers.Requests;
using PaymentService.Application.Features.Payments.Commands.ProcessPayment;
using PaymentService.Application.Features.Payments.Queries.GetPaymentByOrderId;
using PaymentService.Api.Filters;

namespace PaymentService.Api.Controllers;

[ApiController]
[Route("api/payments")]
[ServiceFilter(typeof(ApiKeyAuthFilter))]
public class PaymentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PaymentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Procesa el pago de una orden. Aprueba si el monto es hasta 100.000, si no rechaza.
    /// </summary>
    [HttpPost("process")]
    public async Task<IActionResult> Process([FromBody] ProcessPaymentRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new ProcessPaymentCommand(request.OrderId, request.Amount), ct);
        return Ok(result);
    }

    /// <summary>
    /// Consulta el pago de una orden.
    /// </summary>
    [HttpGet("{orderId:guid}")]
    public async Task<IActionResult> GetByOrderId(Guid orderId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetPaymentByOrderIdQuery(orderId), ct);
        return Ok(result);
    }
}